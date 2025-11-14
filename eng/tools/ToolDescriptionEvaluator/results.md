# Tool Selection Analysis Setup

**Setup completed:** 2025-11-14 20:01:56  
**Tool count:** 180  
**Database setup time:** 6.4976298s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-11-14 20:01:56  
**Tool count:** 180  

## Table of Contents

- [Test 1: azureaibestpractices_get](#test-1)
- [Test 2: azureaibestpractices_get](#test-2)
- [Test 3: azureaibestpractices_get](#test-3)
- [Test 4: azureaibestpractices_get](#test-4)
- [Test 5: azureaibestpractices_get](#test-5)
- [Test 6: foundry_agents_connect](#test-6)
- [Test 7: foundry_agents_create](#test-7)
- [Test 8: foundry_agents_evaluate](#test-8)
- [Test 9: foundry_agents_get-sdk-sample](#test-9)
- [Test 10: foundry_agents_list](#test-10)
- [Test 11: foundry_agents_list](#test-11)
- [Test 12: foundry_agents_query-and-evaluate](#test-12)
- [Test 13: foundry_knowledge_index_list](#test-13)
- [Test 14: foundry_knowledge_index_list](#test-14)
- [Test 15: foundry_knowledge_index_schema](#test-15)
- [Test 16: foundry_knowledge_index_schema](#test-16)
- [Test 17: foundry_models_deploy](#test-17)
- [Test 18: foundry_models_deployments_list](#test-18)
- [Test 19: foundry_models_deployments_list](#test-19)
- [Test 20: foundry_models_list](#test-20)
- [Test 21: foundry_models_list](#test-21)
- [Test 22: foundry_openai_chat-completions-create](#test-22)
- [Test 23: foundry_openai_create-completion](#test-23)
- [Test 24: foundry_openai_embeddings-create](#test-24)
- [Test 25: foundry_openai_embeddings-create](#test-25)
- [Test 26: foundry_openai_models-list](#test-26)
- [Test 27: foundry_openai_models-list](#test-27)
- [Test 28: foundry_resource_get](#test-28)
- [Test 29: foundry_resource_get](#test-29)
- [Test 30: foundry_resource_get](#test-30)
- [Test 31: foundry_threads_create](#test-31)
- [Test 32: foundry_threads_get-messages](#test-32)
- [Test 33: foundry_threads_list](#test-33)
- [Test 34: search_knowledge_base_get](#test-34)
- [Test 35: search_knowledge_base_get](#test-35)
- [Test 36: search_knowledge_base_get](#test-36)
- [Test 37: search_knowledge_base_get](#test-37)
- [Test 38: search_knowledge_base_get](#test-38)
- [Test 39: search_knowledge_base_get](#test-39)
- [Test 40: search_knowledge_base_retrieve](#test-40)
- [Test 41: search_knowledge_base_retrieve](#test-41)
- [Test 42: search_knowledge_base_retrieve](#test-42)
- [Test 43: search_knowledge_base_retrieve](#test-43)
- [Test 44: search_knowledge_base_retrieve](#test-44)
- [Test 45: search_knowledge_base_retrieve](#test-45)
- [Test 46: search_knowledge_base_retrieve](#test-46)
- [Test 47: search_knowledge_base_retrieve](#test-47)
- [Test 48: search_knowledge_source_get](#test-48)
- [Test 49: search_knowledge_source_get](#test-49)
- [Test 50: search_knowledge_source_get](#test-50)
- [Test 51: search_knowledge_source_get](#test-51)
- [Test 52: search_knowledge_source_get](#test-52)
- [Test 53: search_knowledge_source_get](#test-53)
- [Test 54: search_index_get](#test-54)
- [Test 55: search_index_get](#test-55)
- [Test 56: search_index_get](#test-56)
- [Test 57: search_index_query](#test-57)
- [Test 58: search_service_list](#test-58)
- [Test 59: search_service_list](#test-59)
- [Test 60: search_service_list](#test-60)
- [Test 61: speech_stt_recognize](#test-61)
- [Test 62: speech_stt_recognize](#test-62)
- [Test 63: speech_stt_recognize](#test-63)
- [Test 64: speech_stt_recognize](#test-64)
- [Test 65: speech_stt_recognize](#test-65)
- [Test 66: speech_stt_recognize](#test-66)
- [Test 67: speech_stt_recognize](#test-67)
- [Test 68: speech_stt_recognize](#test-68)
- [Test 69: speech_stt_recognize](#test-69)
- [Test 70: speech_stt_recognize](#test-70)
- [Test 71: speech_tts_synthesize](#test-71)
- [Test 72: speech_tts_synthesize](#test-72)
- [Test 73: speech_tts_synthesize](#test-73)
- [Test 74: speech_tts_synthesize](#test-74)
- [Test 75: speech_tts_synthesize](#test-75)
- [Test 76: speech_tts_synthesize](#test-76)
- [Test 77: speech_tts_synthesize](#test-77)
- [Test 78: speech_tts_synthesize](#test-78)
- [Test 79: speech_tts_synthesize](#test-79)
- [Test 80: speech_tts_synthesize](#test-80)
- [Test 81: appconfig_account_list](#test-81)
- [Test 82: appconfig_account_list](#test-82)
- [Test 83: appconfig_account_list](#test-83)
- [Test 84: appconfig_kv_delete](#test-84)
- [Test 85: appconfig_kv_get](#test-85)
- [Test 86: appconfig_kv_get](#test-86)
- [Test 87: appconfig_kv_get](#test-87)
- [Test 88: appconfig_kv_get](#test-88)
- [Test 89: appconfig_kv_lock_set](#test-89)
- [Test 90: appconfig_kv_lock_set](#test-90)
- [Test 91: appconfig_kv_set](#test-91)
- [Test 92: applens_resource_diagnose](#test-92)
- [Test 93: applens_resource_diagnose](#test-93)
- [Test 94: applens_resource_diagnose](#test-94)
- [Test 95: appservice_database_add](#test-95)
- [Test 96: appservice_database_add](#test-96)
- [Test 97: appservice_database_add](#test-97)
- [Test 98: appservice_database_add](#test-98)
- [Test 99: appservice_database_add](#test-99)
- [Test 100: appservice_database_add](#test-100)
- [Test 101: appservice_database_add](#test-101)
- [Test 102: appservice_database_add](#test-102)
- [Test 103: appservice_database_add](#test-103)
- [Test 104: appservice_database_add](#test-104)
- [Test 105: applicationinsights_recommendation_list](#test-105)
- [Test 106: applicationinsights_recommendation_list](#test-106)
- [Test 107: applicationinsights_recommendation_list](#test-107)
- [Test 108: applicationinsights_recommendation_list](#test-108)
- [Test 109: extension_cli_generate](#test-109)
- [Test 110: extension_cli_generate](#test-110)
- [Test 111: extension_cli_generate](#test-111)
- [Test 112: extension_cli_install](#test-112)
- [Test 113: extension_cli_install](#test-113)
- [Test 114: extension_cli_install](#test-114)
- [Test 115: acr_registry_list](#test-115)
- [Test 116: acr_registry_list](#test-116)
- [Test 117: acr_registry_list](#test-117)
- [Test 118: acr_registry_list](#test-118)
- [Test 119: acr_registry_list](#test-119)
- [Test 120: acr_registry_repository_list](#test-120)
- [Test 121: acr_registry_repository_list](#test-121)
- [Test 122: acr_registry_repository_list](#test-122)
- [Test 123: acr_registry_repository_list](#test-123)
- [Test 124: communication_email_send](#test-124)
- [Test 125: communication_email_send](#test-125)
- [Test 126: communication_email_send](#test-126)
- [Test 127: communication_email_send](#test-127)
- [Test 128: communication_email_send](#test-128)
- [Test 129: communication_email_send](#test-129)
- [Test 130: communication_email_send](#test-130)
- [Test 131: communication_email_send](#test-131)
- [Test 132: communication_sms_send](#test-132)
- [Test 133: communication_sms_send](#test-133)
- [Test 134: communication_sms_send](#test-134)
- [Test 135: communication_sms_send](#test-135)
- [Test 136: communication_sms_send](#test-136)
- [Test 137: communication_sms_send](#test-137)
- [Test 138: communication_sms_send](#test-138)
- [Test 139: communication_sms_send](#test-139)
- [Test 140: confidentialledger_entries_append](#test-140)
- [Test 141: confidentialledger_entries_append](#test-141)
- [Test 142: confidentialledger_entries_append](#test-142)
- [Test 143: confidentialledger_entries_append](#test-143)
- [Test 144: confidentialledger_entries_append](#test-144)
- [Test 145: confidentialledger_entries_get](#test-145)
- [Test 146: confidentialledger_entries_get](#test-146)
- [Test 147: cosmos_account_list](#test-147)
- [Test 148: cosmos_account_list](#test-148)
- [Test 149: cosmos_account_list](#test-149)
- [Test 150: cosmos_database_container_item_query](#test-150)
- [Test 151: cosmos_database_container_list](#test-151)
- [Test 152: cosmos_database_container_list](#test-152)
- [Test 153: cosmos_database_list](#test-153)
- [Test 154: cosmos_database_list](#test-154)
- [Test 155: kusto_cluster_get](#test-155)
- [Test 156: kusto_cluster_list](#test-156)
- [Test 157: kusto_cluster_list](#test-157)
- [Test 158: kusto_cluster_list](#test-158)
- [Test 159: kusto_database_list](#test-159)
- [Test 160: kusto_database_list](#test-160)
- [Test 161: kusto_query](#test-161)
- [Test 162: kusto_sample](#test-162)
- [Test 163: kusto_table_list](#test-163)
- [Test 164: kusto_table_list](#test-164)
- [Test 165: kusto_table_schema](#test-165)
- [Test 166: mysql_database_list](#test-166)
- [Test 167: mysql_database_list](#test-167)
- [Test 168: mysql_database_query](#test-168)
- [Test 169: mysql_server_config_get](#test-169)
- [Test 170: mysql_server_list](#test-170)
- [Test 171: mysql_server_list](#test-171)
- [Test 172: mysql_server_list](#test-172)
- [Test 173: mysql_server_param_get](#test-173)
- [Test 174: mysql_server_param_set](#test-174)
- [Test 175: mysql_table_list](#test-175)
- [Test 176: mysql_table_list](#test-176)
- [Test 177: mysql_table_schema_get](#test-177)
- [Test 178: postgres_database_list](#test-178)
- [Test 179: postgres_database_list](#test-179)
- [Test 180: postgres_database_query](#test-180)
- [Test 181: postgres_server_config_get](#test-181)
- [Test 182: postgres_server_list](#test-182)
- [Test 183: postgres_server_list](#test-183)
- [Test 184: postgres_server_list](#test-184)
- [Test 185: postgres_server_param_get](#test-185)
- [Test 186: postgres_server_param_set](#test-186)
- [Test 187: postgres_table_list](#test-187)
- [Test 188: postgres_table_list](#test-188)
- [Test 189: postgres_table_schema_get](#test-189)
- [Test 190: deploy_app_logs_get](#test-190)
- [Test 191: deploy_architecture_diagram_generate](#test-191)
- [Test 192: deploy_iac_rules_get](#test-192)
- [Test 193: deploy_pipeline_guidance_get](#test-193)
- [Test 194: deploy_plan_get](#test-194)
- [Test 195: eventgrid_events_publish](#test-195)
- [Test 196: eventgrid_events_publish](#test-196)
- [Test 197: eventgrid_events_publish](#test-197)
- [Test 198: eventgrid_topic_list](#test-198)
- [Test 199: eventgrid_topic_list](#test-199)
- [Test 200: eventgrid_topic_list](#test-200)
- [Test 201: eventgrid_topic_list](#test-201)
- [Test 202: eventgrid_subscription_list](#test-202)
- [Test 203: eventgrid_subscription_list](#test-203)
- [Test 204: eventgrid_subscription_list](#test-204)
- [Test 205: eventgrid_subscription_list](#test-205)
- [Test 206: eventgrid_subscription_list](#test-206)
- [Test 207: eventgrid_subscription_list](#test-207)
- [Test 208: eventgrid_subscription_list](#test-208)
- [Test 209: eventhubs_eventhub_consumergroup_delete](#test-209)
- [Test 210: eventhubs_eventhub_consumergroup_get](#test-210)
- [Test 211: eventhubs_eventhub_consumergroup_get](#test-211)
- [Test 212: eventhubs_eventhub_consumergroup_update](#test-212)
- [Test 213: eventhubs_eventhub_consumergroup_update](#test-213)
- [Test 214: eventhubs_eventhub_delete](#test-214)
- [Test 215: eventhubs_eventhub_get](#test-215)
- [Test 216: eventhubs_eventhub_get](#test-216)
- [Test 217: eventhubs_eventhub_update](#test-217)
- [Test 218: eventhubs_eventhub_update](#test-218)
- [Test 219: eventhubs_namespace_delete](#test-219)
- [Test 220: eventhubs_namespace_get](#test-220)
- [Test 221: eventhubs_namespace_get](#test-221)
- [Test 222: eventhubs_namespace_update](#test-222)
- [Test 223: eventhubs_namespace_update](#test-223)
- [Test 224: functionapp_get](#test-224)
- [Test 225: functionapp_get](#test-225)
- [Test 226: functionapp_get](#test-226)
- [Test 227: functionapp_get](#test-227)
- [Test 228: functionapp_get](#test-228)
- [Test 229: functionapp_get](#test-229)
- [Test 230: functionapp_get](#test-230)
- [Test 231: functionapp_get](#test-231)
- [Test 232: functionapp_get](#test-232)
- [Test 233: functionapp_get](#test-233)
- [Test 234: functionapp_get](#test-234)
- [Test 235: functionapp_get](#test-235)
- [Test 236: keyvault_admin_settings_get](#test-236)
- [Test 237: keyvault_admin_settings_get](#test-237)
- [Test 238: keyvault_admin_settings_get](#test-238)
- [Test 239: keyvault_certificate_create](#test-239)
- [Test 240: keyvault_certificate_create](#test-240)
- [Test 241: keyvault_certificate_create](#test-241)
- [Test 242: keyvault_certificate_create](#test-242)
- [Test 243: keyvault_certificate_create](#test-243)
- [Test 244: keyvault_certificate_get](#test-244)
- [Test 245: keyvault_certificate_get](#test-245)
- [Test 246: keyvault_certificate_get](#test-246)
- [Test 247: keyvault_certificate_get](#test-247)
- [Test 248: keyvault_certificate_get](#test-248)
- [Test 249: keyvault_certificate_import](#test-249)
- [Test 250: keyvault_certificate_import](#test-250)
- [Test 251: keyvault_certificate_import](#test-251)
- [Test 252: keyvault_certificate_import](#test-252)
- [Test 253: keyvault_certificate_import](#test-253)
- [Test 254: keyvault_certificate_list](#test-254)
- [Test 255: keyvault_certificate_list](#test-255)
- [Test 256: keyvault_certificate_list](#test-256)
- [Test 257: keyvault_certificate_list](#test-257)
- [Test 258: keyvault_certificate_list](#test-258)
- [Test 259: keyvault_certificate_list](#test-259)
- [Test 260: keyvault_key_create](#test-260)
- [Test 261: keyvault_key_create](#test-261)
- [Test 262: keyvault_key_create](#test-262)
- [Test 263: keyvault_key_create](#test-263)
- [Test 264: keyvault_key_create](#test-264)
- [Test 265: keyvault_key_get](#test-265)
- [Test 266: keyvault_key_get](#test-266)
- [Test 267: keyvault_key_get](#test-267)
- [Test 268: keyvault_key_get](#test-268)
- [Test 269: keyvault_key_get](#test-269)
- [Test 270: keyvault_key_list](#test-270)
- [Test 271: keyvault_key_list](#test-271)
- [Test 272: keyvault_key_list](#test-272)
- [Test 273: keyvault_key_list](#test-273)
- [Test 274: keyvault_key_list](#test-274)
- [Test 275: keyvault_key_list](#test-275)
- [Test 276: keyvault_secret_create](#test-276)
- [Test 277: keyvault_secret_create](#test-277)
- [Test 278: keyvault_secret_create](#test-278)
- [Test 279: keyvault_secret_create](#test-279)
- [Test 280: keyvault_secret_create](#test-280)
- [Test 281: keyvault_secret_get](#test-281)
- [Test 282: keyvault_secret_get](#test-282)
- [Test 283: keyvault_secret_get](#test-283)
- [Test 284: keyvault_secret_get](#test-284)
- [Test 285: keyvault_secret_get](#test-285)
- [Test 286: keyvault_secret_list](#test-286)
- [Test 287: keyvault_secret_list](#test-287)
- [Test 288: keyvault_secret_list](#test-288)
- [Test 289: keyvault_secret_list](#test-289)
- [Test 290: keyvault_secret_list](#test-290)
- [Test 291: keyvault_secret_list](#test-291)
- [Test 292: aks_cluster_get](#test-292)
- [Test 293: aks_cluster_get](#test-293)
- [Test 294: aks_cluster_get](#test-294)
- [Test 295: aks_cluster_get](#test-295)
- [Test 296: aks_cluster_get](#test-296)
- [Test 297: aks_cluster_get](#test-297)
- [Test 298: aks_cluster_get](#test-298)
- [Test 299: aks_nodepool_get](#test-299)
- [Test 300: aks_nodepool_get](#test-300)
- [Test 301: aks_nodepool_get](#test-301)
- [Test 302: aks_nodepool_get](#test-302)
- [Test 303: aks_nodepool_get](#test-303)
- [Test 304: aks_nodepool_get](#test-304)
- [Test 305: loadtesting_test_create](#test-305)
- [Test 306: loadtesting_test_get](#test-306)
- [Test 307: loadtesting_testresource_create](#test-307)
- [Test 308: loadtesting_testresource_list](#test-308)
- [Test 309: loadtesting_testrun_create](#test-309)
- [Test 310: loadtesting_testrun_get](#test-310)
- [Test 311: loadtesting_testrun_list](#test-311)
- [Test 312: loadtesting_testrun_update](#test-312)
- [Test 313: grafana_list](#test-313)
- [Test 314: managedlustre_fs_create](#test-314)
- [Test 315: managedlustre_fs_list](#test-315)
- [Test 316: managedlustre_fs_list](#test-316)
- [Test 317: managedlustre_fs_sku_get](#test-317)
- [Test 318: managedlustre_fs_subnetsize_ask](#test-318)
- [Test 319: managedlustre_fs_subnetsize_validate](#test-319)
- [Test 320: managedlustre_fs_update](#test-320)
- [Test 321: marketplace_product_get](#test-321)
- [Test 322: marketplace_product_list](#test-322)
- [Test 323: marketplace_product_list](#test-323)
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

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Get best practices for building AI applications in Azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665808 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.555594 | `get_bestpractices_get` | ❌ |
| 3 | 0.501207 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.480242 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.477596 | `cloudarchitect_design` | ❌ |

---

## Test 2

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Show me the best practices for Azure AI Foundry agents code generation  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685572 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.603773 | `foundry_agents_get-sdk-sample` | ❌ |
| 3 | 0.534202 | `get_bestpractices_get` | ❌ |
| 4 | 0.520223 | `foundry_agents_list` | ❌ |
| 5 | 0.508727 | `azureterraformbestpractices_get` | ❌ |

---

## Test 3

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Get guidance for building agents with Azure AI Foundry  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628010 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.593216 | `foundry_agents_get-sdk-sample` | ❌ |
| 3 | 0.553662 | `foundry_agents_list` | ❌ |
| 4 | 0.534292 | `foundry_agents_create` | ❌ |
| 5 | 0.513217 | `foundry_agents_connect` | ❌ |

---

## Test 4

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Create an AI app that helps me to manage travel queries.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.421728 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.343877 | `foundry_threads_create` | ❌ |
| 3 | 0.327503 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.320532 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.311958 | `foundry_agents_connect` | ❌ |

---

## Test 5

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Create an AI app that helps me to manage travel queries in Azure AI Foundry  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520105 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.478745 | `foundry_openai_embeddings-create` | ❌ |
| 3 | 0.469654 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.466216 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.456701 | `foundry_resource_get` | ❌ |

---

## Test 6

**Expected Tool:** `foundry_agents_connect`  
**Prompt:** Query an agent in my Azure AI foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.705410 | `foundry_agents_connect` | ✅ **EXPECTED** |
| 2 | 0.663568 | `foundry_agents_list` | ❌ |
| 3 | 0.617203 | `foundry_resource_get` | ❌ |
| 4 | 0.548108 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.547990 | `foundry_openai_models-list` | ❌ |

---

## Test 7

**Expected Tool:** `foundry_agents_create`  
**Prompt:** Create a new Azure AI Foundry agent using instructions in the active editor  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587092 | `foundry_agents_create` | ✅ **EXPECTED** |
| 2 | 0.562087 | `foundry_agents_get-sdk-sample` | ❌ |
| 3 | 0.554032 | `foundry_threads_create` | ❌ |
| 4 | 0.525727 | `foundry_models_deploy` | ❌ |
| 5 | 0.525615 | `foundry_agents_list` | ❌ |

---

## Test 8

**Expected Tool:** `foundry_agents_evaluate`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.544111 | `foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.469443 | `foundry_agents_evaluate` | ✅ **EXPECTED** |
| 3 | 0.445964 | `foundry_agents_connect` | ❌ |
| 4 | 0.298035 | `foundry_threads_list` | ❌ |
| 5 | 0.278920 | `foundry_agents_list` | ❌ |

---

## Test 9

**Expected Tool:** `foundry_agents_get-sdk-sample`  
**Prompt:** Create a CLI app that can talk to an Azure AI Foundry Agent using Python SDK  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595766 | `foundry_agents_get-sdk-sample` | ✅ **EXPECTED** |
| 2 | 0.552180 | `foundry_threads_create` | ❌ |
| 3 | 0.521920 | `foundry_agents_connect` | ❌ |
| 4 | 0.518663 | `foundry_agents_create` | ❌ |
| 5 | 0.509764 | `foundry_agents_list` | ❌ |

---

## Test 10

**Expected Tool:** `foundry_agents_list`  
**Prompt:** List all agents in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.797877 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.666025 | `foundry_resource_get` | ❌ |
| 3 | 0.654131 | `foundry_openai_models-list` | ❌ |
| 4 | 0.647274 | `foundry_threads_list` | ❌ |
| 5 | 0.575553 | `foundry_models_deployments_list` | ❌ |

---

## Test 11

**Expected Tool:** `foundry_agents_list`  
**Prompt:** Show me the available agents in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.749879 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.630433 | `foundry_resource_get` | ❌ |
| 3 | 0.611856 | `foundry_openai_models-list` | ❌ |
| 4 | 0.603791 | `foundry_threads_list` | ❌ |
| 5 | 0.557032 | `foundry_agents_get-sdk-sample` | ❌ |

---

## Test 12

**Expected Tool:** `foundry_agents_query-and-evaluate`  
**Prompt:** Query and evaluate an agent in my Azure AI Foundry resource for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.652203 | `foundry_agents_connect` | ❌ |
| 2 | 0.570759 | `foundry_agents_list` | ❌ |
| 3 | 0.553279 | `foundry_agents_query-and-evaluate` | ✅ **EXPECTED** |
| 4 | 0.493791 | `foundry_agents_evaluate` | ❌ |
| 5 | 0.469538 | `foundry_threads_list` | ❌ |

---

## Test 13

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.703647 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.537700 | `foundry_agents_list` | ❌ |
| 3 | 0.526649 | `foundry_knowledge_index_schema` | ❌ |
| 4 | 0.500820 | `foundry_threads_list` | ❌ |
| 5 | 0.475802 | `foundry_models_deployments_list` | ❌ |

---

## Test 14

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** Show me the knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615315 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.489404 | `foundry_knowledge_index_schema` | ❌ |
| 3 | 0.484466 | `foundry_agents_list` | ❌ |
| 4 | 0.454241 | `foundry_threads_list` | ❌ |
| 5 | 0.441496 | `foundry_resource_get` | ❌ |

---

## Test 15

**Expected Tool:** `foundry_knowledge_index_schema`  
**Prompt:** Show me the schema for knowledge index <index-name> in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739976 | `foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.589547 | `foundry_knowledge_index_list` | ❌ |
| 3 | 0.493953 | `foundry_resource_get` | ❌ |
| 4 | 0.490980 | `search_index_get` | ❌ |
| 5 | 0.490048 | `search_knowledge_base_get` | ❌ |

---

## Test 16

**Expected Tool:** `foundry_knowledge_index_schema`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650200 | `foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.432824 | `postgres_table_schema_get` | ❌ |
| 3 | 0.416232 | `kusto_table_schema` | ❌ |
| 4 | 0.398281 | `mysql_table_schema_get` | ❌ |
| 5 | 0.396168 | `foundry_knowledge_index_list` | ❌ |

---

## Test 17

**Expected Tool:** `foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.562920 | `foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.300030 | `foundry_openai_models-list` | ❌ |
| 3 | 0.298490 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.293459 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.290381 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 18

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.681385 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.674466 | `foundry_openai_models-list` | ❌ |
| 3 | 0.572607 | `foundry_threads_list` | ❌ |
| 4 | 0.569058 | `foundry_agents_list` | ❌ |
| 5 | 0.566254 | `foundry_resource_get` | ❌ |

---

## Test 19

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** Show me all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620173 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.619188 | `foundry_openai_models-list` | ❌ |
| 3 | 0.543332 | `foundry_resource_get` | ❌ |
| 4 | 0.540551 | `foundry_agents_list` | ❌ |
| 5 | 0.527113 | `foundry_threads_list` | ❌ |

---

## Test 20

**Expected Tool:** `foundry_models_list`  
**Prompt:** List all AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603372 | `foundry_openai_models-list` | ❌ |
| 2 | 0.560022 | `foundry_models_list` | ✅ **EXPECTED** |
| 3 | 0.553636 | `foundry_threads_list` | ❌ |
| 4 | 0.537980 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.519472 | `foundry_agents_list` | ❌ |

---

## Test 21

**Expected Tool:** `foundry_models_list`  
**Prompt:** Show me the available AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576963 | `foundry_openai_models-list` | ❌ |
| 2 | 0.574938 | `foundry_models_list` | ✅ **EXPECTED** |
| 3 | 0.525455 | `foundry_resource_get` | ❌ |
| 4 | 0.521506 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.518071 | `foundry_models_deployments_list` | ❌ |

---

## Test 22

**Expected Tool:** `foundry_openai_chat-completions-create`  
**Prompt:** Create a chat completion with the message "Hello, how are you today?" using my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641293 | `foundry_openai_chat-completions-create` | ✅ **EXPECTED** |
| 2 | 0.546736 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.420044 | `foundry_threads_create` | ❌ |
| 4 | 0.415482 | `foundry_agents_connect` | ❌ |
| 5 | 0.400827 | `azureaibestpractices_get` | ❌ |

---

## Test 23

**Expected Tool:** `foundry_openai_create-completion`  
**Prompt:** Create a completion with the prompt "What is Azure?" using my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696897 | `foundry_openai_create-completion` | ✅ **EXPECTED** |
| 2 | 0.579179 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.470313 | `azureaibestpractices_get` | ❌ |
| 4 | 0.464301 | `foundry_models_deploy` | ❌ |
| 5 | 0.460325 | `foundry_resource_get` | ❌ |

---

## Test 24

**Expected Tool:** `foundry_openai_embeddings-create`  
**Prompt:** Generate embeddings for the text "Azure OpenAI Service" using my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.766338 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.543339 | `foundry_models_deploy` | ❌ |
| 3 | 0.542214 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.520797 | `foundry_openai_models-list` | ❌ |
| 5 | 0.519385 | `foundry_resource_get` | ❌ |

---

## Test 25

**Expected Tool:** `foundry_openai_embeddings-create`  
**Prompt:** Create vector embeddings for my text using my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.724120 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.494520 | `foundry_resource_get` | ❌ |
| 3 | 0.480296 | `foundry_models_deploy` | ❌ |
| 4 | 0.480218 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.463797 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 26

**Expected Tool:** `foundry_openai_models-list`  
**Prompt:** List all available OpenAI models in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.799070 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.668910 | `foundry_resource_get` | ❌ |
| 3 | 0.667041 | `foundry_models_list` | ❌ |
| 4 | 0.666208 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.657545 | `foundry_agents_list` | ❌ |

---

## Test 27

**Expected Tool:** `foundry_openai_models-list`  
**Prompt:** Show me the OpenAI model deployments in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.741683 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.660160 | `foundry_models_deployments_list` | ❌ |
| 3 | 0.648207 | `foundry_resource_get` | ❌ |
| 4 | 0.640650 | `foundry_models_deploy` | ❌ |
| 5 | 0.619878 | `foundry_agents_list` | ❌ |

---

## Test 28

**Expected Tool:** `foundry_resource_get`  
**Prompt:** List all AI Foundry resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594148 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.571820 | `foundry_openai_models-list` | ❌ |
| 3 | 0.567019 | `foundry_agents_list` | ❌ |
| 4 | 0.558066 | `foundry_threads_list` | ❌ |
| 5 | 0.556154 | `search_service_list` | ❌ |

---

## Test 29

**Expected Tool:** `foundry_resource_get`  
**Prompt:** Show me the AI Foundry resources in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665359 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.585256 | `foundry_openai_models-list` | ❌ |
| 3 | 0.553993 | `foundry_agents_list` | ❌ |
| 4 | 0.518767 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.492911 | `foundry_models_deploy` | ❌ |

---

## Test 30

**Expected Tool:** `foundry_resource_get`  
**Prompt:** Get details for AI Foundry resource <resource_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.735375 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.571882 | `foundry_openai_models-list` | ❌ |
| 3 | 0.510124 | `monitor_webtests_get` | ❌ |
| 4 | 0.497090 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.475722 | `foundry_agents_list` | ❌ |

---

## Test 31

**Expected Tool:** `foundry_threads_create`  
**Prompt:** Create an Azure AI Foundry thread to hold the conversation  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606769 | `foundry_threads_create` | ✅ **EXPECTED** |
| 2 | 0.528321 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.519630 | `foundry_threads_get-messages` | ❌ |
| 4 | 0.506111 | `foundry_threads_list` | ❌ |
| 5 | 0.490812 | `foundry_models_deploy` | ❌ |

---

## Test 32

**Expected Tool:** `foundry_threads_get-messages`  
**Prompt:** Show me the messages in the AI Foundry thread with id <thread_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669937 | `foundry_threads_get-messages` | ✅ **EXPECTED** |
| 2 | 0.584421 | `foundry_threads_create` | ❌ |
| 3 | 0.529366 | `foundry_threads_list` | ❌ |
| 4 | 0.437480 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.427913 | `foundry_agents_create` | ❌ |

---

## Test 33

**Expected Tool:** `foundry_threads_list`  
**Prompt:** List my AI Foundry threads  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.677270 | `foundry_threads_list` | ✅ **EXPECTED** |
| 2 | 0.574068 | `foundry_threads_get-messages` | ❌ |
| 3 | 0.566981 | `foundry_threads_create` | ❌ |
| 4 | 0.471544 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.448963 | `foundry_agents_list` | ❌ |

---

## Test 34

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.785964 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.700824 | `search_knowledge_source_get` | ❌ |
| 3 | 0.692681 | `search_service_list` | ❌ |
| 4 | 0.635863 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.586336 | `search_index_get` | ❌ |

---

## Test 35

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.748319 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.668487 | `search_knowledge_source_get` | ❌ |
| 3 | 0.628582 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.623715 | `search_service_list` | ❌ |
| 5 | 0.566436 | `search_index_get` | ❌ |

---

## Test 36

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702672 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.606001 | `search_knowledge_source_get` | ❌ |
| 3 | 0.583304 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.512835 | `search_service_list` | ❌ |
| 5 | 0.476837 | `foundry_knowledge_index_list` | ❌ |

---

## Test 37

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687794 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.599247 | `search_knowledge_source_get` | ❌ |
| 3 | 0.578499 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.456712 | `search_service_list` | ❌ |
| 5 | 0.439487 | `foundry_knowledge_index_list` | ❌ |

---

## Test 38

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Get the details of knowledge base <agent-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769655 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.685640 | `search_knowledge_source_get` | ❌ |
| 3 | 0.636958 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.586059 | `search_index_get` | ❌ |
| 5 | 0.533298 | `search_service_list` | ❌ |

---

## Test 39

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595280 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.551922 | `search_knowledge_base_retrieve` | ❌ |
| 3 | 0.515480 | `search_knowledge_source_get` | ❌ |
| 4 | 0.366170 | `search_service_list` | ❌ |
| 5 | 0.365288 | `foundry_knowledge_index_list` | ❌ |

---

## Test 40

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in Azure AI Search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.724848 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.650626 | `search_knowledge_base_get` | ❌ |
| 3 | 0.575235 | `search_index_query` | ❌ |
| 4 | 0.567343 | `search_knowledge_source_get` | ❌ |
| 5 | 0.520315 | `foundry_agents_connect` | ❌ |

---

## Test 41

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633766 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.589861 | `search_knowledge_base_get` | ❌ |
| 3 | 0.502085 | `search_knowledge_source_get` | ❌ |
| 4 | 0.422574 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.399595 | `search_index_query` | ❌ |

---

## Test 42

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657866 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.557054 | `search_knowledge_base_get` | ❌ |
| 3 | 0.463605 | `search_knowledge_source_get` | ❌ |
| 4 | 0.436668 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.422173 | `foundry_agents_connect` | ❌ |

---

## Test 43

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633766 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.589861 | `search_knowledge_base_get` | ❌ |
| 3 | 0.502085 | `search_knowledge_source_get` | ❌ |
| 4 | 0.422574 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.399595 | `search_index_query` | ❌ |

---

## Test 44

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Query knowledge base <agent-name> in search service <service-name> about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.598868 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.548151 | `search_knowledge_base_get` | ❌ |
| 3 | 0.467831 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.464904 | `search_knowledge_source_get` | ❌ |
| 5 | 0.412481 | `foundry_agents_connect` | ❌ |

---

## Test 45

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Search knowledge base <agent-name> in Azure AI Search service <service-name> for <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649767 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.631773 | `search_knowledge_base_get` | ❌ |
| 3 | 0.581386 | `search_index_query` | ❌ |
| 4 | 0.571156 | `search_knowledge_source_get` | ❌ |
| 5 | 0.544545 | `search_service_list` | ❌ |

---

## Test 46

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** What does knowledge base <agent-name> in search service <service-name> know about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579716 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.560256 | `search_knowledge_base_get` | ❌ |
| 3 | 0.477941 | `search_knowledge_source_get` | ❌ |
| 4 | 0.402508 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.361175 | `foundry_knowledge_index_list` | ❌ |

---

## Test 47

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Find information about <query> using knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581877 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.527883 | `search_knowledge_base_get` | ❌ |
| 3 | 0.449111 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.448643 | `search_knowledge_source_get` | ❌ |
| 5 | 0.398352 | `foundry_agents_connect` | ❌ |

---

## Test 48

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.760416 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.690829 | `search_service_list` | ❌ |
| 3 | 0.665958 | `search_knowledge_base_get` | ❌ |
| 4 | 0.572490 | `search_index_get` | ❌ |
| 5 | 0.560779 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 49

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737860 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.659236 | `search_service_list` | ❌ |
| 3 | 0.653098 | `search_knowledge_base_get` | ❌ |
| 4 | 0.578438 | `search_index_get` | ❌ |
| 5 | 0.560564 | `search_index_query` | ❌ |

---

## Test 50

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657936 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.558235 | `search_knowledge_base_get` | ❌ |
| 3 | 0.510338 | `search_service_list` | ❌ |
| 4 | 0.470560 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.433613 | `foundry_knowledge_index_list` | ❌ |

---

## Test 51

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650726 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.561117 | `search_knowledge_base_get` | ❌ |
| 3 | 0.484644 | `search_service_list` | ❌ |
| 4 | 0.475892 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.428342 | `search_index_get` | ❌ |

---

## Test 52

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Get the details of knowledge source <source-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.825604 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.693785 | `search_knowledge_base_get` | ❌ |
| 3 | 0.595601 | `search_index_get` | ❌ |
| 4 | 0.540550 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.531085 | `search_service_list` | ❌ |

---

## Test 53

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge source <source-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630840 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.523349 | `search_knowledge_base_get` | ❌ |
| 3 | 0.459923 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.370881 | `search_index_get` | ❌ |
| 5 | 0.370585 | `search_service_list` | ❌ |

---

## Test 54

**Expected Tool:** `search_index_get`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680195 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.544598 | `foundry_knowledge_index_schema` | ❌ |
| 3 | 0.529369 | `search_knowledge_base_get` | ❌ |
| 4 | 0.521765 | `search_knowledge_source_get` | ❌ |
| 5 | 0.490553 | `search_service_list` | ❌ |

---

## Test 55

**Expected Tool:** `search_index_get`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639478 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.619937 | `search_service_list` | ❌ |
| 3 | 0.538930 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.512328 | `search_knowledge_base_get` | ❌ |
| 5 | 0.496039 | `search_knowledge_source_get` | ❌ |

---

## Test 56

**Expected Tool:** `search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620259 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.562503 | `search_service_list` | ❌ |
| 3 | 0.538472 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.501249 | `search_knowledge_base_get` | ❌ |
| 5 | 0.490025 | `search_knowledge_source_get` | ❌ |

---

## Test 57

**Expected Tool:** `search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522060 | `search_index_get` | ❌ |
| 2 | 0.516143 | `search_index_query` | ✅ **EXPECTED** |
| 3 | 0.498511 | `search_service_list` | ❌ |
| 4 | 0.448093 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.438059 | `postgres_database_query` | ❌ |

---

## Test 58

**Expected Tool:** `search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.791678 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.552939 | `kusto_cluster_list` | ❌ |
| 3 | 0.509527 | `subscription_list` | ❌ |
| 4 | 0.505141 | `search_index_get` | ❌ |
| 5 | 0.504633 | `marketplace_product_list` | ❌ |

---

## Test 59

**Expected Tool:** `search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684875 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.484121 | `marketplace_product_list` | ❌ |
| 3 | 0.479320 | `search_index_get` | ❌ |
| 4 | 0.463147 | `search_knowledge_base_get` | ❌ |
| 5 | 0.461826 | `kusto_cluster_list` | ❌ |

---

## Test 60

**Expected Tool:** `search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551312 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.435713 | `search_index_get` | ❌ |
| 3 | 0.416080 | `search_knowledge_base_get` | ❌ |
| 4 | 0.410508 | `search_knowledge_source_get` | ❌ |
| 5 | 0.404774 | `search_index_query` | ❌ |

---

## Test 61

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

## Test 62

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

## Test 63

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe speech from audio file <file_path> with profanity filtering  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.486489 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.335116 | `speech_tts_synthesize` | ❌ |
| 3 | 0.162869 | `foundry_threads_create` | ❌ |
| 4 | 0.160209 | `foundry_agents_connect` | ❌ |
| 5 | 0.156850 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 64

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text from audio file <file_path> using endpoint <endpoint>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611992 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.573185 | `speech_tts_synthesize` | ❌ |
| 3 | 0.309895 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.244229 | `foundry_resource_get` | ❌ |
| 5 | 0.243626 | `foundry_openai_create-completion` | ❌ |

---

## Test 65

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

## Test 66

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text with detailed output format from audio file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546259 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.480203 | `speech_tts_synthesize` | ❌ |
| 3 | 0.218067 | `foundry_resource_get` | ❌ |
| 4 | 0.202935 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.183943 | `extension_azqr` | ❌ |

---

## Test 67

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

## Test 68

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe audio using multiple phrase hints: "Azure", "cognitive services", "machine learning"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549151 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.468161 | `speech_tts_synthesize` | ❌ |
| 3 | 0.390804 | `azureaibestpractices_get` | ❌ |
| 4 | 0.342537 | `extension_cli_generate` | ❌ |
| 5 | 0.337387 | `cloudarchitect_design` | ❌ |

---

## Test 69

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text with comma-separated phrase hints: "Azure, cognitive services, API"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532536 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.515532 | `speech_tts_synthesize` | ❌ |
| 3 | 0.349892 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.348088 | `azureaibestpractices_get` | ❌ |
| 5 | 0.340893 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 70

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe audio with raw profanity output from file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.453448 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.322734 | `speech_tts_synthesize` | ❌ |
| 3 | 0.173251 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.165047 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.160561 | `extension_azqr` | ❌ |

---

## Test 71

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

## Test 72

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

## Test 73

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Generate speech audio from text "Hello world" using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592156 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.534001 | `speech_stt_recognize` | ❌ |
| 3 | 0.339580 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.334496 | `azureaibestpractices_get` | ❌ |
| 5 | 0.327397 | `foundry_openai_create-completion` | ❌ |

---

## Test 74

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

## Test 75

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Synthesize speech with voice en-US-JennyNeural from text "Azure AI Services"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604878 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.496715 | `speech_stt_recognize` | ❌ |
| 3 | 0.417045 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.384759 | `azureaibestpractices_get` | ❌ |
| 5 | 0.379840 | `foundry_openai_create-completion` | ❌ |

---

## Test 76

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Create MP3 audio file from text "Welcome to Azure" with high quality format  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560855 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.510234 | `speech_stt_recognize` | ❌ |
| 3 | 0.349902 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.349146 | `azureaibestpractices_get` | ❌ |
| 5 | 0.348210 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 77

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Generate speech with custom voice model using endpoint ID <endpoint-id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527294 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.455734 | `speech_stt_recognize` | ❌ |
| 3 | 0.353109 | `foundry_resource_get` | ❌ |
| 4 | 0.343308 | `foundry_models_deploy` | ❌ |
| 5 | 0.337888 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 78

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

## Test 79

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Synthesize long text content to audio file with streaming  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.428150 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.369095 | `speech_stt_recognize` | ❌ |
| 3 | 0.230763 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.220852 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.216546 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 80

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Create audio file from text in French language with appropriate voice  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.444461 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.385285 | `speech_stt_recognize` | ❌ |
| 3 | 0.229922 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.228346 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.213241 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 81

**Expected Tool:** `appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786279 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.530613 | `appconfig_kv_get` | ❌ |
| 3 | 0.491380 | `postgres_server_list` | ❌ |
| 4 | 0.481223 | `kusto_cluster_list` | ❌ |
| 5 | 0.478286 | `subscription_list` | ❌ |

---

## Test 82

**Expected Tool:** `appconfig_account_list`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634995 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.464865 | `appconfig_kv_get` | ❌ |
| 3 | 0.396765 | `subscription_list` | ❌ |
| 4 | 0.391300 | `redis_list` | ❌ |
| 5 | 0.372456 | `postgres_server_list` | ❌ |

---

## Test 83

**Expected Tool:** `appconfig_account_list`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565371 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.465344 | `appconfig_kv_get` | ❌ |
| 3 | 0.355877 | `postgres_server_config_get` | ❌ |
| 4 | 0.348661 | `appconfig_kv_delete` | ❌ |
| 5 | 0.327234 | `appconfig_kv_set` | ❌ |

---

## Test 84

**Expected Tool:** `appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618215 | `appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.464365 | `appconfig_kv_get` | ❌ |
| 3 | 0.424304 | `appconfig_kv_set` | ❌ |
| 4 | 0.422689 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.392092 | `appconfig_account_list` | ❌ |

---

## Test 85

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.632687 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.557826 | `appconfig_account_list` | ❌ |
| 3 | 0.530884 | `appconfig_kv_set` | ❌ |
| 4 | 0.464635 | `appconfig_kv_delete` | ❌ |
| 5 | 0.439089 | `appconfig_kv_lock_set` | ❌ |

---

## Test 86

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612555 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.522453 | `appconfig_account_list` | ❌ |
| 3 | 0.512945 | `appconfig_kv_set` | ❌ |
| 4 | 0.468503 | `appconfig_kv_delete` | ❌ |
| 5 | 0.457866 | `appconfig_kv_lock_set` | ❌ |

---

## Test 87

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** List all key-value settings with key name starting with 'prod-' in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512883 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.449965 | `appconfig_account_list` | ❌ |
| 3 | 0.398684 | `appconfig_kv_set` | ❌ |
| 4 | 0.380614 | `appconfig_kv_delete` | ❌ |
| 5 | 0.346166 | `appconfig_kv_lock_set` | ❌ |

---

## Test 88

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552300 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.448912 | `appconfig_kv_set` | ❌ |
| 3 | 0.441713 | `appconfig_kv_delete` | ❌ |
| 4 | 0.437442 | `appconfig_account_list` | ❌ |
| 5 | 0.416264 | `appconfig_kv_lock_set` | ❌ |

---

## Test 89

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591237 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.487174 | `appconfig_kv_get` | ❌ |
| 3 | 0.445551 | `appconfig_kv_set` | ❌ |
| 4 | 0.431516 | `appconfig_kv_delete` | ❌ |
| 5 | 0.373540 | `appconfig_account_list` | ❌ |

---

## Test 90

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555699 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.505681 | `appconfig_kv_get` | ❌ |
| 3 | 0.476497 | `appconfig_kv_delete` | ❌ |
| 4 | 0.425488 | `appconfig_kv_set` | ❌ |
| 5 | 0.409377 | `appconfig_account_list` | ❌ |

---

## Test 91

**Expected Tool:** `appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609635 | `appconfig_kv_set` | ✅ **EXPECTED** |
| 2 | 0.536497 | `appconfig_kv_lock_set` | ❌ |
| 3 | 0.512707 | `appconfig_kv_get` | ❌ |
| 4 | 0.505571 | `appconfig_kv_delete` | ❌ |
| 5 | 0.377851 | `appconfig_account_list` | ❌ |

---

## Test 92

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595632 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.336090 | `deploy_app_logs_get` | ❌ |
| 3 | 0.300786 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.273083 | `cloudarchitect_design` | ❌ |
| 5 | 0.254473 | `monitor_resource_log_query` | ❌ |

---

## Test 93

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

## Test 94

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492822 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.256355 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.242652 | `cloudarchitect_design` | ❌ |
| 4 | 0.225682 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.211643 | `deploy_app_logs_get` | ❌ |

---

## Test 95

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection <connection_string> to my app service <app_name> for database <database_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.717887 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.401337 | `sql_db_rename` | ❌ |
| 3 | 0.399820 | `sql_db_create` | ❌ |
| 4 | 0.362889 | `sql_db_show` | ❌ |
| 5 | 0.357806 | `sql_db_list` | ❌ |

---

## Test 96

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure SQL Server database <database_name> for app service <app_name> with connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688241 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.498219 | `sql_db_rename` | ❌ |
| 3 | 0.497577 | `sql_db_create` | ❌ |
| 4 | 0.469579 | `sql_db_show` | ❌ |
| 5 | 0.453150 | `sql_db_list` | ❌ |

---

## Test 97

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add MySQL database <database_name> to app service <app_name> using connection <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.675365 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.465141 | `sql_db_create` | ❌ |
| 3 | 0.452711 | `sql_db_rename` | ❌ |
| 4 | 0.433306 | `mysql_server_list` | ❌ |
| 5 | 0.410242 | `sql_db_show` | ❌ |

---

## Test 98

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add PostgreSQL database <database_name> to app service <app_name> using connection <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627784 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.444152 | `sql_db_create` | ❌ |
| 3 | 0.404874 | `postgres_database_query` | ❌ |
| 4 | 0.401100 | `postgres_database_list` | ❌ |
| 5 | 0.400754 | `sql_db_rename` | ❌ |

---

## Test 99

**Expected Tool:** `appservice_database_add`  
**Prompt:** Connect CosmosDB database <database_name> using connection string <connection_string> to app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663001 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.446465 | `cosmos_database_list` | ❌ |
| 3 | 0.441966 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.427284 | `cosmos_database_container_list` | ❌ |
| 5 | 0.420445 | `sql_db_rename` | ❌ |

---

## Test 100

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection <connection_string> for database <database_name> on server <database_server> to app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.733775 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.454554 | `sql_db_create` | ❌ |
| 3 | 0.415274 | `sql_db_rename` | ❌ |
| 4 | 0.414045 | `sql_server_create` | ❌ |
| 5 | 0.410260 | `sql_db_list` | ❌ |

---

## Test 101

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection string for <database_name> to app service <app_name> using connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.746370 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.441616 | `sql_db_rename` | ❌ |
| 3 | 0.433929 | `sql_db_create` | ❌ |
| 4 | 0.391303 | `sql_db_list` | ❌ |
| 5 | 0.390203 | `sql_db_show` | ❌ |

---

## Test 102

**Expected Tool:** `appservice_database_add`  
**Prompt:** Connect database <database_name> to my app service <app_name> using connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680080 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.429387 | `sql_db_rename` | ❌ |
| 3 | 0.406085 | `sql_db_create` | ❌ |
| 4 | 0.396828 | `sql_db_show` | ❌ |
| 5 | 0.391408 | `sql_db_list` | ❌ |

---

## Test 103

**Expected Tool:** `appservice_database_add`  
**Prompt:** Set up database <database_name> for app service <app_name> with connection string <connection_string> under resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640268 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.456685 | `sql_db_create` | ❌ |
| 3 | 0.402774 | `sql_db_rename` | ❌ |
| 4 | 0.402139 | `sql_db_show` | ❌ |
| 5 | 0.394363 | `sql_db_list` | ❌ |

---

## Test 104

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure database <database_name> for app service <app_name> with the connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688119 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.448356 | `sql_db_create` | ❌ |
| 3 | 0.448305 | `sql_db_rename` | ❌ |
| 4 | 0.413958 | `sql_db_show` | ❌ |
| 5 | 0.410681 | `sql_db_list` | ❌ |

---

## Test 105

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572473 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.445169 | `azureaibestpractices_get` | ❌ |
| 3 | 0.445157 | `get_bestpractices_get` | ❌ |
| 4 | 0.390478 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.383948 | `applens_resource_diagnose` | ❌ |

---

## Test 106

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** Show me code optimization recommendations for all Application Insights resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696462 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.505528 | `azureaibestpractices_get` | ❌ |
| 3 | 0.468361 | `get_bestpractices_get` | ❌ |
| 4 | 0.452226 | `applens_resource_diagnose` | ❌ |
| 5 | 0.435200 | `azureterraformbestpractices_get` | ❌ |

---

## Test 107

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626722 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.488002 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.479308 | `mysql_server_list` | ❌ |
| 4 | 0.477396 | `applens_resource_diagnose` | ❌ |
| 5 | 0.468847 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 108

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509502 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.429927 | `azureaibestpractices_get` | ❌ |
| 3 | 0.419670 | `applens_resource_diagnose` | ❌ |
| 4 | 0.383767 | `get_bestpractices_get` | ❌ |
| 5 | 0.367278 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 109

**Expected Tool:** `extension_cli_generate`  
**Prompt:** Create a Storage account with name <storage_account_name> using Azure CLI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593241 | `storage_account_create` | ❌ |
| 2 | 0.564940 | `storage_blob_container_create` | ❌ |
| 3 | 0.493653 | `storage_account_get` | ❌ |
| 4 | 0.473547 | `storage_blob_container_get` | ❌ |
| 5 | 0.454194 | `managedlustre_fs_create` | ❌ |

---

## Test 110

**Expected Tool:** `extension_cli_generate`  
**Prompt:** List all virtual machines in my subscription using Azure CLI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591367 | `search_service_list` | ❌ |
| 2 | 0.574330 | `kusto_cluster_list` | ❌ |
| 3 | 0.549135 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.542863 | `monitor_workspace_list` | ❌ |
| 5 | 0.536257 | `subscription_list` | ❌ |

---

## Test 111

**Expected Tool:** `extension_cli_generate`  
**Prompt:** Show me the details of the storage account <account_name> with Azure CLI commands  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710254 | `storage_account_get` | ❌ |
| 2 | 0.601545 | `storage_blob_container_get` | ❌ |
| 3 | 0.543269 | `storage_blob_get` | ❌ |
| 4 | 0.519830 | `storage_account_create` | ❌ |
| 5 | 0.493171 | `cosmos_account_list` | ❌ |

---

## Test 112

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

## Test 113

**Expected Tool:** `extension_cli_install`  
**Prompt:** How to install azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.460416 | `extension_cli_install` | ✅ **EXPECTED** |
| 2 | 0.429600 | `deploy_app_logs_get` | ❌ |
| 3 | 0.365212 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.335279 | `deploy_plan_get` | ❌ |
| 5 | 0.326135 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 114

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

## Test 115

**Expected Tool:** `acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743568 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.711580 | `acr_registry_repository_list` | ❌ |
| 3 | 0.585675 | `kusto_cluster_list` | ❌ |
| 4 | 0.540241 | `search_service_list` | ❌ |
| 5 | 0.514293 | `cosmos_account_list` | ❌ |

---

## Test 116

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586014 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563636 | `acr_registry_repository_list` | ❌ |
| 3 | 0.460834 | `storage_blob_container_get` | ❌ |
| 4 | 0.415552 | `cosmos_database_container_list` | ❌ |
| 5 | 0.402310 | `redis_list` | ❌ |

---

## Test 117

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.637114 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563423 | `acr_registry_repository_list` | ❌ |
| 3 | 0.516725 | `kusto_cluster_list` | ❌ |
| 4 | 0.515377 | `storage_blob_container_get` | ❌ |
| 5 | 0.480297 | `redis_list` | ❌ |

---

## Test 118

**Expected Tool:** `acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654318 | `acr_registry_repository_list` | ❌ |
| 2 | 0.633938 | `acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.475925 | `mysql_server_list` | ❌ |
| 4 | 0.454929 | `group_list` | ❌ |
| 5 | 0.453895 | `datadog_monitoredresources_list` | ❌ |

---

## Test 119

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639391 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.637972 | `acr_registry_repository_list` | ❌ |
| 3 | 0.467956 | `mysql_server_list` | ❌ |
| 4 | 0.449537 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.445741 | `group_list` | ❌ |

---

## Test 120

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626482 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.617504 | `acr_registry_list` | ❌ |
| 3 | 0.544172 | `kusto_cluster_list` | ❌ |
| 4 | 0.508863 | `storage_blob_container_get` | ❌ |
| 5 | 0.495567 | `postgres_server_list` | ❌ |

---

## Test 121

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

## Test 122

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674296 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.541779 | `acr_registry_list` | ❌ |
| 3 | 0.437756 | `storage_blob_container_get` | ❌ |
| 4 | 0.433927 | `cosmos_database_container_list` | ❌ |
| 5 | 0.383159 | `kusto_database_list` | ❌ |

---

## Test 123

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600793 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.501858 | `acr_registry_list` | ❌ |
| 3 | 0.431160 | `storage_blob_container_get` | ❌ |
| 4 | 0.418627 | `cosmos_database_container_list` | ❌ |
| 5 | 0.378215 | `redis_list` | ❌ |

---

## Test 124

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498292 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.229080 | `communication_sms_send` | ❌ |
| 3 | 0.188975 | `eventgrid_events_publish` | ❌ |
| 4 | 0.161191 | `foundry_agents_create` | ❌ |
| 5 | 0.145951 | `servicebus_topic_details` | ❌ |

---

## Test 125

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

## Test 126

**Expected Tool:** `communication_email_send`  
**Prompt:** Send HTML-formatted email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520967 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.207658 | `communication_sms_send` | ❌ |
| 3 | 0.152418 | `eventgrid_events_publish` | ❌ |
| 4 | 0.152013 | `servicebus_topic_details` | ❌ |
| 5 | 0.143671 | `foundry_agents_evaluate` | ❌ |

---

## Test 127

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with CC to <email-address-1> and <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533447 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.219584 | `communication_sms_send` | ❌ |
| 3 | 0.106035 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.103723 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.084905 | `cosmos_account_list` | ❌ |

---

## Test 128

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email to multiple recipients: <email-address-1>, <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.540793 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.244521 | `communication_sms_send` | ❌ |
| 3 | 0.134975 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.114300 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.087035 | `postgres_server_param_set` | ❌ |

---

## Test 129

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with reply-to address set to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512633 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.200154 | `communication_sms_send` | ❌ |
| 3 | 0.164178 | `mysql_server_param_set` | ❌ |
| 4 | 0.158712 | `postgres_server_param_set` | ❌ |
| 5 | 0.143514 | `appconfig_kv_set` | ❌ |

---

## Test 130

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

## Test 131

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email with BCC recipients  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.528789 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.241114 | `communication_sms_send` | ❌ |
| 3 | 0.137538 | `confidentialledger_entries_append` | ❌ |
| 4 | 0.108786 | `confidentialledger_entries_get` | ❌ |
| 5 | 0.105033 | `storage_blob_upload` | ❌ |

---

## Test 132

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send an SMS message to <phone-number> saying "Hello"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533868 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.251429 | `communication_email_send` | ❌ |
| 3 | 0.218656 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.175526 | `foundry_agents_create` | ❌ |
| 5 | 0.166041 | `speech_tts_synthesize` | ❌ |

---

## Test 133

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS to <phone-number-2> from <phone-number-1> with message "Test message"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546019 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.294860 | `communication_email_send` | ❌ |
| 3 | 0.204588 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.200655 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.141089 | `foundry_agents_create` | ❌ |

---

## Test 134

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS to multiple recipients: <phone-number-1>, <phone-number-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545758 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.421945 | `communication_email_send` | ❌ |
| 3 | 0.186018 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.142020 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.113680 | `foundry_threads_get-messages` | ❌ |

---

## Test 135

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS with delivery reporting enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.554908 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.269080 | `communication_email_send` | ❌ |
| 3 | 0.192354 | `extension_azqr` | ❌ |
| 4 | 0.185916 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.170726 | `foundry_agents_query-and-evaluate` | ❌ |

---

## Test 136

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS message with custom tracking tag "campaign1"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.538827 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.269794 | `communication_email_send` | ❌ |
| 3 | 0.188153 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.185403 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.175087 | `foundry_agents_create` | ❌ |

---

## Test 137

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send broadcast SMS to <phone-number-1> and <phone-number-2> saying "Urgent notification"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.474786 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.286338 | `communication_email_send` | ❌ |
| 3 | 0.164293 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.147338 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.128704 | `cosmos_account_list` | ❌ |

---

## Test 138

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS from my communication service to <phone-number-1>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564114 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.302363 | `communication_email_send` | ❌ |
| 3 | 0.238296 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.184246 | `foundry_agents_create` | ❌ |
| 5 | 0.183651 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 139

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send an SMS with delivery receipt tracking  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.598211 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.314134 | `communication_email_send` | ❌ |
| 3 | 0.206925 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.201142 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.187824 | `confidentialledger_entries_append` | ❌ |

---

## Test 140

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Append an entry to my ledger <ledger_name> with data {"key": "value"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510650 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.294679 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.292014 | `appconfig_kv_set` | ❌ |
| 4 | 0.258967 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.249785 | `keyvault_certificate_import` | ❌ |

---

## Test 141

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Write a tamper-proof entry to ledger <ledger_name> containing {"transaction": "data"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602247 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.357443 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.211990 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.195471 | `keyvault_secret_create` | ❌ |
| 5 | 0.183971 | `keyvault_certificate_import` | ❌ |

---

## Test 142

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Append {"hello": "from mcp"} to my confidential ledger <ledger_name> in collection <collection_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546660 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.451733 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.225141 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.215932 | `appconfig_kv_set` | ❌ |
| 5 | 0.203189 | `keyvault_certificate_import` | ❌ |

---

## Test 143

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Create an immutable ledger entry in <ledger_name> with content {"audit": "log"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496023 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.340056 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.218473 | `monitor_activitylog_list` | ❌ |
| 4 | 0.215229 | `storage_blob_container_create` | ❌ |
| 5 | 0.204925 | `monitor_resource_log_query` | ❌ |

---

## Test 144

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Write an entry to confidential ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622138 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.524538 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.252508 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.240252 | `keyvault_secret_create` | ❌ |
| 5 | 0.186890 | `appconfig_kv_set` | ❌ |

---

## Test 145

**Expected Tool:** `confidentialledger_entries_get`  
**Prompt:** Get entry from Confidential Ledger for transaction <transaction_id> on ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.706983 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.551953 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.244600 | `keyvault_secret_get` | ❌ |
| 4 | 0.229943 | `keyvault_key_get` | ❌ |
| 5 | 0.211839 | `loadtesting_testrun_get` | ❌ |

---

## Test 146

**Expected Tool:** `confidentialledger_entries_get`  
**Prompt:** Get transaction <transaction_id> from ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509474 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.416580 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.223959 | `loadtesting_testrun_get` | ❌ |
| 4 | 0.218412 | `monitor_resource_log_query` | ❌ |
| 5 | 0.217712 | `loadtesting_testrun_list` | ❌ |

---

## Test 147

**Expected Tool:** `cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818416 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.668512 | `cosmos_database_list` | ❌ |
| 3 | 0.635365 | `subscription_list` | ❌ |
| 4 | 0.615296 | `cosmos_database_container_list` | ❌ |
| 5 | 0.601484 | `kusto_cluster_list` | ❌ |

---

## Test 148

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665447 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605357 | `cosmos_database_list` | ❌ |
| 3 | 0.571613 | `cosmos_database_container_list` | ❌ |
| 4 | 0.549447 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.503845 | `storage_account_get` | ❌ |

---

## Test 149

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752494 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.606623 | `subscription_list` | ❌ |
| 3 | 0.605125 | `cosmos_database_list` | ❌ |
| 4 | 0.566249 | `cosmos_database_container_list` | ❌ |
| 5 | 0.563922 | `cosmos_database_container_item_query` | ❌ |

---

## Test 150

**Expected Tool:** `cosmos_database_container_item_query`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658766 | `cosmos_database_container_item_query` | ✅ **EXPECTED** |
| 2 | 0.605383 | `cosmos_database_container_list` | ❌ |
| 3 | 0.488450 | `storage_blob_container_get` | ❌ |
| 4 | 0.477982 | `cosmos_database_list` | ❌ |
| 5 | 0.447808 | `cosmos_account_list` | ❌ |

---

## Test 151

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852832 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.681044 | `cosmos_database_list` | ❌ |
| 3 | 0.680762 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.632478 | `storage_blob_container_get` | ❌ |
| 5 | 0.630659 | `cosmos_account_list` | ❌ |

---

## Test 152

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789395 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.648126 | `cosmos_database_container_item_query` | ❌ |
| 3 | 0.614220 | `cosmos_database_list` | ❌ |
| 4 | 0.591350 | `storage_blob_container_get` | ❌ |
| 5 | 0.562062 | `cosmos_account_list` | ❌ |

---

## Test 153

**Expected Tool:** `cosmos_database_list`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815683 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.668515 | `cosmos_account_list` | ❌ |
| 3 | 0.665298 | `cosmos_database_container_list` | ❌ |
| 4 | 0.606433 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.583480 | `kusto_database_list` | ❌ |

---

## Test 154

**Expected Tool:** `cosmos_database_list`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.749370 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.624759 | `cosmos_database_container_list` | ❌ |
| 3 | 0.614572 | `cosmos_account_list` | ❌ |
| 4 | 0.579919 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.537602 | `mysql_database_list` | ❌ |

---

## Test 155

**Expected Tool:** `kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590264 | `kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.463832 | `kusto_cluster_list` | ❌ |
| 3 | 0.428254 | `kusto_query` | ❌ |
| 4 | 0.426990 | `kusto_database_list` | ❌ |
| 5 | 0.426914 | `kusto_table_schema` | ❌ |

---

## Test 156

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793744 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.631383 | `kusto_database_list` | ❌ |
| 3 | 0.573395 | `kusto_cluster_get` | ❌ |
| 4 | 0.525025 | `aks_cluster_get` | ❌ |
| 5 | 0.509368 | `grafana_list` | ❌ |

---

## Test 157

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531307 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.465277 | `kusto_cluster_get` | ❌ |
| 3 | 0.432885 | `kusto_database_list` | ❌ |
| 4 | 0.369596 | `aks_cluster_get` | ❌ |
| 5 | 0.367813 | `kusto_table_schema` | ❌ |

---

## Test 158

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701539 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.571246 | `kusto_cluster_get` | ❌ |
| 3 | 0.550046 | `kusto_database_list` | ❌ |
| 4 | 0.498931 | `aks_cluster_get` | ❌ |
| 5 | 0.474226 | `redis_list` | ❌ |

---

## Test 159

**Expected Tool:** `kusto_database_list`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.677504 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.560592 | `kusto_cluster_list` | ❌ |
| 3 | 0.556646 | `kusto_table_list` | ❌ |
| 4 | 0.553191 | `postgres_database_list` | ❌ |
| 5 | 0.549673 | `cosmos_database_list` | ❌ |

---

## Test 160

**Expected Tool:** `kusto_database_list`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624167 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.509952 | `kusto_cluster_list` | ❌ |
| 3 | 0.506926 | `kusto_table_list` | ❌ |
| 4 | 0.497144 | `cosmos_database_list` | ❌ |
| 5 | 0.491093 | `mysql_database_list` | ❌ |

---

## Test 161

**Expected Tool:** `kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.423751 | `kusto_query` | ✅ **EXPECTED** |
| 2 | 0.411008 | `kusto_table_schema` | ❌ |
| 3 | 0.409526 | `postgres_database_query` | ❌ |
| 4 | 0.407810 | `kusto_sample` | ❌ |
| 5 | 0.403989 | `kusto_cluster_list` | ❌ |

---

## Test 162

**Expected Tool:** `kusto_sample`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595614 | `kusto_sample` | ✅ **EXPECTED** |
| 2 | 0.515069 | `kusto_table_schema` | ❌ |
| 3 | 0.424151 | `kusto_table_list` | ❌ |
| 4 | 0.400883 | `kusto_cluster_list` | ❌ |
| 5 | 0.399485 | `kusto_cluster_get` | ❌ |

---

## Test 163

**Expected Tool:** `kusto_table_list`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.679640 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.585261 | `postgres_table_list` | ❌ |
| 3 | 0.582187 | `kusto_database_list` | ❌ |
| 4 | 0.556698 | `mysql_table_list` | ❌ |
| 5 | 0.549917 | `monitor_table_list` | ❌ |

---

## Test 164

**Expected Tool:** `kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619600 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.559761 | `kusto_table_schema` | ❌ |
| 3 | 0.528863 | `kusto_database_list` | ❌ |
| 4 | 0.524956 | `mysql_table_list` | ❌ |
| 5 | 0.524338 | `postgres_table_list` | ❌ |

---

## Test 165

**Expected Tool:** `kusto_table_schema`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671861 | `kusto_table_schema` | ✅ **EXPECTED** |
| 2 | 0.564312 | `postgres_table_schema_get` | ❌ |
| 3 | 0.527942 | `mysql_table_schema_get` | ❌ |
| 4 | 0.490969 | `kusto_sample` | ❌ |
| 5 | 0.489624 | `kusto_table_list` | ❌ |

---

## Test 166

**Expected Tool:** `mysql_database_list`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633968 | `postgres_database_list` | ❌ |
| 2 | 0.622465 | `mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.534647 | `mysql_table_list` | ❌ |
| 4 | 0.498857 | `mysql_server_list` | ❌ |
| 5 | 0.490148 | `sql_db_list` | ❌ |

---

## Test 167

**Expected Tool:** `mysql_database_list`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587357 | `mysql_database_list` | ✅ **EXPECTED** |
| 2 | 0.573961 | `postgres_database_list` | ❌ |
| 3 | 0.483987 | `mysql_table_list` | ❌ |
| 4 | 0.463185 | `mysql_server_list` | ❌ |
| 5 | 0.444547 | `sql_db_list` | ❌ |

---

## Test 168

**Expected Tool:** `mysql_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476703 | `mysql_table_list` | ❌ |
| 2 | 0.454870 | `mysql_database_list` | ❌ |
| 3 | 0.433386 | `mysql_database_query` | ✅ **EXPECTED** |
| 4 | 0.419745 | `mysql_server_list` | ❌ |
| 5 | 0.409528 | `mysql_table_schema_get` | ❌ |

---

## Test 169

**Expected Tool:** `mysql_server_config_get`  
**Prompt:** Show me the configuration of MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531929 | `postgres_server_config_get` | ❌ |
| 2 | 0.516893 | `mysql_server_param_set` | ❌ |
| 3 | 0.489765 | `mysql_server_config_get` | ✅ **EXPECTED** |
| 4 | 0.476863 | `mysql_server_param_get` | ❌ |
| 5 | 0.426507 | `mysql_table_schema_get` | ❌ |

---

## Test 170

**Expected Tool:** `mysql_server_list`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678473 | `postgres_server_list` | ❌ |
| 2 | 0.556900 | `mysql_database_list` | ❌ |
| 3 | 0.554722 | `mysql_server_list` | ✅ **EXPECTED** |
| 4 | 0.513706 | `kusto_cluster_list` | ❌ |
| 5 | 0.501312 | `mysql_table_list` | ❌ |

---

## Test 171

**Expected Tool:** `mysql_server_list`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476765 | `mysql_database_list` | ❌ |
| 2 | 0.474526 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.435642 | `postgres_server_list` | ❌ |
| 4 | 0.412435 | `mysql_table_list` | ❌ |
| 5 | 0.389882 | `postgres_database_list` | ❌ |

---

## Test 172

**Expected Tool:** `mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636435 | `postgres_server_list` | ❌ |
| 2 | 0.534164 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.529809 | `mysql_database_list` | ❌ |
| 4 | 0.475052 | `kusto_cluster_list` | ❌ |
| 5 | 0.470491 | `redis_list` | ❌ |

---

## Test 173

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

## Test 174

**Expected Tool:** `mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.449428 | `mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.381119 | `mysql_server_param_get` | ❌ |
| 3 | 0.303411 | `postgres_server_param_set` | ❌ |
| 4 | 0.298884 | `mysql_database_query` | ❌ |
| 5 | 0.254194 | `mysql_server_list` | ❌ |

---

## Test 175

**Expected Tool:** `mysql_table_list`  
**Prompt:** List all tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633616 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.573916 | `postgres_table_list` | ❌ |
| 3 | 0.550833 | `postgres_database_list` | ❌ |
| 4 | 0.545614 | `mysql_database_list` | ❌ |
| 5 | 0.511871 | `kusto_table_list` | ❌ |

---

## Test 176

**Expected Tool:** `mysql_table_list`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.608883 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.525765 | `postgres_table_list` | ❌ |
| 3 | 0.524684 | `mysql_database_list` | ❌ |
| 4 | 0.506687 | `mysql_table_schema_get` | ❌ |
| 5 | 0.497778 | `postgres_database_list` | ❌ |

---

## Test 177

**Expected Tool:** `mysql_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630512 | `mysql_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.558229 | `postgres_table_schema_get` | ❌ |
| 3 | 0.545120 | `mysql_table_list` | ❌ |
| 4 | 0.523086 | `kusto_table_schema` | ❌ |
| 5 | 0.455886 | `mysql_database_list` | ❌ |

---

## Test 178

**Expected Tool:** `postgres_database_list`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815496 | `postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.643993 | `postgres_table_list` | ❌ |
| 3 | 0.622790 | `postgres_server_list` | ❌ |
| 4 | 0.542730 | `postgres_server_config_get` | ❌ |
| 5 | 0.490904 | `postgres_server_param_get` | ❌ |

---

## Test 179

**Expected Tool:** `postgres_database_list`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759870 | `postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.589784 | `postgres_server_list` | ❌ |
| 3 | 0.585834 | `postgres_table_list` | ❌ |
| 4 | 0.552687 | `postgres_server_config_get` | ❌ |
| 5 | 0.495629 | `postgres_server_param_get` | ❌ |

---

## Test 180

**Expected Tool:** `postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546170 | `postgres_database_list` | ❌ |
| 2 | 0.523164 | `postgres_database_query` | ✅ **EXPECTED** |
| 3 | 0.503333 | `postgres_table_list` | ❌ |
| 4 | 0.466585 | `postgres_server_list` | ❌ |
| 5 | 0.404011 | `postgres_server_param_get` | ❌ |

---

## Test 181

**Expected Tool:** `postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756571 | `postgres_server_config_get` | ✅ **EXPECTED** |
| 2 | 0.615332 | `postgres_server_param_set` | ❌ |
| 3 | 0.599471 | `postgres_server_param_get` | ❌ |
| 4 | 0.534868 | `postgres_database_list` | ❌ |
| 5 | 0.518574 | `postgres_server_list` | ❌ |

---

## Test 182

**Expected Tool:** `postgres_server_list`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.900023 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.640606 | `postgres_database_list` | ❌ |
| 3 | 0.565925 | `postgres_table_list` | ❌ |
| 4 | 0.539012 | `postgres_server_config_get` | ❌ |
| 5 | 0.534239 | `kusto_cluster_list` | ❌ |

---

## Test 183

**Expected Tool:** `postgres_server_list`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674327 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.606903 | `postgres_database_list` | ❌ |
| 3 | 0.576339 | `postgres_server_config_get` | ❌ |
| 4 | 0.522950 | `postgres_table_list` | ❌ |
| 5 | 0.506171 | `postgres_server_param_get` | ❌ |

---

## Test 184

**Expected Tool:** `postgres_server_list`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.832155 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.579078 | `postgres_database_list` | ❌ |
| 3 | 0.531814 | `postgres_server_config_get` | ❌ |
| 4 | 0.514424 | `postgres_table_list` | ❌ |
| 5 | 0.505869 | `postgres_server_param_get` | ❌ |

---

## Test 185

**Expected Tool:** `postgres_server_param_get`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594753 | `postgres_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.552516 | `postgres_server_param_set` | ❌ |
| 3 | 0.539673 | `postgres_server_config_get` | ❌ |
| 4 | 0.489693 | `postgres_server_list` | ❌ |
| 5 | 0.451742 | `postgres_database_list` | ❌ |

---

## Test 186

**Expected Tool:** `postgres_server_param_set`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579645 | `postgres_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.488469 | `postgres_server_config_get` | ❌ |
| 3 | 0.469794 | `postgres_server_list` | ❌ |
| 4 | 0.447011 | `postgres_server_param_get` | ❌ |
| 5 | 0.440620 | `postgres_database_list` | ❌ |

---

## Test 187

**Expected Tool:** `postgres_table_list`  
**Prompt:** List all tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789971 | `postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.750501 | `postgres_database_list` | ❌ |
| 3 | 0.574975 | `postgres_server_list` | ❌ |
| 4 | 0.519816 | `postgres_table_schema_get` | ❌ |
| 5 | 0.501378 | `postgres_server_config_get` | ❌ |

---

## Test 188

**Expected Tool:** `postgres_table_list`  
**Prompt:** Show me the tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736087 | `postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.689974 | `postgres_database_list` | ❌ |
| 3 | 0.558357 | `postgres_table_schema_get` | ❌ |
| 4 | 0.543331 | `postgres_server_list` | ❌ |
| 5 | 0.521567 | `postgres_server_config_get` | ❌ |

---

## Test 189

**Expected Tool:** `postgres_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.714877 | `postgres_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.597840 | `postgres_table_list` | ❌ |
| 3 | 0.574123 | `postgres_database_list` | ❌ |
| 4 | 0.508243 | `kusto_table_schema` | ❌ |
| 5 | 0.508097 | `postgres_server_config_get` | ❌ |

---

## Test 190

**Expected Tool:** `deploy_app_logs_get`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711770 | `deploy_app_logs_get` | ✅ **EXPECTED** |
| 2 | 0.471692 | `deploy_plan_get` | ❌ |
| 3 | 0.451639 | `monitor_activitylog_list` | ❌ |
| 4 | 0.404891 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.401388 | `monitor_resource_log_query` | ❌ |

---

## Test 191

**Expected Tool:** `deploy_architecture_diagram_generate`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680640 | `deploy_architecture_diagram_generate` | ✅ **EXPECTED** |
| 2 | 0.562521 | `deploy_plan_get` | ❌ |
| 3 | 0.497193 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.489344 | `cloudarchitect_design` | ❌ |
| 5 | 0.435921 | `deploy_iac_rules_get` | ❌ |

---

## Test 192

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

## Test 193

**Expected Tool:** `deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638841 | `deploy_pipeline_guidance_get` | ✅ **EXPECTED** |
| 2 | 0.499242 | `deploy_plan_get` | ❌ |
| 3 | 0.448917 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.385920 | `deploy_app_logs_get` | ❌ |
| 5 | 0.382240 | `get_bestpractices_get` | ❌ |

---

## Test 194

**Expected Tool:** `deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688055 | `deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.587903 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.499385 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.498575 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.448912 | `loadtesting_test_create` | ❌ |

---

## Test 195

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Publish an event to Event Grid topic <topic_name> using <event_schema> with the following data <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755365 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.483390 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.466031 | `eventgrid_topic_list` | ❌ |
| 4 | 0.360638 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.355598 | `servicebus_topic_details` | ❌ |

---

## Test 196

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Publish event to my Event Grid topic <topic_name> with the following events <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654648 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.524852 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.510038 | `eventgrid_topic_list` | ❌ |
| 4 | 0.373718 | `servicebus_topic_details` | ❌ |
| 5 | 0.359885 | `eventhubs_eventhub_update` | ❌ |

---

## Test 197

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Send an event to Event Grid topic <topic_name> in resource group <resource_group_name> with <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600243 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.521267 | `eventgrid_topic_list` | ❌ |
| 3 | 0.505156 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.411120 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.389472 | `eventhubs_eventhub_consumergroup_get` | ❌ |

---

## Test 198

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769875 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.745009 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.562310 | `kusto_cluster_list` | ❌ |
| 4 | 0.544193 | `search_service_list` | ❌ |
| 5 | 0.526550 | `subscription_list` | ❌ |

---

## Test 199

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738258 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.737470 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.492592 | `kusto_cluster_list` | ❌ |
| 4 | 0.480420 | `subscription_list` | ❌ |
| 5 | 0.473459 | `search_service_list` | ❌ |

---

## Test 200

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.770140 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.721298 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.535326 | `kusto_cluster_list` | ❌ |
| 4 | 0.513883 | `search_service_list` | ❌ |
| 5 | 0.496190 | `subscription_list` | ❌ |

---

## Test 201

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.757670 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.703063 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.609034 | `group_list` | ❌ |
| 4 | 0.545283 | `monitor_webtests_list` | ❌ |
| 5 | 0.524957 | `eventhubs_namespace_get` | ❌ |

---

## Test 202

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769056 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.720606 | `eventgrid_topic_list` | ❌ |
| 3 | 0.498615 | `servicebus_topic_details` | ❌ |
| 4 | 0.486216 | `servicebus_topic_subscription_details` | ❌ |
| 5 | 0.486162 | `eventgrid_events_publish` | ❌ |

---

## Test 203

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.718130 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.709806 | `eventgrid_topic_list` | ❌ |
| 3 | 0.539977 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.529286 | `servicebus_topic_details` | ❌ |
| 5 | 0.477876 | `eventgrid_events_publish` | ❌ |

---

## Test 204

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.746737 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.746174 | `eventgrid_topic_list` | ❌ |
| 3 | 0.535569 | `monitor_webtests_list` | ❌ |
| 4 | 0.524919 | `group_list` | ❌ |
| 5 | 0.503158 | `servicebus_topic_details` | ❌ |

---

## Test 205

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736348 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.659728 | `eventgrid_topic_list` | ❌ |
| 3 | 0.569320 | `subscription_list` | ❌ |
| 4 | 0.537922 | `kusto_cluster_list` | ❌ |
| 5 | 0.517276 | `search_service_list` | ❌ |

---

## Test 206

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684442 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.656277 | `eventgrid_topic_list` | ❌ |
| 3 | 0.542206 | `subscription_list` | ❌ |
| 4 | 0.521053 | `kusto_cluster_list` | ❌ |
| 5 | 0.510115 | `group_list` | ❌ |

---

## Test 207

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.695989 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.691739 | `eventgrid_topic_list` | ❌ |
| 3 | 0.557573 | `group_list` | ❌ |
| 4 | 0.510684 | `monitor_webtests_list` | ❌ |
| 5 | 0.504984 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 208

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.709624 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.642095 | `eventgrid_topic_list` | ❌ |
| 3 | 0.506225 | `subscription_list` | ❌ |
| 4 | 0.476396 | `search_service_list` | ❌ |
| 5 | 0.475782 | `kusto_cluster_list` | ❌ |

---

## Test 209

**Expected Tool:** `eventhubs_eventhub_consumergroup_delete`  
**Prompt:** Delete my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.766982 | `eventhubs_eventhub_consumergroup_delete` | ✅ **EXPECTED** |
| 2 | 0.675846 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.641111 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.633787 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.605477 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 210

**Expected Tool:** `eventhubs_eventhub_consumergroup_get`  
**Prompt:** List all consumer groups in my event hub <event_hub_name> in namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.740745 | `eventhubs_eventhub_consumergroup_get` | ✅ **EXPECTED** |
| 2 | 0.634857 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.627100 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.613852 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.601794 | `eventhubs_eventhub_get` | ❌ |

---

## Test 211

**Expected Tool:** `eventhubs_eventhub_consumergroup_get`  
**Prompt:** Get the details of my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712861 | `eventhubs_eventhub_consumergroup_get` | ✅ **EXPECTED** |
| 2 | 0.637170 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.626225 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.576838 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.529940 | `eventhubs_eventhub_get` | ❌ |

---

## Test 212

**Expected Tool:** `eventhubs_eventhub_consumergroup_update`  
**Prompt:** Create a new consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.757614 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
| 2 | 0.688923 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 3 | 0.670256 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.554374 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.545062 | `eventhubs_namespace_get` | ❌ |

---

## Test 213

**Expected Tool:** `eventhubs_eventhub_consumergroup_update`  
**Prompt:** Update my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738818 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
| 2 | 0.655818 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 3 | 0.642219 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.552303 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.524019 | `eventhubs_namespace_delete` | ❌ |

---

## Test 214

**Expected Tool:** `eventhubs_eventhub_delete`  
**Prompt:** Delete my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.699213 | `eventhubs_namespace_delete` | ❌ |
| 2 | 0.688502 | `eventhubs_eventhub_delete` | ✅ **EXPECTED** |
| 3 | 0.627561 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.578757 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.552954 | `eventhubs_eventhub_get` | ❌ |

---

## Test 215

**Expected Tool:** `eventhubs_eventhub_get`  
**Prompt:** List all Event Hubs in my namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.773242 | `eventhubs_eventhub_get` | ✅ **EXPECTED** |
| 2 | 0.687369 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.578718 | `eventhubs_eventhub_update` | ❌ |
| 4 | 0.561544 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.545474 | `eventhubs_eventhub_consumergroup_get` | ❌ |

---

## Test 216

**Expected Tool:** `eventhubs_eventhub_get`  
**Prompt:** Get the details of my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638161 | `eventhubs_namespace_get` | ❌ |
| 2 | 0.627946 | `eventhubs_eventhub_get` | ✅ **EXPECTED** |
| 3 | 0.571007 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.527957 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.521864 | `eventhubs_namespace_delete` | ❌ |

---

## Test 217

**Expected Tool:** `eventhubs_eventhub_update`  
**Prompt:** Create a new event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646084 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.605869 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.574389 | `eventhubs_eventhub_get` | ❌ |
| 4 | 0.571676 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.557550 | `eventhubs_namespace_delete` | ❌ |

---

## Test 218

**Expected Tool:** `eventhubs_eventhub_update`  
**Prompt:** Update my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655304 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.571327 | `eventhubs_eventhub_delete` | ❌ |
| 3 | 0.568700 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 4 | 0.568265 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.565982 | `eventhubs_namespace_delete` | ❌ |

---

## Test 219

**Expected Tool:** `eventhubs_namespace_delete`  
**Prompt:** Delete my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623995 | `eventhubs_namespace_delete` | ✅ **EXPECTED** |
| 2 | 0.525446 | `eventhubs_namespace_update` | ❌ |
| 3 | 0.504956 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.450038 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.435037 | `workbooks_delete` | ❌ |

---

## Test 220

**Expected Tool:** `eventhubs_namespace_get`  
**Prompt:** List all Event Hubs namespaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659838 | `eventhubs_eventhub_get` | ❌ |
| 2 | 0.658684 | `eventhubs_namespace_get` | ✅ **EXPECTED** |
| 3 | 0.607372 | `kusto_cluster_list` | ❌ |
| 4 | 0.557200 | `eventgrid_topic_list` | ❌ |
| 5 | 0.555995 | `eventgrid_subscription_list` | ❌ |

---

## Test 221

**Expected Tool:** `eventhubs_namespace_get`  
**Prompt:** Get the details of my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509642 | `eventhubs_namespace_get` | ✅ **EXPECTED** |
| 2 | 0.509457 | `monitor_webtests_get` | ❌ |
| 3 | 0.496932 | `servicebus_queue_details` | ❌ |
| 4 | 0.490205 | `eventhubs_namespace_update` | ❌ |
| 5 | 0.469763 | `functionapp_get` | ❌ |

---

## Test 222

**Expected Tool:** `eventhubs_namespace_update`  
**Prompt:** Create an new namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610456 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.466850 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.458458 | `eventhubs_namespace_delete` | ❌ |
| 4 | 0.450116 | `workbooks_create` | ❌ |
| 5 | 0.438492 | `eventhubs_eventhub_consumergroup_update` | ❌ |

---

## Test 223

**Expected Tool:** `eventhubs_namespace_update`  
**Prompt:** Update my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622338 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.474098 | `eventhubs_namespace_delete` | ❌ |
| 3 | 0.448823 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.436549 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.372632 | `sql_db_rename` | ❌ |

---

## Test 224

**Expected Tool:** `functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660116 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.451613 | `deploy_app_logs_get` | ❌ |
| 3 | 0.450457 | `applens_resource_diagnose` | ❌ |
| 4 | 0.390054 | `mysql_server_list` | ❌ |
| 5 | 0.380314 | `get_bestpractices_get` | ❌ |

---

## Test 225

**Expected Tool:** `functionapp_get`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607276 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.447545 | `mysql_server_config_get` | ❌ |
| 3 | 0.424457 | `appconfig_account_list` | ❌ |
| 4 | 0.411267 | `appconfig_kv_get` | ❌ |
| 5 | 0.400402 | `deploy_app_logs_get` | ❌ |

---

## Test 226

**Expected Tool:** `functionapp_get`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622384 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.413481 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.390708 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.383533 | `deploy_app_logs_get` | ❌ |
| 5 | 0.360725 | `storage_account_get` | ❌ |

---

## Test 227

**Expected Tool:** `functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690798 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.441948 | `foundry_resource_get` | ❌ |
| 3 | 0.432432 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.432135 | `applens_resource_diagnose` | ❌ |
| 5 | 0.429337 | `storage_account_get` | ❌ |

---

## Test 228

**Expected Tool:** `functionapp_get`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592791 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.417817 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.409712 | `deploy_app_logs_get` | ❌ |
| 4 | 0.400038 | `storage_account_get` | ❌ |
| 5 | 0.392237 | `applens_resource_diagnose` | ❌ |

---

## Test 229

**Expected Tool:** `functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687356 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.449589 | `deploy_app_logs_get` | ❌ |
| 3 | 0.428689 | `applens_resource_diagnose` | ❌ |
| 4 | 0.424648 | `foundry_resource_get` | ❌ |
| 5 | 0.392421 | `monitor_webtests_get` | ❌ |

---

## Test 230

**Expected Tool:** `functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644882 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.430189 | `deploy_app_logs_get` | ❌ |
| 3 | 0.421185 | `storage_account_get` | ❌ |
| 4 | 0.403261 | `signalr_runtime_get` | ❌ |
| 5 | 0.391545 | `foundry_resource_get` | ❌ |

---

## Test 231

**Expected Tool:** `functionapp_get`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555000 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.426734 | `quota_usage_check` | ❌ |
| 3 | 0.424635 | `deploy_app_logs_get` | ❌ |
| 4 | 0.407946 | `deploy_plan_get` | ❌ |
| 5 | 0.381602 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 232

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

## Test 233

**Expected Tool:** `functionapp_get`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646561 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.557549 | `search_service_list` | ❌ |
| 3 | 0.534210 | `subscription_list` | ❌ |
| 4 | 0.529031 | `kusto_cluster_list` | ❌ |
| 5 | 0.516618 | `cosmos_account_list` | ❌ |

---

## Test 234

**Expected Tool:** `functionapp_get`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560249 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.464985 | `deploy_app_logs_get` | ❌ |
| 3 | 0.411323 | `get_bestpractices_get` | ❌ |
| 4 | 0.410461 | `search_service_list` | ❌ |
| 5 | 0.398503 | `extension_cli_install` | ❌ |

---

## Test 235

**Expected Tool:** `functionapp_get`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433675 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.346619 | `deploy_app_logs_get` | ❌ |
| 3 | 0.337966 | `applens_resource_diagnose` | ❌ |
| 4 | 0.316594 | `extension_cli_install` | ❌ |
| 5 | 0.284362 | `get_bestpractices_get` | ❌ |

---

## Test 236

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** Get the account settings for my key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604687 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.532199 | `storage_account_get` | ❌ |
| 3 | 0.496629 | `keyvault_key_get` | ❌ |
| 4 | 0.452367 | `appconfig_kv_set` | ❌ |
| 5 | 0.448616 | `keyvault_secret_get` | ❌ |

---

## Test 237

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** Show me the account settings for managed HSM keyvault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671332 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.455568 | `storage_account_get` | ❌ |
| 3 | 0.441224 | `keyvault_key_get` | ❌ |
| 4 | 0.404666 | `appconfig_kv_set` | ❌ |
| 5 | 0.392135 | `storage_blob_container_get` | ❌ |

---

## Test 238

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** What's the value of the <setting_name> setting in my key vault with name <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.506461 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.496597 | `appconfig_kv_set` | ❌ |
| 3 | 0.419720 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.419093 | `keyvault_key_get` | ❌ |
| 5 | 0.411246 | `keyvault_secret_get` | ❌ |

---

## Test 239

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627876 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.570373 | `keyvault_certificate_import` | ❌ |
| 3 | 0.539870 | `keyvault_key_create` | ❌ |
| 4 | 0.519218 | `keyvault_certificate_get` | ❌ |
| 5 | 0.500038 | `keyvault_certificate_list` | ❌ |

---

## Test 240

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Generate a certificate named <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600136 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.561536 | `keyvault_certificate_import` | ❌ |
| 3 | 0.522706 | `keyvault_certificate_get` | ❌ |
| 4 | 0.501747 | `keyvault_key_create` | ❌ |
| 5 | 0.497171 | `keyvault_certificate_list` | ❌ |

---

## Test 241

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Request creation of certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.574236 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.527881 | `keyvault_certificate_import` | ❌ |
| 3 | 0.498278 | `keyvault_certificate_get` | ❌ |
| 4 | 0.481062 | `keyvault_key_create` | ❌ |
| 5 | 0.469623 | `keyvault_certificate_list` | ❌ |

---

## Test 242

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592059 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.562388 | `keyvault_certificate_import` | ❌ |
| 3 | 0.522147 | `keyvault_certificate_get` | ❌ |
| 4 | 0.502132 | `keyvault_key_create` | ❌ |
| 5 | 0.480025 | `keyvault_certificate_list` | ❌ |

---

## Test 243

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Issue a certificate <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623059 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.558582 | `keyvault_certificate_import` | ❌ |
| 3 | 0.534512 | `keyvault_certificate_get` | ❌ |
| 4 | 0.521416 | `keyvault_certificate_list` | ❌ |
| 5 | 0.464646 | `keyvault_key_create` | ❌ |

---

## Test 244

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600625 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.528409 | `keyvault_certificate_list` | ❌ |
| 3 | 0.519046 | `keyvault_certificate_import` | ❌ |
| 4 | 0.499674 | `keyvault_certificate_create` | ❌ |
| 5 | 0.486608 | `keyvault_key_get` | ❌ |

---

## Test 245

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646098 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.562988 | `keyvault_key_get` | ❌ |
| 3 | 0.510600 | `keyvault_secret_get` | ❌ |
| 4 | 0.509440 | `keyvault_certificate_list` | ❌ |
| 5 | 0.507737 | `keyvault_certificate_import` | ❌ |

---

## Test 246

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Get the certificate <certificate_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609523 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.515575 | `keyvault_certificate_list` | ❌ |
| 3 | 0.511488 | `keyvault_certificate_create` | ❌ |
| 4 | 0.507810 | `keyvault_certificate_import` | ❌ |
| 5 | 0.474394 | `keyvault_key_get` | ❌ |

---

## Test 247

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Display the certificate details for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.647669 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527400 | `keyvault_key_get` | ❌ |
| 3 | 0.521559 | `keyvault_certificate_list` | ❌ |
| 4 | 0.509795 | `keyvault_certificate_import` | ❌ |
| 5 | 0.499377 | `keyvault_secret_get` | ❌ |

---

## Test 248

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595959 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527406 | `keyvault_certificate_list` | ❌ |
| 3 | 0.518999 | `keyvault_certificate_import` | ❌ |
| 4 | 0.501593 | `keyvault_certificate_create` | ❌ |
| 5 | 0.465174 | `keyvault_key_get` | ❌ |

---

## Test 249

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585834 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.420747 | `keyvault_certificate_get` | ❌ |
| 3 | 0.402843 | `keyvault_certificate_create` | ❌ |
| 4 | 0.399363 | `keyvault_certificate_list` | ❌ |
| 5 | 0.352483 | `keyvault_key_create` | ❌ |

---

## Test 250

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622506 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.504314 | `keyvault_certificate_get` | ❌ |
| 3 | 0.498994 | `keyvault_certificate_create` | ❌ |
| 4 | 0.448151 | `keyvault_certificate_list` | ❌ |
| 5 | 0.419419 | `keyvault_key_create` | ❌ |

---

## Test 251

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Upload certificate file <file_path> to key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595918 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.454099 | `keyvault_certificate_create` | ❌ |
| 3 | 0.452551 | `keyvault_certificate_get` | ❌ |
| 4 | 0.418249 | `keyvault_certificate_list` | ❌ |
| 5 | 0.413076 | `keyvault_key_create` | ❌ |

---

## Test 252

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619757 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.517804 | `keyvault_certificate_get` | ❌ |
| 3 | 0.480965 | `keyvault_certificate_create` | ❌ |
| 4 | 0.444397 | `keyvault_certificate_list` | ❌ |
| 5 | 0.381454 | `keyvault_key_create` | ❌ |

---

## Test 253

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595777 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.452610 | `keyvault_certificate_create` | ❌ |
| 3 | 0.441616 | `keyvault_certificate_get` | ❌ |
| 4 | 0.407786 | `keyvault_key_create` | ❌ |
| 5 | 0.392244 | `keyvault_secret_create` | ❌ |

---

## Test 254

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726177 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.583066 | `keyvault_key_list` | ❌ |
| 3 | 0.531942 | `keyvault_secret_list` | ❌ |
| 4 | 0.515236 | `keyvault_certificate_get` | ❌ |
| 5 | 0.486262 | `keyvault_certificate_create` | ❌ |

---

## Test 255

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615577 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.522453 | `keyvault_certificate_get` | ❌ |
| 3 | 0.475146 | `keyvault_key_list` | ❌ |
| 4 | 0.461486 | `keyvault_certificate_create` | ❌ |
| 5 | 0.448139 | `keyvault_key_get` | ❌ |

---

## Test 256

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** What certificates are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624749 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.519739 | `keyvault_certificate_get` | ❌ |
| 3 | 0.510489 | `keyvault_certificate_create` | ❌ |
| 4 | 0.505459 | `keyvault_certificate_import` | ❌ |
| 5 | 0.497328 | `keyvault_key_list` | ❌ |

---

## Test 257

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** List certificate names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672676 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.553925 | `keyvault_key_list` | ❌ |
| 3 | 0.511907 | `keyvault_secret_list` | ❌ |
| 4 | 0.507062 | `keyvault_certificate_get` | ❌ |
| 5 | 0.492744 | `keyvault_certificate_create` | ❌ |

---

## Test 258

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Enumerate certificates in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747439 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.594217 | `keyvault_key_list` | ❌ |
| 3 | 0.558641 | `keyvault_secret_list` | ❌ |
| 4 | 0.515568 | `keyvault_certificate_get` | ❌ |
| 5 | 0.491398 | `keyvault_certificate_create` | ❌ |

---

## Test 259

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show certificate names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639790 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.512475 | `keyvault_certificate_get` | ❌ |
| 3 | 0.507515 | `keyvault_key_list` | ❌ |
| 4 | 0.483033 | `keyvault_certificate_create` | ❌ |
| 5 | 0.464740 | `keyvault_secret_list` | ❌ |

---

## Test 260

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661186 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.456580 | `keyvault_secret_create` | ❌ |
| 3 | 0.451901 | `keyvault_certificate_create` | ❌ |
| 4 | 0.429716 | `keyvault_certificate_import` | ❌ |
| 5 | 0.399326 | `keyvault_key_get` | ❌ |

---

## Test 261

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Generate a key <key_name> with type <key_type> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640735 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.428502 | `keyvault_key_get` | ❌ |
| 3 | 0.422930 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420045 | `keyvault_secret_create` | ❌ |
| 5 | 0.405644 | `appconfig_kv_set` | ❌ |

---

## Test 262

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an oct key in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546974 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.462869 | `keyvault_secret_create` | ❌ |
| 3 | 0.447367 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420696 | `keyvault_key_get` | ❌ |
| 5 | 0.403895 | `keyvault_certificate_import` | ❌ |

---

## Test 263

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an RSA key in the vault <key_vault_account_name> with name <key_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641093 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.501636 | `keyvault_secret_create` | ❌ |
| 3 | 0.491871 | `keyvault_certificate_create` | ❌ |
| 4 | 0.464709 | `keyvault_certificate_import` | ❌ |
| 5 | 0.451016 | `keyvault_key_get` | ❌ |

---

## Test 264

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an EC key with name <key_name> in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571577 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.443668 | `keyvault_certificate_create` | ❌ |
| 3 | 0.434720 | `keyvault_secret_create` | ❌ |
| 4 | 0.421643 | `keyvault_key_get` | ❌ |
| 5 | 0.400727 | `keyvault_certificate_import` | ❌ |

---

## Test 265

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549488 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.464630 | `keyvault_secret_get` | ❌ |
| 3 | 0.452565 | `keyvault_key_create` | ❌ |
| 4 | 0.439927 | `keyvault_key_list` | ❌ |
| 5 | 0.426545 | `keyvault_certificate_get` | ❌ |

---

## Test 266

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629995 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.529632 | `keyvault_secret_get` | ❌ |
| 3 | 0.513213 | `storage_account_get` | ❌ |
| 4 | 0.496379 | `keyvault_certificate_get` | ❌ |
| 5 | 0.457369 | `keyvault_key_create` | ❌ |

---

## Test 267

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Get the key <key_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.484645 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.442960 | `keyvault_key_create` | ❌ |
| 3 | 0.410507 | `keyvault_secret_get` | ❌ |
| 4 | 0.395392 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.383519 | `appconfig_kv_lock_set` | ❌ |

---

## Test 268

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Display the key details for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590303 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.485375 | `keyvault_secret_get` | ❌ |
| 3 | 0.476540 | `storage_account_get` | ❌ |
| 4 | 0.460796 | `keyvault_certificate_get` | ❌ |
| 5 | 0.436396 | `keyvault_admin_settings_get` | ❌ |

---

## Test 269

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Retrieve key metadata for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518984 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.433121 | `storage_account_get` | ❌ |
| 3 | 0.432806 | `keyvault_admin_settings_get` | ❌ |
| 4 | 0.429019 | `keyvault_key_create` | ❌ |
| 5 | 0.423311 | `keyvault_secret_get` | ❌ |

---

## Test 270

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701404 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.601661 | `keyvault_certificate_list` | ❌ |
| 3 | 0.587492 | `keyvault_secret_list` | ❌ |
| 4 | 0.498735 | `cosmos_account_list` | ❌ |
| 5 | 0.480120 | `keyvault_admin_settings_get` | ❌ |

---

## Test 271

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550747 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.507741 | `keyvault_key_get` | ❌ |
| 3 | 0.476424 | `keyvault_certificate_list` | ❌ |
| 4 | 0.472274 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.452998 | `keyvault_secret_get` | ❌ |

---

## Test 272

**Expected Tool:** `keyvault_key_list`  
**Prompt:** What keys are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581886 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.502200 | `keyvault_admin_settings_get` | ❌ |
| 3 | 0.501485 | `keyvault_certificate_list` | ❌ |
| 4 | 0.476412 | `keyvault_key_get` | ❌ |
| 5 | 0.472295 | `keyvault_secret_list` | ❌ |

---

## Test 273

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List key names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641174 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.559603 | `keyvault_certificate_list` | ❌ |
| 3 | 0.553568 | `keyvault_secret_list` | ❌ |
| 4 | 0.486286 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.475992 | `cosmos_account_list` | ❌ |

---

## Test 274

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Enumerate keys in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.723204 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.611416 | `keyvault_certificate_list` | ❌ |
| 3 | 0.611082 | `keyvault_secret_list` | ❌ |
| 4 | 0.473811 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.441881 | `keyvault_key_get` | ❌ |

---

## Test 275

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Show key names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570324 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.501073 | `keyvault_key_get` | ❌ |
| 3 | 0.500182 | `keyvault_certificate_list` | ❌ |
| 4 | 0.496848 | `storage_account_get` | ❌ |
| 5 | 0.490384 | `keyvault_secret_list` | ❌ |

---

## Test 276

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678482 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.552754 | `keyvault_key_create` | ❌ |
| 3 | 0.515324 | `keyvault_secret_get` | ❌ |
| 4 | 0.475248 | `keyvault_certificate_create` | ❌ |
| 5 | 0.461437 | `appconfig_kv_set` | ❌ |

---

## Test 277

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663094 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.521308 | `keyvault_secret_get` | ❌ |
| 3 | 0.512233 | `appconfig_kv_set` | ❌ |
| 4 | 0.458351 | `keyvault_key_create` | ❌ |
| 5 | 0.429785 | `appconfig_kv_lock_set` | ❌ |

---

## Test 278

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639910 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.511557 | `keyvault_secret_get` | ❌ |
| 3 | 0.485135 | `appconfig_kv_set` | ❌ |
| 4 | 0.484343 | `keyvault_key_create` | ❌ |
| 5 | 0.448938 | `appconfig_kv_lock_set` | ❌ |

---

## Test 279

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.675165 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.501381 | `keyvault_secret_get` | ❌ |
| 3 | 0.498048 | `keyvault_key_create` | ❌ |
| 4 | 0.479538 | `keyvault_certificate_import` | ❌ |
| 5 | 0.458556 | `appconfig_kv_set` | ❌ |

---

## Test 280

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Update secret <secret_name> to value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571637 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.514811 | `keyvault_secret_get` | ❌ |
| 3 | 0.441790 | `appconfig_kv_set` | ❌ |
| 4 | 0.418066 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.407765 | `keyvault_key_get` | ❌ |

---

## Test 281

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599926 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.504212 | `keyvault_key_get` | ❌ |
| 3 | 0.501397 | `keyvault_secret_create` | ❌ |
| 4 | 0.478697 | `keyvault_secret_list` | ❌ |
| 5 | 0.439521 | `keyvault_certificate_get` | ❌ |

---

## Test 282

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650847 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.566786 | `keyvault_key_get` | ❌ |
| 3 | 0.517580 | `storage_account_get` | ❌ |
| 4 | 0.496050 | `keyvault_certificate_get` | ❌ |
| 5 | 0.485177 | `keyvault_secret_list` | ❌ |

---

## Test 283

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Get the secret <secret_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580232 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.492213 | `keyvault_key_get` | ❌ |
| 3 | 0.488705 | `keyvault_secret_create` | ❌ |
| 4 | 0.443591 | `keyvault_secret_list` | ❌ |
| 5 | 0.424071 | `keyvault_admin_settings_get` | ❌ |

---

## Test 284

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Display the secret details for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646799 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.546992 | `keyvault_key_get` | ❌ |
| 3 | 0.497443 | `storage_account_get` | ❌ |
| 4 | 0.492583 | `keyvault_certificate_get` | ❌ |
| 5 | 0.491539 | `keyvault_secret_list` | ❌ |

---

## Test 285

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Retrieve secret metadata for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576730 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.474237 | `keyvault_key_get` | ❌ |
| 3 | 0.466666 | `keyvault_secret_create` | ❌ |
| 4 | 0.446999 | `keyvault_secret_list` | ❌ |
| 5 | 0.438695 | `storage_account_get` | ❌ |

---

## Test 286

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701158 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.563675 | `keyvault_key_list` | ❌ |
| 3 | 0.538379 | `keyvault_certificate_list` | ❌ |
| 4 | 0.499911 | `keyvault_secret_get` | ❌ |
| 5 | 0.455500 | `cosmos_account_list` | ❌ |

---

## Test 287

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555609 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.540833 | `keyvault_secret_get` | ❌ |
| 3 | 0.497525 | `keyvault_key_get` | ❌ |
| 4 | 0.464620 | `keyvault_key_list` | ❌ |
| 5 | 0.453061 | `keyvault_admin_settings_get` | ❌ |

---

## Test 288

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** What secrets are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572490 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.530078 | `keyvault_secret_get` | ❌ |
| 3 | 0.493701 | `keyvault_key_list` | ❌ |
| 4 | 0.487541 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.475273 | `keyvault_key_get` | ❌ |

---

## Test 289

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** List secrets names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624393 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.559505 | `keyvault_key_list` | ❌ |
| 3 | 0.517514 | `keyvault_certificate_list` | ❌ |
| 4 | 0.480158 | `keyvault_secret_get` | ❌ |
| 5 | 0.453259 | `storage_blob_container_get` | ❌ |

---

## Test 290

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Enumerate secrets in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.742213 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.601141 | `keyvault_key_list` | ❌ |
| 3 | 0.567836 | `keyvault_certificate_list` | ❌ |
| 4 | 0.497314 | `keyvault_secret_get` | ❌ |
| 5 | 0.437487 | `keyvault_admin_settings_get` | ❌ |

---

## Test 291

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Show secrets names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567105 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.520983 | `keyvault_secret_get` | ❌ |
| 3 | 0.476248 | `keyvault_key_list` | ❌ |
| 4 | 0.462677 | `keyvault_secret_create` | ❌ |
| 5 | 0.461326 | `keyvault_key_get` | ❌ |

---

## Test 292

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588300 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.544432 | `aks_nodepool_get` | ❌ |
| 3 | 0.517279 | `kusto_cluster_get` | ❌ |
| 4 | 0.481571 | `mysql_server_config_get` | ❌ |
| 5 | 0.430993 | `postgres_server_config_get` | ❌ |

---

## Test 293

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621759 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.575643 | `aks_nodepool_get` | ❌ |
| 3 | 0.567870 | `kusto_cluster_get` | ❌ |
| 4 | 0.461466 | `sql_db_show` | ❌ |
| 5 | 0.444982 | `monitor_webtests_get` | ❌ |

---

## Test 294

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522525 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.483324 | `aks_nodepool_get` | ❌ |
| 3 | 0.434684 | `kusto_cluster_get` | ❌ |
| 4 | 0.380361 | `mysql_server_config_get` | ❌ |
| 5 | 0.366689 | `kusto_cluster_list` | ❌ |

---

## Test 295

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588634 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.550569 | `aks_nodepool_get` | ❌ |
| 3 | 0.527511 | `kusto_cluster_get` | ❌ |
| 4 | 0.445807 | `storage_account_get` | ❌ |
| 5 | 0.435561 | `foundry_resource_get` | ❌ |

---

## Test 296

**Expected Tool:** `aks_cluster_get`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756444 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.749345 | `kusto_cluster_list` | ❌ |
| 3 | 0.590289 | `aks_nodepool_get` | ❌ |
| 4 | 0.569039 | `kusto_database_list` | ❌ |
| 5 | 0.560506 | `search_service_list` | ❌ |

---

## Test 297

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612123 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.586661 | `kusto_cluster_list` | ❌ |
| 3 | 0.507761 | `aks_nodepool_get` | ❌ |
| 4 | 0.489724 | `kusto_cluster_get` | ❌ |
| 5 | 0.464234 | `kusto_database_list` | ❌ |

---

## Test 298

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628429 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.563390 | `aks_nodepool_get` | ❌ |
| 3 | 0.526756 | `kusto_cluster_list` | ❌ |
| 4 | 0.426157 | `kusto_cluster_get` | ❌ |
| 5 | 0.409890 | `kusto_database_list` | ❌ |

---

## Test 299

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728859 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.517021 | `kusto_cluster_get` | ❌ |
| 3 | 0.509820 | `aks_cluster_get` | ❌ |
| 4 | 0.468472 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.463216 | `sql_elastic-pool_list` | ❌ |

---

## Test 300

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654139 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.458590 | `sql_elastic-pool_list` | ❌ |
| 3 | 0.446043 | `aks_cluster_get` | ❌ |
| 4 | 0.440189 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.413815 | `kusto_cluster_get` | ❌ |

---

## Test 301

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592838 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.402556 | `aks_cluster_get` | ❌ |
| 3 | 0.385236 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.383122 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.355090 | `kusto_cluster_get` | ❌ |

---

## Test 302

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692363 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.519037 | `aks_cluster_get` | ❌ |
| 3 | 0.506704 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.500749 | `kusto_cluster_list` | ❌ |
| 5 | 0.487680 | `sql_elastic-pool_list` | ❌ |

---

## Test 303

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732218 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.561829 | `aks_cluster_get` | ❌ |
| 3 | 0.510307 | `sql_elastic-pool_list` | ❌ |
| 4 | 0.509840 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.486700 | `kusto_cluster_list` | ❌ |

---

## Test 304

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629446 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.456911 | `aks_cluster_get` | ❌ |
| 3 | 0.443942 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.433006 | `kusto_cluster_list` | ❌ |
| 5 | 0.425452 | `sql_elastic-pool_list` | ❌ |

---

## Test 305

**Expected Tool:** `loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577869 | `loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.519844 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.512099 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.472763 | `monitor_webtests_create` | ❌ |
| 5 | 0.460717 | `loadtesting_testresource_list` | ❌ |

---

## Test 306

**Expected Tool:** `loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626226 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.619944 | `loadtesting_test_get` | ✅ **EXPECTED** |
| 3 | 0.595020 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.591140 | `monitor_webtests_get` | ❌ |
| 5 | 0.536024 | `monitor_webtests_list` | ❌ |

---

## Test 307

**Expected Tool:** `loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645227 | `loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.618713 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.541714 | `loadtesting_test_create` | ❌ |
| 4 | 0.539805 | `loadtesting_testrun_create` | ❌ |
| 5 | 0.526788 | `monitor_webtests_list` | ❌ |

---

## Test 308

**Expected Tool:** `loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.794326 | `loadtesting_testresource_list` | ✅ **EXPECTED** |
| 2 | 0.653165 | `monitor_webtests_list` | ❌ |
| 3 | 0.577408 | `group_list` | ❌ |
| 4 | 0.575652 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.565348 | `datadog_monitoredresources_list` | ❌ |

---

## Test 309

**Expected Tool:** `loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688976 | `loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.594774 | `loadtesting_testrun_update` | ❌ |
| 3 | 0.558566 | `loadtesting_test_create` | ❌ |
| 4 | 0.547546 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.496224 | `loadtesting_testresource_list` | ❌ |

---

## Test 310

**Expected Tool:** `loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619042 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.601408 | `loadtesting_test_get` | ❌ |
| 3 | 0.597843 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.577802 | `monitor_webtests_get` | ❌ |
| 5 | 0.565417 | `loadtesting_testrun_list` | ❌ |

---

## Test 311

**Expected Tool:** `loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669180 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.640355 | `loadtesting_testrun_list` | ✅ **EXPECTED** |
| 3 | 0.601075 | `loadtesting_test_get` | ❌ |
| 4 | 0.577811 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.570009 | `monitor_webtests_get` | ❌ |

---

## Test 312

**Expected Tool:** `loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.706350 | `loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.514487 | `loadtesting_testrun_create` | ❌ |
| 3 | 0.487061 | `monitor_webtests_update` | ❌ |
| 4 | 0.470535 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.468315 | `monitor_webtests_get` | ❌ |

---

## Test 313

**Expected Tool:** `grafana_list`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599427 | `kusto_cluster_list` | ❌ |
| 2 | 0.578887 | `grafana_list` | ✅ **EXPECTED** |
| 3 | 0.549957 | `search_service_list` | ❌ |
| 4 | 0.549827 | `subscription_list` | ❌ |
| 5 | 0.531208 | `redis_list` | ❌ |

---

## Test 314

**Expected Tool:** `managedlustre_fs_create`  
**Prompt:** Create an Azure Managed Lustre filesystem with name <filesystem_name>, size <filesystem_size>, SKU <sku>, and subnet <subnet_id> for availability zone <zone> in location <location>. Maintenance should occur on <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728113 | `managedlustre_fs_create` | ✅ **EXPECTED** |
| 2 | 0.616169 | `managedlustre_fs_list` | ❌ |
| 3 | 0.605772 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.598293 | `managedlustre_fs_update` | ❌ |
| 5 | 0.557741 | `managedlustre_fs_subnetsize_validate` | ❌ |

---

## Test 315

**Expected Tool:** `managedlustre_fs_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.750584 | `managedlustre_fs_list` | ✅ **EXPECTED** |
| 2 | 0.631782 | `managedlustre_fs_sku_get` | ❌ |
| 3 | 0.582660 | `managedlustre_fs_create` | ❌ |
| 4 | 0.562377 | `kusto_cluster_list` | ❌ |
| 5 | 0.512086 | `search_service_list` | ❌ |

---

## Test 316

**Expected Tool:** `managedlustre_fs_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743843 | `managedlustre_fs_list` | ✅ **EXPECTED** |
| 2 | 0.613211 | `managedlustre_fs_sku_get` | ❌ |
| 3 | 0.565856 | `managedlustre_fs_create` | ❌ |
| 4 | 0.519902 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.515433 | `loadtesting_testresource_list` | ❌ |

---

## Test 317

**Expected Tool:** `managedlustre_fs_sku_get`  
**Prompt:** List the Azure Managed Lustre SKUs available in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.827348 | `managedlustre_fs_sku_get` | ✅ **EXPECTED** |
| 2 | 0.613537 | `managedlustre_fs_list` | ❌ |
| 3 | 0.513242 | `managedlustre_fs_create` | ❌ |
| 4 | 0.496177 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 5 | 0.470241 | `kusto_cluster_list` | ❌ |

---

## Test 318

**Expected Tool:** `managedlustre_fs_subnetsize_ask`  
**Prompt:** Tell me how many IP addresses I need for an Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739766 | `managedlustre_fs_subnetsize_ask` | ✅ **EXPECTED** |
| 2 | 0.651586 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 3 | 0.594652 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.559458 | `managedlustre_fs_list` | ❌ |
| 5 | 0.533684 | `managedlustre_fs_create` | ❌ |

---

## Test 319

**Expected Tool:** `managedlustre_fs_subnetsize_validate`  
**Prompt:** Validate if the network <subnet_id> can host Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879517 | `managedlustre_fs_subnetsize_validate` | ✅ **EXPECTED** |
| 2 | 0.622580 | `managedlustre_fs_subnetsize_ask` | ❌ |
| 3 | 0.542943 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.515984 | `managedlustre_fs_create` | ❌ |
| 5 | 0.480875 | `managedlustre_fs_list` | ❌ |

---

## Test 320

**Expected Tool:** `managedlustre_fs_update`  
**Prompt:** Update the maintenance window of the Azure Managed Lustre filesystem <filesystem_name> to <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739126 | `managedlustre_fs_update` | ✅ **EXPECTED** |
| 2 | 0.527525 | `managedlustre_fs_create` | ❌ |
| 3 | 0.487206 | `managedlustre_fs_list` | ❌ |
| 4 | 0.385359 | `managedlustre_fs_sku_get` | ❌ |
| 5 | 0.344872 | `managedlustre_fs_subnetsize_validate` | ❌ |

---

## Test 321

**Expected Tool:** `marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570167 | `marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.499184 | `marketplace_product_list` | ❌ |
| 3 | 0.353256 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.333160 | `servicebus_topic_details` | ❌ |
| 5 | 0.330935 | `servicebus_queue_details` | ❌ |

---

## Test 322

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607917 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.443071 | `marketplace_product_get` | ❌ |
| 3 | 0.341341 | `search_service_list` | ❌ |
| 4 | 0.330500 | `foundry_models_list` | ❌ |
| 5 | 0.328705 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 323

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537726 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.385117 | `marketplace_product_get` | ❌ |
| 3 | 0.308769 | `foundry_models_list` | ❌ |
| 4 | 0.288027 | `redis_list` | ❌ |
| 5 | 0.260408 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 324

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646844 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.642779 | `azureaibestpractices_get` | ❌ |
| 3 | 0.635406 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.586907 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.531728 | `deploy_pipeline_guidance_get` | ❌ |

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
| 3 | 0.535973 | `azureaibestpractices_get` | ❌ |
| 4 | 0.518643 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.465573 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 327

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624273 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.576355 | `azureaibestpractices_get` | ❌ |
| 3 | 0.570488 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.522998 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.493998 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 328

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581850 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.497350 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.495659 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.486886 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.474511 | `deploy_plan_get` | ❌ |

---

## Test 329

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610986 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.532790 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.512172 | `azureaibestpractices_get` | ❌ |
| 4 | 0.487322 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.458060 | `deploy_pipeline_guidance_get` | ❌ |

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
| 4 | 0.500519 | `azureaibestpractices_get` | ❌ |
| 5 | 0.483705 | `deploy_pipeline_guidance_get` | ❌ |

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
| 4 | 0.471773 | `azureaibestpractices_get` | ❌ |
| 5 | 0.433134 | `deploy_pipeline_guidance_get` | ❌ |

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
| 3 | 0.371728 | `monitor_workspace_log_query` | ❌ |
| 4 | 0.363126 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.344544 | `datadog_monitoredresources_list` | ❌ |

---

## Test 334

**Expected Tool:** `monitor_healthmodels_entity_get`  
**Prompt:** Show me the health status of entity <entity_id> using the health model <health_model_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660878 | `monitor_healthmodels_entity_get` | ✅ **EXPECTED** |
| 2 | 0.609189 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.351603 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.327812 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.288788 | `foundry_models_deployments_list` | ❌ |

---

## Test 335

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592640 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424087 | `monitor_metrics_query` | ❌ |
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
| 1 | 0.607538 | `storage_account_get` | ❌ |
| 2 | 0.587736 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.544043 | `storage_blob_container_get` | ❌ |
| 4 | 0.495829 | `storage_blob_get` | ❌ |
| 5 | 0.473423 | `managedlustre_fs_list` | ❌ |

---

## Test 337

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633173 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495461 | `monitor_metrics_query` | ❌ |
| 3 | 0.433945 | `monitor_resource_log_query` | ❌ |
| 4 | 0.392960 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.388750 | `bicepschema_get` | ❌ |

---

## Test 338

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555380 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.527530 | `monitor_resource_log_query` | ❌ |
| 3 | 0.464743 | `applens_resource_diagnose` | ❌ |
| 4 | 0.419763 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.413282 | `applicationinsights_recommendation_list` | ❌ |

---

## Test 339

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557804 | `monitor_metrics_query` | ✅ **EXPECTED** |
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
| 1 | 0.461130 | `monitor_metrics_query` | ✅ **EXPECTED** |
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
| 2 | 0.492154 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 3 | 0.448148 | `applens_resource_diagnose` | ❌ |
| 4 | 0.411705 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.397335 | `quota_usage_check` | ❌ |

---

## Test 342

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525450 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.406322 | `monitor_resource_log_query` | ❌ |
| 3 | 0.384530 | `monitor_metrics_definitions` | ❌ |
| 4 | 0.347654 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.330465 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 343

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480100 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.444779 | `monitor_resource_log_query` | ❌ |
| 3 | 0.388382 | `applens_resource_diagnose` | ❌ |
| 4 | 0.363411 | `quota_usage_check` | ❌ |
| 5 | 0.350075 | `resourcehealth_health-events_list` | ❌ |

---

## Test 344

**Expected Tool:** `monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687849 | `monitor_resource_log_query` | ✅ **EXPECTED** |
| 2 | 0.621913 | `monitor_workspace_log_query` | ❌ |
| 3 | 0.598390 | `monitor_activitylog_list` | ❌ |
| 4 | 0.485637 | `deploy_app_logs_get` | ❌ |
| 5 | 0.469762 | `monitor_metrics_query` | ❌ |

---

## Test 345

**Expected Tool:** `monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.850709 | `monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725738 | `monitor_table_type_list` | ❌ |
| 3 | 0.620445 | `monitor_workspace_list` | ❌ |
| 4 | 0.541983 | `kusto_table_list` | ❌ |
| 5 | 0.539481 | `monitor_workspace_log_query` | ❌ |

---

## Test 346

**Expected Tool:** `monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798113 | `monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.700725 | `monitor_table_type_list` | ❌ |
| 3 | 0.600180 | `monitor_workspace_list` | ❌ |
| 4 | 0.543242 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.503293 | `monitor_resource_log_query` | ❌ |

---

## Test 347

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881524 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.765553 | `monitor_table_list` | ❌ |
| 3 | 0.569921 | `monitor_workspace_list` | ❌ |
| 4 | 0.504784 | `mysql_table_list` | ❌ |
| 5 | 0.497622 | `monitor_workspace_log_query` | ❌ |

---

## Test 348

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843139 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.736736 | `monitor_table_list` | ❌ |
| 3 | 0.576731 | `monitor_workspace_list` | ❌ |
| 4 | 0.509598 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.481249 | `mysql_table_list` | ❌ |

---

## Test 349

**Expected Tool:** `monitor_webtests_create`  
**Prompt:** Create a new Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650643 | `monitor_webtests_create` | ✅ **EXPECTED** |
| 2 | 0.570201 | `monitor_webtests_list` | ❌ |
| 3 | 0.549934 | `monitor_webtests_update` | ❌ |
| 4 | 0.533323 | `monitor_webtests_get` | ❌ |
| 5 | 0.482108 | `loadtesting_testresource_create` | ❌ |

---

## Test 350

**Expected Tool:** `monitor_webtests_get`  
**Prompt:** Get Web Test details for <webtest_resource_name> in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759358 | `monitor_webtests_get` | ✅ **EXPECTED** |
| 2 | 0.725442 | `monitor_webtests_list` | ❌ |
| 3 | 0.583815 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.562797 | `monitor_webtests_update` | ❌ |
| 5 | 0.530626 | `monitor_webtests_create` | ❌ |

---

## Test 351

**Expected Tool:** `monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.730754 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.610001 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.547168 | `grafana_list` | ❌ |
| 4 | 0.519680 | `redis_list` | ❌ |
| 5 | 0.496948 | `monitor_webtests_get` | ❌ |

---

## Test 352

**Expected Tool:** `monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793807 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.675965 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.584929 | `monitor_webtests_get` | ❌ |
| 4 | 0.573602 | `group_list` | ❌ |
| 5 | 0.546088 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 353

**Expected Tool:** `monitor_webtests_update`  
**Prompt:** Update an existing Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686449 | `monitor_webtests_update` | ✅ **EXPECTED** |
| 2 | 0.559184 | `monitor_webtests_get` | ❌ |
| 3 | 0.558227 | `monitor_webtests_create` | ❌ |
| 4 | 0.553726 | `monitor_webtests_list` | ❌ |
| 5 | 0.508759 | `loadtesting_testrun_update` | ❌ |

---

## Test 354

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813902 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.680254 | `grafana_list` | ❌ |
| 3 | 0.659475 | `monitor_table_list` | ❌ |
| 4 | 0.610623 | `kusto_cluster_list` | ❌ |
| 5 | 0.599636 | `search_service_list` | ❌ |

---

## Test 355

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656194 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.584750 | `monitor_table_list` | ❌ |
| 3 | 0.531083 | `monitor_table_type_list` | ❌ |
| 4 | 0.518304 | `grafana_list` | ❌ |
| 5 | 0.506772 | `monitor_workspace_log_query` | ❌ |

---

## Test 356

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732962 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.601523 | `grafana_list` | ❌ |
| 3 | 0.579659 | `monitor_table_list` | ❌ |
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
| 5 | 0.485494 | `monitor_table_list` | ❌ |

---

## Test 358

**Expected Tool:** `datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.668985 | `datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.454272 | `redis_list` | ❌ |
| 3 | 0.413645 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.413246 | `monitor_metrics_query` | ❌ |
| 5 | 0.401672 | `grafana_list` | ❌ |

---

## Test 359

**Expected Tool:** `datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624253 | `datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.443536 | `monitor_metrics_query` | ❌ |
| 3 | 0.440102 | `redis_list` | ❌ |
| 4 | 0.424436 | `monitor_resource_log_query` | ❌ |
| 5 | 0.385157 | `loadtesting_testresource_list` | ❌ |

---

## Test 360

**Expected Tool:** `extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533164 | `quota_usage_check` | ❌ |
| 2 | 0.481143 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.476708 | `extension_azqr` | ✅ **EXPECTED** |
| 4 | 0.471828 | `subscription_list` | ❌ |
| 5 | 0.468404 | `applens_resource_diagnose` | ❌ |

---

## Test 361

**Expected Tool:** `extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532780 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.492873 | `get_bestpractices_get` | ❌ |
| 3 | 0.476184 | `applicationinsights_recommendation_list` | ❌ |
| 4 | 0.473354 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.469887 | `azureaibestpractices_get` | ❌ |

---

## Test 362

**Expected Tool:** `extension_azqr`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536934 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.516780 | `extension_azqr` | ✅ **EXPECTED** |
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
| 1 | 0.590878 | `quota_region_availability_list` | ✅ **EXPECTED** |
| 2 | 0.413274 | `quota_usage_check` | ❌ |
| 3 | 0.391374 | `redis_list` | ❌ |
| 4 | 0.372940 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.369832 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 364

**Expected Tool:** `quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609244 | `quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.491058 | `quota_region_availability_list` | ❌ |
| 3 | 0.384350 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.376368 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.371458 | `redis_list` | ❌ |

---

## Test 365

**Expected Tool:** `role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645258 | `role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.539461 | `subscription_list` | ❌ |
| 3 | 0.483988 | `group_list` | ❌ |
| 4 | 0.478766 | `grafana_list` | ❌ |
| 5 | 0.471364 | `cosmos_account_list` | ❌ |

---

## Test 366

**Expected Tool:** `role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609704 | `role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.514313 | `subscription_list` | ❌ |
| 3 | 0.457012 | `grafana_list` | ❌ |
| 4 | 0.449049 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.445069 | `redis_list` | ❌ |

---

## Test 367

**Expected Tool:** `redis_list`  
**Prompt:** List all Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.810487 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.587804 | `grafana_list` | ❌ |
| 3 | 0.512954 | `kusto_cluster_list` | ❌ |
| 4 | 0.508380 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.501218 | `postgres_server_list` | ❌ |

---

## Test 368

**Expected Tool:** `redis_list`  
**Prompt:** Show me my Redis resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685260 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.374254 | `grafana_list` | ❌ |
| 3 | 0.364214 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.359538 | `mysql_server_list` | ❌ |
| 5 | 0.331233 | `mysql_database_list` | ❌ |

---

## Test 369

**Expected Tool:** `redis_list`  
**Prompt:** Show me the Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.781214 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.539176 | `grafana_list` | ❌ |
| 3 | 0.449087 | `postgres_server_list` | ❌ |
| 4 | 0.449040 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.442841 | `kusto_cluster_list` | ❌ |

---

## Test 370

**Expected Tool:** `redis_list`  
**Prompt:** Show me my Redis caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572914 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.316573 | `mysql_database_list` | ❌ |
| 3 | 0.301714 | `postgres_database_list` | ❌ |
| 4 | 0.286492 | `mysql_server_list` | ❌ |
| 5 | 0.273014 | `kusto_cluster_list` | ❌ |

---

## Test 371

**Expected Tool:** `redis_list`  
**Prompt:** Get Redis clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478186 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.456308 | `kusto_cluster_list` | ❌ |
| 3 | 0.384630 | `kusto_cluster_get` | ❌ |
| 4 | 0.358521 | `kusto_database_list` | ❌ |
| 5 | 0.343305 | `aks_cluster_get` | ❌ |

---

## Test 372

**Expected Tool:** `group_list`  
**Prompt:** List all resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755935 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.566552 | `workbooks_list` | ❌ |
| 3 | 0.564566 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.552354 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.549477 | `monitor_webtests_list` | ❌ |

---

## Test 373

**Expected Tool:** `group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529538 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.464657 | `redis_list` | ❌ |
| 3 | 0.463610 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.462337 | `mysql_server_list` | ❌ |
| 5 | 0.460320 | `loadtesting_testresource_list` | ❌ |

---

## Test 374

**Expected Tool:** `group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665772 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.532415 | `redis_list` | ❌ |
| 3 | 0.532373 | `datadog_monitoredresources_list` | ❌ |
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
| 5 | 0.349844 | `datadog_monitoredresources_list` | ❌ |

---

## Test 376

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576524 | `storage_account_get` | ❌ |
| 2 | 0.564128 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.555636 | `storage_blob_container_get` | ❌ |
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
| 3 | 0.424899 | `mysql_server_list` | ❌ |
| 4 | 0.412025 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.393542 | `managedlustre_fs_list` | ❌ |

---

## Test 378

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737219 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.585354 | `redis_list` | ❌ |
| 3 | 0.549914 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.548568 | `grafana_list` | ❌ |
| 5 | 0.543785 | `subscription_list` | ❌ |

---

## Test 379

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644982 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.545208 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.509017 | `resourcehealth_health-events_list` | ❌ |
| 4 | 0.508252 | `quota_usage_check` | ❌ |
| 5 | 0.505745 | `redis_list` | ❌ |

---

## Test 380

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.596890 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.549900 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.496535 | `resourcehealth_health-events_list` | ❌ |
| 4 | 0.441921 | `applens_resource_diagnose` | ❌ |
| 5 | 0.433614 | `loadtesting_testresource_list` | ❌ |

---

## Test 381

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690352 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.553485 | `search_service_list` | ❌ |
| 3 | 0.534251 | `eventgrid_topic_list` | ❌ |
| 4 | 0.529644 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.518372 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 382

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686157 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.534500 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.513302 | `search_service_list` | ❌ |
| 4 | 0.513259 | `eventgrid_topic_list` | ❌ |
| 5 | 0.501302 | `subscription_list` | ❌ |

---

## Test 383

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** What service issues have occurred in the last 30 days?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.450568 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.267663 | `applens_resource_diagnose` | ❌ |
| 3 | 0.245720 | `cloudarchitect_design` | ❌ |
| 4 | 0.216847 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.211043 | `search_service_list` | ❌ |

---

## Test 384

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** List active service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685170 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.527887 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.524080 | `eventgrid_topic_list` | ❌ |
| 4 | 0.518720 | `search_service_list` | ❌ |
| 5 | 0.502156 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 385

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564833 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.436322 | `search_service_list` | ❌ |
| 3 | 0.403679 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.402493 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.397735 | `quota_usage_check` | ❌ |

---

## Test 386

**Expected Tool:** `servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642862 | `servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.460909 | `servicebus_topic_subscription_details` | ❌ |
| 3 | 0.436933 | `servicebus_topic_details` | ❌ |
| 4 | 0.386101 | `search_knowledge_base_get` | ❌ |
| 5 | 0.384167 | `storage_account_get` | ❌ |

---

## Test 387

**Expected Tool:** `servicebus_topic_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642952 | `servicebus_topic_details` | ✅ **EXPECTED** |
| 2 | 0.571860 | `servicebus_topic_subscription_details` | ❌ |
| 3 | 0.483976 | `servicebus_queue_details` | ❌ |
| 4 | 0.482958 | `eventgrid_topic_list` | ❌ |
| 5 | 0.458787 | `eventgrid_subscription_list` | ❌ |

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
| 5 | 0.472059 | `eventgrid_subscription_list` | ❌ |

---

## Test 389

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show me the details of SignalR <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532697 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.355076 | `redis_list` | ❌ |
| 3 | 0.329646 | `foundry_resource_get` | ❌ |
| 4 | 0.319974 | `sql_server_show` | ❌ |
| 5 | 0.304388 | `servicebus_queue_details` | ❌ |

---

## Test 390

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show me the network information of SignalR runtime <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573540 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.337342 | `sql_server_show` | ❌ |
| 3 | 0.306499 | `foundry_resource_get` | ❌ |
| 4 | 0.305076 | `redis_list` | ❌ |
| 5 | 0.300956 | `servicebus_topic_details` | ❌ |

---

## Test 391

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Describe the SignalR runtime <signalr_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710191 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.411341 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.410475 | `foundry_resource_get` | ❌ |
| 4 | 0.399379 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.382041 | `sql_server_list` | ❌ |

---

## Test 392

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Get information about my SignalR runtime <signalr_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.715547 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.459127 | `foundry_resource_get` | ❌ |
| 3 | 0.430532 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.430490 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.416705 | `functionapp_get` | ❌ |

---

## Test 393

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show all the SignalRs information in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563935 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.501071 | `redis_list` | ❌ |
| 3 | 0.494511 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.481461 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.462016 | `mysql_server_list` | ❌ |

---

## Test 394

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** List all SignalRs in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530514 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.507654 | `postgres_server_list` | ❌ |
| 3 | 0.495108 | `redis_list` | ❌ |
| 4 | 0.494498 | `kusto_cluster_list` | ❌ |
| 5 | 0.487168 | `subscription_list` | ❌ |

---

## Test 395

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a new SQL database named <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516780 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.470892 | `sql_server_create` | ❌ |
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
| 2 | 0.459672 | `sql_server_create` | ❌ |
| 3 | 0.437525 | `sql_server_delete` | ❌ |
| 4 | 0.420843 | `sql_db_show` | ❌ |
| 5 | 0.417661 | `sql_db_delete` | ❌ |

---

## Test 397

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604472 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.545906 | `sql_server_create` | ❌ |
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
| 2 | 0.490893 | `sql_server_delete` | ❌ |
| 3 | 0.364399 | `postgres_database_list` | ❌ |
| 4 | 0.355545 | `mysql_database_list` | ❌ |
| 5 | 0.347837 | `sql_db_rename` | ❌ |

---

## Test 401

**Expected Tool:** `sql_db_list`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643186 | `sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.640909 | `mysql_database_list` | ❌ |
| 3 | 0.609101 | `postgres_database_list` | ❌ |
| 4 | 0.602889 | `cosmos_database_list` | ❌ |
| 5 | 0.570511 | `kusto_database_list` | ❌ |

---

## Test 402

**Expected Tool:** `sql_db_list`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.617746 | `sql_server_show` | ❌ |
| 2 | 0.609322 | `sql_db_list` | ✅ **EXPECTED** |
| 3 | 0.558742 | `mysql_database_list` | ❌ |
| 4 | 0.553611 | `mysql_server_config_get` | ❌ |
| 5 | 0.524274 | `sql_db_show` | ❌ |

---

## Test 403

**Expected Tool:** `sql_db_rename`  
**Prompt:** Rename the SQL database <database_name> on server <server_name> to <new_database_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593348 | `sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.425282 | `sql_server_delete` | ❌ |
| 3 | 0.416207 | `sql_db_delete` | ❌ |
| 4 | 0.396947 | `sql_db_create` | ❌ |
| 5 | 0.346018 | `sql_db_show` | ❌ |

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
| 5 | 0.433898 | `sql_server_show` | ❌ |

---

## Test 405

**Expected Tool:** `sql_db_show`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610991 | `sql_server_show` | ❌ |
| 2 | 0.593212 | `postgres_server_config_get` | ❌ |
| 3 | 0.530581 | `mysql_server_config_get` | ❌ |
| 4 | 0.528136 | `sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.465693 | `sql_db_list` | ❌ |

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
| 5 | 0.434183 | `mysql_database_list` | ❌ |

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
| 1 | 0.678126 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502376 | `sql_db_list` | ❌ |
| 3 | 0.499428 | `mysql_database_list` | ❌ |
| 4 | 0.485248 | `aks_nodepool_get` | ❌ |
| 5 | 0.479044 | `sql_server_show` | ❌ |

---

## Test 410

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606450 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502877 | `sql_server_show` | ❌ |
| 3 | 0.457164 | `sql_db_list` | ❌ |
| 4 | 0.450749 | `aks_nodepool_get` | ❌ |
| 5 | 0.434311 | `mysql_database_list` | ❌ |

---

## Test 411

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592849 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.422001 | `mysql_database_list` | ❌ |
| 3 | 0.407174 | `aks_nodepool_get` | ❌ |
| 4 | 0.402504 | `mysql_server_list` | ❌ |
| 5 | 0.397686 | `sql_db_list` | ❌ |

---

## Test 412

**Expected Tool:** `sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682352 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.563828 | `sql_db_create` | ❌ |
| 3 | 0.528986 | `sql_server_list` | ❌ |
| 4 | 0.482031 | `storage_account_create` | ❌ |
| 5 | 0.473625 | `sql_db_show` | ❌ |

---

## Test 413

**Expected Tool:** `sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618302 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.510242 | `sql_db_create` | ❌ |
| 3 | 0.472500 | `sql_server_show` | ❌ |
| 4 | 0.441244 | `sql_server_delete` | ❌ |
| 5 | 0.400990 | `sql_db_rename` | ❌ |

---

## Test 414

**Expected Tool:** `sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589818 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.501403 | `sql_db_create` | ❌ |
| 3 | 0.497890 | `sql_server_list` | ❌ |
| 4 | 0.461181 | `sql_db_rename` | ❌ |
| 5 | 0.442876 | `mysql_server_list` | ❌ |

---

## Test 415

**Expected Tool:** `sql_server_delete`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656593 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.548064 | `sql_db_delete` | ❌ |
| 3 | 0.518037 | `sql_server_list` | ❌ |
| 4 | 0.495550 | `sql_server_create` | ❌ |
| 5 | 0.483132 | `workbooks_delete` | ❌ |

---

## Test 416

**Expected Tool:** `sql_server_delete`  
**Prompt:** Remove the SQL server <server_name> from my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.614979 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.393915 | `postgres_server_list` | ❌ |
| 3 | 0.379678 | `sql_db_delete` | ❌ |
| 4 | 0.376739 | `sql_server_show` | ❌ |
| 5 | 0.350151 | `sql_server_list` | ❌ |

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
| 3 | 0.434868 | `sql_server_list` | ❌ |
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
| 3 | 0.368082 | `sql_server_list` | ❌ |
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
| 3 | 0.322155 | `sql_server_list` | ❌ |
| 4 | 0.307823 | `sql_server_create` | ❌ |
| 5 | 0.269788 | `sql_server_delete` | ❌ |

---

## Test 421

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.635073 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.532742 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.522222 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.448903 | `sql_server_create` | ❌ |
| 5 | 0.440949 | `sql_server_delete` | ❌ |

---

## Test 422

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670248 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533562 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503648 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.316619 | `sql_server_list` | ❌ |
| 5 | 0.302362 | `sql_server_delete` | ❌ |

---

## Test 423

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684693 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.574404 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539648 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.428985 | `sql_server_create` | ❌ |
| 5 | 0.395248 | `sql_db_create` | ❌ |

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
| 4 | 0.539983 | `sql_server_firewall-rule_create` | ❌ |
| 5 | 0.498444 | `sql_db_delete` | ❌ |

---

## Test 425

**Expected Tool:** `sql_server_firewall-rule_delete`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670179 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.574340 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.530097 | `sql_server_firewall-rule_create` | ❌ |
| 4 | 0.488418 | `sql_server_delete` | ❌ |
| 5 | 0.360381 | `sql_db_delete` | ❌ |

---

## Test 426

**Expected Tool:** `sql_server_firewall-rule_delete`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671211 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.601231 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.576869 | `sql_server_firewall-rule_create` | ❌ |
| 4 | 0.499272 | `sql_server_delete` | ❌ |
| 5 | 0.378586 | `sql_db_delete` | ❌ |

---

## Test 427

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729372 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.549322 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.513114 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.468812 | `sql_server_show` | ❌ |
| 5 | 0.418817 | `sql_server_list` | ❌ |

---

## Test 428

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630731 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.523793 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.476757 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.410680 | `sql_server_show` | ❌ |
| 5 | 0.348100 | `sql_server_list` | ❌ |

---

## Test 429

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630601 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.532360 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.473519 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.413344 | `sql_server_show` | ❌ |
| 5 | 0.351075 | `sql_server_list` | ❌ |

---

## Test 430

**Expected Tool:** `sql_server_list`  
**Prompt:** List all Azure SQL servers in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694350 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.596459 | `mysql_server_list` | ❌ |
| 3 | 0.578164 | `sql_db_list` | ❌ |
| 4 | 0.515621 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.509738 | `sql_db_show` | ❌ |

---

## Test 431

**Expected Tool:** `sql_server_list`  
**Prompt:** Show me every SQL server available in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618189 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.593656 | `mysql_server_list` | ❌ |
| 3 | 0.542333 | `sql_db_list` | ❌ |
| 4 | 0.507462 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.496299 | `group_list` | ❌ |

---

## Test 432

**Expected Tool:** `sql_server_show`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629672 | `sql_db_show` | ❌ |
| 2 | 0.595184 | `sql_server_show` | ✅ **EXPECTED** |
| 3 | 0.587728 | `sql_server_list` | ❌ |
| 4 | 0.559829 | `mysql_server_list` | ❌ |
| 5 | 0.540218 | `sql_db_list` | ❌ |

---

## Test 433

**Expected Tool:** `sql_server_show`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658806 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.610474 | `postgres_server_config_get` | ❌ |
| 3 | 0.538189 | `mysql_server_config_get` | ❌ |
| 4 | 0.471606 | `sql_db_show` | ❌ |
| 5 | 0.445360 | `postgres_server_param_get` | ❌ |

---

## Test 434

**Expected Tool:** `sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563143 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.392565 | `postgres_server_config_get` | ❌ |
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
| 3 | 0.418121 | `storage_account_get` | ❌ |
| 4 | 0.413950 | `storage_blob_container_get` | ❌ |
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
| 3 | 0.407175 | `storage_account_get` | ❌ |
| 4 | 0.406804 | `storage_blob_container_create` | ❌ |
| 5 | 0.400212 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 437

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589041 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.535517 | `managedlustre_fs_create` | ❌ |
| 3 | 0.509778 | `storage_blob_container_create` | ❌ |
| 4 | 0.462417 | `storage_account_get` | ❌ |
| 5 | 0.447162 | `sql_db_create` | ❌ |

---

## Test 438

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.673726 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.607762 | `storage_blob_container_get` | ❌ |
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
| 1 | 0.692653 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.577173 | `storage_blob_container_get` | ❌ |
| 3 | 0.529205 | `storage_blob_get` | ❌ |
| 4 | 0.518215 | `storage_account_create` | ❌ |
| 5 | 0.448506 | `storage_blob_container_create` | ❌ |

---

## Test 440

**Expected Tool:** `storage_account_get`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649209 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.557097 | `managedlustre_fs_sku_get` | ❌ |
| 3 | 0.549457 | `storage_blob_container_get` | ❌ |
| 4 | 0.546778 | `subscription_list` | ❌ |
| 5 | 0.536840 | `cosmos_account_list` | ❌ |

---

## Test 441

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.556838 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.481664 | `storage_blob_container_get` | ❌ |
| 3 | 0.461299 | `managedlustre_fs_list` | ❌ |
| 4 | 0.421642 | `cosmos_account_list` | ❌ |
| 5 | 0.410587 | `storage_blob_get` | ❌ |

---

## Test 442

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619418 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.555677 | `storage_blob_container_get` | ❌ |
| 3 | 0.518229 | `storage_blob_get` | ❌ |
| 4 | 0.473598 | `cosmos_account_list` | ❌ |
| 5 | 0.464867 | `subscription_list` | ❌ |

---

## Test 443

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

## Test 444

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682161 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.590826 | `storage_blob_container_get` | ❌ |
| 3 | 0.559264 | `storage_blob_get` | ❌ |
| 4 | 0.500625 | `storage_account_create` | ❌ |
| 5 | 0.420405 | `storage_account_get` | ❌ |

---

## Test 445

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625490 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.544084 | `storage_blob_container_get` | ❌ |
| 3 | 0.497791 | `storage_blob_get` | ❌ |
| 4 | 0.463198 | `storage_account_create` | ❌ |
| 5 | 0.435103 | `cosmos_database_container_list` | ❌ |

---

## Test 446

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.703348 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.623681 | `storage_blob_get` | ❌ |
| 3 | 0.577842 | `storage_account_get` | ❌ |
| 4 | 0.549804 | `storage_blob_container_create` | ❌ |
| 5 | 0.523289 | `cosmos_database_container_list` | ❌ |

---

## Test 447

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712012 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.680802 | `storage_blob_get` | ❌ |
| 3 | 0.613933 | `cosmos_database_container_list` | ❌ |
| 4 | 0.556319 | `storage_blob_container_create` | ❌ |
| 5 | 0.518196 | `storage_account_get` | ❌ |

---

## Test 448

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713080 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.592373 | `cosmos_database_container_list` | ❌ |
| 3 | 0.586169 | `storage_blob_get` | ❌ |
| 4 | 0.523245 | `storage_account_get` | ❌ |
| 5 | 0.487520 | `storage_blob_container_create` | ❌ |

---

## Test 449

**Expected Tool:** `storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.700972 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.648309 | `storage_blob_container_get` | ❌ |
| 3 | 0.541019 | `storage_blob_container_create` | ❌ |
| 4 | 0.527354 | `storage_account_get` | ❌ |
| 5 | 0.477946 | `cosmos_database_container_list` | ❌ |

---

## Test 450

**Expected Tool:** `storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694973 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.633389 | `storage_blob_container_get` | ❌ |
| 3 | 0.589164 | `storage_blob_container_create` | ❌ |
| 4 | 0.580140 | `storage_account_get` | ❌ |
| 5 | 0.457016 | `storage_account_create` | ❌ |

---

## Test 451

**Expected Tool:** `storage_blob_get`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.733586 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.702342 | `storage_blob_container_get` | ❌ |
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
| 2 | 0.666342 | `storage_blob_container_get` | ❌ |
| 3 | 0.561557 | `storage_blob_container_create` | ❌ |
| 4 | 0.533515 | `cosmos_database_container_list` | ❌ |
| 5 | 0.483941 | `storage_account_get` | ❌ |

---

## Test 453

**Expected Tool:** `storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566287 | `storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.525674 | `storage_blob_container_create` | ❌ |
| 3 | 0.517616 | `storage_blob_get` | ❌ |
| 4 | 0.474550 | `storage_blob_container_get` | ❌ |
| 5 | 0.382123 | `storage_account_create` | ❌ |

---

## Test 454

**Expected Tool:** `subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653388 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512964 | `cosmos_account_list` | ❌ |
| 3 | 0.471653 | `postgres_server_list` | ❌ |
| 4 | 0.469023 | `kusto_cluster_list` | ❌ |
| 5 | 0.460951 | `redis_list` | ❌ |

---

## Test 455

**Expected Tool:** `subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.458000 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.407348 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.393695 | `eventgrid_topic_list` | ❌ |
| 4 | 0.391478 | `redis_list` | ❌ |
| 5 | 0.381238 | `postgres_server_list` | ❌ |

---

## Test 456

**Expected Tool:** `subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.432595 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.319579 | `marketplace_product_list` | ❌ |
| 3 | 0.315575 | `marketplace_product_get` | ❌ |
| 4 | 0.293722 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.289334 | `eventgrid_topic_list` | ❌ |

---

## Test 457

**Expected Tool:** `subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476039 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.357510 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.354286 | `marketplace_product_list` | ❌ |
| 4 | 0.344507 | `redis_list` | ❌ |
| 5 | 0.340836 | `eventgrid_topic_list` | ❌ |

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
| 5 | 0.473856 | `azureaibestpractices_get` | ❌ |

---

## Test 459

**Expected Tool:** `azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581316 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.512141 | `get_bestpractices_get` | ❌ |
| 3 | 0.510005 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.472108 | `keyvault_secret_get` | ❌ |
| 5 | 0.451188 | `azureaibestpractices_get` | ❌ |

---

## Test 460

**Expected Tool:** `virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711929 | `virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
| 2 | 0.659763 | `virtualdesktop_hostpool_host_list` | ❌ |
| 3 | 0.620665 | `kusto_cluster_list` | ❌ |
| 4 | 0.546744 | `search_service_list` | ❌ |
| 5 | 0.536219 | `virtualdesktop_hostpool_host_user-list` | ❌ |

---

## Test 461

**Expected Tool:** `virtualdesktop_hostpool_host_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.727054 | `virtualdesktop_hostpool_host_list` | ✅ **EXPECTED** |
| 2 | 0.715326 | `virtualdesktop_hostpool_host_user-list` | ❌ |
| 3 | 0.573394 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.438691 | `aks_nodepool_get` | ❌ |
| 5 | 0.393757 | `sql_elastic-pool_list` | ❌ |

---

## Test 462

**Expected Tool:** `virtualdesktop_hostpool_host_user-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813368 | `virtualdesktop_hostpool_host_user-list` | ✅ **EXPECTED** |
| 2 | 0.659140 | `virtualdesktop_hostpool_host_list` | ❌ |
| 3 | 0.501060 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.357530 | `aks_nodepool_get` | ❌ |
| 5 | 0.336308 | `monitor_workspace_list` | ❌ |

---

## Test 463

**Expected Tool:** `workbooks_create`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552365 | `workbooks_create` | ✅ **EXPECTED** |
| 2 | 0.417950 | `workbooks_update` | ❌ |
| 3 | 0.361364 | `workbooks_delete` | ❌ |
| 4 | 0.329118 | `workbooks_show` | ❌ |
| 5 | 0.328113 | `workbooks_list` | ❌ |

---

## Test 464

**Expected Tool:** `workbooks_delete`  
**Prompt:** Delete the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621310 | `workbooks_delete` | ✅ **EXPECTED** |
| 2 | 0.498518 | `workbooks_show` | ❌ |
| 3 | 0.432453 | `workbooks_create` | ❌ |
| 4 | 0.425569 | `workbooks_list` | ❌ |
| 5 | 0.421897 | `workbooks_update` | ❌ |

---

## Test 465

**Expected Tool:** `workbooks_list`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.772430 | `workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.563085 | `workbooks_create` | ❌ |
| 3 | 0.516725 | `grafana_list` | ❌ |
| 4 | 0.494073 | `workbooks_show` | ❌ |
| 5 | 0.488599 | `group_list` | ❌ |

---

## Test 466

**Expected Tool:** `workbooks_list`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.708612 | `workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.570791 | `workbooks_create` | ❌ |
| 3 | 0.499716 | `workbooks_show` | ❌ |
| 4 | 0.485504 | `workbooks_delete` | ❌ |
| 5 | 0.472353 | `grafana_list` | ❌ |

---

## Test 467

**Expected Tool:** `workbooks_show`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686087 | `workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.498789 | `workbooks_create` | ❌ |
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
| 1 | 0.581501 | `workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.500475 | `workbooks_list` | ❌ |
| 3 | 0.469354 | `workbooks_create` | ❌ |
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
| 2 | 0.382852 | `workbooks_create` | ❌ |
| 3 | 0.349689 | `workbooks_delete` | ❌ |
| 4 | 0.347944 | `workbooks_show` | ❌ |
| 5 | 0.292979 | `loadtesting_testrun_update` | ❌ |

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
| 4 | 0.475878 | `azureaibestpractices_get` | ❌ |
| 5 | 0.453282 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 471

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502125 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.290902 | `storage_blob_upload` | ❌ |
| 3 | 0.259162 | `managedlustre_fs_create` | ❌ |
| 4 | 0.254991 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.245059 | `managedlustre_fs_subnetsize_validate` | ❌ |

---

## Test 472

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Help me design an Azure cloud service that will serve as an ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.508153 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.377941 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.341462 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.336438 | `azureaibestpractices_get` | ❌ |
| 5 | 0.328747 | `get_bestpractices_get` | ❌ |

---

## Test 473

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.423577 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.271943 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.265972 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.242581 | `deploy_plan_get` | ❌ |
| 5 | 0.235171 | `azureaibestpractices_get` | ❌ |

---

## Test 474

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.534690 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.369969 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.356331 | `managedlustre_fs_create` | ❌ |
| 4 | 0.352797 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.323920 | `storage_blob_upload` | ❌ |

---

## Summary

**Total Prompts Tested:** 474  
**Analysis Execution Time:** 111.1291285s  

### Success Rate Metrics

**Top Choice Success:** 92.2% (437/474 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 3.2% (15/474 tests)  
**🎯 High Confidence (≥0.7):** 22.4% (106/474 tests)  
**✅ Good Confidence (≥0.6):** 61.2% (290/474 tests)  
**👍 Fair Confidence (≥0.5):** 91.8% (435/474 tests)  
**👌 Acceptable Confidence (≥0.4):** 99.6% (472/474 tests)  
**❌ Low Confidence (<0.4):** 0.4% (2/474 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 3.2% (15/474 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 22.4% (106/474 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 59.3% (281/474 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 86.3% (409/474 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 92.2% (437/474 tests)  

### Success Rate Analysis

🟢 **Excellent** - The tool selection system is performing exceptionally well.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

