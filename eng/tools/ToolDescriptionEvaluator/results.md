# Tool Selection Analysis Setup

**Setup completed:** 2025-11-10 11:23:50  
**Tool count:** 179  
**Database setup time:** 2.2959325s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-11-10 11:23:50  
**Tool count:** 179  

## Table of Contents

- [Test 1: foundry_agents_connect](#test-1)
- [Test 2: foundry_agents_create](#test-2)
- [Test 3: foundry_agents_evaluate](#test-3)
- [Test 4: foundry_agents_get-sdk-sample](#test-4)
- [Test 5: foundry_agents_list](#test-5)
- [Test 6: foundry_agents_list](#test-6)
- [Test 7: foundry_agents_query-and-evaluate](#test-7)
- [Test 8: foundry_knowledge_index_list](#test-8)
- [Test 9: foundry_knowledge_index_list](#test-9)
- [Test 10: foundry_knowledge_index_schema](#test-10)
- [Test 11: foundry_knowledge_index_schema](#test-11)
- [Test 12: foundry_models_deploy](#test-12)
- [Test 13: foundry_models_deployments_list](#test-13)
- [Test 14: foundry_models_deployments_list](#test-14)
- [Test 15: foundry_models_list](#test-15)
- [Test 16: foundry_models_list](#test-16)
- [Test 17: foundry_openai_chat-completions-create](#test-17)
- [Test 18: foundry_openai_create-completion](#test-18)
- [Test 19: foundry_openai_embeddings-create](#test-19)
- [Test 20: foundry_openai_embeddings-create](#test-20)
- [Test 21: foundry_openai_models-list](#test-21)
- [Test 22: foundry_openai_models-list](#test-22)
- [Test 23: foundry_resource_get](#test-23)
- [Test 24: foundry_resource_get](#test-24)
- [Test 25: foundry_resource_get](#test-25)
- [Test 26: foundry_threads_create](#test-26)
- [Test 27: foundry_threads_get-messages](#test-27)
- [Test 28: foundry_threads_list](#test-28)
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
- [Test 61: speech_stt_recognize](#test-61)
- [Test 62: speech_stt_recognize](#test-62)
- [Test 63: speech_stt_recognize](#test-63)
- [Test 64: speech_stt_recognize](#test-64)
- [Test 65: speech_stt_recognize](#test-65)
- [Test 66: speech_tts_synthesize](#test-66)
- [Test 67: speech_tts_synthesize](#test-67)
- [Test 68: speech_tts_synthesize](#test-68)
- [Test 69: speech_tts_synthesize](#test-69)
- [Test 70: speech_tts_synthesize](#test-70)
- [Test 71: speech_tts_synthesize](#test-71)
- [Test 72: speech_tts_synthesize](#test-72)
- [Test 73: speech_tts_synthesize](#test-73)
- [Test 74: speech_tts_synthesize](#test-74)
- [Test 75: speech_tts_synthesize](#test-75)
- [Test 76: appconfig_account_list](#test-76)
- [Test 77: appconfig_account_list](#test-77)
- [Test 78: appconfig_account_list](#test-78)
- [Test 79: appconfig_kv_delete](#test-79)
- [Test 80: appconfig_kv_get](#test-80)
- [Test 81: appconfig_kv_get](#test-81)
- [Test 82: appconfig_kv_get](#test-82)
- [Test 83: appconfig_kv_get](#test-83)
- [Test 84: appconfig_kv_lock_set](#test-84)
- [Test 85: appconfig_kv_lock_set](#test-85)
- [Test 86: appconfig_kv_set](#test-86)
- [Test 87: applens_resource_diagnose](#test-87)
- [Test 88: applens_resource_diagnose](#test-88)
- [Test 89: applens_resource_diagnose](#test-89)
- [Test 90: appservice_database_add](#test-90)
- [Test 91: appservice_database_add](#test-91)
- [Test 92: appservice_database_add](#test-92)
- [Test 93: appservice_database_add](#test-93)
- [Test 94: appservice_database_add](#test-94)
- [Test 95: appservice_database_add](#test-95)
- [Test 96: appservice_database_add](#test-96)
- [Test 97: appservice_database_add](#test-97)
- [Test 98: appservice_database_add](#test-98)
- [Test 99: appservice_database_add](#test-99)
- [Test 100: applicationinsights_recommendation_list](#test-100)
- [Test 101: applicationinsights_recommendation_list](#test-101)
- [Test 102: applicationinsights_recommendation_list](#test-102)
- [Test 103: applicationinsights_recommendation_list](#test-103)
- [Test 104: extension_cli_generate](#test-104)
- [Test 105: extension_cli_generate](#test-105)
- [Test 106: extension_cli_generate](#test-106)
- [Test 107: extension_cli_install](#test-107)
- [Test 108: extension_cli_install](#test-108)
- [Test 109: extension_cli_install](#test-109)
- [Test 110: acr_registry_list](#test-110)
- [Test 111: acr_registry_list](#test-111)
- [Test 112: acr_registry_list](#test-112)
- [Test 113: acr_registry_list](#test-113)
- [Test 114: acr_registry_list](#test-114)
- [Test 115: acr_registry_repository_list](#test-115)
- [Test 116: acr_registry_repository_list](#test-116)
- [Test 117: acr_registry_repository_list](#test-117)
- [Test 118: acr_registry_repository_list](#test-118)
- [Test 119: communication_email_send](#test-119)
- [Test 120: communication_email_send](#test-120)
- [Test 121: communication_email_send](#test-121)
- [Test 122: communication_email_send](#test-122)
- [Test 123: communication_email_send](#test-123)
- [Test 124: communication_email_send](#test-124)
- [Test 125: communication_email_send](#test-125)
- [Test 126: communication_email_send](#test-126)
- [Test 127: communication_sms_send](#test-127)
- [Test 128: communication_sms_send](#test-128)
- [Test 129: communication_sms_send](#test-129)
- [Test 130: communication_sms_send](#test-130)
- [Test 131: communication_sms_send](#test-131)
- [Test 132: communication_sms_send](#test-132)
- [Test 133: communication_sms_send](#test-133)
- [Test 134: communication_sms_send](#test-134)
- [Test 135: confidentialledger_entries_append](#test-135)
- [Test 136: confidentialledger_entries_append](#test-136)
- [Test 137: confidentialledger_entries_append](#test-137)
- [Test 138: confidentialledger_entries_append](#test-138)
- [Test 139: confidentialledger_entries_append](#test-139)
- [Test 140: confidentialledger_entries_get](#test-140)
- [Test 141: confidentialledger_entries_get](#test-141)
- [Test 142: cosmos_account_list](#test-142)
- [Test 143: cosmos_account_list](#test-143)
- [Test 144: cosmos_account_list](#test-144)
- [Test 145: cosmos_database_container_item_query](#test-145)
- [Test 146: cosmos_database_container_list](#test-146)
- [Test 147: cosmos_database_container_list](#test-147)
- [Test 148: cosmos_database_list](#test-148)
- [Test 149: cosmos_database_list](#test-149)
- [Test 150: kusto_cluster_get](#test-150)
- [Test 151: kusto_cluster_list](#test-151)
- [Test 152: kusto_cluster_list](#test-152)
- [Test 153: kusto_cluster_list](#test-153)
- [Test 154: kusto_database_list](#test-154)
- [Test 155: kusto_database_list](#test-155)
- [Test 156: kusto_query](#test-156)
- [Test 157: kusto_sample](#test-157)
- [Test 158: kusto_table_list](#test-158)
- [Test 159: kusto_table_list](#test-159)
- [Test 160: kusto_table_schema](#test-160)
- [Test 161: mysql_database_list](#test-161)
- [Test 162: mysql_database_list](#test-162)
- [Test 163: mysql_database_query](#test-163)
- [Test 164: mysql_server_config_get](#test-164)
- [Test 165: mysql_server_list](#test-165)
- [Test 166: mysql_server_list](#test-166)
- [Test 167: mysql_server_list](#test-167)
- [Test 168: mysql_server_param_get](#test-168)
- [Test 169: mysql_server_param_set](#test-169)
- [Test 170: mysql_table_list](#test-170)
- [Test 171: mysql_table_list](#test-171)
- [Test 172: mysql_table_schema_get](#test-172)
- [Test 173: postgres_database_list](#test-173)
- [Test 174: postgres_database_list](#test-174)
- [Test 175: postgres_database_query](#test-175)
- [Test 176: postgres_server_config_get](#test-176)
- [Test 177: postgres_server_list](#test-177)
- [Test 178: postgres_server_list](#test-178)
- [Test 179: postgres_server_list](#test-179)
- [Test 180: postgres_server_param_get](#test-180)
- [Test 181: postgres_server_param_set](#test-181)
- [Test 182: postgres_table_list](#test-182)
- [Test 183: postgres_table_list](#test-183)
- [Test 184: postgres_table_schema_get](#test-184)
- [Test 185: deploy_app_logs_get](#test-185)
- [Test 186: deploy_architecture_diagram_generate](#test-186)
- [Test 187: deploy_iac_rules_get](#test-187)
- [Test 188: deploy_pipeline_guidance_get](#test-188)
- [Test 189: deploy_plan_get](#test-189)
- [Test 190: eventgrid_events_publish](#test-190)
- [Test 191: eventgrid_events_publish](#test-191)
- [Test 192: eventgrid_events_publish](#test-192)
- [Test 193: eventgrid_topic_list](#test-193)
- [Test 194: eventgrid_topic_list](#test-194)
- [Test 195: eventgrid_topic_list](#test-195)
- [Test 196: eventgrid_topic_list](#test-196)
- [Test 197: eventgrid_subscription_list](#test-197)
- [Test 198: eventgrid_subscription_list](#test-198)
- [Test 199: eventgrid_subscription_list](#test-199)
- [Test 200: eventgrid_subscription_list](#test-200)
- [Test 201: eventgrid_subscription_list](#test-201)
- [Test 202: eventgrid_subscription_list](#test-202)
- [Test 203: eventgrid_subscription_list](#test-203)
- [Test 204: eventhubs_eventhub_consumergroup_delete](#test-204)
- [Test 205: eventhubs_eventhub_consumergroup_get](#test-205)
- [Test 206: eventhubs_eventhub_consumergroup_get](#test-206)
- [Test 207: eventhubs_eventhub_consumergroup_update](#test-207)
- [Test 208: eventhubs_eventhub_consumergroup_update](#test-208)
- [Test 209: eventhubs_eventhub_delete](#test-209)
- [Test 210: eventhubs_eventhub_get](#test-210)
- [Test 211: eventhubs_eventhub_get](#test-211)
- [Test 212: eventhubs_eventhub_update](#test-212)
- [Test 213: eventhubs_eventhub_update](#test-213)
- [Test 214: eventhubs_namespace_delete](#test-214)
- [Test 215: eventhubs_namespace_get](#test-215)
- [Test 216: eventhubs_namespace_get](#test-216)
- [Test 217: eventhubs_namespace_update](#test-217)
- [Test 218: eventhubs_namespace_update](#test-218)
- [Test 219: functionapp_get](#test-219)
- [Test 220: functionapp_get](#test-220)
- [Test 221: functionapp_get](#test-221)
- [Test 222: functionapp_get](#test-222)
- [Test 223: functionapp_get](#test-223)
- [Test 224: functionapp_get](#test-224)
- [Test 225: functionapp_get](#test-225)
- [Test 226: functionapp_get](#test-226)
- [Test 227: functionapp_get](#test-227)
- [Test 228: functionapp_get](#test-228)
- [Test 229: functionapp_get](#test-229)
- [Test 230: functionapp_get](#test-230)
- [Test 231: keyvault_admin_settings_get](#test-231)
- [Test 232: keyvault_admin_settings_get](#test-232)
- [Test 233: keyvault_admin_settings_get](#test-233)
- [Test 234: keyvault_certificate_create](#test-234)
- [Test 235: keyvault_certificate_create](#test-235)
- [Test 236: keyvault_certificate_create](#test-236)
- [Test 237: keyvault_certificate_create](#test-237)
- [Test 238: keyvault_certificate_create](#test-238)
- [Test 239: keyvault_certificate_get](#test-239)
- [Test 240: keyvault_certificate_get](#test-240)
- [Test 241: keyvault_certificate_get](#test-241)
- [Test 242: keyvault_certificate_get](#test-242)
- [Test 243: keyvault_certificate_get](#test-243)
- [Test 244: keyvault_certificate_import](#test-244)
- [Test 245: keyvault_certificate_import](#test-245)
- [Test 246: keyvault_certificate_import](#test-246)
- [Test 247: keyvault_certificate_import](#test-247)
- [Test 248: keyvault_certificate_import](#test-248)
- [Test 249: keyvault_certificate_list](#test-249)
- [Test 250: keyvault_certificate_list](#test-250)
- [Test 251: keyvault_certificate_list](#test-251)
- [Test 252: keyvault_certificate_list](#test-252)
- [Test 253: keyvault_certificate_list](#test-253)
- [Test 254: keyvault_certificate_list](#test-254)
- [Test 255: keyvault_key_create](#test-255)
- [Test 256: keyvault_key_create](#test-256)
- [Test 257: keyvault_key_create](#test-257)
- [Test 258: keyvault_key_create](#test-258)
- [Test 259: keyvault_key_create](#test-259)
- [Test 260: keyvault_key_get](#test-260)
- [Test 261: keyvault_key_get](#test-261)
- [Test 262: keyvault_key_get](#test-262)
- [Test 263: keyvault_key_get](#test-263)
- [Test 264: keyvault_key_get](#test-264)
- [Test 265: keyvault_key_list](#test-265)
- [Test 266: keyvault_key_list](#test-266)
- [Test 267: keyvault_key_list](#test-267)
- [Test 268: keyvault_key_list](#test-268)
- [Test 269: keyvault_key_list](#test-269)
- [Test 270: keyvault_key_list](#test-270)
- [Test 271: keyvault_secret_create](#test-271)
- [Test 272: keyvault_secret_create](#test-272)
- [Test 273: keyvault_secret_create](#test-273)
- [Test 274: keyvault_secret_create](#test-274)
- [Test 275: keyvault_secret_create](#test-275)
- [Test 276: keyvault_secret_get](#test-276)
- [Test 277: keyvault_secret_get](#test-277)
- [Test 278: keyvault_secret_get](#test-278)
- [Test 279: keyvault_secret_get](#test-279)
- [Test 280: keyvault_secret_get](#test-280)
- [Test 281: keyvault_secret_list](#test-281)
- [Test 282: keyvault_secret_list](#test-282)
- [Test 283: keyvault_secret_list](#test-283)
- [Test 284: keyvault_secret_list](#test-284)
- [Test 285: keyvault_secret_list](#test-285)
- [Test 286: keyvault_secret_list](#test-286)
- [Test 287: aks_cluster_get](#test-287)
- [Test 288: aks_cluster_get](#test-288)
- [Test 289: aks_cluster_get](#test-289)
- [Test 290: aks_cluster_get](#test-290)
- [Test 291: aks_cluster_get](#test-291)
- [Test 292: aks_cluster_get](#test-292)
- [Test 293: aks_cluster_get](#test-293)
- [Test 294: aks_nodepool_get](#test-294)
- [Test 295: aks_nodepool_get](#test-295)
- [Test 296: aks_nodepool_get](#test-296)
- [Test 297: aks_nodepool_get](#test-297)
- [Test 298: aks_nodepool_get](#test-298)
- [Test 299: aks_nodepool_get](#test-299)
- [Test 300: loadtesting_test_create](#test-300)
- [Test 301: loadtesting_test_get](#test-301)
- [Test 302: loadtesting_testresource_create](#test-302)
- [Test 303: loadtesting_testresource_list](#test-303)
- [Test 304: loadtesting_testrun_create](#test-304)
- [Test 305: loadtesting_testrun_get](#test-305)
- [Test 306: loadtesting_testrun_list](#test-306)
- [Test 307: loadtesting_testrun_update](#test-307)
- [Test 308: grafana_list](#test-308)
- [Test 309: managedlustre_fs_create](#test-309)
- [Test 310: managedlustre_fs_list](#test-310)
- [Test 311: managedlustre_fs_list](#test-311)
- [Test 312: managedlustre_fs_sku_get](#test-312)
- [Test 313: managedlustre_fs_subnetsize_ask](#test-313)
- [Test 314: managedlustre_fs_subnetsize_validate](#test-314)
- [Test 315: managedlustre_fs_update](#test-315)
- [Test 316: marketplace_product_get](#test-316)
- [Test 317: marketplace_product_list](#test-317)
- [Test 318: marketplace_product_list](#test-318)
- [Test 319: azureaibestpractices_get](#test-319)
- [Test 320: azureaibestpractices_get](#test-320)
- [Test 321: azureaibestpractices_get](#test-321)
- [Test 322: azureaibestpractices_get](#test-322)
- [Test 323: azureaibestpractices_get](#test-323)
- [Test 324: get_bestpractices_get](#test-324)
- [Test 325: get_bestpractices_get](#test-325)
- [Test 326: get_bestpractices_get](#test-326)
- [Test 327: get_bestpractices_get](#test-327)
- [Test 328: get_bestpractices_get](#test-328)
- [Test 329: get_bestpractices_get](#test-329)
- [Test 330: get_bestpractices_get](#test-330)
- [Test 331: get_bestpractices_get](#test-331)
- [Test 332: get_bestpractices_get](#test-332)
- [Test 333: monitor_activitylog_list](#test-333)
- [Test 334: monitor_healthmodels_entity_get](#test-334)
- [Test 335: monitor_metrics_definitions](#test-335)
- [Test 336: monitor_metrics_definitions](#test-336)
- [Test 337: monitor_metrics_definitions](#test-337)
- [Test 338: monitor_metrics_query](#test-338)
- [Test 339: monitor_metrics_query](#test-339)
- [Test 340: monitor_metrics_query](#test-340)
- [Test 341: monitor_metrics_query](#test-341)
- [Test 342: monitor_metrics_query](#test-342)
- [Test 343: monitor_metrics_query](#test-343)
- [Test 344: monitor_resource_log_query](#test-344)
- [Test 345: monitor_table_list](#test-345)
- [Test 346: monitor_table_list](#test-346)
- [Test 347: monitor_table_type_list](#test-347)
- [Test 348: monitor_table_type_list](#test-348)
- [Test 349: monitor_webtests_create](#test-349)
- [Test 350: monitor_webtests_get](#test-350)
- [Test 351: monitor_webtests_list](#test-351)
- [Test 352: monitor_webtests_list](#test-352)
- [Test 353: monitor_webtests_update](#test-353)
- [Test 354: monitor_workspace_list](#test-354)
- [Test 355: monitor_workspace_list](#test-355)
- [Test 356: monitor_workspace_list](#test-356)
- [Test 357: monitor_workspace_log_query](#test-357)
- [Test 358: datadog_monitoredresources_list](#test-358)
- [Test 359: datadog_monitoredresources_list](#test-359)
- [Test 360: extension_azqr](#test-360)
- [Test 361: extension_azqr](#test-361)
- [Test 362: extension_azqr](#test-362)
- [Test 363: quota_region_availability_list](#test-363)
- [Test 364: quota_usage_check](#test-364)
- [Test 365: role_assignment_list](#test-365)
- [Test 366: role_assignment_list](#test-366)
- [Test 367: redis_list](#test-367)
- [Test 368: redis_list](#test-368)
- [Test 369: redis_list](#test-369)
- [Test 370: redis_list](#test-370)
- [Test 371: redis_list](#test-371)
- [Test 372: group_list](#test-372)
- [Test 373: group_list](#test-373)
- [Test 374: group_list](#test-374)
- [Test 375: resourcehealth_availability-status_get](#test-375)
- [Test 376: resourcehealth_availability-status_get](#test-376)
- [Test 377: resourcehealth_availability-status_get](#test-377)
- [Test 378: resourcehealth_availability-status_list](#test-378)
- [Test 379: resourcehealth_availability-status_list](#test-379)
- [Test 380: resourcehealth_availability-status_list](#test-380)
- [Test 381: resourcehealth_health-events_list](#test-381)
- [Test 382: resourcehealth_health-events_list](#test-382)
- [Test 383: resourcehealth_health-events_list](#test-383)
- [Test 384: resourcehealth_health-events_list](#test-384)
- [Test 385: resourcehealth_health-events_list](#test-385)
- [Test 386: servicebus_queue_details](#test-386)
- [Test 387: servicebus_topic_details](#test-387)
- [Test 388: servicebus_topic_subscription_details](#test-388)
- [Test 389: signalr_runtime_get](#test-389)
- [Test 390: signalr_runtime_get](#test-390)
- [Test 391: signalr_runtime_get](#test-391)
- [Test 392: signalr_runtime_get](#test-392)
- [Test 393: signalr_runtime_get](#test-393)
- [Test 394: signalr_runtime_get](#test-394)
- [Test 395: sql_db_create](#test-395)
- [Test 396: sql_db_create](#test-396)
- [Test 397: sql_db_create](#test-397)
- [Test 398: sql_db_delete](#test-398)
- [Test 399: sql_db_delete](#test-399)
- [Test 400: sql_db_delete](#test-400)
- [Test 401: sql_db_list](#test-401)
- [Test 402: sql_db_list](#test-402)
- [Test 403: sql_db_rename](#test-403)
- [Test 404: sql_db_rename](#test-404)
- [Test 405: sql_db_show](#test-405)
- [Test 406: sql_db_show](#test-406)
- [Test 407: sql_db_update](#test-407)
- [Test 408: sql_db_update](#test-408)
- [Test 409: sql_elastic-pool_list](#test-409)
- [Test 410: sql_elastic-pool_list](#test-410)
- [Test 411: sql_elastic-pool_list](#test-411)
- [Test 412: sql_server_create](#test-412)
- [Test 413: sql_server_create](#test-413)
- [Test 414: sql_server_create](#test-414)
- [Test 415: sql_server_delete](#test-415)
- [Test 416: sql_server_delete](#test-416)
- [Test 417: sql_server_delete](#test-417)
- [Test 418: sql_server_entra-admin_list](#test-418)
- [Test 419: sql_server_entra-admin_list](#test-419)
- [Test 420: sql_server_entra-admin_list](#test-420)
- [Test 421: sql_server_firewall-rule_create](#test-421)
- [Test 422: sql_server_firewall-rule_create](#test-422)
- [Test 423: sql_server_firewall-rule_create](#test-423)
- [Test 424: sql_server_firewall-rule_delete](#test-424)
- [Test 425: sql_server_firewall-rule_delete](#test-425)
- [Test 426: sql_server_firewall-rule_delete](#test-426)
- [Test 427: sql_server_firewall-rule_list](#test-427)
- [Test 428: sql_server_firewall-rule_list](#test-428)
- [Test 429: sql_server_firewall-rule_list](#test-429)
- [Test 430: sql_server_list](#test-430)
- [Test 431: sql_server_list](#test-431)
- [Test 432: sql_server_show](#test-432)
- [Test 433: sql_server_show](#test-433)
- [Test 434: sql_server_show](#test-434)
- [Test 435: storage_account_create](#test-435)
- [Test 436: storage_account_create](#test-436)
- [Test 437: storage_account_create](#test-437)
- [Test 438: storage_account_get](#test-438)
- [Test 439: storage_account_get](#test-439)
- [Test 440: storage_account_get](#test-440)
- [Test 441: storage_account_get](#test-441)
- [Test 442: storage_account_get](#test-442)
- [Test 443: storage_blob_container_create](#test-443)
- [Test 444: storage_blob_container_create](#test-444)
- [Test 445: storage_blob_container_create](#test-445)
- [Test 446: storage_blob_container_get](#test-446)
- [Test 447: storage_blob_container_get](#test-447)
- [Test 448: storage_blob_container_get](#test-448)
- [Test 449: storage_blob_get](#test-449)
- [Test 450: storage_blob_get](#test-450)
- [Test 451: storage_blob_get](#test-451)
- [Test 452: storage_blob_get](#test-452)
- [Test 453: storage_blob_upload](#test-453)
- [Test 454: subscription_list](#test-454)
- [Test 455: subscription_list](#test-455)
- [Test 456: subscription_list](#test-456)
- [Test 457: subscription_list](#test-457)
- [Test 458: azureterraformbestpractices_get](#test-458)
- [Test 459: azureterraformbestpractices_get](#test-459)
- [Test 460: virtualdesktop_hostpool_list](#test-460)
- [Test 461: virtualdesktop_hostpool_host_list](#test-461)
- [Test 462: virtualdesktop_hostpool_host_user-list](#test-462)
- [Test 463: workbooks_create](#test-463)
- [Test 464: workbooks_delete](#test-464)
- [Test 465: workbooks_list](#test-465)
- [Test 466: workbooks_list](#test-466)
- [Test 467: workbooks_show](#test-467)
- [Test 468: workbooks_show](#test-468)
- [Test 469: workbooks_update](#test-469)
- [Test 470: bicepschema_get](#test-470)
- [Test 471: cloudarchitect_design](#test-471)
- [Test 472: cloudarchitect_design](#test-472)
- [Test 473: cloudarchitect_design](#test-473)
- [Test 474: cloudarchitect_design](#test-474)

---

## Test 1

**Expected Tool:** `foundry_agents_connect`  
**Prompt:** Query an agent in my Azure AI foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.705410 | `foundry_agents_connect` | ✅ **EXPECTED** |
| 2 | 0.663568 | `foundry_agents_list` | ❌ |
| 3 | 0.617213 | `foundry_resource_get` | ❌ |
| 4 | 0.548108 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.548044 | `foundry_openai_models-list` | ❌ |

---

## Test 2

**Expected Tool:** `foundry_agents_create`  
**Prompt:** Create a new Azure AI Foundry agent using instructions in the active editor  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586996 | `foundry_agents_create` | ✅ **EXPECTED** |
| 2 | 0.562087 | `foundry_agents_get-sdk-sample` | ❌ |
| 3 | 0.554009 | `foundry_threads_create` | ❌ |
| 4 | 0.525727 | `foundry_models_deploy` | ❌ |
| 5 | 0.525615 | `foundry_agents_list` | ❌ |

---

## Test 3

**Expected Tool:** `foundry_agents_evaluate`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.544099 | `foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.469428 | `foundry_agents_evaluate` | ✅ **EXPECTED** |
| 3 | 0.445964 | `foundry_agents_connect` | ❌ |
| 4 | 0.297986 | `foundry_threads_list` | ❌ |
| 5 | 0.278921 | `foundry_agents_list` | ❌ |

---

## Test 4

**Expected Tool:** `foundry_agents_get-sdk-sample`  
**Prompt:** Create a CLI app that can talk to an Azure AI Foundry Agent using Python SDK  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595766 | `foundry_agents_get-sdk-sample` | ✅ **EXPECTED** |
| 2 | 0.552180 | `foundry_threads_create` | ❌ |
| 3 | 0.521920 | `foundry_agents_connect` | ❌ |
| 4 | 0.518652 | `foundry_agents_create` | ❌ |
| 5 | 0.509764 | `foundry_agents_list` | ❌ |

---

## Test 5

**Expected Tool:** `foundry_agents_list`  
**Prompt:** List all agents in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.797877 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.666021 | `foundry_resource_get` | ❌ |
| 3 | 0.654206 | `foundry_openai_models-list` | ❌ |
| 4 | 0.647246 | `foundry_threads_list` | ❌ |
| 5 | 0.575553 | `foundry_models_deployments_list` | ❌ |

---

## Test 6

**Expected Tool:** `foundry_agents_list`  
**Prompt:** Show me the available agents in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.749829 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.630288 | `foundry_resource_get` | ❌ |
| 3 | 0.611722 | `foundry_openai_models-list` | ❌ |
| 4 | 0.603689 | `foundry_threads_list` | ❌ |
| 5 | 0.556990 | `foundry_agents_get-sdk-sample` | ❌ |

---

## Test 7

**Expected Tool:** `foundry_agents_query-and-evaluate`  
**Prompt:** Query and evaluate an agent in my Azure AI Foundry resource for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.652200 | `foundry_agents_connect` | ❌ |
| 2 | 0.570788 | `foundry_agents_list` | ❌ |
| 3 | 0.553190 | `foundry_agents_query-and-evaluate` | ✅ **EXPECTED** |
| 4 | 0.493779 | `foundry_agents_evaluate` | ❌ |
| 5 | 0.469431 | `foundry_threads_list` | ❌ |

---

## Test 8

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.703772 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.537700 | `foundry_agents_list` | ❌ |
| 3 | 0.526528 | `foundry_knowledge_index_schema` | ❌ |
| 4 | 0.500786 | `foundry_threads_list` | ❌ |
| 5 | 0.475802 | `foundry_models_deployments_list` | ❌ |

---

## Test 9

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** Show me the knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615458 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.489311 | `foundry_knowledge_index_schema` | ❌ |
| 3 | 0.484466 | `foundry_agents_list` | ❌ |
| 4 | 0.454174 | `foundry_threads_list` | ❌ |
| 5 | 0.441521 | `foundry_resource_get` | ❌ |

---

## Test 10

**Expected Tool:** `foundry_knowledge_index_schema`  
**Prompt:** Show me the schema for knowledge index <index-name> in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739885 | `foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.589536 | `foundry_knowledge_index_list` | ❌ |
| 3 | 0.494004 | `foundry_resource_get` | ❌ |
| 4 | 0.491510 | `search_index_get` | ❌ |
| 5 | 0.490410 | `search_knowledge_base_get` | ❌ |

---

## Test 11

**Expected Tool:** `foundry_knowledge_index_schema`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650269 | `foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.432759 | `postgres_table_schema_get` | ❌ |
| 3 | 0.417421 | `kusto_table_schema` | ❌ |
| 4 | 0.398186 | `mysql_table_schema_get` | ❌ |
| 5 | 0.396194 | `foundry_knowledge_index_list` | ❌ |

---

## Test 12

**Expected Tool:** `foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.562920 | `foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.299986 | `foundry_openai_models-list` | ❌ |
| 3 | 0.298490 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.293050 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.290381 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 13

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.681385 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.674510 | `foundry_openai_models-list` | ❌ |
| 3 | 0.572625 | `foundry_threads_list` | ❌ |
| 4 | 0.569059 | `foundry_agents_list` | ❌ |
| 5 | 0.566272 | `foundry_resource_get` | ❌ |

---

## Test 14

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** Show me all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620173 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.619231 | `foundry_openai_models-list` | ❌ |
| 3 | 0.543352 | `foundry_resource_get` | ❌ |
| 4 | 0.540551 | `foundry_agents_list` | ❌ |
| 5 | 0.527121 | `foundry_threads_list` | ❌ |

---

## Test 15

**Expected Tool:** `foundry_models_list`  
**Prompt:** List all AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603415 | `foundry_openai_models-list` | ❌ |
| 2 | 0.560022 | `foundry_models_list` | ✅ **EXPECTED** |
| 3 | 0.553634 | `foundry_threads_list` | ❌ |
| 4 | 0.537981 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.519472 | `foundry_agents_list` | ❌ |

---

## Test 16

**Expected Tool:** `foundry_models_list`  
**Prompt:** Show me the available AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576904 | `foundry_openai_models-list` | ❌ |
| 2 | 0.574818 | `foundry_models_list` | ✅ **EXPECTED** |
| 3 | 0.525312 | `foundry_resource_get` | ❌ |
| 4 | 0.521474 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.517980 | `foundry_models_deployments_list` | ❌ |

---

## Test 17

**Expected Tool:** `foundry_openai_chat-completions-create`  
**Prompt:** Create a chat completion with the message "Hello, how are you today?" using my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641293 | `foundry_openai_chat-completions-create` | ✅ **EXPECTED** |
| 2 | 0.546736 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.419991 | `foundry_threads_create` | ❌ |
| 4 | 0.415483 | `foundry_agents_connect` | ❌ |
| 5 | 0.399383 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 18

**Expected Tool:** `foundry_openai_create-completion`  
**Prompt:** Create a completion with the prompt "What is Azure?" using my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696936 | `foundry_openai_create-completion` | ✅ **EXPECTED** |
| 2 | 0.579108 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.463703 | `foundry_models_deploy` | ❌ |
| 4 | 0.459126 | `foundry_resource_get` | ❌ |
| 5 | 0.458622 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 19

**Expected Tool:** `foundry_openai_embeddings-create`  
**Prompt:** Generate embeddings for the text "Azure OpenAI Service" using my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.766123 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.542818 | `foundry_models_deploy` | ❌ |
| 3 | 0.542113 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.520274 | `foundry_openai_models-list` | ❌ |
| 5 | 0.518834 | `foundry_resource_get` | ❌ |

---

## Test 20

**Expected Tool:** `foundry_openai_embeddings-create`  
**Prompt:** Create vector embeddings for my text using my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.724120 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.494485 | `foundry_resource_get` | ❌ |
| 3 | 0.480296 | `foundry_models_deploy` | ❌ |
| 4 | 0.480218 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.463797 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 21

**Expected Tool:** `foundry_openai_models-list`  
**Prompt:** List all available OpenAI models in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.799059 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.668887 | `foundry_resource_get` | ❌ |
| 3 | 0.667040 | `foundry_models_list` | ❌ |
| 4 | 0.666207 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.657546 | `foundry_agents_list` | ❌ |

---

## Test 22

**Expected Tool:** `foundry_openai_models-list`  
**Prompt:** Show me the OpenAI model deployments in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.741659 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.660160 | `foundry_models_deployments_list` | ❌ |
| 3 | 0.648219 | `foundry_resource_get` | ❌ |
| 4 | 0.640650 | `foundry_models_deploy` | ❌ |
| 5 | 0.619878 | `foundry_agents_list` | ❌ |

---

## Test 23

**Expected Tool:** `foundry_resource_get`  
**Prompt:** List all AI Foundry resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594096 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.571916 | `foundry_openai_models-list` | ❌ |
| 3 | 0.567019 | `foundry_agents_list` | ❌ |
| 4 | 0.558290 | `search_service_list` | ❌ |
| 5 | 0.558076 | `foundry_threads_list` | ❌ |

---

## Test 24

**Expected Tool:** `foundry_resource_get`  
**Prompt:** Show me the AI Foundry resources in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665311 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.585305 | `foundry_openai_models-list` | ❌ |
| 3 | 0.553993 | `foundry_agents_list` | ❌ |
| 4 | 0.518767 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.492911 | `foundry_models_deploy` | ❌ |

---

## Test 25

**Expected Tool:** `foundry_resource_get`  
**Prompt:** Get details for AI Foundry resource <resource_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.735316 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.571906 | `foundry_openai_models-list` | ❌ |
| 3 | 0.509484 | `monitor_webtests_get` | ❌ |
| 4 | 0.497090 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.475722 | `foundry_agents_list` | ❌ |

---

## Test 26

**Expected Tool:** `foundry_threads_create`  
**Prompt:** Create an Azure AI Foundry thread to hold the conversation  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606760 | `foundry_threads_create` | ✅ **EXPECTED** |
| 2 | 0.528310 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.519794 | `foundry_threads_get-messages` | ❌ |
| 4 | 0.506089 | `foundry_threads_list` | ❌ |
| 5 | 0.490796 | `foundry_models_deploy` | ❌ |

---

## Test 27

**Expected Tool:** `foundry_threads_get-messages`  
**Prompt:** Show me the messages in the AI Foundry thread with id <thread_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669946 | `foundry_threads_get-messages` | ✅ **EXPECTED** |
| 2 | 0.584411 | `foundry_threads_create` | ❌ |
| 3 | 0.529381 | `foundry_threads_list` | ❌ |
| 4 | 0.437480 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.427594 | `foundry_agents_create` | ❌ |

---

## Test 28

**Expected Tool:** `foundry_threads_list`  
**Prompt:** List my AI Foundry threads  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.677248 | `foundry_threads_list` | ✅ **EXPECTED** |
| 2 | 0.574065 | `foundry_threads_get-messages` | ❌ |
| 3 | 0.566910 | `foundry_threads_create` | ❌ |
| 4 | 0.471544 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.448963 | `foundry_agents_list` | ❌ |

---

## Test 29

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.785967 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.700824 | `search_knowledge_source_get` | ❌ |
| 3 | 0.693471 | `search_service_list` | ❌ |
| 4 | 0.635863 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.586574 | `search_index_get` | ❌ |

---

## Test 30

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.748213 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.668487 | `search_knowledge_source_get` | ❌ |
| 3 | 0.628582 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.624479 | `search_service_list` | ❌ |
| 5 | 0.566618 | `search_index_get` | ❌ |

---

## Test 31

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702942 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.605964 | `search_knowledge_source_get` | ❌ |
| 3 | 0.583234 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.513638 | `search_service_list` | ❌ |
| 5 | 0.476816 | `foundry_knowledge_index_list` | ❌ |

---

## Test 32

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688051 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.599247 | `search_knowledge_source_get` | ❌ |
| 3 | 0.578499 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.457619 | `search_service_list` | ❌ |
| 5 | 0.439529 | `foundry_knowledge_index_list` | ❌ |

---

## Test 33

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Get the details of knowledge base <agent-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769384 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.685640 | `search_knowledge_source_get` | ❌ |
| 3 | 0.636958 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.585949 | `search_index_get` | ❌ |
| 5 | 0.533700 | `search_service_list` | ❌ |

---

## Test 34

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595585 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.551922 | `search_knowledge_base_retrieve` | ❌ |
| 3 | 0.515480 | `search_knowledge_source_get` | ❌ |
| 4 | 0.366893 | `search_service_list` | ❌ |
| 5 | 0.365633 | `search_index_get` | ❌ |

---

## Test 35

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in Azure AI Search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.724846 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.650590 | `search_knowledge_base_get` | ❌ |
| 3 | 0.575307 | `search_index_query` | ❌ |
| 4 | 0.567361 | `search_knowledge_source_get` | ❌ |
| 5 | 0.520360 | `foundry_agents_connect` | ❌ |

---

## Test 36

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633766 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.589869 | `search_knowledge_base_get` | ❌ |
| 3 | 0.502085 | `search_knowledge_source_get` | ❌ |
| 4 | 0.422671 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.399595 | `search_index_query` | ❌ |

---

## Test 37

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657865 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.557206 | `search_knowledge_base_get` | ❌ |
| 3 | 0.463605 | `search_knowledge_source_get` | ❌ |
| 4 | 0.436739 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.422173 | `foundry_agents_connect` | ❌ |

---

## Test 38

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633677 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.589633 | `search_knowledge_base_get` | ❌ |
| 3 | 0.501914 | `search_knowledge_source_get` | ❌ |
| 4 | 0.422494 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.399138 | `search_index_query` | ❌ |

---

## Test 39

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Query knowledge base <agent-name> in search service <service-name> about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.598868 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.547862 | `search_knowledge_base_get` | ❌ |
| 3 | 0.467907 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.464904 | `search_knowledge_source_get` | ❌ |
| 5 | 0.412481 | `foundry_agents_connect` | ❌ |

---

## Test 40

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Search knowledge base <agent-name> in Azure AI Search service <service-name> for <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649090 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.630905 | `search_knowledge_base_get` | ❌ |
| 3 | 0.581015 | `search_index_query` | ❌ |
| 4 | 0.570684 | `search_knowledge_source_get` | ❌ |
| 5 | 0.544428 | `search_service_list` | ❌ |

---

## Test 41

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** What does knowledge base <agent-name> in search service <service-name> know about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579716 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.560688 | `search_knowledge_base_get` | ❌ |
| 3 | 0.477942 | `search_knowledge_source_get` | ❌ |
| 4 | 0.402582 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.361231 | `foundry_knowledge_index_list` | ❌ |

---

## Test 42

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Find information about <query> using knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582662 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.528610 | `search_knowledge_base_get` | ❌ |
| 3 | 0.449336 | `search_knowledge_source_get` | ❌ |
| 4 | 0.447780 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.397187 | `foundry_agents_connect` | ❌ |

---

## Test 43

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.760416 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.691931 | `search_service_list` | ❌ |
| 3 | 0.665923 | `search_knowledge_base_get` | ❌ |
| 4 | 0.573012 | `search_index_get` | ❌ |
| 5 | 0.560779 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 44

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737860 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.660170 | `search_service_list` | ❌ |
| 3 | 0.652969 | `search_knowledge_base_get` | ❌ |
| 4 | 0.578835 | `search_index_get` | ❌ |
| 5 | 0.560564 | `search_index_query` | ❌ |

---

## Test 45

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657935 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.558516 | `search_knowledge_base_get` | ❌ |
| 3 | 0.511469 | `search_service_list` | ❌ |
| 4 | 0.470560 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.433657 | `foundry_knowledge_index_list` | ❌ |

---

## Test 46

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.652945 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.563270 | `search_knowledge_base_get` | ❌ |
| 3 | 0.487022 | `search_service_list` | ❌ |
| 4 | 0.477636 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.430518 | `search_index_get` | ❌ |

---

## Test 47

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Get the details of knowledge source <source-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.825604 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.693437 | `search_knowledge_base_get` | ❌ |
| 3 | 0.595643 | `search_index_get` | ❌ |
| 4 | 0.540550 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.531247 | `search_service_list` | ❌ |

---

## Test 48

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge source <source-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630840 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.523643 | `search_knowledge_base_get` | ❌ |
| 3 | 0.459923 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.371465 | `search_index_get` | ❌ |
| 5 | 0.370838 | `search_service_list` | ❌ |

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
| 4 | 0.521765 | `search_knowledge_source_get` | ❌ |
| 5 | 0.490624 | `search_service_list` | ❌ |

---

## Test 50

**Expected Tool:** `search_index_get`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640256 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.620140 | `search_service_list` | ❌ |
| 3 | 0.538885 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.511485 | `search_knowledge_base_get` | ❌ |
| 5 | 0.496094 | `search_knowledge_source_get` | ❌ |

---

## Test 51

**Expected Tool:** `search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620759 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.562775 | `search_service_list` | ❌ |
| 3 | 0.538471 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.500365 | `search_knowledge_base_get` | ❌ |
| 5 | 0.490025 | `search_knowledge_source_get` | ❌ |

---

## Test 52

**Expected Tool:** `search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522826 | `search_index_get` | ❌ |
| 2 | 0.515870 | `search_index_query` | ✅ **EXPECTED** |
| 3 | 0.497467 | `search_service_list` | ❌ |
| 4 | 0.447977 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.437665 | `postgres_database_query` | ❌ |

---

## Test 53

**Expected Tool:** `search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793651 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.553011 | `kusto_cluster_list` | ❌ |
| 3 | 0.509461 | `subscription_list` | ❌ |
| 4 | 0.505971 | `search_index_get` | ❌ |
| 5 | 0.504693 | `marketplace_product_list` | ❌ |

---

## Test 54

**Expected Tool:** `search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686140 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.484092 | `marketplace_product_list` | ❌ |
| 3 | 0.479898 | `search_index_get` | ❌ |
| 4 | 0.462336 | `search_knowledge_base_get` | ❌ |
| 5 | 0.461786 | `kusto_cluster_list` | ❌ |

---

## Test 55

**Expected Tool:** `search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553025 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.436230 | `search_index_get` | ❌ |
| 3 | 0.415277 | `search_knowledge_base_get` | ❌ |
| 4 | 0.410461 | `search_knowledge_source_get` | ❌ |
| 5 | 0.404758 | `search_index_query` | ❌ |

---

## Test 56

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert this audio file to text using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682065 | `speech_tts_synthesize` | ❌ |
| 2 | 0.666038 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 3 | 0.377022 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.351127 | `deploy_plan_get` | ❌ |
| 5 | 0.338137 | `extension_cli_generate` | ❌ |

---

## Test 57

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Recognize speech from my audio file with language detection  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.511324 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.344404 | `speech_tts_synthesize` | ❌ |
| 3 | 0.197854 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.192450 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.170157 | `foundry_openai_create-completion` | ❌ |

---

## Test 58

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe speech from audio file <file_path> with profanity filtering  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.486489 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.335115 | `speech_tts_synthesize` | ❌ |
| 3 | 0.162870 | `foundry_threads_create` | ❌ |
| 4 | 0.160209 | `foundry_agents_connect` | ❌ |
| 5 | 0.156850 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 59

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text from audio file <file_path> using endpoint <endpoint>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611992 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.573185 | `speech_tts_synthesize` | ❌ |
| 3 | 0.309895 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.244218 | `foundry_resource_get` | ❌ |
| 5 | 0.243626 | `foundry_openai_create-completion` | ❌ |

---

## Test 60

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe the audio file <file_path> in Spanish language  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.410533 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.353783 | `speech_tts_synthesize` | ❌ |
| 3 | 0.152391 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.152137 | `foundry_models_deploy` | ❌ |
| 5 | 0.151632 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 61

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text with detailed output format from audio file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546259 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.480203 | `speech_tts_synthesize` | ❌ |
| 3 | 0.218092 | `foundry_resource_get` | ❌ |
| 4 | 0.202935 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.183420 | `extension_azqr` | ❌ |

---

## Test 62

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Recognize speech from <file_path> with phrase hints for better accuracy  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.539963 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.367401 | `speech_tts_synthesize` | ❌ |
| 3 | 0.228587 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.203413 | `foundry_agents_connect` | ❌ |
| 5 | 0.199585 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 63

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe audio using multiple phrase hints: "Azure", "cognitive services", "machine learning"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549151 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.468161 | `speech_tts_synthesize` | ❌ |
| 3 | 0.342537 | `extension_cli_generate` | ❌ |
| 4 | 0.338302 | `cloudarchitect_design` | ❌ |
| 5 | 0.335741 | `foundry_openai_create-completion` | ❌ |

---

## Test 64

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text with comma-separated phrase hints: "Azure, cognitive services, API"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532536 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.515532 | `speech_tts_synthesize` | ❌ |
| 3 | 0.349892 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.340893 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.332669 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 65

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe audio with raw profanity output from file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.453396 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.322710 | `speech_tts_synthesize` | ❌ |
| 3 | 0.173205 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.164990 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.160483 | `foundry_agents_connect` | ❌ |

---

## Test 66

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Convert text to speech and save to output.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521797 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.422457 | `speech_stt_recognize` | ❌ |
| 3 | 0.196049 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.189438 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.174955 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 67

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Synthesize speech from "Hello, welcome to Azure" and save to welcome.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516973 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.486019 | `speech_stt_recognize` | ❌ |
| 3 | 0.329765 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.323728 | `extension_cli_generate` | ❌ |
| 5 | 0.317525 | `azureterraformbestpractices_get` | ❌ |

---

## Test 68

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Generate speech audio from text "Hello world" using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592156 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.534002 | `speech_stt_recognize` | ❌ |
| 3 | 0.339580 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.327397 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.315764 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 69

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Convert text to speech with Spanish language and save to spanish-audio.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.501096 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.452648 | `speech_stt_recognize` | ❌ |
| 3 | 0.210841 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.196766 | `foundry_models_deploy` | ❌ |
| 5 | 0.191812 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 70

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Synthesize speech with voice en-US-JennyNeural from text "Azure AI Services"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604878 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.496715 | `speech_stt_recognize` | ❌ |
| 3 | 0.417045 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.379840 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.354130 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 71

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Create MP3 audio file from text "Welcome to Azure" with high quality format  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.561285 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.510908 | `speech_stt_recognize` | ❌ |
| 3 | 0.348757 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.347597 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.345073 | `deploy_iac_rules_get` | ❌ |

---

## Test 72

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Generate speech with custom voice model using endpoint ID <endpoint-id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527400 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.455811 | `speech_stt_recognize` | ❌ |
| 3 | 0.353132 | `foundry_resource_get` | ❌ |
| 4 | 0.343330 | `foundry_models_deploy` | ❌ |
| 5 | 0.337912 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 73

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Convert text to OGG/Opus format audio file  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.432836 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.410086 | `speech_stt_recognize` | ❌ |
| 3 | 0.234237 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.196153 | `extension_cli_generate` | ❌ |
| 5 | 0.175963 | `foundry_openai_create-completion` | ❌ |

---

## Test 74

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Synthesize long text content to audio file with streaming  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.428079 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.369045 | `speech_stt_recognize` | ❌ |
| 3 | 0.230725 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.220793 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.216475 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 75

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Create audio file from text in French language with appropriate voice  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.444444 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.385267 | `speech_stt_recognize` | ❌ |
| 3 | 0.229890 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.228317 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.213222 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 76

**Expected Tool:** `appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786360 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.530613 | `appconfig_kv_get` | ❌ |
| 3 | 0.491380 | `postgres_server_list` | ❌ |
| 4 | 0.481223 | `kusto_cluster_list` | ❌ |
| 5 | 0.479988 | `subscription_list` | ❌ |

---

## Test 77

**Expected Tool:** `appconfig_account_list`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634978 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.464865 | `appconfig_kv_get` | ❌ |
| 3 | 0.398495 | `subscription_list` | ❌ |
| 4 | 0.391291 | `redis_list` | ❌ |
| 5 | 0.372456 | `postgres_server_list` | ❌ |

---

## Test 78

**Expected Tool:** `appconfig_account_list`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565435 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.465344 | `appconfig_kv_get` | ❌ |
| 3 | 0.355916 | `postgres_server_config_get` | ❌ |
| 4 | 0.348661 | `appconfig_kv_delete` | ❌ |
| 5 | 0.327234 | `appconfig_kv_set` | ❌ |

---

## Test 79

**Expected Tool:** `appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618277 | `appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.464358 | `appconfig_kv_get` | ❌ |
| 3 | 0.424344 | `appconfig_kv_set` | ❌ |
| 4 | 0.422700 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.392016 | `appconfig_account_list` | ❌ |

---

## Test 80

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.632687 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.557810 | `appconfig_account_list` | ❌ |
| 3 | 0.530884 | `appconfig_kv_set` | ❌ |
| 4 | 0.464635 | `appconfig_kv_delete` | ❌ |
| 5 | 0.439089 | `appconfig_kv_lock_set` | ❌ |

---

## Test 81

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612555 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.522426 | `appconfig_account_list` | ❌ |
| 3 | 0.512945 | `appconfig_kv_set` | ❌ |
| 4 | 0.468503 | `appconfig_kv_delete` | ❌ |
| 5 | 0.457866 | `appconfig_kv_lock_set` | ❌ |

---

## Test 82

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** List all key-value settings with key name starting with 'prod-' in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.513021 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.450004 | `appconfig_account_list` | ❌ |
| 3 | 0.398800 | `appconfig_kv_set` | ❌ |
| 4 | 0.380746 | `appconfig_kv_delete` | ❌ |
| 5 | 0.346260 | `appconfig_kv_lock_set` | ❌ |

---

## Test 83

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552300 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.448912 | `appconfig_kv_set` | ❌ |
| 3 | 0.441713 | `appconfig_kv_delete` | ❌ |
| 4 | 0.437432 | `appconfig_account_list` | ❌ |
| 5 | 0.416264 | `appconfig_kv_lock_set` | ❌ |

---

## Test 84

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591237 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.487174 | `appconfig_kv_get` | ❌ |
| 3 | 0.445551 | `appconfig_kv_set` | ❌ |
| 4 | 0.431516 | `appconfig_kv_delete` | ❌ |
| 5 | 0.373656 | `appconfig_account_list` | ❌ |

---

## Test 85

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555699 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.505681 | `appconfig_kv_get` | ❌ |
| 3 | 0.476496 | `appconfig_kv_delete` | ❌ |
| 4 | 0.425488 | `appconfig_kv_set` | ❌ |
| 5 | 0.409406 | `appconfig_account_list` | ❌ |

---

## Test 86

**Expected Tool:** `appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609635 | `appconfig_kv_set` | ✅ **EXPECTED** |
| 2 | 0.536497 | `appconfig_kv_lock_set` | ❌ |
| 3 | 0.512707 | `appconfig_kv_get` | ❌ |
| 4 | 0.505571 | `appconfig_kv_delete` | ❌ |
| 5 | 0.377919 | `appconfig_account_list` | ❌ |

---

## Test 87

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595737 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.336142 | `deploy_app_logs_get` | ❌ |
| 3 | 0.300835 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.272689 | `cloudarchitect_design` | ❌ |
| 5 | 0.254527 | `monitor_resource_log_query` | ❌ |

---

## Test 88

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** Use app lens to check why my app is slow?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502361 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.316297 | `deploy_app_logs_get` | ❌ |
| 3 | 0.255570 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.249583 | `monitor_resource_log_query` | ❌ |
| 5 | 0.225972 | `quota_usage_check` | ❌ |

---

## Test 89

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492820 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.256325 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.242800 | `cloudarchitect_design` | ❌ |
| 4 | 0.225608 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.211564 | `deploy_app_logs_get` | ❌ |

---

## Test 90

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection <connection_string> to my app service <app_name> for database <database_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.718027 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.402135 | `sql_db_rename` | ❌ |
| 3 | 0.400714 | `sql_db_create` | ❌ |
| 4 | 0.363505 | `sql_db_show` | ❌ |
| 5 | 0.358956 | `sql_db_list` | ❌ |

---

## Test 91

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure SQL Server database <database_name> for app service <app_name> with connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688364 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.498175 | `sql_db_rename` | ❌ |
| 3 | 0.497522 | `sql_db_create` | ❌ |
| 4 | 0.469526 | `sql_db_show` | ❌ |
| 5 | 0.453088 | `sql_db_list` | ❌ |

---

## Test 92

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add MySQL database <database_name> to app service <app_name> using connection <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.675443 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.465187 | `sql_db_create` | ❌ |
| 3 | 0.452537 | `sql_db_rename` | ❌ |
| 4 | 0.432946 | `mysql_server_list` | ❌ |
| 5 | 0.410266 | `sql_db_show` | ❌ |

---

## Test 93

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add PostgreSQL database <database_name> to app service <app_name> using connection <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627784 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.444152 | `sql_db_create` | ❌ |
| 3 | 0.404874 | `postgres_database_query` | ❌ |
| 4 | 0.401137 | `postgres_database_list` | ❌ |
| 5 | 0.400754 | `sql_db_rename` | ❌ |

---

## Test 94

**Expected Tool:** `appservice_database_add`  
**Prompt:** Connect CosmosDB database <database_name> using connection string <connection_string> to app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663057 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.446328 | `cosmos_database_list` | ❌ |
| 3 | 0.441849 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.427159 | `cosmos_database_container_list` | ❌ |
| 5 | 0.420379 | `sql_db_rename` | ❌ |

---

## Test 95

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection <connection_string> for database <database_name> on server <database_server> to app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.733775 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.454554 | `sql_db_create` | ❌ |
| 3 | 0.415274 | `sql_db_rename` | ❌ |
| 4 | 0.414101 | `sql_server_create` | ❌ |
| 5 | 0.410260 | `sql_db_list` | ❌ |

---

## Test 96

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection string for <database_name> to app service <app_name> using connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.746518 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.441688 | `sql_db_rename` | ❌ |
| 3 | 0.433979 | `sql_db_create` | ❌ |
| 4 | 0.391370 | `sql_db_list` | ❌ |
| 5 | 0.390219 | `sql_db_show` | ❌ |

---

## Test 97

**Expected Tool:** `appservice_database_add`  
**Prompt:** Connect database <database_name> to my app service <app_name> using connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680411 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.429294 | `sql_db_rename` | ❌ |
| 3 | 0.406245 | `sql_db_create` | ❌ |
| 4 | 0.396491 | `sql_db_show` | ❌ |
| 5 | 0.391479 | `sql_db_list` | ❌ |

---

## Test 98

**Expected Tool:** `appservice_database_add`  
**Prompt:** Set up database <database_name> for app service <app_name> with connection string <connection_string> under resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640585 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.456892 | `sql_db_create` | ❌ |
| 3 | 0.402757 | `sql_db_rename` | ❌ |
| 4 | 0.402144 | `sql_db_show` | ❌ |
| 5 | 0.394206 | `sql_db_list` | ❌ |

---

## Test 99

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure database <database_name> for app service <app_name> with the connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688394 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.449133 | `sql_db_rename` | ❌ |
| 3 | 0.448418 | `sql_db_create` | ❌ |
| 4 | 0.414400 | `sql_db_show` | ❌ |
| 5 | 0.411818 | `sql_db_list` | ❌ |

---

## Test 100

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572473 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.445157 | `get_bestpractices_get` | ❌ |
| 3 | 0.390478 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.383948 | `applens_resource_diagnose` | ❌ |
| 5 | 0.375286 | `deploy_iac_rules_get` | ❌ |

---

## Test 101

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** Show me code optimization recommendations for all Application Insights resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696531 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.468384 | `get_bestpractices_get` | ❌ |
| 3 | 0.452231 | `applens_resource_diagnose` | ❌ |
| 4 | 0.435241 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.424622 | `search_service_list` | ❌ |

---

## Test 102

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626722 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.488002 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.479425 | `mysql_server_list` | ❌ |
| 4 | 0.477396 | `applens_resource_diagnose` | ❌ |
| 5 | 0.468847 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 103

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509502 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.419670 | `applens_resource_diagnose` | ❌ |
| 3 | 0.383767 | `get_bestpractices_get` | ❌ |
| 4 | 0.367278 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.343879 | `cloudarchitect_design` | ❌ |

---

## Test 104

**Expected Tool:** `extension_cli_generate`  
**Prompt:** Create a Storage account with name <storage_account_name> using Azure CLI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593242 | `storage_account_create` | ❌ |
| 2 | 0.564940 | `storage_blob_container_create` | ❌ |
| 3 | 0.493684 | `storage_account_get` | ❌ |
| 4 | 0.474399 | `storage_blob_container_get` | ❌ |
| 5 | 0.454194 | `managedlustre_fs_create` | ❌ |

---

## Test 105

**Expected Tool:** `extension_cli_generate`  
**Prompt:** List all virtual machines in my subscription using Azure CLI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593467 | `search_service_list` | ❌ |
| 2 | 0.575274 | `kusto_cluster_list` | ❌ |
| 3 | 0.549966 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.544412 | `monitor_workspace_list` | ❌ |
| 5 | 0.536252 | `subscription_list` | ❌ |

---

## Test 106

**Expected Tool:** `extension_cli_generate`  
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

## Test 107

**Expected Tool:** `extension_cli_install`  
**Prompt:** <Ask the MCP host to uninstall az cli on your machine and run test prompts for extension_cli_generate>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.479590 | `extension_cli_install` | ✅ **EXPECTED** |
| 2 | 0.473250 | `extension_cli_generate` | ❌ |
| 3 | 0.389354 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.382389 | `deploy_plan_get` | ❌ |
| 5 | 0.366012 | `get_bestpractices_get` | ❌ |

---

## Test 108

**Expected Tool:** `extension_cli_install`  
**Prompt:** How to install azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.460416 | `extension_cli_install` | ✅ **EXPECTED** |
| 2 | 0.429599 | `deploy_app_logs_get` | ❌ |
| 3 | 0.365212 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.335279 | `deploy_plan_get` | ❌ |
| 5 | 0.326135 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 109

**Expected Tool:** `extension_cli_install`  
**Prompt:** What is Azure Functions Core tools and how to install it  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622705 | `extension_cli_install` | ✅ **EXPECTED** |
| 2 | 0.439474 | `get_bestpractices_get` | ❌ |
| 3 | 0.432913 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.430723 | `extension_cli_generate` | ❌ |
| 5 | 0.418161 | `deploy_plan_get` | ❌ |

---

## Test 110

**Expected Tool:** `acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743568 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.711580 | `acr_registry_repository_list` | ❌ |
| 3 | 0.585675 | `kusto_cluster_list` | ❌ |
| 4 | 0.541506 | `search_service_list` | ❌ |
| 5 | 0.514293 | `cosmos_account_list` | ❌ |

---

## Test 111

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586014 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563636 | `acr_registry_repository_list` | ❌ |
| 3 | 0.460544 | `storage_blob_container_get` | ❌ |
| 4 | 0.415552 | `cosmos_database_container_list` | ❌ |
| 5 | 0.402247 | `redis_list` | ❌ |

---

## Test 112

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.637130 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563476 | `acr_registry_repository_list` | ❌ |
| 3 | 0.516769 | `kusto_cluster_list` | ❌ |
| 4 | 0.515378 | `storage_blob_container_get` | ❌ |
| 5 | 0.480352 | `redis_list` | ❌ |

---

## Test 113

**Expected Tool:** `acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654318 | `acr_registry_repository_list` | ❌ |
| 2 | 0.633938 | `acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.476294 | `mysql_server_list` | ❌ |
| 4 | 0.454887 | `group_list` | ❌ |
| 5 | 0.454003 | `datadog_monitoredresources_list` | ❌ |

---

## Test 114

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639391 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.637972 | `acr_registry_repository_list` | ❌ |
| 3 | 0.468371 | `mysql_server_list` | ❌ |
| 4 | 0.449649 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.445729 | `group_list` | ❌ |

---

## Test 115

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626482 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.617504 | `acr_registry_list` | ❌ |
| 3 | 0.544172 | `kusto_cluster_list` | ❌ |
| 4 | 0.508483 | `storage_blob_container_get` | ❌ |
| 5 | 0.495567 | `postgres_server_list` | ❌ |

---

## Test 116

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546333 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.469295 | `acr_registry_list` | ❌ |
| 3 | 0.451083 | `storage_blob_container_get` | ❌ |
| 4 | 0.407973 | `cosmos_database_container_list` | ❌ |
| 5 | 0.373464 | `storage_blob_get` | ❌ |

---

## Test 117

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674296 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.541779 | `acr_registry_list` | ❌ |
| 3 | 0.437509 | `storage_blob_container_get` | ❌ |
| 4 | 0.433927 | `cosmos_database_container_list` | ❌ |
| 5 | 0.383387 | `kusto_database_list` | ❌ |

---

## Test 118

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600780 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.501842 | `acr_registry_list` | ❌ |
| 3 | 0.430880 | `storage_blob_container_get` | ❌ |
| 4 | 0.418623 | `cosmos_database_container_list` | ❌ |
| 5 | 0.378151 | `redis_list` | ❌ |

---

## Test 119

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498292 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.229081 | `communication_sms_send` | ❌ |
| 3 | 0.188975 | `eventgrid_events_publish` | ❌ |
| 4 | 0.161150 | `foundry_agents_create` | ❌ |
| 5 | 0.145951 | `servicebus_topic_details` | ❌ |

---

## Test 120

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email from my communication service to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498406 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.314462 | `communication_sms_send` | ❌ |
| 3 | 0.235127 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.215392 | `speech_tts_synthesize` | ❌ |
| 5 | 0.211154 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 121

**Expected Tool:** `communication_email_send`  
**Prompt:** Send HTML-formatted email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520967 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.207658 | `communication_sms_send` | ❌ |
| 3 | 0.152418 | `eventgrid_events_publish` | ❌ |
| 4 | 0.152013 | `servicebus_topic_details` | ❌ |
| 5 | 0.143660 | `foundry_agents_evaluate` | ❌ |

---

## Test 122

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with CC to <email-address-1> and <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533447 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.219584 | `communication_sms_send` | ❌ |
| 3 | 0.106026 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.103723 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.084905 | `cosmos_account_list` | ❌ |

---

## Test 123

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email to multiple recipients: <email-address-1>, <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.540792 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.244521 | `communication_sms_send` | ❌ |
| 3 | 0.134975 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.114324 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.087063 | `postgres_server_param_set` | ❌ |

---

## Test 124

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with reply-to address set to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512623 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.200177 | `communication_sms_send` | ❌ |
| 3 | 0.164115 | `mysql_server_param_set` | ❌ |
| 4 | 0.158759 | `postgres_server_param_set` | ❌ |
| 5 | 0.143574 | `appconfig_kv_set` | ❌ |

---

## Test 125

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with custom sender name <sender-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.473175 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.255169 | `communication_sms_send` | ❌ |
| 3 | 0.164811 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.160393 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.156869 | `cosmos_database_container_item_query` | ❌ |

---

## Test 126

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email with BCC recipients  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.528759 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.241047 | `communication_sms_send` | ❌ |
| 3 | 0.137545 | `confidentialledger_entries_append` | ❌ |
| 4 | 0.108728 | `confidentialledger_entries_get` | ❌ |
| 5 | 0.105048 | `storage_blob_upload` | ❌ |

---

## Test 127

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send an SMS message to <phone-number> saying "Hello"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533868 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.251429 | `communication_email_send` | ❌ |
| 3 | 0.218656 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.175341 | `foundry_agents_create` | ❌ |
| 5 | 0.166041 | `speech_tts_synthesize` | ❌ |

---

## Test 128

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS to <phone-number-2> from <phone-number-1> with message "Test message"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545976 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.294793 | `communication_email_send` | ❌ |
| 3 | 0.204688 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.200676 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.141118 | `foundry_agents_create` | ❌ |

---

## Test 129

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS to multiple recipients: <phone-number-1>, <phone-number-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545755 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.421988 | `communication_email_send` | ❌ |
| 3 | 0.186088 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.142030 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.113656 | `foundry_threads_get-messages` | ❌ |

---

## Test 130

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS with delivery reporting enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.554908 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.269080 | `communication_email_send` | ❌ |
| 3 | 0.191848 | `extension_azqr` | ❌ |
| 4 | 0.185916 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.170726 | `foundry_agents_query-and-evaluate` | ❌ |

---

## Test 131

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS message with custom tracking tag "campaign1"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.538827 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.269794 | `communication_email_send` | ❌ |
| 3 | 0.188153 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.185403 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.174747 | `foundry_agents_create` | ❌ |

---

## Test 132

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send broadcast SMS to <phone-number-1> and <phone-number-2> saying "Urgent notification"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.474786 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.286338 | `communication_email_send` | ❌ |
| 3 | 0.164289 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.147338 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.128704 | `cosmos_account_list` | ❌ |

---

## Test 133

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS from my communication service to <phone-number-1>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564114 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.302363 | `communication_email_send` | ❌ |
| 3 | 0.238296 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.184482 | `foundry_agents_create` | ❌ |
| 5 | 0.183651 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 134

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send an SMS with delivery receipt tracking  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.598211 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.314134 | `communication_email_send` | ❌ |
| 3 | 0.206916 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.201142 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.187855 | `confidentialledger_entries_append` | ❌ |

---

## Test 135

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Append an entry to my ledger <ledger_name> with data {"key": "value"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510554 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.294885 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.292014 | `appconfig_kv_set` | ❌ |
| 4 | 0.258967 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.249908 | `keyvault_certificate_import` | ❌ |

---

## Test 136

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Write a tamper-proof entry to ledger <ledger_name> containing {"transaction": "data"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602324 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.357780 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.211969 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.195485 | `keyvault_secret_create` | ❌ |
| 5 | 0.184066 | `keyvault_certificate_import` | ❌ |

---

## Test 137

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Append {"hello": "from mcp"} to my confidential ledger <ledger_name> in collection <collection_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546394 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.451837 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.225163 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.216036 | `appconfig_kv_set` | ❌ |
| 5 | 0.203220 | `keyvault_certificate_import` | ❌ |

---

## Test 138

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Create an immutable ledger entry in <ledger_name> with content {"audit": "log"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496008 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.340187 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.218473 | `monitor_activitylog_list` | ❌ |
| 4 | 0.215229 | `storage_blob_container_create` | ❌ |
| 5 | 0.204925 | `monitor_resource_log_query` | ❌ |

---

## Test 139

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Write an entry to confidential ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622014 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.524777 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.252508 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.240315 | `keyvault_secret_create` | ❌ |
| 5 | 0.186890 | `appconfig_kv_set` | ❌ |

---

## Test 140

**Expected Tool:** `confidentialledger_entries_get`  
**Prompt:** Get entry from Confidential Ledger for transaction <transaction_id> on ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.707252 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.551952 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.245541 | `keyvault_secret_get` | ❌ |
| 4 | 0.229943 | `keyvault_key_get` | ❌ |
| 5 | 0.211839 | `loadtesting_testrun_get` | ❌ |

---

## Test 141

**Expected Tool:** `confidentialledger_entries_get`  
**Prompt:** Get transaction <transaction_id> from ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509714 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.416730 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.223959 | `loadtesting_testrun_get` | ❌ |
| 4 | 0.218412 | `monitor_resource_log_query` | ❌ |
| 5 | 0.217671 | `loadtesting_testrun_list` | ❌ |

---

## Test 142

**Expected Tool:** `cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818357 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.668480 | `cosmos_database_list` | ❌ |
| 3 | 0.636036 | `subscription_list` | ❌ |
| 4 | 0.615268 | `cosmos_database_container_list` | ❌ |
| 5 | 0.601467 | `kusto_cluster_list` | ❌ |

---

## Test 143

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665477 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605386 | `cosmos_database_list` | ❌ |
| 3 | 0.571628 | `cosmos_database_container_list` | ❌ |
| 4 | 0.549485 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.503849 | `storage_account_get` | ❌ |

---

## Test 144

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752413 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.606937 | `subscription_list` | ❌ |
| 3 | 0.605196 | `cosmos_database_list` | ❌ |
| 4 | 0.566379 | `cosmos_database_container_list` | ❌ |
| 5 | 0.564082 | `cosmos_database_container_item_query` | ❌ |

---

## Test 145

**Expected Tool:** `cosmos_database_container_item_query`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658701 | `cosmos_database_container_item_query` | ✅ **EXPECTED** |
| 2 | 0.605253 | `cosmos_database_container_list` | ❌ |
| 3 | 0.487612 | `storage_blob_container_get` | ❌ |
| 4 | 0.477874 | `cosmos_database_list` | ❌ |
| 5 | 0.447757 | `cosmos_account_list` | ❌ |

---

## Test 146

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852826 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.681006 | `cosmos_database_list` | ❌ |
| 3 | 0.680795 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.632368 | `storage_blob_container_get` | ❌ |
| 5 | 0.630666 | `cosmos_account_list` | ❌ |

---

## Test 147

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789395 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.648126 | `cosmos_database_container_item_query` | ❌ |
| 3 | 0.614220 | `cosmos_database_list` | ❌ |
| 4 | 0.591361 | `storage_blob_container_get` | ❌ |
| 5 | 0.562062 | `cosmos_account_list` | ❌ |

---

## Test 148

**Expected Tool:** `cosmos_database_list`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815683 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.668515 | `cosmos_account_list` | ❌ |
| 3 | 0.665298 | `cosmos_database_container_list` | ❌ |
| 4 | 0.606433 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.583402 | `kusto_database_list` | ❌ |

---

## Test 149

**Expected Tool:** `cosmos_database_list`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.749370 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.624759 | `cosmos_database_container_list` | ❌ |
| 3 | 0.614572 | `cosmos_account_list` | ❌ |
| 4 | 0.579919 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.538045 | `mysql_database_list` | ❌ |

---

## Test 150

**Expected Tool:** `kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590264 | `kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.463832 | `kusto_cluster_list` | ❌ |
| 3 | 0.428159 | `kusto_query` | ❌ |
| 4 | 0.425508 | `kusto_database_list` | ❌ |
| 5 | 0.422577 | `kusto_table_schema` | ❌ |

---

## Test 151

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793744 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.630260 | `kusto_database_list` | ❌ |
| 3 | 0.573395 | `kusto_cluster_get` | ❌ |
| 4 | 0.525025 | `aks_cluster_get` | ❌ |
| 5 | 0.509396 | `grafana_list` | ❌ |

---

## Test 152

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531307 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.465277 | `kusto_cluster_get` | ❌ |
| 3 | 0.432028 | `kusto_database_list` | ❌ |
| 4 | 0.369596 | `aks_cluster_get` | ❌ |
| 5 | 0.363119 | `kusto_table_schema` | ❌ |

---

## Test 153

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701484 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.571191 | `kusto_cluster_get` | ❌ |
| 3 | 0.548216 | `kusto_database_list` | ❌ |
| 4 | 0.498909 | `aks_cluster_get` | ❌ |
| 5 | 0.474201 | `redis_list` | ❌ |

---

## Test 154

**Expected Tool:** `kusto_database_list`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.677160 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.560715 | `kusto_cluster_list` | ❌ |
| 3 | 0.556662 | `kusto_table_list` | ❌ |
| 4 | 0.553239 | `postgres_database_list` | ❌ |
| 5 | 0.549605 | `cosmos_database_list` | ❌ |

---

## Test 155

**Expected Tool:** `kusto_database_list`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623592 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.509953 | `kusto_cluster_list` | ❌ |
| 3 | 0.507073 | `kusto_table_list` | ❌ |
| 4 | 0.497144 | `cosmos_database_list` | ❌ |
| 5 | 0.491166 | `mysql_database_list` | ❌ |

---

## Test 156

**Expected Tool:** `kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.423660 | `kusto_query` | ✅ **EXPECTED** |
| 2 | 0.409526 | `postgres_database_query` | ❌ |
| 3 | 0.408178 | `kusto_table_schema` | ❌ |
| 4 | 0.407741 | `kusto_sample` | ❌ |
| 5 | 0.403990 | `kusto_cluster_list` | ❌ |

---

## Test 157

**Expected Tool:** `kusto_sample`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595554 | `kusto_sample` | ✅ **EXPECTED** |
| 2 | 0.510233 | `kusto_table_schema` | ❌ |
| 3 | 0.424212 | `kusto_table_list` | ❌ |
| 4 | 0.400924 | `kusto_cluster_list` | ❌ |
| 5 | 0.399525 | `kusto_cluster_get` | ❌ |

---

## Test 158

**Expected Tool:** `kusto_table_list`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.679642 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.585237 | `postgres_table_list` | ❌ |
| 3 | 0.580885 | `kusto_database_list` | ❌ |
| 4 | 0.556724 | `mysql_table_list` | ❌ |
| 5 | 0.549940 | `monitor_table_list` | ❌ |

---

## Test 159

**Expected Tool:** `kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619252 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.554333 | `kusto_table_schema` | ❌ |
| 3 | 0.527314 | `kusto_database_list` | ❌ |
| 4 | 0.524691 | `mysql_table_list` | ❌ |
| 5 | 0.523432 | `postgres_table_list` | ❌ |

---

## Test 160

**Expected Tool:** `kusto_table_schema`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.666757 | `kusto_table_schema` | ✅ **EXPECTED** |
| 2 | 0.564118 | `postgres_table_schema_get` | ❌ |
| 3 | 0.527778 | `mysql_table_schema_get` | ❌ |
| 4 | 0.490798 | `kusto_sample` | ❌ |
| 5 | 0.489446 | `kusto_table_list` | ❌ |

---

## Test 161

**Expected Tool:** `mysql_database_list`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634056 | `postgres_database_list` | ❌ |
| 2 | 0.623043 | `mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.534457 | `mysql_table_list` | ❌ |
| 4 | 0.499107 | `mysql_server_list` | ❌ |
| 5 | 0.490148 | `sql_db_list` | ❌ |

---

## Test 162

**Expected Tool:** `mysql_database_list`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587863 | `mysql_database_list` | ✅ **EXPECTED** |
| 2 | 0.574089 | `postgres_database_list` | ❌ |
| 3 | 0.483855 | `mysql_table_list` | ❌ |
| 4 | 0.463405 | `mysql_server_list` | ❌ |
| 5 | 0.444547 | `sql_db_list` | ❌ |

---

## Test 163

**Expected Tool:** `mysql_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476256 | `mysql_table_list` | ❌ |
| 2 | 0.456053 | `mysql_database_list` | ❌ |
| 3 | 0.433203 | `mysql_database_query` | ✅ **EXPECTED** |
| 4 | 0.419831 | `mysql_server_list` | ❌ |
| 5 | 0.409321 | `mysql_table_schema_get` | ❌ |

---

## Test 164

**Expected Tool:** `mysql_server_config_get`  
**Prompt:** Show me the configuration of MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531887 | `postgres_server_config_get` | ❌ |
| 2 | 0.516894 | `mysql_server_param_set` | ❌ |
| 3 | 0.489816 | `mysql_server_config_get` | ✅ **EXPECTED** |
| 4 | 0.476863 | `mysql_server_param_get` | ❌ |
| 5 | 0.426507 | `mysql_table_schema_get` | ❌ |

---

## Test 165

**Expected Tool:** `mysql_server_list`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678472 | `postgres_server_list` | ❌ |
| 2 | 0.558115 | `mysql_database_list` | ❌ |
| 3 | 0.554998 | `mysql_server_list` | ✅ **EXPECTED** |
| 4 | 0.513706 | `kusto_cluster_list` | ❌ |
| 5 | 0.501199 | `mysql_table_list` | ❌ |

---

## Test 166

**Expected Tool:** `mysql_server_list`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478503 | `mysql_database_list` | ❌ |
| 2 | 0.474891 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.435642 | `postgres_server_list` | ❌ |
| 4 | 0.412380 | `mysql_table_list` | ❌ |
| 5 | 0.389993 | `postgres_database_list` | ❌ |

---

## Test 167

**Expected Tool:** `mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636435 | `postgres_server_list` | ❌ |
| 2 | 0.534464 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.530312 | `mysql_database_list` | ❌ |
| 4 | 0.475052 | `kusto_cluster_list` | ❌ |
| 5 | 0.470468 | `redis_list` | ❌ |

---

## Test 168

**Expected Tool:** `mysql_server_param_get`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.495071 | `mysql_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.438075 | `mysql_server_param_set` | ❌ |
| 3 | 0.333841 | `mysql_database_query` | ❌ |
| 4 | 0.313150 | `mysql_table_schema_get` | ❌ |
| 5 | 0.310834 | `postgres_server_param_get` | ❌ |

---

## Test 169

**Expected Tool:** `mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.449419 | `mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.381144 | `mysql_server_param_get` | ❌ |
| 3 | 0.303499 | `postgres_server_param_set` | ❌ |
| 4 | 0.298911 | `mysql_database_query` | ❌ |
| 5 | 0.254159 | `mysql_server_list` | ❌ |

---

## Test 170

**Expected Tool:** `mysql_table_list`  
**Prompt:** List all tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633448 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.573844 | `postgres_table_list` | ❌ |
| 3 | 0.550898 | `postgres_database_list` | ❌ |
| 4 | 0.546779 | `mysql_database_list` | ❌ |
| 5 | 0.511847 | `kusto_table_list` | ❌ |

---

## Test 171

**Expected Tool:** `mysql_table_list`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609131 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.526236 | `postgres_table_list` | ❌ |
| 3 | 0.525627 | `mysql_database_list` | ❌ |
| 4 | 0.507258 | `mysql_table_schema_get` | ❌ |
| 5 | 0.498050 | `postgres_database_list` | ❌ |

---

## Test 172

**Expected Tool:** `mysql_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630623 | `mysql_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.558306 | `postgres_table_schema_get` | ❌ |
| 3 | 0.545025 | `mysql_table_list` | ❌ |
| 4 | 0.517419 | `kusto_table_schema` | ❌ |
| 5 | 0.457648 | `mysql_database_list` | ❌ |

---

## Test 173

**Expected Tool:** `postgres_database_list`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815617 | `postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.644014 | `postgres_table_list` | ❌ |
| 3 | 0.622790 | `postgres_server_list` | ❌ |
| 4 | 0.542685 | `postgres_server_config_get` | ❌ |
| 5 | 0.490904 | `postgres_server_param_get` | ❌ |

---

## Test 174

**Expected Tool:** `postgres_database_list`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.760033 | `postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.589783 | `postgres_server_list` | ❌ |
| 3 | 0.585891 | `postgres_table_list` | ❌ |
| 4 | 0.552660 | `postgres_server_config_get` | ❌ |
| 5 | 0.495629 | `postgres_server_param_get` | ❌ |

---

## Test 175

**Expected Tool:** `postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546211 | `postgres_database_list` | ❌ |
| 2 | 0.523127 | `postgres_database_query` | ✅ **EXPECTED** |
| 3 | 0.503267 | `postgres_table_list` | ❌ |
| 4 | 0.466599 | `postgres_server_list` | ❌ |
| 5 | 0.403969 | `postgres_server_param_get` | ❌ |

---

## Test 176

**Expected Tool:** `postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756593 | `postgres_server_config_get` | ✅ **EXPECTED** |
| 2 | 0.615429 | `postgres_server_param_set` | ❌ |
| 3 | 0.599471 | `postgres_server_param_get` | ❌ |
| 4 | 0.535049 | `postgres_database_list` | ❌ |
| 5 | 0.518574 | `postgres_server_list` | ❌ |

---

## Test 177

**Expected Tool:** `postgres_server_list`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.900023 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.640733 | `postgres_database_list` | ❌ |
| 3 | 0.565914 | `postgres_table_list` | ❌ |
| 4 | 0.538997 | `postgres_server_config_get` | ❌ |
| 5 | 0.534239 | `kusto_cluster_list` | ❌ |

---

## Test 178

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

---

## Test 179

**Expected Tool:** `postgres_server_list`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.832155 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.579232 | `postgres_database_list` | ❌ |
| 3 | 0.531804 | `postgres_server_config_get` | ❌ |
| 4 | 0.514445 | `postgres_table_list` | ❌ |
| 5 | 0.505869 | `postgres_server_param_get` | ❌ |

---

## Test 180

**Expected Tool:** `postgres_server_param_get`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594753 | `postgres_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.552678 | `postgres_server_param_set` | ❌ |
| 3 | 0.539671 | `postgres_server_config_get` | ❌ |
| 4 | 0.489693 | `postgres_server_list` | ❌ |
| 5 | 0.451871 | `postgres_database_list` | ❌ |

---

## Test 181

**Expected Tool:** `postgres_server_param_set`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579873 | `postgres_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.488474 | `postgres_server_config_get` | ❌ |
| 3 | 0.469794 | `postgres_server_list` | ❌ |
| 4 | 0.447011 | `postgres_server_param_get` | ❌ |
| 5 | 0.440760 | `postgres_database_list` | ❌ |

---

## Test 182

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

---

## Test 183

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

## Test 184

**Expected Tool:** `postgres_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.714596 | `postgres_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.597838 | `postgres_table_list` | ❌ |
| 3 | 0.574339 | `postgres_database_list` | ❌ |
| 4 | 0.507791 | `postgres_server_config_get` | ❌ |
| 5 | 0.502705 | `kusto_table_schema` | ❌ |

---

## Test 185

**Expected Tool:** `deploy_app_logs_get`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711770 | `deploy_app_logs_get` | ✅ **EXPECTED** |
| 2 | 0.471692 | `deploy_plan_get` | ❌ |
| 3 | 0.451638 | `monitor_activitylog_list` | ❌ |
| 4 | 0.404890 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.401388 | `monitor_resource_log_query` | ❌ |

---

## Test 186

**Expected Tool:** `deploy_architecture_diagram_generate`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680640 | `deploy_architecture_diagram_generate` | ✅ **EXPECTED** |
| 2 | 0.562521 | `deploy_plan_get` | ❌ |
| 3 | 0.497193 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.490040 | `cloudarchitect_design` | ❌ |
| 5 | 0.435921 | `deploy_iac_rules_get` | ❌ |

---

## Test 187

**Expected Tool:** `deploy_iac_rules_get`  
**Prompt:** Show me the rules to generate bicep scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529092 | `deploy_iac_rules_get` | ✅ **EXPECTED** |
| 2 | 0.479903 | `bicepschema_get` | ❌ |
| 3 | 0.391965 | `get_bestpractices_get` | ❌ |
| 4 | 0.383210 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.375561 | `extension_cli_generate` | ❌ |

---

## Test 188

**Expected Tool:** `deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638841 | `deploy_pipeline_guidance_get` | ✅ **EXPECTED** |
| 2 | 0.499242 | `deploy_plan_get` | ❌ |
| 3 | 0.448918 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.385920 | `deploy_app_logs_get` | ❌ |
| 5 | 0.382240 | `get_bestpractices_get` | ❌ |

---

## Test 189

**Expected Tool:** `deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688055 | `deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.587903 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.499385 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.498575 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.448692 | `loadtesting_test_create` | ❌ |

---

## Test 190

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Publish an event to Event Grid topic <topic_name> using <event_schema> with the following data <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755140 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.482731 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.465891 | `eventgrid_topic_list` | ❌ |
| 4 | 0.360374 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.355481 | `servicebus_topic_details` | ❌ |

---

## Test 191

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Publish event to my Event Grid topic <topic_name> with the following events <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654647 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.524503 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.510039 | `eventgrid_topic_list` | ❌ |
| 4 | 0.373718 | `servicebus_topic_details` | ❌ |
| 5 | 0.359908 | `eventhubs_eventhub_update` | ❌ |

---

## Test 192

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Send an event to Event Grid topic <topic_name> in resource group <resource_group_name> with <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600271 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.521247 | `eventgrid_topic_list` | ❌ |
| 3 | 0.504794 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.411140 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.389466 | `eventhubs_eventhub_consumergroup_get` | ❌ |

---

## Test 193

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.770140 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.745470 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.561862 | `kusto_cluster_list` | ❌ |
| 4 | 0.545540 | `search_service_list` | ❌ |
| 5 | 0.526138 | `subscription_list` | ❌ |

---

## Test 194

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738258 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.737486 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.492592 | `kusto_cluster_list` | ❌ |
| 4 | 0.480287 | `subscription_list` | ❌ |
| 5 | 0.475119 | `search_service_list` | ❌ |

---

## Test 195

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.770140 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.721362 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.535326 | `kusto_cluster_list` | ❌ |
| 4 | 0.514248 | `search_service_list` | ❌ |
| 5 | 0.495987 | `subscription_list` | ❌ |

---

## Test 196

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.758595 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.704232 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.609085 | `group_list` | ❌ |
| 4 | 0.544465 | `monitor_webtests_list` | ❌ |
| 5 | 0.524019 | `eventhubs_namespace_get` | ❌ |

---

## Test 197

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769097 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.720606 | `eventgrid_topic_list` | ❌ |
| 3 | 0.498615 | `servicebus_topic_details` | ❌ |
| 4 | 0.486216 | `servicebus_topic_subscription_details` | ❌ |
| 5 | 0.486162 | `eventgrid_events_publish` | ❌ |

---

## Test 198

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.718109 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.709805 | `eventgrid_topic_list` | ❌ |
| 3 | 0.539977 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.529286 | `servicebus_topic_details` | ❌ |
| 5 | 0.477876 | `eventgrid_events_publish` | ❌ |

---

## Test 199

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.746815 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.746174 | `eventgrid_topic_list` | ❌ |
| 3 | 0.535484 | `monitor_webtests_list` | ❌ |
| 4 | 0.524923 | `group_list` | ❌ |
| 5 | 0.503158 | `servicebus_topic_details` | ❌ |

---

## Test 200

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736436 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.659727 | `eventgrid_topic_list` | ❌ |
| 3 | 0.569254 | `subscription_list` | ❌ |
| 4 | 0.537922 | `kusto_cluster_list` | ❌ |
| 5 | 0.518857 | `search_service_list` | ❌ |

---

## Test 201

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684522 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.656271 | `eventgrid_topic_list` | ❌ |
| 3 | 0.542366 | `subscription_list` | ❌ |
| 4 | 0.521031 | `kusto_cluster_list` | ❌ |
| 5 | 0.510078 | `group_list` | ❌ |

---

## Test 202

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696101 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.691739 | `eventgrid_topic_list` | ❌ |
| 3 | 0.557598 | `group_list` | ❌ |
| 4 | 0.510586 | `monitor_webtests_list` | ❌ |
| 5 | 0.504984 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 203

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.709801 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.642095 | `eventgrid_topic_list` | ❌ |
| 3 | 0.506697 | `subscription_list` | ❌ |
| 4 | 0.476763 | `search_service_list` | ❌ |
| 5 | 0.475782 | `kusto_cluster_list` | ❌ |

---

## Test 204

**Expected Tool:** `eventhubs_eventhub_consumergroup_delete`  
**Prompt:** Delete my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.766923 | `eventhubs_eventhub_consumergroup_delete` | ✅ **EXPECTED** |
| 2 | 0.675846 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.641111 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.633787 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.605477 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 205

**Expected Tool:** `eventhubs_eventhub_consumergroup_get`  
**Prompt:** List all consumer groups in my event hub <event_hub_name> in namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738475 | `eventhubs_eventhub_consumergroup_get` | ✅ **EXPECTED** |
| 2 | 0.634517 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.626485 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.606619 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.593098 | `eventhubs_eventhub_get` | ❌ |

---

## Test 206

**Expected Tool:** `eventhubs_eventhub_consumergroup_get`  
**Prompt:** Get the details of my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712861 | `eventhubs_eventhub_consumergroup_get` | ✅ **EXPECTED** |
| 2 | 0.637170 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.625913 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.576800 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.529940 | `eventhubs_eventhub_get` | ❌ |

---

## Test 207

**Expected Tool:** `eventhubs_eventhub_consumergroup_update`  
**Prompt:** Create a new consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.757614 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
| 2 | 0.688923 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 3 | 0.670026 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.554314 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.545003 | `eventhubs_namespace_get` | ❌ |

---

## Test 208

**Expected Tool:** `eventhubs_eventhub_consumergroup_update`  
**Prompt:** Update my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738818 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
| 2 | 0.655614 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 3 | 0.642219 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.552234 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.524019 | `eventhubs_namespace_delete` | ❌ |

---

## Test 209

**Expected Tool:** `eventhubs_eventhub_delete`  
**Prompt:** Delete my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.699271 | `eventhubs_namespace_delete` | ❌ |
| 2 | 0.688649 | `eventhubs_eventhub_delete` | ✅ **EXPECTED** |
| 3 | 0.627530 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.578627 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.553129 | `eventhubs_eventhub_get` | ❌ |

---

## Test 210

**Expected Tool:** `eventhubs_eventhub_get`  
**Prompt:** List all Event Hubs in my namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.773242 | `eventhubs_eventhub_get` | ✅ **EXPECTED** |
| 2 | 0.687582 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.578689 | `eventhubs_eventhub_update` | ❌ |
| 4 | 0.561545 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.545475 | `eventhubs_eventhub_consumergroup_get` | ❌ |

---

## Test 211

**Expected Tool:** `eventhubs_eventhub_get`  
**Prompt:** Get the details of my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638083 | `eventhubs_namespace_get` | ❌ |
| 2 | 0.627638 | `eventhubs_eventhub_get` | ✅ **EXPECTED** |
| 3 | 0.570904 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.527646 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.521920 | `eventhubs_namespace_delete` | ❌ |

---

## Test 212

**Expected Tool:** `eventhubs_eventhub_update`  
**Prompt:** Create a new event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646034 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.605911 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.574464 | `eventhubs_eventhub_get` | ❌ |
| 4 | 0.571638 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.557592 | `eventhubs_namespace_delete` | ❌ |

---

## Test 213

**Expected Tool:** `eventhubs_eventhub_update`  
**Prompt:** Update my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655283 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.571661 | `eventhubs_eventhub_delete` | ❌ |
| 3 | 0.568606 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 4 | 0.568396 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.565977 | `eventhubs_namespace_delete` | ❌ |

---

## Test 214

**Expected Tool:** `eventhubs_namespace_delete`  
**Prompt:** Delete my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623995 | `eventhubs_namespace_delete` | ✅ **EXPECTED** |
| 2 | 0.525823 | `eventhubs_namespace_update` | ❌ |
| 3 | 0.505082 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.449841 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.435037 | `workbooks_delete` | ❌ |

---

## Test 215

**Expected Tool:** `eventhubs_namespace_get`  
**Prompt:** List all Event Hubs namespaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659838 | `eventhubs_eventhub_get` | ❌ |
| 2 | 0.658827 | `eventhubs_namespace_get` | ✅ **EXPECTED** |
| 3 | 0.607372 | `kusto_cluster_list` | ❌ |
| 4 | 0.557200 | `eventgrid_topic_list` | ❌ |
| 5 | 0.556126 | `eventgrid_subscription_list` | ❌ |

---

## Test 216

**Expected Tool:** `eventhubs_namespace_get`  
**Prompt:** Get the details of my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509749 | `eventhubs_namespace_get` | ✅ **EXPECTED** |
| 2 | 0.509431 | `monitor_webtests_get` | ❌ |
| 3 | 0.497399 | `servicebus_queue_details` | ❌ |
| 4 | 0.489992 | `eventhubs_namespace_update` | ❌ |
| 5 | 0.470455 | `functionapp_get` | ❌ |

---

## Test 217

**Expected Tool:** `eventhubs_namespace_update`  
**Prompt:** Create an new namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610205 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.466721 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.458458 | `eventhubs_namespace_delete` | ❌ |
| 4 | 0.449724 | `workbooks_create` | ❌ |
| 5 | 0.438492 | `eventhubs_eventhub_consumergroup_update` | ❌ |

---

## Test 218

**Expected Tool:** `eventhubs_namespace_update`  
**Prompt:** Update my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622140 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.474099 | `eventhubs_namespace_delete` | ❌ |
| 3 | 0.448723 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.436549 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.372632 | `sql_db_rename` | ❌ |

---

## Test 219

**Expected Tool:** `functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660116 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.451613 | `deploy_app_logs_get` | ❌ |
| 3 | 0.450457 | `applens_resource_diagnose` | ❌ |
| 4 | 0.390229 | `mysql_server_list` | ❌ |
| 5 | 0.380314 | `get_bestpractices_get` | ❌ |

---

## Test 220

**Expected Tool:** `functionapp_get`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607276 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.447400 | `mysql_server_config_get` | ❌ |
| 3 | 0.424693 | `appconfig_account_list` | ❌ |
| 4 | 0.411267 | `appconfig_kv_get` | ❌ |
| 5 | 0.400402 | `deploy_app_logs_get` | ❌ |

---

## Test 221

**Expected Tool:** `functionapp_get`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622384 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.413481 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.390708 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.383533 | `deploy_app_logs_get` | ❌ |
| 5 | 0.360665 | `storage_account_get` | ❌ |

---

## Test 222

**Expected Tool:** `functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690933 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.441937 | `foundry_resource_get` | ❌ |
| 3 | 0.432317 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.431821 | `applens_resource_diagnose` | ❌ |
| 5 | 0.429077 | `storage_account_get` | ❌ |

---

## Test 223

**Expected Tool:** `functionapp_get`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592791 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.417817 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.409712 | `deploy_app_logs_get` | ❌ |
| 4 | 0.399953 | `storage_account_get` | ❌ |
| 5 | 0.392237 | `applens_resource_diagnose` | ❌ |

---

## Test 224

**Expected Tool:** `functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687356 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.449588 | `deploy_app_logs_get` | ❌ |
| 3 | 0.428689 | `applens_resource_diagnose` | ❌ |
| 4 | 0.424686 | `foundry_resource_get` | ❌ |
| 5 | 0.391781 | `monitor_webtests_get` | ❌ |

---

## Test 225

**Expected Tool:** `functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644882 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.430189 | `deploy_app_logs_get` | ❌ |
| 3 | 0.421082 | `storage_account_get` | ❌ |
| 4 | 0.403311 | `signalr_runtime_get` | ❌ |
| 5 | 0.391615 | `foundry_resource_get` | ❌ |

---

## Test 226

**Expected Tool:** `functionapp_get`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.554980 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.426703 | `quota_usage_check` | ❌ |
| 3 | 0.424610 | `deploy_app_logs_get` | ❌ |
| 4 | 0.408011 | `deploy_plan_get` | ❌ |
| 5 | 0.381629 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 227

**Expected Tool:** `functionapp_get`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565797 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.403665 | `deploy_app_logs_get` | ❌ |
| 3 | 0.384159 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.369868 | `applens_resource_diagnose` | ❌ |
| 5 | 0.355044 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 228

**Expected Tool:** `functionapp_get`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646561 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.559382 | `search_service_list` | ❌ |
| 3 | 0.534930 | `subscription_list` | ❌ |
| 4 | 0.529031 | `kusto_cluster_list` | ❌ |
| 5 | 0.516618 | `cosmos_account_list` | ❌ |

---

## Test 229

**Expected Tool:** `functionapp_get`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560249 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.464985 | `deploy_app_logs_get` | ❌ |
| 3 | 0.412646 | `search_service_list` | ❌ |
| 4 | 0.411323 | `get_bestpractices_get` | ❌ |
| 5 | 0.398503 | `extension_cli_install` | ❌ |

---

## Test 230

**Expected Tool:** `functionapp_get`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433674 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.346619 | `deploy_app_logs_get` | ❌ |
| 3 | 0.337966 | `applens_resource_diagnose` | ❌ |
| 4 | 0.316594 | `extension_cli_install` | ❌ |
| 5 | 0.284362 | `get_bestpractices_get` | ❌ |

---

## Test 231

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** Get the account settings for my key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604780 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.532196 | `storage_account_get` | ❌ |
| 3 | 0.496629 | `keyvault_key_get` | ❌ |
| 4 | 0.452366 | `appconfig_kv_set` | ❌ |
| 5 | 0.448039 | `keyvault_secret_get` | ❌ |

---

## Test 232

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** Show me the account settings for managed HSM keyvault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671370 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.455561 | `storage_account_get` | ❌ |
| 3 | 0.441225 | `keyvault_key_get` | ❌ |
| 4 | 0.404666 | `appconfig_kv_set` | ❌ |
| 5 | 0.395274 | `keyvault_secret_get` | ❌ |

---

## Test 233

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** What's the value of the <setting_name> setting in my key vault with name <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.505750 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.496540 | `appconfig_kv_set` | ❌ |
| 3 | 0.420145 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.419126 | `keyvault_key_get` | ❌ |
| 5 | 0.410215 | `keyvault_secret_get` | ❌ |

---

## Test 234

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627713 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.570324 | `keyvault_certificate_import` | ❌ |
| 3 | 0.540181 | `keyvault_key_create` | ❌ |
| 4 | 0.515939 | `keyvault_certificate_get` | ❌ |
| 5 | 0.500018 | `keyvault_certificate_list` | ❌ |

---

## Test 235

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Generate a certificate named <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600005 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.561459 | `keyvault_certificate_import` | ❌ |
| 3 | 0.519600 | `keyvault_certificate_get` | ❌ |
| 4 | 0.502052 | `keyvault_key_create` | ❌ |
| 5 | 0.497159 | `keyvault_certificate_list` | ❌ |

---

## Test 236

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Request creation of certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573920 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.527631 | `keyvault_certificate_import` | ❌ |
| 3 | 0.495169 | `keyvault_certificate_get` | ❌ |
| 4 | 0.481471 | `keyvault_key_create` | ❌ |
| 5 | 0.469500 | `keyvault_certificate_list` | ❌ |

---

## Test 237

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591697 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.562265 | `keyvault_certificate_import` | ❌ |
| 3 | 0.518739 | `keyvault_certificate_get` | ❌ |
| 4 | 0.502529 | `keyvault_key_create` | ❌ |
| 5 | 0.479992 | `keyvault_certificate_list` | ❌ |

---

## Test 238

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Issue a certificate <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622788 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.558532 | `keyvault_certificate_import` | ❌ |
| 3 | 0.531287 | `keyvault_certificate_get` | ❌ |
| 4 | 0.521316 | `keyvault_certificate_list` | ❌ |
| 5 | 0.465056 | `keyvault_key_create` | ❌ |

---

## Test 239

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603235 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.528405 | `keyvault_certificate_list` | ❌ |
| 3 | 0.519037 | `keyvault_certificate_import` | ❌ |
| 4 | 0.499293 | `keyvault_certificate_create` | ❌ |
| 5 | 0.486609 | `keyvault_key_get` | ❌ |

---

## Test 240

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649214 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.563103 | `keyvault_key_get` | ❌ |
| 3 | 0.514222 | `keyvault_secret_get` | ❌ |
| 4 | 0.509503 | `keyvault_certificate_list` | ❌ |
| 5 | 0.507757 | `keyvault_certificate_import` | ❌ |

---

## Test 241

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Get the certificate <certificate_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606958 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.515570 | `keyvault_certificate_list` | ❌ |
| 3 | 0.511197 | `keyvault_certificate_create` | ❌ |
| 4 | 0.507768 | `keyvault_certificate_import` | ❌ |
| 5 | 0.474394 | `keyvault_key_get` | ❌ |

---

## Test 242

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Display the certificate details for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649758 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527400 | `keyvault_key_get` | ❌ |
| 3 | 0.521556 | `keyvault_certificate_list` | ❌ |
| 4 | 0.509796 | `keyvault_certificate_import` | ❌ |
| 5 | 0.501988 | `keyvault_secret_get` | ❌ |

---

## Test 243

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594012 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527404 | `keyvault_certificate_list` | ❌ |
| 3 | 0.519059 | `keyvault_certificate_import` | ❌ |
| 4 | 0.501138 | `keyvault_certificate_create` | ❌ |
| 5 | 0.465174 | `keyvault_key_get` | ❌ |

---

## Test 244

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585481 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.420009 | `keyvault_certificate_get` | ❌ |
| 3 | 0.402595 | `keyvault_certificate_create` | ❌ |
| 4 | 0.399342 | `keyvault_certificate_list` | ❌ |
| 5 | 0.352905 | `keyvault_key_create` | ❌ |

---

## Test 245

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622125 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.501864 | `keyvault_certificate_get` | ❌ |
| 3 | 0.498847 | `keyvault_certificate_create` | ❌ |
| 4 | 0.448105 | `keyvault_certificate_list` | ❌ |
| 5 | 0.419811 | `keyvault_key_create` | ❌ |

---

## Test 246

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Upload certificate file <file_path> to key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595707 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.453929 | `keyvault_certificate_create` | ❌ |
| 3 | 0.451713 | `keyvault_certificate_get` | ❌ |
| 4 | 0.418203 | `keyvault_certificate_list` | ❌ |
| 5 | 0.413377 | `keyvault_key_create` | ❌ |

---

## Test 247

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619480 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.515610 | `keyvault_certificate_get` | ❌ |
| 3 | 0.480815 | `keyvault_certificate_create` | ❌ |
| 4 | 0.444386 | `keyvault_certificate_list` | ❌ |
| 5 | 0.381873 | `keyvault_key_create` | ❌ |

---

## Test 248

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595417 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.452489 | `keyvault_certificate_create` | ❌ |
| 3 | 0.440366 | `keyvault_certificate_get` | ❌ |
| 4 | 0.408018 | `keyvault_key_create` | ❌ |
| 5 | 0.392284 | `keyvault_secret_create` | ❌ |

---

## Test 249

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726124 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.583110 | `keyvault_key_list` | ❌ |
| 3 | 0.531838 | `keyvault_secret_list` | ❌ |
| 4 | 0.514152 | `keyvault_certificate_get` | ❌ |
| 5 | 0.485792 | `keyvault_certificate_create` | ❌ |

---

## Test 250

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615541 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.525122 | `keyvault_certificate_get` | ❌ |
| 3 | 0.475156 | `keyvault_key_list` | ❌ |
| 4 | 0.460973 | `keyvault_certificate_create` | ❌ |
| 5 | 0.448139 | `keyvault_key_get` | ❌ |

---

## Test 251

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** What certificates are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624711 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.518577 | `keyvault_certificate_get` | ❌ |
| 3 | 0.510048 | `keyvault_certificate_create` | ❌ |
| 4 | 0.505534 | `keyvault_certificate_import` | ❌ |
| 5 | 0.497356 | `keyvault_key_list` | ❌ |

---

## Test 252

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** List certificate names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672622 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.553990 | `keyvault_key_list` | ❌ |
| 3 | 0.511668 | `keyvault_secret_list` | ❌ |
| 4 | 0.505198 | `keyvault_certificate_get` | ❌ |
| 5 | 0.492357 | `keyvault_certificate_create` | ❌ |

---

## Test 253

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Enumerate certificates in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747407 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.594216 | `keyvault_key_list` | ❌ |
| 3 | 0.558644 | `keyvault_secret_list` | ❌ |
| 4 | 0.513381 | `keyvault_certificate_get` | ❌ |
| 5 | 0.490876 | `keyvault_certificate_create` | ❌ |

---

## Test 254

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show certificate names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639711 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.512269 | `keyvault_certificate_get` | ❌ |
| 3 | 0.507572 | `keyvault_key_list` | ❌ |
| 4 | 0.482583 | `keyvault_certificate_create` | ❌ |
| 5 | 0.464535 | `keyvault_secret_list` | ❌ |

---

## Test 255

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661466 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.456633 | `keyvault_secret_create` | ❌ |
| 3 | 0.451790 | `keyvault_certificate_create` | ❌ |
| 4 | 0.429614 | `keyvault_certificate_import` | ❌ |
| 5 | 0.399326 | `keyvault_key_get` | ❌ |

---

## Test 256

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Generate a key <key_name> with type <key_type> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641070 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.428502 | `keyvault_key_get` | ❌ |
| 3 | 0.422763 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420135 | `keyvault_secret_create` | ❌ |
| 5 | 0.405644 | `appconfig_kv_set` | ❌ |

---

## Test 257

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an oct key in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547493 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.463630 | `keyvault_secret_create` | ❌ |
| 3 | 0.447410 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420366 | `keyvault_key_get` | ❌ |
| 5 | 0.404350 | `keyvault_certificate_import` | ❌ |

---

## Test 258

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an RSA key in the vault <key_vault_account_name> with name <key_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641369 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.501664 | `keyvault_secret_create` | ❌ |
| 3 | 0.491735 | `keyvault_certificate_create` | ❌ |
| 4 | 0.464557 | `keyvault_certificate_import` | ❌ |
| 5 | 0.451016 | `keyvault_key_get` | ❌ |

---

## Test 259

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an EC key with name <key_name> in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571718 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.443369 | `keyvault_certificate_create` | ❌ |
| 3 | 0.434701 | `keyvault_secret_create` | ❌ |
| 4 | 0.421721 | `keyvault_key_get` | ❌ |
| 5 | 0.400533 | `keyvault_certificate_import` | ❌ |

---

## Test 260

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549488 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.468165 | `keyvault_secret_get` | ❌ |
| 3 | 0.452816 | `keyvault_key_create` | ❌ |
| 4 | 0.439969 | `keyvault_key_list` | ❌ |
| 5 | 0.430038 | `keyvault_certificate_get` | ❌ |

---

## Test 261

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629552 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.532651 | `keyvault_secret_get` | ❌ |
| 3 | 0.512278 | `storage_account_get` | ❌ |
| 4 | 0.499757 | `keyvault_certificate_get` | ❌ |
| 5 | 0.456992 | `keyvault_key_create` | ❌ |

---

## Test 262

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Get the key <key_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.484645 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.443182 | `keyvault_key_create` | ❌ |
| 3 | 0.409388 | `keyvault_secret_get` | ❌ |
| 4 | 0.395491 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.383519 | `appconfig_kv_lock_set` | ❌ |

---

## Test 263

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Display the key details for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590303 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.488213 | `keyvault_secret_get` | ❌ |
| 3 | 0.476498 | `storage_account_get` | ❌ |
| 4 | 0.464283 | `keyvault_certificate_get` | ❌ |
| 5 | 0.436511 | `keyvault_admin_settings_get` | ❌ |

---

## Test 264

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Retrieve key metadata for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518839 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.432982 | `storage_account_get` | ❌ |
| 3 | 0.432733 | `keyvault_admin_settings_get` | ❌ |
| 4 | 0.429089 | `keyvault_key_create` | ❌ |
| 5 | 0.422498 | `keyvault_secret_get` | ❌ |

---

## Test 265

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701448 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.601513 | `keyvault_certificate_list` | ❌ |
| 3 | 0.587218 | `keyvault_secret_list` | ❌ |
| 4 | 0.498767 | `cosmos_account_list` | ❌ |
| 5 | 0.480129 | `keyvault_admin_settings_get` | ❌ |

---

## Test 266

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549453 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.506815 | `keyvault_key_get` | ❌ |
| 3 | 0.475507 | `keyvault_certificate_list` | ❌ |
| 4 | 0.472465 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.455683 | `keyvault_secret_get` | ❌ |

---

## Test 267

**Expected Tool:** `keyvault_key_list`  
**Prompt:** What keys are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581970 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.502245 | `keyvault_admin_settings_get` | ❌ |
| 3 | 0.501481 | `keyvault_certificate_list` | ❌ |
| 4 | 0.476470 | `keyvault_key_get` | ❌ |
| 5 | 0.472124 | `keyvault_secret_list` | ❌ |

---

## Test 268

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List key names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641314 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.559550 | `keyvault_certificate_list` | ❌ |
| 3 | 0.553257 | `keyvault_secret_list` | ❌ |
| 4 | 0.486377 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.475992 | `cosmos_account_list` | ❌ |

---

## Test 269

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Enumerate keys in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.723266 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.611366 | `keyvault_certificate_list` | ❌ |
| 3 | 0.611042 | `keyvault_secret_list` | ❌ |
| 4 | 0.473886 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.441881 | `keyvault_key_get` | ❌ |

---

## Test 270

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Show key names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570444 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.501073 | `keyvault_key_get` | ❌ |
| 3 | 0.500103 | `keyvault_certificate_list` | ❌ |
| 4 | 0.496817 | `storage_account_get` | ❌ |
| 5 | 0.490100 | `keyvault_secret_list` | ❌ |

---

## Test 271

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678470 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.552708 | `keyvault_key_create` | ❌ |
| 3 | 0.512617 | `keyvault_secret_get` | ❌ |
| 4 | 0.474664 | `keyvault_certificate_create` | ❌ |
| 5 | 0.462012 | `appconfig_kv_set` | ❌ |

---

## Test 272

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663147 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.519601 | `keyvault_secret_get` | ❌ |
| 3 | 0.512233 | `appconfig_kv_set` | ❌ |
| 4 | 0.458502 | `keyvault_key_create` | ❌ |
| 5 | 0.429785 | `appconfig_kv_lock_set` | ❌ |

---

## Test 273

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639917 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.509674 | `keyvault_secret_get` | ❌ |
| 3 | 0.485203 | `appconfig_kv_set` | ❌ |
| 4 | 0.484680 | `keyvault_key_create` | ❌ |
| 5 | 0.448995 | `appconfig_kv_lock_set` | ❌ |

---

## Test 274

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.675185 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.499612 | `keyvault_secret_get` | ❌ |
| 3 | 0.498228 | `keyvault_key_create` | ❌ |
| 4 | 0.479174 | `keyvault_certificate_import` | ❌ |
| 5 | 0.458574 | `appconfig_kv_set` | ❌ |

---

## Test 275

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Update secret <secret_name> to value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571490 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.513686 | `keyvault_secret_get` | ❌ |
| 3 | 0.440666 | `appconfig_kv_set` | ❌ |
| 4 | 0.417799 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.407937 | `keyvault_key_get` | ❌ |

---

## Test 276

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602769 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.504212 | `keyvault_key_get` | ❌ |
| 3 | 0.501412 | `keyvault_secret_create` | ❌ |
| 4 | 0.478600 | `keyvault_secret_list` | ❌ |
| 5 | 0.442183 | `keyvault_certificate_get` | ❌ |

---

## Test 277

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653871 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.566786 | `keyvault_key_get` | ❌ |
| 3 | 0.517547 | `storage_account_get` | ❌ |
| 4 | 0.499014 | `keyvault_certificate_get` | ❌ |
| 5 | 0.485117 | `keyvault_secret_list` | ❌ |

---

## Test 278

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Get the secret <secret_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578479 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.492213 | `keyvault_key_get` | ❌ |
| 3 | 0.488680 | `keyvault_secret_create` | ❌ |
| 4 | 0.443595 | `keyvault_secret_list` | ❌ |
| 5 | 0.424167 | `keyvault_admin_settings_get` | ❌ |

---

## Test 279

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Display the secret details for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649267 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.546992 | `keyvault_key_get` | ❌ |
| 3 | 0.497402 | `storage_account_get` | ❌ |
| 4 | 0.494759 | `keyvault_certificate_get` | ❌ |
| 5 | 0.491412 | `keyvault_secret_list` | ❌ |

---

## Test 280

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Retrieve secret metadata for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577477 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.475443 | `keyvault_key_get` | ❌ |
| 3 | 0.466873 | `keyvault_secret_create` | ❌ |
| 4 | 0.447533 | `keyvault_secret_list` | ❌ |
| 5 | 0.439583 | `storage_account_get` | ❌ |

---

## Test 281

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701015 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.563736 | `keyvault_key_list` | ❌ |
| 3 | 0.538337 | `keyvault_certificate_list` | ❌ |
| 4 | 0.499642 | `keyvault_secret_get` | ❌ |
| 5 | 0.455500 | `cosmos_account_list` | ❌ |

---

## Test 282

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555367 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.543861 | `keyvault_secret_get` | ❌ |
| 3 | 0.497525 | `keyvault_key_get` | ❌ |
| 4 | 0.464661 | `keyvault_key_list` | ❌ |
| 5 | 0.453130 | `keyvault_admin_settings_get` | ❌ |

---

## Test 283

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** What secrets are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572149 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.529258 | `keyvault_secret_get` | ❌ |
| 3 | 0.493761 | `keyvault_key_list` | ❌ |
| 4 | 0.487620 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.475273 | `keyvault_key_get` | ❌ |

---

## Test 284

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** List secrets names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624070 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.559681 | `keyvault_key_list` | ❌ |
| 3 | 0.517516 | `keyvault_certificate_list` | ❌ |
| 4 | 0.479547 | `keyvault_secret_get` | ❌ |
| 5 | 0.454288 | `storage_blob_container_get` | ❌ |

---

## Test 285

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Enumerate secrets in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.742121 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.601183 | `keyvault_key_list` | ❌ |
| 3 | 0.567827 | `keyvault_certificate_list` | ❌ |
| 4 | 0.496127 | `keyvault_secret_get` | ❌ |
| 5 | 0.437560 | `keyvault_admin_settings_get` | ❌ |

---

## Test 286

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Show secrets names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566824 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.522398 | `keyvault_secret_get` | ❌ |
| 3 | 0.476309 | `keyvault_key_list` | ❌ |
| 4 | 0.462720 | `keyvault_secret_create` | ❌ |
| 5 | 0.461326 | `keyvault_key_get` | ❌ |

---

## Test 287

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588300 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.544309 | `aks_nodepool_get` | ❌ |
| 3 | 0.517279 | `kusto_cluster_get` | ❌ |
| 4 | 0.481416 | `mysql_server_config_get` | ❌ |
| 5 | 0.430975 | `postgres_server_config_get` | ❌ |

---

## Test 288

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621759 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.575634 | `aks_nodepool_get` | ❌ |
| 3 | 0.567870 | `kusto_cluster_get` | ❌ |
| 4 | 0.461466 | `sql_db_show` | ❌ |
| 5 | 0.444327 | `monitor_webtests_get` | ❌ |

---

## Test 289

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522525 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.483225 | `aks_nodepool_get` | ❌ |
| 3 | 0.434684 | `kusto_cluster_get` | ❌ |
| 4 | 0.380301 | `mysql_server_config_get` | ❌ |
| 5 | 0.366689 | `kusto_cluster_list` | ❌ |

---

## Test 290

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588634 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.550582 | `aks_nodepool_get` | ❌ |
| 3 | 0.527511 | `kusto_cluster_get` | ❌ |
| 4 | 0.445722 | `storage_account_get` | ❌ |
| 5 | 0.435597 | `foundry_resource_get` | ❌ |

---

## Test 291

**Expected Tool:** `aks_cluster_get`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756471 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.749416 | `kusto_cluster_list` | ❌ |
| 3 | 0.590161 | `aks_nodepool_get` | ❌ |
| 4 | 0.568502 | `kusto_database_list` | ❌ |
| 5 | 0.562043 | `search_service_list` | ❌ |

---

## Test 292

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612123 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.586661 | `kusto_cluster_list` | ❌ |
| 3 | 0.507701 | `aks_nodepool_get` | ❌ |
| 4 | 0.489724 | `kusto_cluster_get` | ❌ |
| 5 | 0.462957 | `kusto_database_list` | ❌ |

---

## Test 293

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628429 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.563208 | `aks_nodepool_get` | ❌ |
| 3 | 0.526756 | `kusto_cluster_list` | ❌ |
| 4 | 0.426157 | `kusto_cluster_get` | ❌ |
| 5 | 0.409308 | `kusto_database_list` | ❌ |

---

## Test 294

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728958 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.517021 | `kusto_cluster_get` | ❌ |
| 3 | 0.509820 | `aks_cluster_get` | ❌ |
| 4 | 0.468392 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.463192 | `sql_elastic-pool_list` | ❌ |

---

## Test 295

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654192 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.458497 | `sql_elastic-pool_list` | ❌ |
| 3 | 0.446296 | `aks_cluster_get` | ❌ |
| 4 | 0.440322 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.414154 | `kusto_cluster_get` | ❌ |

---

## Test 296

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592931 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.402605 | `aks_cluster_get` | ❌ |
| 3 | 0.385275 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.383112 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.355131 | `kusto_cluster_get` | ❌ |

---

## Test 297

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692235 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.519037 | `aks_cluster_get` | ❌ |
| 3 | 0.506624 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.500749 | `kusto_cluster_list` | ❌ |
| 5 | 0.487707 | `sql_elastic-pool_list` | ❌ |

---

## Test 298

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732102 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.561829 | `aks_cluster_get` | ❌ |
| 3 | 0.510269 | `sql_elastic-pool_list` | ❌ |
| 4 | 0.509732 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.486700 | `kusto_cluster_list` | ❌ |

---

## Test 299

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629316 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.456894 | `aks_cluster_get` | ❌ |
| 3 | 0.443957 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.432997 | `kusto_cluster_list` | ❌ |
| 5 | 0.425570 | `sql_elastic-pool_list` | ❌ |

---

## Test 300

**Expected Tool:** `loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577811 | `loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.519418 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.512099 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.472777 | `monitor_webtests_create` | ❌ |
| 5 | 0.460717 | `loadtesting_testresource_list` | ❌ |

---

## Test 301

**Expected Tool:** `loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626226 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.619944 | `loadtesting_test_get` | ✅ **EXPECTED** |
| 3 | 0.594666 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.590697 | `monitor_webtests_get` | ❌ |
| 5 | 0.535650 | `monitor_webtests_list` | ❌ |

---

## Test 302

**Expected Tool:** `loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645537 | `loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.618773 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.541746 | `loadtesting_test_create` | ❌ |
| 4 | 0.539771 | `loadtesting_testrun_create` | ❌ |
| 5 | 0.526226 | `monitor_webtests_list` | ❌ |

---

## Test 303

**Expected Tool:** `loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.794326 | `loadtesting_testresource_list` | ✅ **EXPECTED** |
| 2 | 0.652990 | `monitor_webtests_list` | ❌ |
| 3 | 0.577427 | `group_list` | ❌ |
| 4 | 0.575172 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.565565 | `datadog_monitoredresources_list` | ❌ |

---

## Test 304

**Expected Tool:** `loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688976 | `loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.594879 | `loadtesting_testrun_update` | ❌ |
| 3 | 0.558636 | `loadtesting_test_create` | ❌ |
| 4 | 0.547102 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.496224 | `loadtesting_testresource_list` | ❌ |

---

## Test 305

**Expected Tool:** `loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618909 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.601963 | `loadtesting_test_get` | ❌ |
| 3 | 0.597266 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.577220 | `monitor_webtests_get` | ❌ |
| 5 | 0.566095 | `loadtesting_testrun_list` | ❌ |

---

## Test 306

**Expected Tool:** `loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669180 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.640360 | `loadtesting_testrun_list` | ✅ **EXPECTED** |
| 3 | 0.601075 | `loadtesting_test_get` | ❌ |
| 4 | 0.577460 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.569424 | `monitor_webtests_get` | ❌ |

---

## Test 307

**Expected Tool:** `loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.706747 | `loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.514428 | `loadtesting_testrun_create` | ❌ |
| 3 | 0.486980 | `monitor_webtests_update` | ❌ |
| 4 | 0.470337 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.468374 | `monitor_webtests_get` | ❌ |

---

## Test 308

**Expected Tool:** `grafana_list`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599427 | `kusto_cluster_list` | ❌ |
| 2 | 0.578892 | `grafana_list` | ✅ **EXPECTED** |
| 3 | 0.551851 | `search_service_list` | ❌ |
| 4 | 0.550372 | `subscription_list` | ❌ |
| 5 | 0.531259 | `redis_list` | ❌ |

---

## Test 309

**Expected Tool:** `managedlustre_fs_create`  
**Prompt:** Create an Azure Managed Lustre filesystem with name <filesystem_name>, size <filesystem_size>, SKU <sku>, and subnet <subnet_id> for availability zone <zone> in location <location>. Maintenance should occur on <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728113 | `managedlustre_fs_create` | ✅ **EXPECTED** |
| 2 | 0.616164 | `managedlustre_fs_list` | ❌ |
| 3 | 0.605775 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.598255 | `managedlustre_fs_update` | ❌ |
| 5 | 0.557720 | `managedlustre_fs_subnetsize_validate` | ❌ |

---

## Test 310

**Expected Tool:** `managedlustre_fs_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.750675 | `managedlustre_fs_list` | ✅ **EXPECTED** |
| 2 | 0.631770 | `managedlustre_fs_sku_get` | ❌ |
| 3 | 0.582660 | `managedlustre_fs_create` | ❌ |
| 4 | 0.562377 | `kusto_cluster_list` | ❌ |
| 5 | 0.513156 | `search_service_list` | ❌ |

---

## Test 311

**Expected Tool:** `managedlustre_fs_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743881 | `managedlustre_fs_list` | ✅ **EXPECTED** |
| 2 | 0.613165 | `managedlustre_fs_sku_get` | ❌ |
| 3 | 0.565820 | `managedlustre_fs_create` | ❌ |
| 4 | 0.520005 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.515449 | `loadtesting_testresource_list` | ❌ |

---

## Test 312

**Expected Tool:** `managedlustre_fs_sku_get`  
**Prompt:** List the Azure Managed Lustre SKUs available in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.827381 | `managedlustre_fs_sku_get` | ✅ **EXPECTED** |
| 2 | 0.613674 | `managedlustre_fs_list` | ❌ |
| 3 | 0.513242 | `managedlustre_fs_create` | ❌ |
| 4 | 0.496242 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 5 | 0.470241 | `kusto_cluster_list` | ❌ |

---

## Test 313

**Expected Tool:** `managedlustre_fs_subnetsize_ask`  
**Prompt:** Tell me how many IP addresses I need for an Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739766 | `managedlustre_fs_subnetsize_ask` | ✅ **EXPECTED** |
| 2 | 0.651598 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 3 | 0.594585 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.559498 | `managedlustre_fs_list` | ❌ |
| 5 | 0.533684 | `managedlustre_fs_create` | ❌ |

---

## Test 314

**Expected Tool:** `managedlustre_fs_subnetsize_validate`  
**Prompt:** Validate if the network <subnet_id> can host Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879389 | `managedlustre_fs_subnetsize_validate` | ✅ **EXPECTED** |
| 2 | 0.622463 | `managedlustre_fs_subnetsize_ask` | ❌ |
| 3 | 0.542808 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.515936 | `managedlustre_fs_create` | ❌ |
| 5 | 0.480855 | `managedlustre_fs_list` | ❌ |

---

## Test 315

**Expected Tool:** `managedlustre_fs_update`  
**Prompt:** Update the maintenance window of the Azure Managed Lustre filesystem <filesystem_name> to <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739000 | `managedlustre_fs_update` | ✅ **EXPECTED** |
| 2 | 0.527525 | `managedlustre_fs_create` | ❌ |
| 3 | 0.487193 | `managedlustre_fs_list` | ❌ |
| 4 | 0.385349 | `managedlustre_fs_sku_get` | ❌ |
| 5 | 0.344891 | `managedlustre_fs_subnetsize_validate` | ❌ |

---

## Test 316

**Expected Tool:** `marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570189 | `marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.499184 | `marketplace_product_list` | ❌ |
| 3 | 0.353256 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.333160 | `servicebus_topic_details` | ❌ |
| 5 | 0.330935 | `servicebus_queue_details` | ❌ |

---

## Test 317

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607916 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.443000 | `marketplace_product_get` | ❌ |
| 3 | 0.343549 | `search_service_list` | ❌ |
| 4 | 0.330500 | `foundry_models_list` | ❌ |
| 5 | 0.328676 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 318

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537726 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.385151 | `marketplace_product_get` | ❌ |
| 3 | 0.308769 | `foundry_models_list` | ❌ |
| 4 | 0.288006 | `redis_list` | ❌ |
| 5 | 0.260387 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 319

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Get best practices for building AI applications in Azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555579 | `get_bestpractices_get` | ❌ |
| 2 | 0.501211 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.480235 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.478355 | `cloudarchitect_design` | ❌ |
| 5 | 0.476579 | `deploy_iac_rules_get` | ❌ |

---

## Test 320

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Show me the best practices for Azure AI Foundry agents code generation  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603773 | `foundry_agents_get-sdk-sample` | ❌ |
| 2 | 0.534202 | `get_bestpractices_get` | ❌ |
| 3 | 0.520223 | `foundry_agents_list` | ❌ |
| 4 | 0.508727 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.480034 | `deploy_plan_get` | ❌ |

---

## Test 321

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Get guidance for building agents with Azure AI Foundry  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593216 | `foundry_agents_get-sdk-sample` | ❌ |
| 2 | 0.553662 | `foundry_agents_list` | ❌ |
| 3 | 0.534160 | `foundry_agents_create` | ❌ |
| 4 | 0.513217 | `foundry_agents_connect` | ❌ |
| 5 | 0.505706 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 322

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Create an AI app that helps me to manage travel queries.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.343793 | `foundry_threads_create` | ❌ |
| 2 | 0.327503 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.320532 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.311958 | `foundry_agents_connect` | ❌ |
| 5 | 0.305073 | `foundry_agents_get-sdk-sample` | ❌ |

---

## Test 323

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Create an AI app that helps me to manage travel queries in Azure AI Foundry  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478745 | `foundry_openai_embeddings-create` | ❌ |
| 2 | 0.469654 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.466216 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.456719 | `foundry_resource_get` | ❌ |
| 5 | 0.448502 | `foundry_agents_list` | ❌ |

---

## Test 324

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646844 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.635406 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.586907 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.531727 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.490235 | `deploy_plan_get` | ❌ |

---

## Test 325

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600903 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.548542 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.541091 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.516852 | `deploy_plan_get` | ❌ |
| 5 | 0.516443 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 326

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625259 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.594323 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.518643 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.465572 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.451502 | `cloudarchitect_design` | ❌ |

---

## Test 327

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624273 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.570488 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.522998 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.493998 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.467377 | `extension_cli_install` | ❌ |

---

## Test 328

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581868 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.497378 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.495688 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.486928 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.474572 | `deploy_plan_get` | ❌ |

---

## Test 329

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610986 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.532790 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.487322 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.458060 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.448034 | `extension_cli_install` | ❌ |

---

## Test 330

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557862 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.513262 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.505123 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.483705 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.422144 | `cloudarchitect_design` | ❌ |

---

## Test 331

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582541 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.500368 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.472112 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.433134 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.432810 | `cloudarchitect_design` | ❌ |

---

## Test 332

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** configure azure mcp in coding agent for my repo  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488855 | `deploy_plan_get` | ❌ |
| 2 | 0.460956 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.390270 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.370298 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.369169 | `extension_cli_install` | ❌ |

---

## Test 333

**Expected Tool:** `monitor_activitylog_list`  
**Prompt:** List the activity logs of the last month for <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537893 | `monitor_activitylog_list` | ✅ **EXPECTED** |
| 2 | 0.506212 | `monitor_resource_log_query` | ❌ |
| 3 | 0.371727 | `monitor_workspace_log_query` | ❌ |
| 4 | 0.363798 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.344629 | `datadog_monitoredresources_list` | ❌ |

---

## Test 334

**Expected Tool:** `monitor_healthmodels_entity_get`  
**Prompt:** Show me the health status of entity <entity_id> using the health model <health_model_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660947 | `monitor_healthmodels_entity_get` | ✅ **EXPECTED** |
| 2 | 0.609276 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.351697 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.328321 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.288705 | `foundry_models_deployments_list` | ❌ |

---

## Test 335

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592640 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424141 | `monitor_metrics_query` | ❌ |
| 3 | 0.368319 | `bicepschema_get` | ❌ |
| 4 | 0.332356 | `monitor_table_type_list` | ❌ |
| 5 | 0.324986 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 336

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607600 | `storage_account_get` | ❌ |
| 2 | 0.587736 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.544781 | `storage_blob_container_get` | ❌ |
| 4 | 0.495829 | `storage_blob_get` | ❌ |
| 5 | 0.473421 | `managedlustre_fs_list` | ❌ |

---

## Test 337

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633136 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495555 | `monitor_metrics_query` | ❌ |
| 3 | 0.434042 | `monitor_resource_log_query` | ❌ |
| 4 | 0.392971 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.388754 | `bicepschema_get` | ❌ |

---

## Test 338

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

## Test 339

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557830 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.476671 | `monitor_resource_log_query` | ❌ |
| 3 | 0.460611 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.455904 | `quota_usage_check` | ❌ |
| 5 | 0.438233 | `monitor_metrics_definitions` | ❌ |

---

## Test 340

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461249 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.390029 | `monitor_metrics_definitions` | ❌ |
| 3 | 0.338557 | `monitor_resource_log_query` | ❌ |
| 4 | 0.334519 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.306338 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 341

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496878 | `monitor_resource_log_query` | ❌ |
| 2 | 0.492138 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 3 | 0.448148 | `applens_resource_diagnose` | ❌ |
| 4 | 0.412184 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.397335 | `quota_usage_check` | ❌ |

---

## Test 342

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525585 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.406185 | `monitor_resource_log_query` | ❌ |
| 3 | 0.384482 | `monitor_metrics_definitions` | ❌ |
| 4 | 0.347723 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.330713 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 343

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480140 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.444779 | `monitor_resource_log_query` | ❌ |
| 3 | 0.388382 | `applens_resource_diagnose` | ❌ |
| 4 | 0.363412 | `quota_usage_check` | ❌ |
| 5 | 0.350076 | `resourcehealth_health-events_list` | ❌ |

---

## Test 344

**Expected Tool:** `monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687852 | `monitor_resource_log_query` | ✅ **EXPECTED** |
| 2 | 0.621919 | `monitor_workspace_log_query` | ❌ |
| 3 | 0.598393 | `monitor_activitylog_list` | ❌ |
| 4 | 0.485633 | `deploy_app_logs_get` | ❌ |
| 5 | 0.469703 | `monitor_metrics_query` | ❌ |

---

## Test 345

**Expected Tool:** `monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.850711 | `monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725738 | `monitor_table_type_list` | ❌ |
| 3 | 0.620445 | `monitor_workspace_list` | ❌ |
| 4 | 0.541928 | `kusto_table_list` | ❌ |
| 5 | 0.539481 | `monitor_workspace_log_query` | ❌ |

---

## Test 346

**Expected Tool:** `monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798147 | `monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.701122 | `monitor_table_type_list` | ❌ |
| 3 | 0.599917 | `monitor_workspace_list` | ❌ |
| 4 | 0.542821 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.502882 | `monitor_resource_log_query` | ❌ |

---

## Test 347

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881524 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.765557 | `monitor_table_list` | ❌ |
| 3 | 0.569921 | `monitor_workspace_list` | ❌ |
| 4 | 0.504683 | `mysql_table_list` | ❌ |
| 5 | 0.497622 | `monitor_workspace_log_query` | ❌ |

---

## Test 348

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843138 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.736728 | `monitor_table_list` | ❌ |
| 3 | 0.576731 | `monitor_workspace_list` | ❌ |
| 4 | 0.509598 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.481189 | `mysql_table_list` | ❌ |

---

## Test 349

**Expected Tool:** `monitor_webtests_create`  
**Prompt:** Create a new Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650766 | `monitor_webtests_create` | ✅ **EXPECTED** |
| 2 | 0.569868 | `monitor_webtests_list` | ❌ |
| 3 | 0.550072 | `monitor_webtests_update` | ❌ |
| 4 | 0.533352 | `monitor_webtests_get` | ❌ |
| 5 | 0.482145 | `loadtesting_testresource_create` | ❌ |

---

## Test 350

**Expected Tool:** `monitor_webtests_get`  
**Prompt:** Get Web Test details for <webtest_resource_name> in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759015 | `monitor_webtests_get` | ✅ **EXPECTED** |
| 2 | 0.725302 | `monitor_webtests_list` | ❌ |
| 3 | 0.583816 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.562797 | `monitor_webtests_update` | ❌ |
| 5 | 0.530581 | `monitor_webtests_create` | ❌ |

---

## Test 351

**Expected Tool:** `monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.730837 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.610160 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.547708 | `grafana_list` | ❌ |
| 4 | 0.520829 | `redis_list` | ❌ |
| 5 | 0.496166 | `monitor_webtests_get` | ❌ |

---

## Test 352

**Expected Tool:** `monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793880 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.675965 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.584429 | `monitor_webtests_get` | ❌ |
| 4 | 0.573620 | `group_list` | ❌ |
| 5 | 0.546088 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 353

**Expected Tool:** `monitor_webtests_update`  
**Prompt:** Update an existing Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686449 | `monitor_webtests_update` | ✅ **EXPECTED** |
| 2 | 0.559296 | `monitor_webtests_get` | ❌ |
| 3 | 0.558239 | `monitor_webtests_create` | ❌ |
| 4 | 0.553466 | `monitor_webtests_list` | ❌ |
| 5 | 0.508736 | `loadtesting_testrun_update` | ❌ |

---

## Test 354

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813902 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.680201 | `grafana_list` | ❌ |
| 3 | 0.659497 | `monitor_table_list` | ❌ |
| 4 | 0.610623 | `kusto_cluster_list` | ❌ |
| 5 | 0.600802 | `search_service_list` | ❌ |

---

## Test 355

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656194 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.584758 | `monitor_table_list` | ❌ |
| 3 | 0.531083 | `monitor_table_type_list` | ❌ |
| 4 | 0.518254 | `grafana_list` | ❌ |
| 5 | 0.506772 | `monitor_workspace_log_query` | ❌ |

---

## Test 356

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732962 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.601481 | `grafana_list` | ❌ |
| 3 | 0.579669 | `monitor_table_list` | ❌ |
| 4 | 0.523782 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.522749 | `kusto_cluster_list` | ❌ |

---

## Test 357

**Expected Tool:** `monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610115 | `monitor_workspace_log_query` | ✅ **EXPECTED** |
| 2 | 0.587614 | `monitor_resource_log_query` | ❌ |
| 3 | 0.527733 | `monitor_activitylog_list` | ❌ |
| 4 | 0.498269 | `deploy_app_logs_get` | ❌ |
| 5 | 0.485470 | `monitor_table_list` | ❌ |

---

## Test 358

**Expected Tool:** `datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.668827 | `datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.454270 | `redis_list` | ❌ |
| 3 | 0.413661 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.413173 | `monitor_metrics_query` | ❌ |
| 5 | 0.401731 | `grafana_list` | ❌ |

---

## Test 359

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

## Test 360

**Expected Tool:** `extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533164 | `quota_usage_check` | ❌ |
| 2 | 0.481143 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.476826 | `extension_azqr` | ✅ **EXPECTED** |
| 4 | 0.471499 | `subscription_list` | ❌ |
| 5 | 0.468404 | `applens_resource_diagnose` | ❌ |

---

## Test 361

**Expected Tool:** `extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532792 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.492863 | `get_bestpractices_get` | ❌ |
| 3 | 0.476164 | `applicationinsights_recommendation_list` | ❌ |
| 4 | 0.473365 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.464954 | `cloudarchitect_design` | ❌ |

---

## Test 362

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

---

## Test 363

**Expected Tool:** `quota_region_availability_list`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590950 | `quota_region_availability_list` | ✅ **EXPECTED** |
| 2 | 0.413274 | `quota_usage_check` | ❌ |
| 3 | 0.391332 | `redis_list` | ❌ |
| 4 | 0.372940 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.369855 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 364

**Expected Tool:** `quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609378 | `quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.491300 | `quota_region_availability_list` | ❌ |
| 3 | 0.384035 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.376096 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.371392 | `redis_list` | ❌ |

---

## Test 365

**Expected Tool:** `role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645259 | `role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.539761 | `subscription_list` | ❌ |
| 3 | 0.484047 | `group_list` | ❌ |
| 4 | 0.478700 | `grafana_list` | ❌ |
| 5 | 0.471364 | `cosmos_account_list` | ❌ |

---

## Test 366

**Expected Tool:** `role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609705 | `role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.514696 | `subscription_list` | ❌ |
| 3 | 0.456956 | `grafana_list` | ❌ |
| 4 | 0.449210 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.445149 | `redis_list` | ❌ |

---

## Test 367

**Expected Tool:** `redis_list`  
**Prompt:** List all Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.810504 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.587836 | `grafana_list` | ❌ |
| 3 | 0.512954 | `kusto_cluster_list` | ❌ |
| 4 | 0.508531 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.501218 | `postgres_server_list` | ❌ |

---

## Test 368

**Expected Tool:** `redis_list`  
**Prompt:** Show me my Redis resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685128 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.374328 | `grafana_list` | ❌ |
| 3 | 0.364197 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.359774 | `mysql_server_list` | ❌ |
| 5 | 0.331841 | `mysql_database_list` | ❌ |

---

## Test 369

**Expected Tool:** `redis_list`  
**Prompt:** Show me the Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.781228 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.539177 | `grafana_list` | ❌ |
| 3 | 0.449276 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.449014 | `postgres_server_list` | ❌ |
| 5 | 0.442854 | `kusto_cluster_list` | ❌ |

---

## Test 370

**Expected Tool:** `redis_list`  
**Prompt:** Show me my Redis caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572767 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.316869 | `mysql_database_list` | ❌ |
| 3 | 0.301786 | `postgres_database_list` | ❌ |
| 4 | 0.286679 | `mysql_server_list` | ❌ |
| 5 | 0.273014 | `kusto_cluster_list` | ❌ |

---

## Test 371

**Expected Tool:** `redis_list`  
**Prompt:** Get Redis clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478070 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.456309 | `kusto_cluster_list` | ❌ |
| 3 | 0.384630 | `kusto_cluster_get` | ❌ |
| 4 | 0.359573 | `kusto_database_list` | ❌ |
| 5 | 0.343305 | `aks_cluster_get` | ❌ |

---

## Test 372

**Expected Tool:** `group_list`  
**Prompt:** List all resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755933 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.566552 | `workbooks_list` | ❌ |
| 3 | 0.564566 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.552633 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.549537 | `monitor_webtests_list` | ❌ |

---

## Test 373

**Expected Tool:** `group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529503 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.464690 | `redis_list` | ❌ |
| 3 | 0.463685 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.462699 | `mysql_server_list` | ❌ |
| 5 | 0.460280 | `loadtesting_testresource_list` | ❌ |

---

## Test 374

**Expected Tool:** `group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665819 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.532656 | `datadog_monitoredresources_list` | ❌ |
| 3 | 0.532505 | `redis_list` | ❌ |
| 4 | 0.532054 | `eventgrid_topic_list` | ❌ |
| 5 | 0.531920 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 375

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.556629 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.538273 | `resourcehealth_availability-status_list` | ❌ |
| 3 | 0.377586 | `quota_usage_check` | ❌ |
| 4 | 0.373112 | `monitor_healthmodels_entity_get` | ❌ |
| 5 | 0.349980 | `datadog_monitoredresources_list` | ❌ |

---

## Test 376

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576591 | `storage_account_get` | ❌ |
| 2 | 0.564128 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.556167 | `storage_blob_container_get` | ❌ |
| 4 | 0.487207 | `storage_blob_get` | ❌ |
| 5 | 0.466885 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 377

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577398 | `resourcehealth_availability-status_list` | ❌ |
| 2 | 0.501568 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.425180 | `mysql_server_list` | ❌ |
| 4 | 0.412025 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.393479 | `managedlustre_fs_list` | ❌ |

---

## Test 378

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737219 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.585501 | `redis_list` | ❌ |
| 3 | 0.549914 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.548549 | `grafana_list` | ❌ |
| 5 | 0.544505 | `subscription_list` | ❌ |

---

## Test 379

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644982 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.545208 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.509740 | `resourcehealth_health-events_list` | ❌ |
| 4 | 0.508252 | `quota_usage_check` | ❌ |
| 5 | 0.505776 | `redis_list` | ❌ |

---

## Test 380

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.596890 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.549900 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.496640 | `resourcehealth_health-events_list` | ❌ |
| 4 | 0.441921 | `applens_resource_diagnose` | ❌ |
| 5 | 0.433614 | `loadtesting_testresource_list` | ❌ |

---

## Test 381

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690719 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.554895 | `search_service_list` | ❌ |
| 3 | 0.534250 | `eventgrid_topic_list` | ❌ |
| 4 | 0.529761 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.518372 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 382

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686448 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.534556 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.513815 | `search_service_list` | ❌ |
| 4 | 0.513259 | `eventgrid_topic_list` | ❌ |
| 5 | 0.501135 | `subscription_list` | ❌ |

---

## Test 383

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** What service issues have occurred in the last 30 days?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.450841 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.267663 | `applens_resource_diagnose` | ❌ |
| 3 | 0.245709 | `cloudarchitect_design` | ❌ |
| 4 | 0.216847 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.211842 | `search_service_list` | ❌ |

---

## Test 384

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** List active service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685391 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.527905 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.524063 | `eventgrid_topic_list` | ❌ |
| 4 | 0.520197 | `search_service_list` | ❌ |
| 5 | 0.502064 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 385

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565851 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.437868 | `search_service_list` | ❌ |
| 3 | 0.403665 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.402493 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.397735 | `quota_usage_check` | ❌ |

---

## Test 386

**Expected Tool:** `servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642896 | `servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.460952 | `servicebus_topic_subscription_details` | ❌ |
| 3 | 0.436934 | `servicebus_topic_details` | ❌ |
| 4 | 0.385791 | `search_knowledge_base_get` | ❌ |
| 5 | 0.384199 | `storage_account_get` | ❌ |

---

## Test 387

**Expected Tool:** `servicebus_topic_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642952 | `servicebus_topic_details` | ✅ **EXPECTED** |
| 2 | 0.571861 | `servicebus_topic_subscription_details` | ❌ |
| 3 | 0.483976 | `servicebus_queue_details` | ❌ |
| 4 | 0.482958 | `eventgrid_topic_list` | ❌ |
| 5 | 0.458711 | `eventgrid_subscription_list` | ❌ |

---

## Test 388

**Expected Tool:** `servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633187 | `servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.517623 | `servicebus_topic_details` | ❌ |
| 3 | 0.494515 | `servicebus_queue_details` | ❌ |
| 4 | 0.493853 | `eventgrid_topic_list` | ❌ |
| 5 | 0.472128 | `eventgrid_subscription_list` | ❌ |

---

## Test 389

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show me the details of SignalR <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532544 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.355028 | `redis_list` | ❌ |
| 3 | 0.329804 | `foundry_resource_get` | ❌ |
| 4 | 0.319981 | `sql_server_show` | ❌ |
| 5 | 0.304420 | `servicebus_queue_details` | ❌ |

---

## Test 390

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show me the network information of SignalR runtime <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573446 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.337342 | `sql_server_show` | ❌ |
| 3 | 0.306559 | `foundry_resource_get` | ❌ |
| 4 | 0.305021 | `redis_list` | ❌ |
| 5 | 0.300956 | `servicebus_topic_details` | ❌ |

---

## Test 391

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Describe the SignalR runtime <signalr_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710353 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.411396 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.410606 | `foundry_resource_get` | ❌ |
| 4 | 0.399412 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.382099 | `sql_server_list` | ❌ |

---

## Test 392

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Get information about my SignalR runtime <signalr_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.715974 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.459045 | `foundry_resource_get` | ❌ |
| 3 | 0.430829 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.430765 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.417032 | `functionapp_get` | ❌ |

---

## Test 393

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show all the SignalRs information in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564072 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.501077 | `redis_list` | ❌ |
| 3 | 0.494478 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.481428 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.462417 | `mysql_server_list` | ❌ |

---

## Test 394

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** List all SignalRs in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530646 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.507653 | `postgres_server_list` | ❌ |
| 3 | 0.495157 | `redis_list` | ❌ |
| 4 | 0.494498 | `kusto_cluster_list` | ❌ |
| 5 | 0.487856 | `subscription_list` | ❌ |

---

## Test 395

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a new SQL database named <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516780 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.470913 | `sql_server_create` | ❌ |
| 3 | 0.420504 | `sql_db_rename` | ❌ |
| 4 | 0.408515 | `sql_db_delete` | ❌ |
| 5 | 0.404860 | `sql_server_delete` | ❌ |

---

## Test 396

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a SQL database <database_name> with Basic tier in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571760 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.459683 | `sql_server_create` | ❌ |
| 3 | 0.437526 | `sql_server_delete` | ❌ |
| 4 | 0.420843 | `sql_db_show` | ❌ |
| 5 | 0.417662 | `sql_db_delete` | ❌ |

---

## Test 397

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604472 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.545986 | `sql_server_create` | ❌ |
| 3 | 0.504013 | `sql_db_rename` | ❌ |
| 4 | 0.494377 | `sql_db_show` | ❌ |
| 5 | 0.473975 | `sql_db_list` | ❌ |

---

## Test 398

**Expected Tool:** `sql_db_delete`  
**Prompt:** Delete the SQL database <database_name> from server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.568196 | `sql_db_delete` | ✅ **EXPECTED** |
| 2 | 0.567412 | `sql_server_delete` | ❌ |
| 3 | 0.391509 | `sql_db_rename` | ❌ |
| 4 | 0.386564 | `sql_server_firewall-rule_delete` | ❌ |
| 5 | 0.364776 | `sql_db_show` | ❌ |

---

## Test 399

**Expected Tool:** `sql_db_delete`  
**Prompt:** Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567513 | `sql_server_delete` | ❌ |
| 2 | 0.543440 | `sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.500756 | `sql_db_show` | ❌ |
| 4 | 0.481083 | `sql_db_rename` | ❌ |
| 5 | 0.478729 | `sql_db_list` | ❌ |

---

## Test 400

**Expected Tool:** `sql_db_delete`  
**Prompt:** Delete the database called <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509916 | `sql_db_delete` | ✅ **EXPECTED** |
| 2 | 0.490892 | `sql_server_delete` | ❌ |
| 3 | 0.364494 | `postgres_database_list` | ❌ |
| 4 | 0.354710 | `mysql_database_list` | ❌ |
| 5 | 0.347837 | `sql_db_rename` | ❌ |

---

## Test 401

**Expected Tool:** `sql_db_list`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643186 | `sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.639398 | `mysql_database_list` | ❌ |
| 3 | 0.609178 | `postgres_database_list` | ❌ |
| 4 | 0.602890 | `cosmos_database_list` | ❌ |
| 5 | 0.570278 | `kusto_database_list` | ❌ |

---

## Test 402

**Expected Tool:** `sql_db_list`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.617746 | `sql_server_show` | ❌ |
| 2 | 0.609322 | `sql_db_list` | ✅ **EXPECTED** |
| 3 | 0.557245 | `mysql_database_list` | ❌ |
| 4 | 0.553488 | `mysql_server_config_get` | ❌ |
| 5 | 0.524274 | `sql_db_show` | ❌ |

---

## Test 403

**Expected Tool:** `sql_db_rename`  
**Prompt:** Rename the SQL database <database_name> on server <server_name> to <new_database_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593278 | `sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.425161 | `sql_server_delete` | ❌ |
| 3 | 0.416057 | `sql_db_delete` | ❌ |
| 4 | 0.396824 | `sql_db_create` | ❌ |
| 5 | 0.345805 | `sql_db_show` | ❌ |

---

## Test 404

**Expected Tool:** `sql_db_rename`  
**Prompt:** Rename my Azure SQL database <database_name> to <new_database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711063 | `sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.516485 | `sql_server_delete` | ❌ |
| 3 | 0.506499 | `sql_db_delete` | ❌ |
| 4 | 0.501476 | `sql_db_create` | ❌ |
| 5 | 0.433897 | `sql_server_show` | ❌ |

---

## Test 405

**Expected Tool:** `sql_db_show`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610788 | `sql_server_show` | ❌ |
| 2 | 0.593239 | `postgres_server_config_get` | ❌ |
| 3 | 0.530655 | `mysql_server_config_get` | ❌ |
| 4 | 0.528543 | `sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.465617 | `sql_db_list` | ❌ |

---

## Test 406

**Expected Tool:** `sql_db_show`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530095 | `sql_db_show` | ✅ **EXPECTED** |
| 2 | 0.503681 | `sql_server_show` | ❌ |
| 3 | 0.440073 | `sql_db_list` | ❌ |
| 4 | 0.438622 | `mysql_table_schema_get` | ❌ |
| 5 | 0.432990 | `mysql_database_list` | ❌ |

---

## Test 407

**Expected Tool:** `sql_db_update`  
**Prompt:** Update the performance tier of SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603366 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.467571 | `sql_db_create` | ❌ |
| 3 | 0.440493 | `sql_db_rename` | ❌ |
| 4 | 0.427621 | `sql_db_show` | ❌ |
| 5 | 0.413941 | `sql_server_delete` | ❌ |

---

## Test 408

**Expected Tool:** `sql_db_update`  
**Prompt:** Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550556 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.418358 | `sql_server_delete` | ❌ |
| 3 | 0.401817 | `sql_db_list` | ❌ |
| 4 | 0.395518 | `sql_db_rename` | ❌ |
| 5 | 0.394770 | `sql_db_show` | ❌ |

---

## Test 409

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678124 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502376 | `sql_db_list` | ❌ |
| 3 | 0.498208 | `mysql_database_list` | ❌ |
| 4 | 0.485167 | `aks_nodepool_get` | ❌ |
| 5 | 0.479044 | `sql_server_show` | ❌ |

---

## Test 410

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606425 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502877 | `sql_server_show` | ❌ |
| 3 | 0.457163 | `sql_db_list` | ❌ |
| 4 | 0.450655 | `aks_nodepool_get` | ❌ |
| 5 | 0.432815 | `mysql_database_list` | ❌ |

---

## Test 411

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592709 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.420481 | `mysql_database_list` | ❌ |
| 3 | 0.407084 | `aks_nodepool_get` | ❌ |
| 4 | 0.402474 | `mysql_server_list` | ❌ |
| 5 | 0.397670 | `sql_db_list` | ❌ |

---

## Test 412

**Expected Tool:** `sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682736 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.563708 | `sql_db_create` | ❌ |
| 3 | 0.529372 | `sql_server_list` | ❌ |
| 4 | 0.482102 | `storage_account_create` | ❌ |
| 5 | 0.474207 | `sql_db_rename` | ❌ |

---

## Test 413

**Expected Tool:** `sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618390 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.510169 | `sql_db_create` | ❌ |
| 3 | 0.472463 | `sql_server_show` | ❌ |
| 4 | 0.441174 | `sql_server_delete` | ❌ |
| 5 | 0.400939 | `sql_db_rename` | ❌ |

---

## Test 414

**Expected Tool:** `sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589934 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.501403 | `sql_db_create` | ❌ |
| 3 | 0.498302 | `sql_server_list` | ❌ |
| 4 | 0.461181 | `sql_db_rename` | ❌ |
| 5 | 0.442943 | `mysql_server_list` | ❌ |

---

## Test 415

**Expected Tool:** `sql_server_delete`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656593 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.548064 | `sql_db_delete` | ❌ |
| 3 | 0.518178 | `sql_server_list` | ❌ |
| 4 | 0.495640 | `sql_server_create` | ❌ |
| 5 | 0.483132 | `workbooks_delete` | ❌ |

---

## Test 416

**Expected Tool:** `sql_server_delete`  
**Prompt:** Remove the SQL server <server_name> from my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615073 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.393885 | `postgres_server_list` | ❌ |
| 3 | 0.379760 | `sql_db_delete` | ❌ |
| 4 | 0.376660 | `sql_server_show` | ❌ |
| 5 | 0.350228 | `sql_server_list` | ❌ |

---

## Test 417

**Expected Tool:** `sql_server_delete`  
**Prompt:** Delete SQL server <server_name> permanently  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624310 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.454892 | `sql_db_delete` | ❌ |
| 3 | 0.362389 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.341503 | `sql_server_show` | ❌ |
| 5 | 0.318758 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 418

**Expected Tool:** `sql_server_entra-admin_list`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.783479 | `sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.456051 | `sql_server_show` | ❌ |
| 3 | 0.434815 | `sql_server_list` | ❌ |
| 4 | 0.401908 | `sql_server_firewall-rule_list` | ❌ |
| 5 | 0.376055 | `sql_db_list` | ❌ |

---

## Test 419

**Expected Tool:** `sql_server_entra-admin_list`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713306 | `sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.413144 | `sql_server_show` | ❌ |
| 3 | 0.368036 | `sql_server_list` | ❌ |
| 4 | 0.315966 | `sql_db_list` | ❌ |
| 5 | 0.311085 | `postgres_server_list` | ❌ |

---

## Test 420

**Expected Tool:** `sql_server_entra-admin_list`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646419 | `sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.356025 | `sql_server_show` | ❌ |
| 3 | 0.322358 | `sql_server_list` | ❌ |
| 4 | 0.307885 | `sql_server_create` | ❌ |
| 5 | 0.269788 | `sql_server_delete` | ❌ |

---

## Test 421

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.635466 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.532712 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.522184 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.448938 | `sql_server_create` | ❌ |
| 5 | 0.440845 | `sql_server_delete` | ❌ |

---

## Test 422

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670189 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533562 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503648 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.316596 | `sql_server_list` | ❌ |
| 5 | 0.302362 | `sql_server_delete` | ❌ |

---

## Test 423

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685107 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.574336 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539577 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.428986 | `sql_server_create` | ❌ |
| 5 | 0.395165 | `sql_db_create` | ❌ |

---

## Test 424

**Expected Tool:** `sql_server_firewall-rule_delete`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691421 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.584379 | `sql_server_delete` | ❌ |
| 3 | 0.543857 | `sql_server_firewall-rule_list` | ❌ |
| 4 | 0.540333 | `sql_server_firewall-rule_create` | ❌ |
| 5 | 0.498444 | `sql_db_delete` | ❌ |

---

## Test 425

**Expected Tool:** `sql_server_firewall-rule_delete`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670091 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.574319 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.530412 | `sql_server_firewall-rule_create` | ❌ |
| 4 | 0.488400 | `sql_server_delete` | ❌ |
| 5 | 0.360385 | `sql_db_delete` | ❌ |

---

## Test 426

**Expected Tool:** `sql_server_firewall-rule_delete`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671212 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.601230 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.577330 | `sql_server_firewall-rule_create` | ❌ |
| 4 | 0.499272 | `sql_server_delete` | ❌ |
| 5 | 0.378585 | `sql_db_delete` | ❌ |

---

## Test 427

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729372 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.549667 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.513114 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.468812 | `sql_server_show` | ❌ |
| 5 | 0.418681 | `sql_server_list` | ❌ |

---

## Test 428

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630731 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.524126 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.476757 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.410680 | `sql_server_show` | ❌ |
| 5 | 0.348049 | `sql_server_list` | ❌ |

---

## Test 429

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630555 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.532484 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.473499 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.412906 | `sql_server_show` | ❌ |
| 5 | 0.350385 | `sql_server_list` | ❌ |

---

## Test 430

**Expected Tool:** `sql_server_list`  
**Prompt:** List all Azure SQL servers in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694271 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.596669 | `mysql_server_list` | ❌ |
| 3 | 0.578222 | `sql_db_list` | ❌ |
| 4 | 0.515894 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.509835 | `sql_db_show` | ❌ |

---

## Test 431

**Expected Tool:** `sql_server_list`  
**Prompt:** Show me every SQL server available in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618187 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.594043 | `mysql_server_list` | ❌ |
| 3 | 0.542398 | `sql_db_list` | ❌ |
| 4 | 0.507404 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.496257 | `group_list` | ❌ |

---

## Test 432

**Expected Tool:** `sql_server_show`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629672 | `sql_db_show` | ❌ |
| 2 | 0.595184 | `sql_server_show` | ✅ **EXPECTED** |
| 3 | 0.587768 | `sql_server_list` | ❌ |
| 4 | 0.560004 | `mysql_server_list` | ❌ |
| 5 | 0.540218 | `sql_db_list` | ❌ |

---

## Test 433

**Expected Tool:** `sql_server_show`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658817 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.610507 | `postgres_server_config_get` | ❌ |
| 3 | 0.538034 | `mysql_server_config_get` | ❌ |
| 4 | 0.471541 | `sql_db_show` | ❌ |
| 5 | 0.445430 | `postgres_server_param_get` | ❌ |

---

## Test 434

**Expected Tool:** `sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563143 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.392532 | `postgres_server_config_get` | ❌ |
| 3 | 0.380021 | `postgres_server_param_get` | ❌ |
| 4 | 0.372194 | `sql_server_firewall-rule_list` | ❌ |
| 5 | 0.370539 | `sql_db_show` | ❌ |

---

## Test 435

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533552 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.438046 | `storage_blob_container_create` | ❌ |
| 3 | 0.418191 | `storage_account_get` | ❌ |
| 4 | 0.414518 | `storage_blob_container_get` | ❌ |
| 5 | 0.370957 | `managedlustre_fs_create` | ❌ |

---

## Test 436

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500638 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.483202 | `managedlustre_fs_create` | ❌ |
| 3 | 0.407222 | `storage_account_get` | ❌ |
| 4 | 0.406804 | `storage_blob_container_create` | ❌ |
| 5 | 0.400151 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 437

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589003 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.535501 | `managedlustre_fs_create` | ❌ |
| 3 | 0.509731 | `storage_blob_container_create` | ❌ |
| 4 | 0.462519 | `storage_account_get` | ❌ |
| 5 | 0.447156 | `sql_db_create` | ❌ |

---

## Test 438

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.673749 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.608256 | `storage_blob_container_get` | ❌ |
| 3 | 0.556457 | `storage_blob_get` | ❌ |
| 4 | 0.483435 | `storage_account_create` | ❌ |
| 5 | 0.439236 | `cosmos_account_list` | ❌ |

---

## Test 439

**Expected Tool:** `storage_account_get`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692687 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.577547 | `storage_blob_container_get` | ❌ |
| 3 | 0.529205 | `storage_blob_get` | ❌ |
| 4 | 0.518215 | `storage_account_create` | ❌ |
| 5 | 0.448507 | `storage_blob_container_create` | ❌ |

---

## Test 440

**Expected Tool:** `storage_account_get`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649215 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.557016 | `managedlustre_fs_sku_get` | ❌ |
| 3 | 0.550148 | `storage_blob_container_get` | ❌ |
| 4 | 0.547647 | `subscription_list` | ❌ |
| 5 | 0.536909 | `cosmos_account_list` | ❌ |

---

## Test 441

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.556860 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.482418 | `storage_blob_container_get` | ❌ |
| 3 | 0.461284 | `managedlustre_fs_list` | ❌ |
| 4 | 0.421642 | `cosmos_account_list` | ❌ |
| 5 | 0.410587 | `storage_blob_get` | ❌ |

---

## Test 442

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619462 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.556436 | `storage_blob_container_get` | ❌ |
| 3 | 0.518229 | `storage_blob_get` | ❌ |
| 4 | 0.473598 | `cosmos_account_list` | ❌ |
| 5 | 0.465571 | `subscription_list` | ❌ |

---

## Test 443

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649793 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.583896 | `storage_blob_container_get` | ❌ |
| 3 | 0.524779 | `storage_account_create` | ❌ |
| 4 | 0.496679 | `storage_blob_get` | ❌ |
| 5 | 0.447784 | `cosmos_database_container_list` | ❌ |

---

## Test 444

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682161 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.590160 | `storage_blob_container_get` | ❌ |
| 3 | 0.559263 | `storage_blob_get` | ❌ |
| 4 | 0.500624 | `storage_account_create` | ❌ |
| 5 | 0.420514 | `storage_account_get` | ❌ |

---

## Test 445

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625490 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.543562 | `storage_blob_container_get` | ❌ |
| 3 | 0.497792 | `storage_blob_get` | ❌ |
| 4 | 0.463198 | `storage_account_create` | ❌ |
| 5 | 0.435103 | `cosmos_database_container_list` | ❌ |

---

## Test 446

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701642 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.623681 | `storage_blob_get` | ❌ |
| 3 | 0.577921 | `storage_account_get` | ❌ |
| 4 | 0.549803 | `storage_blob_container_create` | ❌ |
| 5 | 0.523288 | `cosmos_database_container_list` | ❌ |

---

## Test 447

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712037 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.680802 | `storage_blob_get` | ❌ |
| 3 | 0.613933 | `cosmos_database_container_list` | ❌ |
| 4 | 0.556319 | `storage_blob_container_create` | ❌ |
| 5 | 0.518266 | `storage_account_get` | ❌ |

---

## Test 448

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713527 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.592373 | `cosmos_database_container_list` | ❌ |
| 3 | 0.586169 | `storage_blob_get` | ❌ |
| 4 | 0.523322 | `storage_account_get` | ❌ |
| 5 | 0.487521 | `storage_blob_container_create` | ❌ |

---

## Test 449

**Expected Tool:** `storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.700973 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.646973 | `storage_blob_container_get` | ❌ |
| 3 | 0.541019 | `storage_blob_container_create` | ❌ |
| 4 | 0.527427 | `storage_account_get` | ❌ |
| 5 | 0.477946 | `cosmos_database_container_list` | ❌ |

---

## Test 450

**Expected Tool:** `storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694997 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.631161 | `storage_blob_container_get` | ❌ |
| 3 | 0.589152 | `storage_blob_container_create` | ❌ |
| 4 | 0.580226 | `storage_account_get` | ❌ |
| 5 | 0.457038 | `storage_account_create` | ❌ |

---

## Test 451

**Expected Tool:** `storage_blob_get`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.733586 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.700891 | `storage_blob_container_get` | ❌ |
| 3 | 0.605993 | `storage_blob_container_create` | ❌ |
| 4 | 0.579070 | `cosmos_database_container_list` | ❌ |
| 5 | 0.506639 | `cosmos_database_container_item_query` | ❌ |

---

## Test 452

**Expected Tool:** `storage_blob_get`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.704426 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.664940 | `storage_blob_container_get` | ❌ |
| 3 | 0.561557 | `storage_blob_container_create` | ❌ |
| 4 | 0.533515 | `cosmos_database_container_list` | ❌ |
| 5 | 0.484018 | `storage_account_get` | ❌ |

---

## Test 453

**Expected Tool:** `storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566319 | `storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.525579 | `storage_blob_container_create` | ❌ |
| 3 | 0.517548 | `storage_blob_get` | ❌ |
| 4 | 0.473592 | `storage_blob_container_get` | ❌ |
| 5 | 0.381886 | `storage_account_create` | ❌ |

---

## Test 454

**Expected Tool:** `subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654071 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512964 | `cosmos_account_list` | ❌ |
| 3 | 0.471653 | `postgres_server_list` | ❌ |
| 4 | 0.469023 | `kusto_cluster_list` | ❌ |
| 5 | 0.461078 | `redis_list` | ❌ |

---

## Test 455

**Expected Tool:** `subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.458821 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.407471 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.393695 | `eventgrid_topic_list` | ❌ |
| 4 | 0.391555 | `redis_list` | ❌ |
| 5 | 0.381238 | `postgres_server_list` | ❌ |

---

## Test 456

**Expected Tool:** `subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433196 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.319579 | `marketplace_product_list` | ❌ |
| 3 | 0.315615 | `marketplace_product_get` | ❌ |
| 4 | 0.293772 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.289334 | `eventgrid_topic_list` | ❌ |

---

## Test 457

**Expected Tool:** `subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.477592 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.357625 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.354286 | `marketplace_product_list` | ❌ |
| 4 | 0.344549 | `redis_list` | ❌ |
| 5 | 0.340837 | `eventgrid_topic_list` | ❌ |

---

## Test 458

**Expected Tool:** `azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686971 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.625325 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.605182 | `get_bestpractices_get` | ❌ |
| 4 | 0.483016 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.466241 | `deploy_plan_get` | ❌ |

---

## Test 459

**Expected Tool:** `azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581316 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.512141 | `get_bestpractices_get` | ❌ |
| 3 | 0.510004 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.473596 | `keyvault_secret_get` | ❌ |
| 5 | 0.444297 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 460

**Expected Tool:** `virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711969 | `virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
| 2 | 0.659763 | `virtualdesktop_hostpool_host_list` | ❌ |
| 3 | 0.620666 | `kusto_cluster_list` | ❌ |
| 4 | 0.548888 | `search_service_list` | ❌ |
| 5 | 0.535739 | `virtualdesktop_hostpool_host_user-list` | ❌ |

---

## Test 461

**Expected Tool:** `virtualdesktop_hostpool_host_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.727054 | `virtualdesktop_hostpool_host_list` | ✅ **EXPECTED** |
| 2 | 0.714469 | `virtualdesktop_hostpool_host_user-list` | ❌ |
| 3 | 0.573352 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.438621 | `aks_nodepool_get` | ❌ |
| 5 | 0.393721 | `sql_elastic-pool_list` | ❌ |

---

## Test 462

**Expected Tool:** `virtualdesktop_hostpool_host_user-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812659 | `virtualdesktop_hostpool_host_user-list` | ✅ **EXPECTED** |
| 2 | 0.659212 | `virtualdesktop_hostpool_host_list` | ❌ |
| 3 | 0.501167 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.357540 | `aks_nodepool_get` | ❌ |
| 5 | 0.336385 | `monitor_workspace_list` | ❌ |

---

## Test 463

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

## Test 464

**Expected Tool:** `workbooks_delete`  
**Prompt:** Delete the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621310 | `workbooks_delete` | ✅ **EXPECTED** |
| 2 | 0.498506 | `workbooks_show` | ❌ |
| 3 | 0.432454 | `workbooks_create` | ❌ |
| 4 | 0.425569 | `workbooks_list` | ❌ |
| 5 | 0.421897 | `workbooks_update` | ❌ |

---

## Test 465

**Expected Tool:** `workbooks_list`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.772431 | `workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.562485 | `workbooks_create` | ❌ |
| 3 | 0.516739 | `grafana_list` | ❌ |
| 4 | 0.493975 | `workbooks_show` | ❌ |
| 5 | 0.488609 | `group_list` | ❌ |

---

## Test 466

**Expected Tool:** `workbooks_list`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.708612 | `workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.570259 | `workbooks_create` | ❌ |
| 3 | 0.499633 | `workbooks_show` | ❌ |
| 4 | 0.485504 | `workbooks_delete` | ❌ |
| 5 | 0.472378 | `grafana_list` | ❌ |

---

## Test 467

**Expected Tool:** `workbooks_show`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686095 | `workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.498390 | `workbooks_create` | ❌ |
| 3 | 0.494708 | `workbooks_list` | ❌ |
| 4 | 0.463156 | `workbooks_update` | ❌ |
| 5 | 0.452348 | `workbooks_delete` | ❌ |

---

## Test 468

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

## Test 469

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

## Test 470

**Expected Tool:** `bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543154 | `bicepschema_get` | ✅ **EXPECTED** |
| 2 | 0.485970 | `foundry_models_deploy` | ❌ |
| 3 | 0.485889 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.453282 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.448373 | `get_bestpractices_get` | ❌ |

---

## Test 471

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502110 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.290902 | `storage_blob_upload` | ❌ |
| 3 | 0.259162 | `managedlustre_fs_create` | ❌ |
| 4 | 0.254991 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.245034 | `managedlustre_fs_subnetsize_validate` | ❌ |

---

## Test 472

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Help me design an Azure cloud service that will serve as an ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.508504 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.377941 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.341462 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.328747 | `get_bestpractices_get` | ❌ |
| 5 | 0.321855 | `deploy_plan_get` | ❌ |

---

## Test 473

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.423059 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.271943 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.265972 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.242581 | `deploy_plan_get` | ❌ |
| 5 | 0.229074 | `extension_cli_generate` | ❌ |

---

## Test 474

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.535224 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.369969 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.356331 | `managedlustre_fs_create` | ❌ |
| 4 | 0.352797 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.323920 | `storage_blob_upload` | ❌ |

---

## Summary

**Total Prompts Tested:** 474  
**Analysis Execution Time:** 105.8478990s  

### Success Rate Metrics

**Top Choice Success:** 91.4% (433/474 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 3.2% (15/474 tests)  
**🎯 High Confidence (≥0.7):** 22.4% (106/474 tests)  
**✅ Good Confidence (≥0.6):** 60.5% (287/474 tests)  
**👍 Fair Confidence (≥0.5):** 90.9% (431/474 tests)  
**👌 Acceptable Confidence (≥0.4):** 98.5% (467/474 tests)  
**❌ Low Confidence (<0.4):** 1.5% (7/474 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 3.2% (15/474 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 22.4% (106/474 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 58.6% (278/474 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 85.7% (406/474 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 91.4% (433/474 tests)  

### Success Rate Analysis

🟢 **Excellent** - The tool selection system is performing exceptionally well.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

