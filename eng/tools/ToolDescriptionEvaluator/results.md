# Tool Selection Analysis Setup

**Setup completed:** 2025-11-13 15:03:55  
**Tool count:** 173  
**Database setup time:** 17.7958506s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-11-13 15:03:55  
**Tool count:** 173  

## Table of Contents

- [Test 1: foundry_agents_connect](#test-1)
- [Test 2: foundry_agents_evaluate](#test-2)
- [Test 3: foundry_agents_list](#test-3)
- [Test 4: foundry_agents_list](#test-4)
- [Test 5: foundry_agents_query-and-evaluate](#test-5)
- [Test 6: foundry_knowledge_index_list](#test-6)
- [Test 7: foundry_knowledge_index_list](#test-7)
- [Test 8: foundry_knowledge_index_schema](#test-8)
- [Test 9: foundry_knowledge_index_schema](#test-9)
- [Test 10: foundry_models_deploy](#test-10)
- [Test 11: foundry_models_deployments_list](#test-11)
- [Test 12: foundry_models_deployments_list](#test-12)
- [Test 13: foundry_models_list](#test-13)
- [Test 14: foundry_models_list](#test-14)
- [Test 15: foundry_openai_chat-completions-create](#test-15)
- [Test 16: foundry_openai_create-completion](#test-16)
- [Test 17: foundry_openai_embeddings-create](#test-17)
- [Test 18: foundry_openai_embeddings-create](#test-18)
- [Test 19: foundry_openai_models-list](#test-19)
- [Test 20: foundry_openai_models-list](#test-20)
- [Test 21: foundry_resource_get](#test-21)
- [Test 22: foundry_resource_get](#test-22)
- [Test 23: foundry_resource_get](#test-23)
- [Test 24: search_knowledge_base_get](#test-24)
- [Test 25: search_knowledge_base_get](#test-25)
- [Test 26: search_knowledge_base_get](#test-26)
- [Test 27: search_knowledge_base_get](#test-27)
- [Test 28: search_knowledge_base_get](#test-28)
- [Test 29: search_knowledge_base_get](#test-29)
- [Test 30: search_knowledge_base_retrieve](#test-30)
- [Test 31: search_knowledge_base_retrieve](#test-31)
- [Test 32: search_knowledge_base_retrieve](#test-32)
- [Test 33: search_knowledge_base_retrieve](#test-33)
- [Test 34: search_knowledge_base_retrieve](#test-34)
- [Test 35: search_knowledge_base_retrieve](#test-35)
- [Test 36: search_knowledge_base_retrieve](#test-36)
- [Test 37: search_knowledge_base_retrieve](#test-37)
- [Test 38: search_knowledge_source_get](#test-38)
- [Test 39: search_knowledge_source_get](#test-39)
- [Test 40: search_knowledge_source_get](#test-40)
- [Test 41: search_knowledge_source_get](#test-41)
- [Test 42: search_knowledge_source_get](#test-42)
- [Test 43: search_knowledge_source_get](#test-43)
- [Test 44: search_index_get](#test-44)
- [Test 45: search_index_get](#test-45)
- [Test 46: search_index_get](#test-46)
- [Test 47: search_index_query](#test-47)
- [Test 48: search_service_list](#test-48)
- [Test 49: search_service_list](#test-49)
- [Test 50: search_service_list](#test-50)
- [Test 51: speech_stt_recognize](#test-51)
- [Test 52: speech_stt_recognize](#test-52)
- [Test 53: speech_stt_recognize](#test-53)
- [Test 54: speech_stt_recognize](#test-54)
- [Test 55: speech_stt_recognize](#test-55)
- [Test 56: speech_stt_recognize](#test-56)
- [Test 57: speech_stt_recognize](#test-57)
- [Test 58: speech_stt_recognize](#test-58)
- [Test 59: speech_stt_recognize](#test-59)
- [Test 60: speech_stt_recognize](#test-60)
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
- [Test 80: appservice_database_add](#test-80)
- [Test 81: appservice_database_add](#test-81)
- [Test 82: appservice_database_add](#test-82)
- [Test 83: appservice_database_add](#test-83)
- [Test 84: appservice_database_add](#test-84)
- [Test 85: applicationinsights_recommendation_list](#test-85)
- [Test 86: applicationinsights_recommendation_list](#test-86)
- [Test 87: applicationinsights_recommendation_list](#test-87)
- [Test 88: applicationinsights_recommendation_list](#test-88)
- [Test 89: extension_cli_generate](#test-89)
- [Test 90: extension_cli_generate](#test-90)
- [Test 91: extension_cli_generate](#test-91)
- [Test 92: extension_cli_install](#test-92)
- [Test 93: extension_cli_install](#test-93)
- [Test 94: extension_cli_install](#test-94)
- [Test 95: acr_registry_list](#test-95)
- [Test 96: acr_registry_list](#test-96)
- [Test 97: acr_registry_list](#test-97)
- [Test 98: acr_registry_list](#test-98)
- [Test 99: acr_registry_list](#test-99)
- [Test 100: acr_registry_repository_list](#test-100)
- [Test 101: acr_registry_repository_list](#test-101)
- [Test 102: acr_registry_repository_list](#test-102)
- [Test 103: acr_registry_repository_list](#test-103)
- [Test 104: communication_email_send](#test-104)
- [Test 105: communication_email_send](#test-105)
- [Test 106: communication_email_send](#test-106)
- [Test 107: communication_email_send](#test-107)
- [Test 108: communication_email_send](#test-108)
- [Test 109: communication_email_send](#test-109)
- [Test 110: communication_email_send](#test-110)
- [Test 111: communication_email_send](#test-111)
- [Test 112: communication_sms_send](#test-112)
- [Test 113: communication_sms_send](#test-113)
- [Test 114: communication_sms_send](#test-114)
- [Test 115: communication_sms_send](#test-115)
- [Test 116: communication_sms_send](#test-116)
- [Test 117: communication_sms_send](#test-117)
- [Test 118: communication_sms_send](#test-118)
- [Test 119: communication_sms_send](#test-119)
- [Test 120: confidentialledger_entries_append](#test-120)
- [Test 121: confidentialledger_entries_append](#test-121)
- [Test 122: confidentialledger_entries_append](#test-122)
- [Test 123: confidentialledger_entries_append](#test-123)
- [Test 124: confidentialledger_entries_append](#test-124)
- [Test 125: confidentialledger_entries_get](#test-125)
- [Test 126: confidentialledger_entries_get](#test-126)
- [Test 127: cosmos_account_list](#test-127)
- [Test 128: cosmos_account_list](#test-128)
- [Test 129: cosmos_account_list](#test-129)
- [Test 130: cosmos_database_container_item_query](#test-130)
- [Test 131: cosmos_database_container_list](#test-131)
- [Test 132: cosmos_database_container_list](#test-132)
- [Test 133: cosmos_database_list](#test-133)
- [Test 134: cosmos_database_list](#test-134)
- [Test 135: kusto_cluster_get](#test-135)
- [Test 136: kusto_cluster_list](#test-136)
- [Test 137: kusto_cluster_list](#test-137)
- [Test 138: kusto_cluster_list](#test-138)
- [Test 139: kusto_database_list](#test-139)
- [Test 140: kusto_database_list](#test-140)
- [Test 141: kusto_query](#test-141)
- [Test 142: kusto_sample](#test-142)
- [Test 143: kusto_table_list](#test-143)
- [Test 144: kusto_table_list](#test-144)
- [Test 145: kusto_table_schema](#test-145)
- [Test 146: mysql_database_list](#test-146)
- [Test 147: mysql_database_list](#test-147)
- [Test 148: mysql_database_query](#test-148)
- [Test 149: mysql_server_config_get](#test-149)
- [Test 150: mysql_server_list](#test-150)
- [Test 151: mysql_server_list](#test-151)
- [Test 152: mysql_server_list](#test-152)
- [Test 153: mysql_server_param_get](#test-153)
- [Test 154: mysql_server_param_set](#test-154)
- [Test 155: mysql_table_list](#test-155)
- [Test 156: mysql_table_list](#test-156)
- [Test 157: mysql_table_schema_get](#test-157)
- [Test 158: postgres_database_list](#test-158)
- [Test 159: postgres_database_list](#test-159)
- [Test 160: postgres_database_query](#test-160)
- [Test 161: postgres_server_config_get](#test-161)
- [Test 162: postgres_server_list](#test-162)
- [Test 163: postgres_server_list](#test-163)
- [Test 164: postgres_server_list](#test-164)
- [Test 165: postgres_server_param_get](#test-165)
- [Test 166: postgres_server_param_set](#test-166)
- [Test 167: postgres_table_list](#test-167)
- [Test 168: postgres_table_list](#test-168)
- [Test 169: postgres_table_schema_get](#test-169)
- [Test 170: deploy_app_logs_get](#test-170)
- [Test 171: deploy_architecture_diagram_generate](#test-171)
- [Test 172: deploy_iac_rules_get](#test-172)
- [Test 173: deploy_pipeline_guidance_get](#test-173)
- [Test 174: deploy_plan_get](#test-174)
- [Test 175: eventgrid_events_publish](#test-175)
- [Test 176: eventgrid_events_publish](#test-176)
- [Test 177: eventgrid_events_publish](#test-177)
- [Test 178: eventgrid_topic_list](#test-178)
- [Test 179: eventgrid_topic_list](#test-179)
- [Test 180: eventgrid_topic_list](#test-180)
- [Test 181: eventgrid_topic_list](#test-181)
- [Test 182: eventgrid_subscription_list](#test-182)
- [Test 183: eventgrid_subscription_list](#test-183)
- [Test 184: eventgrid_subscription_list](#test-184)
- [Test 185: eventgrid_subscription_list](#test-185)
- [Test 186: eventgrid_subscription_list](#test-186)
- [Test 187: eventgrid_subscription_list](#test-187)
- [Test 188: eventgrid_subscription_list](#test-188)
- [Test 189: eventhubs_eventhub_consumergroup_delete](#test-189)
- [Test 190: eventhubs_eventhub_consumergroup_get](#test-190)
- [Test 191: eventhubs_eventhub_consumergroup_get](#test-191)
- [Test 192: eventhubs_eventhub_consumergroup_update](#test-192)
- [Test 193: eventhubs_eventhub_consumergroup_update](#test-193)
- [Test 194: eventhubs_eventhub_delete](#test-194)
- [Test 195: eventhubs_eventhub_get](#test-195)
- [Test 196: eventhubs_eventhub_get](#test-196)
- [Test 197: eventhubs_eventhub_update](#test-197)
- [Test 198: eventhubs_eventhub_update](#test-198)
- [Test 199: eventhubs_namespace_delete](#test-199)
- [Test 200: eventhubs_namespace_get](#test-200)
- [Test 201: eventhubs_namespace_get](#test-201)
- [Test 202: eventhubs_namespace_update](#test-202)
- [Test 203: eventhubs_namespace_update](#test-203)
- [Test 204: functionapp_get](#test-204)
- [Test 205: functionapp_get](#test-205)
- [Test 206: functionapp_get](#test-206)
- [Test 207: functionapp_get](#test-207)
- [Test 208: functionapp_get](#test-208)
- [Test 209: functionapp_get](#test-209)
- [Test 210: functionapp_get](#test-210)
- [Test 211: functionapp_get](#test-211)
- [Test 212: functionapp_get](#test-212)
- [Test 213: functionapp_get](#test-213)
- [Test 214: functionapp_get](#test-214)
- [Test 215: functionapp_get](#test-215)
- [Test 216: keyvault_admin_settings_get](#test-216)
- [Test 217: keyvault_admin_settings_get](#test-217)
- [Test 218: keyvault_admin_settings_get](#test-218)
- [Test 219: keyvault_certificate_create](#test-219)
- [Test 220: keyvault_certificate_create](#test-220)
- [Test 221: keyvault_certificate_create](#test-221)
- [Test 222: keyvault_certificate_create](#test-222)
- [Test 223: keyvault_certificate_create](#test-223)
- [Test 224: keyvault_certificate_get](#test-224)
- [Test 225: keyvault_certificate_get](#test-225)
- [Test 226: keyvault_certificate_get](#test-226)
- [Test 227: keyvault_certificate_get](#test-227)
- [Test 228: keyvault_certificate_get](#test-228)
- [Test 229: keyvault_certificate_import](#test-229)
- [Test 230: keyvault_certificate_import](#test-230)
- [Test 231: keyvault_certificate_import](#test-231)
- [Test 232: keyvault_certificate_import](#test-232)
- [Test 233: keyvault_certificate_import](#test-233)
- [Test 234: keyvault_certificate_list](#test-234)
- [Test 235: keyvault_certificate_list](#test-235)
- [Test 236: keyvault_certificate_list](#test-236)
- [Test 237: keyvault_certificate_list](#test-237)
- [Test 238: keyvault_certificate_list](#test-238)
- [Test 239: keyvault_certificate_list](#test-239)
- [Test 240: keyvault_key_create](#test-240)
- [Test 241: keyvault_key_create](#test-241)
- [Test 242: keyvault_key_create](#test-242)
- [Test 243: keyvault_key_create](#test-243)
- [Test 244: keyvault_key_create](#test-244)
- [Test 245: keyvault_key_get](#test-245)
- [Test 246: keyvault_key_get](#test-246)
- [Test 247: keyvault_key_get](#test-247)
- [Test 248: keyvault_key_get](#test-248)
- [Test 249: keyvault_key_get](#test-249)
- [Test 250: keyvault_key_list](#test-250)
- [Test 251: keyvault_key_list](#test-251)
- [Test 252: keyvault_key_list](#test-252)
- [Test 253: keyvault_key_list](#test-253)
- [Test 254: keyvault_key_list](#test-254)
- [Test 255: keyvault_key_list](#test-255)
- [Test 256: keyvault_secret_create](#test-256)
- [Test 257: keyvault_secret_create](#test-257)
- [Test 258: keyvault_secret_create](#test-258)
- [Test 259: keyvault_secret_create](#test-259)
- [Test 260: keyvault_secret_create](#test-260)
- [Test 261: keyvault_secret_get](#test-261)
- [Test 262: keyvault_secret_get](#test-262)
- [Test 263: keyvault_secret_get](#test-263)
- [Test 264: keyvault_secret_get](#test-264)
- [Test 265: keyvault_secret_get](#test-265)
- [Test 266: keyvault_secret_list](#test-266)
- [Test 267: keyvault_secret_list](#test-267)
- [Test 268: keyvault_secret_list](#test-268)
- [Test 269: keyvault_secret_list](#test-269)
- [Test 270: keyvault_secret_list](#test-270)
- [Test 271: keyvault_secret_list](#test-271)
- [Test 272: aks_cluster_get](#test-272)
- [Test 273: aks_cluster_get](#test-273)
- [Test 274: aks_cluster_get](#test-274)
- [Test 275: aks_cluster_get](#test-275)
- [Test 276: aks_cluster_get](#test-276)
- [Test 277: aks_cluster_get](#test-277)
- [Test 278: aks_cluster_get](#test-278)
- [Test 279: aks_nodepool_get](#test-279)
- [Test 280: aks_nodepool_get](#test-280)
- [Test 281: aks_nodepool_get](#test-281)
- [Test 282: aks_nodepool_get](#test-282)
- [Test 283: aks_nodepool_get](#test-283)
- [Test 284: aks_nodepool_get](#test-284)
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

---

## Test 1

**Expected Tool:** `foundry_agents_connect`  
**Prompt:** Query an agent in my Azure AI foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881793 | `foundry_agents_connect` | ✅ **EXPECTED** |
| 2 | 0.870328 | `foundry_agents_list` | ❌ |
| 3 | 0.834069 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.831692 | `search_index_query` | ❌ |
| 5 | 0.830610 | `foundry_openai_models-list` | ❌ |

---

## Test 2

**Expected Tool:** `foundry_agents_evaluate`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.816702 | `foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.799720 | `foundry_agents_connect` | ❌ |
| 3 | 0.774396 | `foundry_agents_evaluate` | ✅ **EXPECTED** |
| 4 | 0.741379 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.731191 | `search_index_query` | ❌ |

---

## Test 3

**Expected Tool:** `foundry_agents_list`  
**Prompt:** List all agents in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.904196 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.857844 | `foundry_openai_models-list` | ❌ |
| 3 | 0.827685 | `foundry_agents_connect` | ❌ |
| 4 | 0.827196 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.826253 | `search_service_list` | ❌ |

---

## Test 4

**Expected Tool:** `foundry_agents_list`  
**Prompt:** Show me the available agents in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.890393 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.857190 | `foundry_openai_models-list` | ❌ |
| 3 | 0.831875 | `foundry_agents_connect` | ❌ |
| 4 | 0.830352 | `foundry_resource_get` | ❌ |
| 5 | 0.827807 | `foundry_models_deployments_list` | ❌ |

---

## Test 5

**Expected Tool:** `foundry_agents_query-and-evaluate`  
**Prompt:** Query and evaluate an agent in my Azure AI Foundry resource for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.846345 | `foundry_agents_connect` | ❌ |
| 2 | 0.815235 | `foundry_agents_list` | ❌ |
| 3 | 0.792655 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.784386 | `foundry_models_deploy` | ❌ |
| 5 | 0.783527 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 6

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868507 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.841394 | `foundry_agents_list` | ❌ |
| 3 | 0.827830 | `foundry_models_deployments_list` | ❌ |
| 4 | 0.821285 | `foundry_openai_models-list` | ❌ |
| 5 | 0.804421 | `foundry_resource_get` | ❌ |

---

## Test 7

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** Show me the knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.831844 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.818907 | `foundry_agents_list` | ❌ |
| 3 | 0.816956 | `foundry_models_deployments_list` | ❌ |
| 4 | 0.801182 | `foundry_openai_models-list` | ❌ |
| 5 | 0.800072 | `foundry_knowledge_index_schema` | ❌ |

---

## Test 8

**Expected Tool:** `foundry_knowledge_index_schema`  
**Prompt:** Show me the schema for knowledge index <index-name> in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.878392 | `foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.829232 | `foundry_resource_get` | ❌ |
| 3 | 0.826422 | `kusto_table_schema` | ❌ |
| 4 | 0.814568 | `foundry_openai_models-list` | ❌ |
| 5 | 0.813466 | `foundry_knowledge_index_list` | ❌ |

---

## Test 9

**Expected Tool:** `foundry_knowledge_index_schema`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.849814 | `foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.784873 | `kusto_table_schema` | ❌ |
| 3 | 0.784505 | `postgres_table_schema_get` | ❌ |
| 4 | 0.778622 | `postgres_server_config_get` | ❌ |
| 5 | 0.757097 | `search_index_get` | ❌ |

---

## Test 10

**Expected Tool:** `foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.837512 | `foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.745079 | `foundry_openai_embeddings-create` | ❌ |
| 3 | 0.744135 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.744065 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.739731 | `deploy_plan_get` | ❌ |

---

## Test 11

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.888006 | `foundry_openai_models-list` | ❌ |
| 2 | 0.883021 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 3 | 0.838549 | `foundry_models_deploy` | ❌ |
| 4 | 0.826723 | `foundry_resource_get` | ❌ |
| 5 | 0.826350 | `foundry_agents_list` | ❌ |

---

## Test 12

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** Show me all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.877974 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.877493 | `foundry_openai_models-list` | ❌ |
| 3 | 0.847226 | `foundry_models_deploy` | ❌ |
| 4 | 0.829691 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.827027 | `foundry_resource_get` | ❌ |

---

## Test 13

**Expected Tool:** `foundry_models_list`  
**Prompt:** List all AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.859783 | `foundry_openai_models-list` | ❌ |
| 2 | 0.843346 | `foundry_models_deployments_list` | ❌ |
| 3 | 0.836513 | `foundry_models_list` | ✅ **EXPECTED** |
| 4 | 0.822032 | `foundry_agents_list` | ❌ |
| 5 | 0.820567 | `foundry_resource_get` | ❌ |

---

## Test 14

**Expected Tool:** `foundry_models_list`  
**Prompt:** Show me the available AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.846452 | `foundry_openai_models-list` | ❌ |
| 2 | 0.832450 | `foundry_models_list` | ✅ **EXPECTED** |
| 3 | 0.831551 | `foundry_models_deployments_list` | ❌ |
| 4 | 0.821665 | `foundry_models_deploy` | ❌ |
| 5 | 0.813178 | `foundry_openai_create-completion` | ❌ |

---

## Test 15

**Expected Tool:** `foundry_openai_chat-completions-create`  
**Prompt:** Create a chat completion with the message "Hello, how are you today?" using my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.899423 | `foundry_openai_chat-completions-create` | ✅ **EXPECTED** |
| 2 | 0.869771 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.801101 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.796614 | `foundry_agents_connect` | ❌ |
| 5 | 0.795629 | `speech_stt_recognize` | ❌ |

---

## Test 16

**Expected Tool:** `foundry_openai_create-completion`  
**Prompt:** Create a completion with the prompt "What is Azure?" using my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.902958 | `foundry_openai_create-completion` | ✅ **EXPECTED** |
| 2 | 0.865244 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.811196 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.797768 | `foundry_agents_connect` | ❌ |
| 5 | 0.795332 | `foundry_models_deploy` | ❌ |

---

## Test 17

**Expected Tool:** `foundry_openai_embeddings-create`  
**Prompt:** Generate embeddings for the text "Azure OpenAI Service" using my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.922888 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.855480 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.838077 | `foundry_models_deploy` | ❌ |
| 4 | 0.832414 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.826502 | `foundry_openai_models-list` | ❌ |

---

## Test 18

**Expected Tool:** `foundry_openai_embeddings-create`  
**Prompt:** Create vector embeddings for my text using my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.929172 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.838973 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.817051 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.814583 | `foundry_models_deploy` | ❌ |
| 5 | 0.810202 | `foundry_openai_models-list` | ❌ |

---

## Test 19

**Expected Tool:** `foundry_openai_models-list`  
**Prompt:** List all available OpenAI models in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.918503 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.867696 | `foundry_models_deployments_list` | ❌ |
| 3 | 0.856405 | `foundry_models_deploy` | ❌ |
| 4 | 0.854275 | `foundry_resource_get` | ❌ |
| 5 | 0.851571 | `foundry_models_list` | ❌ |

---

## Test 20

**Expected Tool:** `foundry_openai_models-list`  
**Prompt:** Show me the OpenAI model deployments in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.906966 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.887493 | `foundry_models_deployments_list` | ❌ |
| 3 | 0.885840 | `foundry_models_deploy` | ❌ |
| 4 | 0.863420 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.854245 | `foundry_openai_create-completion` | ❌ |

---

## Test 21

**Expected Tool:** `foundry_resource_get`  
**Prompt:** List all AI Foundry resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.854192 | `search_service_list` | ❌ |
| 2 | 0.852660 | `foundry_openai_models-list` | ❌ |
| 3 | 0.851024 | `foundry_resource_get` | ✅ **EXPECTED** |
| 4 | 0.830298 | `foundry_agents_list` | ❌ |
| 5 | 0.825627 | `foundry_models_deployments_list` | ❌ |

---

## Test 22

**Expected Tool:** `foundry_resource_get`  
**Prompt:** Show me the AI Foundry resources in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.847986 | `foundry_openai_models-list` | ❌ |
| 2 | 0.833935 | `foundry_resource_get` | ✅ **EXPECTED** |
| 3 | 0.829025 | `foundry_agents_list` | ❌ |
| 4 | 0.827415 | `foundry_models_deploy` | ❌ |
| 5 | 0.827211 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 23

**Expected Tool:** `foundry_resource_get`  
**Prompt:** Get details for AI Foundry resource <resource_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.856134 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.852295 | `foundry_openai_models-list` | ❌ |
| 3 | 0.816948 | `foundry_agents_list` | ❌ |
| 4 | 0.816194 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.811930 | `foundry_openai_create-completion` | ❌ |

---

## Test 24

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.883581 | `search_service_list` | ❌ |
| 2 | 0.874295 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 3 | 0.845190 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.835400 | `search_index_query` | ❌ |
| 5 | 0.831530 | `search_knowledge_source_get` | ❌ |

---

## Test 25

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.861872 | `search_service_list` | ❌ |
| 2 | 0.856137 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 3 | 0.843258 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.839577 | `search_index_query` | ❌ |
| 5 | 0.823722 | `search_index_get` | ❌ |

---

## Test 26

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851712 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.827051 | `search_knowledge_base_retrieve` | ❌ |
| 3 | 0.819336 | `search_service_list` | ❌ |
| 4 | 0.798747 | `search_knowledge_source_get` | ❌ |
| 5 | 0.796343 | `postgres_database_list` | ❌ |

---

## Test 27

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.830523 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.820975 | `search_knowledge_base_retrieve` | ❌ |
| 3 | 0.802298 | `search_service_list` | ❌ |
| 4 | 0.779583 | `search_knowledge_source_get` | ❌ |
| 5 | 0.778498 | `aks_cluster_get` | ❌ |

---

## Test 28

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Get the details of knowledge base <agent-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.857497 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.855167 | `search_service_list` | ❌ |
| 3 | 0.842658 | `search_index_query` | ❌ |
| 4 | 0.840920 | `search_index_get` | ❌ |
| 5 | 0.838583 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 29

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818518 | `search_knowledge_base_retrieve` | ❌ |
| 2 | 0.805246 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 3 | 0.799337 | `search_service_list` | ❌ |
| 4 | 0.771196 | `search_knowledge_source_get` | ❌ |
| 5 | 0.770715 | `search_index_query` | ❌ |

---

## Test 30

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in Azure AI Search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.867527 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.850080 | `foundry_agents_connect` | ❌ |
| 3 | 0.845408 | `search_index_query` | ❌ |
| 4 | 0.810555 | `search_service_list` | ❌ |
| 5 | 0.804040 | `postgres_database_query` | ❌ |

---

## Test 31

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.834849 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.801363 | `foundry_agents_connect` | ❌ |
| 3 | 0.794166 | `search_knowledge_base_get` | ❌ |
| 4 | 0.783103 | `search_index_query` | ❌ |
| 5 | 0.779504 | `foundry_agents_query-and-evaluate` | ❌ |

---

## Test 32

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.855967 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.805741 | `foundry_agents_connect` | ❌ |
| 3 | 0.789843 | `foundry_agents_evaluate` | ❌ |
| 4 | 0.787000 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.783063 | `postgres_database_query` | ❌ |

---

## Test 33

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.834849 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.801363 | `foundry_agents_connect` | ❌ |
| 3 | 0.794166 | `search_knowledge_base_get` | ❌ |
| 4 | 0.783103 | `search_index_query` | ❌ |
| 5 | 0.779504 | `foundry_agents_query-and-evaluate` | ❌ |

---

## Test 34

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Query knowledge base <agent-name> in search service <service-name> about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.826851 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.806853 | `foundry_agents_connect` | ❌ |
| 3 | 0.795963 | `search_index_query` | ❌ |
| 4 | 0.791363 | `kusto_query` | ❌ |
| 5 | 0.790295 | `postgres_database_query` | ❌ |

---

## Test 35

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Search knowledge base <agent-name> in Azure AI Search service <service-name> for <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.855258 | `search_index_query` | ❌ |
| 2 | 0.837748 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 3 | 0.836542 | `search_service_list` | ❌ |
| 4 | 0.832405 | `foundry_agents_connect` | ❌ |
| 5 | 0.803245 | `postgres_database_query` | ❌ |

---

## Test 36

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** What does knowledge base <agent-name> in search service <service-name> know about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.810631 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.783115 | `foundry_agents_connect` | ❌ |
| 3 | 0.777486 | `search_knowledge_base_get` | ❌ |
| 4 | 0.773467 | `search_index_query` | ❌ |
| 5 | 0.764916 | `foundry_agents_query-and-evaluate` | ❌ |

---

## Test 37

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Find information about <query> using knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.828871 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.794857 | `foundry_agents_connect` | ❌ |
| 3 | 0.789486 | `search_knowledge_base_get` | ❌ |
| 4 | 0.782309 | `kusto_query` | ❌ |
| 5 | 0.781063 | `search_index_query` | ❌ |

---

## Test 38

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.888833 | `search_service_list` | ❌ |
| 2 | 0.870413 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 3 | 0.842855 | `search_knowledge_base_get` | ❌ |
| 4 | 0.842109 | `search_index_query` | ❌ |
| 5 | 0.829867 | `search_index_get` | ❌ |

---

## Test 39

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868552 | `search_service_list` | ❌ |
| 2 | 0.851623 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 3 | 0.842740 | `search_index_query` | ❌ |
| 4 | 0.830612 | `search_knowledge_base_get` | ❌ |
| 5 | 0.827743 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 40

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.836042 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.817888 | `search_service_list` | ❌ |
| 3 | 0.810200 | `search_knowledge_base_get` | ❌ |
| 4 | 0.804014 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.780384 | `aks_cluster_get` | ❌ |

---

## Test 41

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.810764 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.803303 | `search_service_list` | ❌ |
| 3 | 0.795723 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.792056 | `search_knowledge_base_get` | ❌ |
| 5 | 0.774091 | `applens_resource_diagnose` | ❌ |

---

## Test 42

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Get the details of knowledge source <source-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.880060 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.849916 | `search_service_list` | ❌ |
| 3 | 0.842498 | `search_index_get` | ❌ |
| 4 | 0.841087 | `search_knowledge_base_get` | ❌ |
| 5 | 0.834899 | `search_index_query` | ❌ |

---

## Test 43

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge source <source-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.801259 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.785640 | `search_knowledge_base_retrieve` | ❌ |
| 3 | 0.780791 | `search_service_list` | ❌ |
| 4 | 0.769001 | `search_knowledge_base_get` | ❌ |
| 5 | 0.760896 | `postgres_database_query` | ❌ |

---

## Test 44

**Expected Tool:** `search_index_get`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.842818 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.819586 | `search_service_list` | ❌ |
| 3 | 0.818524 | `search_index_query` | ❌ |
| 4 | 0.797119 | `foundry_resource_get` | ❌ |
| 5 | 0.778269 | `kusto_table_schema` | ❌ |

---

## Test 45

**Expected Tool:** `search_index_get`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.835471 | `search_service_list` | ❌ |
| 2 | 0.820695 | `search_index_get` | ✅ **EXPECTED** |
| 3 | 0.794088 | `search_index_query` | ❌ |
| 4 | 0.783545 | `kusto_cluster_list` | ❌ |
| 5 | 0.772731 | `foundry_resource_get` | ❌ |

---

## Test 46

**Expected Tool:** `search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.822042 | `search_service_list` | ❌ |
| 2 | 0.816723 | `search_index_get` | ✅ **EXPECTED** |
| 3 | 0.800452 | `search_index_query` | ❌ |
| 4 | 0.775644 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.775571 | `foundry_resource_get` | ❌ |

---

## Test 47

**Expected Tool:** `search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.811321 | `search_index_query` | ✅ **EXPECTED** |
| 2 | 0.803180 | `search_service_list` | ❌ |
| 3 | 0.782806 | `postgres_database_query` | ❌ |
| 4 | 0.779808 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.779115 | `kusto_query` | ❌ |

---

## Test 48

**Expected Tool:** `search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.893047 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.816139 | `kusto_cluster_list` | ❌ |
| 3 | 0.805669 | `redis_list` | ❌ |
| 4 | 0.800641 | `foundry_resource_get` | ❌ |
| 5 | 0.792097 | `marketplace_product_list` | ❌ |

---

## Test 49

**Expected Tool:** `search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.863446 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.806040 | `redis_list` | ❌ |
| 3 | 0.804274 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.795736 | `marketplace_product_list` | ❌ |
| 5 | 0.794684 | `foundry_resource_get` | ❌ |

---

## Test 50

**Expected Tool:** `search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813442 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.789942 | `search_index_query` | ❌ |
| 3 | 0.786346 | `deploy_app_logs_get` | ❌ |
| 4 | 0.784980 | `foundry_resource_get` | ❌ |
| 5 | 0.779511 | `applens_resource_diagnose` | ❌ |

---

## Test 51

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert this audio file to text using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.907718 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.788713 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.784881 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.780943 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.771735 | `search_index_query` | ❌ |

---

## Test 52

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Recognize speech from my audio file with language detection  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.871430 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.735317 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.727502 | `cloudarchitect_design` | ❌ |
| 4 | 0.727033 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.726304 | `foundry_models_deploy` | ❌ |

---

## Test 53

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe speech from audio file <file_path> with profanity filtering  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.800249 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.676003 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.672941 | `mysql_database_query` | ❌ |
| 4 | 0.666322 | `extension_cli_generate` | ❌ |
| 5 | 0.664659 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 54

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text from audio file <file_path> using endpoint <endpoint>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.833756 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.718511 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.708149 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.705456 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.696789 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 55

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe the audio file <file_path> in Spanish language  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.801635 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.705158 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.701174 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.700967 | `extension_cli_generate` | ❌ |
| 5 | 0.698001 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 56

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text with detailed output format from audio file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813033 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.697441 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.683163 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.681918 | `extension_cli_generate` | ❌ |
| 5 | 0.680727 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 57

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Recognize speech from <file_path> with phrase hints for better accuracy  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.820376 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.720565 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.713195 | `extension_cli_generate` | ❌ |
| 4 | 0.711085 | `cloudarchitect_design` | ❌ |
| 5 | 0.705694 | `applens_resource_diagnose` | ❌ |

---

## Test 58

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe audio using multiple phrase hints: "Azure", "cognitive services", "machine learning"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851789 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.795157 | `cloudarchitect_design` | ❌ |
| 3 | 0.786191 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.784342 | `applens_resource_diagnose` | ❌ |
| 5 | 0.782812 | `search_index_query` | ❌ |

---

## Test 59

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text with comma-separated phrase hints: "Azure, cognitive services, API"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.849995 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.807433 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.796005 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.785389 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.783810 | `search_index_query` | ❌ |

---

## Test 60

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe audio with raw profanity output from file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.782169 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.672639 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.670181 | `extension_cli_generate` | ❌ |
| 4 | 0.667653 | `foundry_agents_connect` | ❌ |
| 5 | 0.664799 | `extension_azqr` | ❌ |

---

## Test 61

**Expected Tool:** `appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.921068 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.811400 | `appconfig_kv_get` | ❌ |
| 3 | 0.809799 | `kusto_cluster_list` | ❌ |
| 4 | 0.804719 | `redis_list` | ❌ |
| 5 | 0.803314 | `subscription_list` | ❌ |

---

## Test 62

**Expected Tool:** `appconfig_account_list`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881722 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.805855 | `redis_list` | ❌ |
| 3 | 0.804700 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.803939 | `appconfig_kv_get` | ❌ |
| 5 | 0.799546 | `eventgrid_topic_list` | ❌ |

---

## Test 63

**Expected Tool:** `appconfig_account_list`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.838378 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.806416 | `appconfig_kv_get` | ❌ |
| 3 | 0.771104 | `deploy_app_logs_get` | ❌ |
| 4 | 0.762381 | `appconfig_kv_set` | ❌ |
| 5 | 0.754373 | `postgres_server_config_get` | ❌ |

---

## Test 64

**Expected Tool:** `appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.828986 | `appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.786760 | `appconfig_kv_set` | ❌ |
| 3 | 0.786360 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.782200 | `appconfig_kv_get` | ❌ |
| 5 | 0.738970 | `appconfig_account_list` | ❌ |

---

## Test 65

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.871372 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.838423 | `appconfig_account_list` | ❌ |
| 3 | 0.822332 | `appconfig_kv_set` | ❌ |
| 4 | 0.792724 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.788942 | `appconfig_kv_delete` | ❌ |

---

## Test 66

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.860199 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.828954 | `appconfig_kv_set` | ❌ |
| 3 | 0.811705 | `appconfig_account_list` | ❌ |
| 4 | 0.802938 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.795144 | `appconfig_kv_delete` | ❌ |

---

## Test 67

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** List all key-value settings with key name starting with 'prod-' in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.822577 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.798993 | `appconfig_account_list` | ❌ |
| 3 | 0.773696 | `appconfig_kv_set` | ❌ |
| 4 | 0.751419 | `appconfig_kv_delete` | ❌ |
| 5 | 0.747242 | `appconfig_kv_lock_set` | ❌ |

---

## Test 68

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.821011 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.815229 | `appconfig_kv_set` | ❌ |
| 3 | 0.776697 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.776106 | `appconfig_account_list` | ❌ |
| 5 | 0.762513 | `appconfig_kv_delete` | ❌ |

---

## Test 69

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.838178 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.798798 | `appconfig_kv_set` | ❌ |
| 3 | 0.784101 | `appconfig_kv_get` | ❌ |
| 4 | 0.782583 | `appconfig_kv_delete` | ❌ |
| 5 | 0.738791 | `appconfig_account_list` | ❌ |

---

## Test 70

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.834308 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.801828 | `appconfig_kv_set` | ❌ |
| 3 | 0.798289 | `appconfig_kv_get` | ❌ |
| 4 | 0.783738 | `appconfig_kv_delete` | ❌ |
| 5 | 0.751469 | `appconfig_account_list` | ❌ |

---

## Test 71

**Expected Tool:** `appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843491 | `appconfig_kv_set` | ✅ **EXPECTED** |
| 2 | 0.814172 | `appconfig_kv_lock_set` | ❌ |
| 3 | 0.790744 | `appconfig_kv_get` | ❌ |
| 4 | 0.788584 | `appconfig_kv_delete` | ❌ |
| 5 | 0.751340 | `mysql_server_param_set` | ❌ |

---

## Test 72

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.860204 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.808555 | `deploy_app_logs_get` | ❌ |
| 3 | 0.763654 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.755670 | `cloudarchitect_design` | ❌ |
| 5 | 0.741877 | `extension_cli_generate` | ❌ |

---

## Test 73

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** Use app lens to check why my app is slow?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.834916 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.794117 | `deploy_app_logs_get` | ❌ |
| 3 | 0.743505 | `cloudarchitect_design` | ❌ |
| 4 | 0.737515 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.723613 | `quota_usage_check` | ❌ |

---

## Test 74

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.827699 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.773674 | `deploy_app_logs_get` | ❌ |
| 3 | 0.728346 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.727222 | `cloudarchitect_design` | ❌ |
| 5 | 0.721066 | `extension_cli_install` | ❌ |

---

## Test 75

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection <connection_string> to my app service <app_name> for database <database_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.897919 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.754033 | `mysql_server_list` | ❌ |
| 3 | 0.746018 | `kusto_table_list` | ❌ |
| 4 | 0.744550 | `sql_db_create` | ❌ |
| 5 | 0.743978 | `mysql_database_list` | ❌ |

---

## Test 76

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure SQL Server database <database_name> for app service <app_name> with connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.884527 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.773900 | `sql_db_create` | ❌ |
| 3 | 0.772206 | `sql_server_delete` | ❌ |
| 4 | 0.769539 | `sql_db_update` | ❌ |
| 5 | 0.768535 | `sql_server_show` | ❌ |

---

## Test 77

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add MySQL database <database_name> to app service <app_name> using connection <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881476 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.769664 | `mysql_server_list` | ❌ |
| 3 | 0.757291 | `mysql_database_list` | ❌ |
| 4 | 0.756193 | `mysql_table_list` | ❌ |
| 5 | 0.749496 | `kusto_table_list` | ❌ |

---

## Test 78

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add PostgreSQL database <database_name> to app service <app_name> using connection <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.872626 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.766107 | `postgres_database_query` | ❌ |
| 3 | 0.765076 | `postgres_database_list` | ❌ |
| 4 | 0.753598 | `postgres_server_param_set` | ❌ |
| 5 | 0.751675 | `postgres_table_list` | ❌ |

---

## Test 79

**Expected Tool:** `appservice_database_add`  
**Prompt:** Connect CosmosDB database <database_name> using connection string <connection_string> to app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.869715 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.792674 | `cosmos_database_container_list` | ❌ |
| 3 | 0.792432 | `kusto_query` | ❌ |
| 4 | 0.791978 | `cosmos_database_list` | ❌ |
| 5 | 0.788846 | `cosmos_database_container_item_query` | ❌ |

---

## Test 80

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection <connection_string> for database <database_name> on server <database_server> to app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.904503 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.762192 | `mysql_server_list` | ❌ |
| 3 | 0.752833 | `sql_server_delete` | ❌ |
| 4 | 0.751123 | `sql_db_create` | ❌ |
| 5 | 0.750808 | `mysql_database_list` | ❌ |

---

## Test 81

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection string for <database_name> to app service <app_name> using connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.906557 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.746562 | `sql_db_rename` | ❌ |
| 3 | 0.739074 | `kusto_table_list` | ❌ |
| 4 | 0.736701 | `sql_db_create` | ❌ |
| 5 | 0.736203 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 82

**Expected Tool:** `appservice_database_add`  
**Prompt:** Connect database <database_name> to my app service <app_name> using connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.887945 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.765721 | `deploy_app_logs_get` | ❌ |
| 3 | 0.764405 | `mysql_server_list` | ❌ |
| 4 | 0.763303 | `kusto_query` | ❌ |
| 5 | 0.763053 | `kusto_table_list` | ❌ |

---

## Test 83

**Expected Tool:** `appservice_database_add`  
**Prompt:** Set up database <database_name> for app service <app_name> with connection string <connection_string> under resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.889735 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.778608 | `sql_db_create` | ❌ |
| 3 | 0.771983 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.771033 | `kusto_table_list` | ❌ |
| 5 | 0.766209 | `mysql_server_list` | ❌ |

---

## Test 84

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure database <database_name> for app service <app_name> with the connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.893730 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.770268 | `sql_db_create` | ❌ |
| 3 | 0.770237 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.770072 | `sql_db_update` | ❌ |
| 5 | 0.765463 | `postgres_server_param_set` | ❌ |

---

## Test 85

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.841405 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.791605 | `applens_resource_diagnose` | ❌ |
| 3 | 0.782883 | `deploy_app_logs_get` | ❌ |
| 4 | 0.781903 | `get_bestpractices_get` | ❌ |
| 5 | 0.781779 | `deploy_iac_rules_get` | ❌ |

---

## Test 86

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** Show me code optimization recommendations for all Application Insights resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.882969 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.821414 | `applens_resource_diagnose` | ❌ |
| 3 | 0.807270 | `redis_list` | ❌ |
| 4 | 0.804153 | `search_service_list` | ❌ |
| 5 | 0.802166 | `deploy_app_logs_get` | ❌ |

---

## Test 87

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.878587 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.808693 | `applens_resource_diagnose` | ❌ |
| 3 | 0.786629 | `deploy_app_logs_get` | ❌ |
| 4 | 0.780562 | `get_bestpractices_get` | ❌ |
| 5 | 0.772448 | `datadog_monitoredresources_list` | ❌ |

---

## Test 88

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815200 | `applens_resource_diagnose` | ❌ |
| 2 | 0.814016 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 3 | 0.798283 | `deploy_app_logs_get` | ❌ |
| 4 | 0.788448 | `cloudarchitect_design` | ❌ |
| 5 | 0.773014 | `deploy_iac_rules_get` | ❌ |

---

## Test 89

**Expected Tool:** `extension_cli_generate`  
**Prompt:** Create a Storage account with name <storage_account_name> using Azure CLI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.844988 | `storage_account_create` | ❌ |
| 2 | 0.840701 | `storage_blob_container_create` | ❌ |
| 3 | 0.832012 | `storage_account_get` | ❌ |
| 4 | 0.796441 | `storage_blob_container_get` | ❌ |
| 5 | 0.785067 | `keyvault_secret_create` | ❌ |

---

## Test 90

**Expected Tool:** `extension_cli_generate`  
**Prompt:** List all virtual machines in my subscription using Azure CLI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.853361 | `search_service_list` | ❌ |
| 2 | 0.842535 | `kusto_cluster_list` | ❌ |
| 3 | 0.829272 | `subscription_list` | ❌ |
| 4 | 0.824854 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.821018 | `redis_list` | ❌ |

---

## Test 91

**Expected Tool:** `extension_cli_generate`  
**Prompt:** Show me the details of the storage account <account_name> with Azure CLI commands  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.887952 | `storage_account_get` | ❌ |
| 2 | 0.855618 | `storage_blob_container_get` | ❌ |
| 3 | 0.838257 | `storage_blob_get` | ❌ |
| 4 | 0.823724 | `storage_account_create` | ❌ |
| 5 | 0.821905 | `storage_blob_container_create` | ❌ |

---

## Test 92

**Expected Tool:** `extension_cli_install`  
**Prompt:** <uninstall az cli on your machine and run test prompts for extension_cli_generate>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.805865 | `extension_cli_install` | ✅ **EXPECTED** |
| 2 | 0.805174 | `extension_cli_generate` | ❌ |
| 3 | 0.760405 | `extension_azqr` | ❌ |
| 4 | 0.757640 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.754401 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 93

**Expected Tool:** `extension_cli_install`  
**Prompt:** How to install azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.790601 | `extension_cli_install` | ✅ **EXPECTED** |
| 2 | 0.728892 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.721054 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.717521 | `extension_cli_generate` | ❌ |
| 5 | 0.714683 | `extension_azqr` | ❌ |

---

## Test 94

**Expected Tool:** `extension_cli_install`  
**Prompt:** What is Azure Functions Core tools and how to install it  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.892433 | `extension_cli_install` | ✅ **EXPECTED** |
| 2 | 0.812380 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.811273 | `extension_cli_generate` | ❌ |
| 4 | 0.803642 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.800275 | `get_bestpractices_get` | ❌ |

---

## Test 95

**Expected Tool:** `acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.893506 | `acr_registry_repository_list` | ❌ |
| 2 | 0.872986 | `acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.862416 | `kusto_cluster_list` | ❌ |
| 4 | 0.856360 | `search_service_list` | ❌ |
| 5 | 0.837896 | `redis_list` | ❌ |

---

## Test 96

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.863667 | `acr_registry_repository_list` | ❌ |
| 2 | 0.828738 | `acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.819051 | `storage_blob_container_get` | ❌ |
| 4 | 0.813630 | `quota_usage_check` | ❌ |
| 5 | 0.803314 | `redis_list` | ❌ |

---

## Test 97

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.845409 | `acr_registry_repository_list` | ❌ |
| 2 | 0.823566 | `acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.814685 | `redis_list` | ❌ |
| 4 | 0.805737 | `storage_blob_container_get` | ❌ |
| 5 | 0.804288 | `eventgrid_subscription_list` | ❌ |

---

## Test 98

**Expected Tool:** `acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.866058 | `acr_registry_repository_list` | ❌ |
| 2 | 0.833004 | `acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.788833 | `kusto_cluster_list` | ❌ |
| 4 | 0.781741 | `group_list` | ❌ |
| 5 | 0.778240 | `storage_blob_container_get` | ❌ |

---

## Test 99

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.867445 | `acr_registry_repository_list` | ❌ |
| 2 | 0.824300 | `acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.793830 | `storage_blob_container_get` | ❌ |
| 4 | 0.786488 | `redis_list` | ❌ |
| 5 | 0.776447 | `eventgrid_topic_list` | ❌ |

---

## Test 100

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.886429 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.837955 | `acr_registry_list` | ❌ |
| 3 | 0.826558 | `kusto_cluster_list` | ❌ |
| 4 | 0.813168 | `search_service_list` | ❌ |
| 5 | 0.808497 | `redis_list` | ❌ |

---

## Test 101

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.846471 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.787159 | `storage_blob_container_get` | ❌ |
| 3 | 0.771812 | `acr_registry_list` | ❌ |
| 4 | 0.768761 | `deploy_app_logs_get` | ❌ |
| 5 | 0.762557 | `redis_list` | ❌ |

---

## Test 102

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868239 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.793332 | `acr_registry_list` | ❌ |
| 3 | 0.773097 | `storage_blob_container_get` | ❌ |
| 4 | 0.759247 | `cosmos_database_container_list` | ❌ |
| 5 | 0.755409 | `kusto_database_list` | ❌ |

---

## Test 103

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.854075 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.783735 | `storage_blob_container_get` | ❌ |
| 3 | 0.779666 | `acr_registry_list` | ❌ |
| 4 | 0.753688 | `redis_list` | ❌ |
| 5 | 0.750924 | `storage_blob_get` | ❌ |

---

## Test 104

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868273 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.742303 | `communication_sms_send` | ❌ |
| 3 | 0.718726 | `eventgrid_topic_list` | ❌ |
| 4 | 0.711620 | `kusto_query` | ❌ |
| 5 | 0.711309 | `extension_cli_generate` | ❌ |

---

## Test 105

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email from my communication service to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852579 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.744727 | `communication_sms_send` | ❌ |
| 3 | 0.723001 | `speech_stt_recognize` | ❌ |
| 4 | 0.721091 | `extension_cli_generate` | ❌ |
| 5 | 0.715609 | `cloudarchitect_design` | ❌ |

---

## Test 106

**Expected Tool:** `communication_email_send`  
**Prompt:** Send HTML-formatted email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.872349 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.741165 | `communication_sms_send` | ❌ |
| 3 | 0.709321 | `eventgrid_topic_list` | ❌ |
| 4 | 0.704800 | `extension_cli_generate` | ❌ |
| 5 | 0.699148 | `eventgrid_subscription_list` | ❌ |

---

## Test 107

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with CC to <email-address-1> and <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.880319 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.749198 | `communication_sms_send` | ❌ |
| 3 | 0.715848 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.713742 | `cloudarchitect_design` | ❌ |
| 5 | 0.703625 | `deploy_iac_rules_get` | ❌ |

---

## Test 108

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email to multiple recipients: <email-address-1>, <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.904716 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.782326 | `communication_sms_send` | ❌ |
| 3 | 0.710614 | `cloudarchitect_design` | ❌ |
| 4 | 0.705415 | `eventgrid_topic_list` | ❌ |
| 5 | 0.701401 | `eventgrid_subscription_list` | ❌ |

---

## Test 109

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with reply-to address set to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.866825 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.736970 | `communication_sms_send` | ❌ |
| 3 | 0.695710 | `postgres_server_param_set` | ❌ |
| 4 | 0.695080 | `extension_cli_generate` | ❌ |
| 5 | 0.691557 | `cloudarchitect_design` | ❌ |

---

## Test 110

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with custom sender name <sender-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.837896 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.721563 | `communication_sms_send` | ❌ |
| 3 | 0.693903 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.688230 | `extension_cli_generate` | ❌ |
| 5 | 0.684073 | `deploy_app_logs_get` | ❌ |

---

## Test 111

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email with BCC recipients  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879273 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.755289 | `communication_sms_send` | ❌ |
| 3 | 0.704048 | `extension_cli_generate` | ❌ |
| 4 | 0.702136 | `cloudarchitect_design` | ❌ |
| 5 | 0.699241 | `deploy_iac_rules_get` | ❌ |

---

## Test 112

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send an SMS message to <phone-number> saying "Hello"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.820770 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.769285 | `communication_email_send` | ❌ |
| 3 | 0.708279 | `speech_stt_recognize` | ❌ |
| 4 | 0.695624 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.689916 | `foundry_openai_create-completion` | ❌ |

---

## Test 113

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS to <phone-number-2> from <phone-number-1> with message "Test message"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.821947 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.775770 | `communication_email_send` | ❌ |
| 3 | 0.690468 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.682759 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.681911 | `loadtesting_testrun_create` | ❌ |

---

## Test 114

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS to multiple recipients: <phone-number-1>, <phone-number-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.870041 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.844795 | `communication_email_send` | ❌ |
| 3 | 0.704269 | `eventgrid_topic_list` | ❌ |
| 4 | 0.701594 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.699785 | `deploy_app_logs_get` | ❌ |

---

## Test 115

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS with delivery reporting enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851135 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.775737 | `communication_email_send` | ❌ |
| 3 | 0.733053 | `deploy_app_logs_get` | ❌ |
| 4 | 0.726200 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.715318 | `eventgrid_topic_list` | ❌ |

---

## Test 116

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS message with custom tracking tag "campaign1"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.797352 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.766617 | `communication_email_send` | ❌ |
| 3 | 0.704560 | `deploy_app_logs_get` | ❌ |
| 4 | 0.699615 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.694501 | `foundry_agents_connect` | ❌ |

---

## Test 117

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send broadcast SMS to <phone-number-1> and <phone-number-2> saying "Urgent notification"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.828175 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.767957 | `communication_email_send` | ❌ |
| 3 | 0.696027 | `eventgrid_topic_list` | ❌ |
| 4 | 0.691350 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.690576 | `deploy_app_logs_get` | ❌ |

---

## Test 118

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS from my communication service to <phone-number-1>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.824706 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.788074 | `communication_email_send` | ❌ |
| 3 | 0.707065 | `speech_stt_recognize` | ❌ |
| 4 | 0.700512 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.698445 | `deploy_app_logs_get` | ❌ |

---

## Test 119

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send an SMS with delivery receipt tracking  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852343 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.784622 | `communication_email_send` | ❌ |
| 3 | 0.736237 | `deploy_app_logs_get` | ❌ |
| 4 | 0.724217 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.718669 | `eventgrid_subscription_list` | ❌ |

---

## Test 120

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Append an entry to my ledger <ledger_name> with data {"key": "value"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.777179 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.737943 | `appconfig_kv_set` | ❌ |
| 3 | 0.717614 | `keyvault_secret_create` | ❌ |
| 4 | 0.714412 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.710804 | `keyvault_key_create` | ❌ |

---

## Test 121

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Write a tamper-proof entry to ledger <ledger_name> containing {"transaction": "data"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.827993 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.743736 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.697179 | `eventgrid_events_publish` | ❌ |
| 4 | 0.693981 | `keyvault_secret_create` | ❌ |
| 5 | 0.691976 | `keyvault_certificate_import` | ❌ |

---

## Test 122

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Append {"hello": "from mcp"} to my confidential ledger <ledger_name> in collection <collection_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.782923 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.750702 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.693034 | `appconfig_kv_set` | ❌ |
| 4 | 0.686606 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.683013 | `keyvault_secret_create` | ❌ |

---

## Test 123

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Create an immutable ledger entry in <ledger_name> with content {"audit": "log"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.746159 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.721142 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.708151 | `keyvault_secret_create` | ❌ |
| 4 | 0.700277 | `appconfig_kv_set` | ❌ |
| 5 | 0.698238 | `monitor_resource_log_query` | ❌ |

---

## Test 124

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Write an entry to confidential ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.835313 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.787289 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.732012 | `keyvault_secret_create` | ❌ |
| 4 | 0.718905 | `keyvault_certificate_import` | ❌ |
| 5 | 0.718362 | `appconfig_kv_lock_set` | ❌ |

---

## Test 125

**Expected Tool:** `confidentialledger_entries_get`  
**Prompt:** Get entry from Confidential Ledger for transaction <transaction_id> on ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.873263 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.817855 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.717987 | `keyvault_certificate_get` | ❌ |
| 4 | 0.705108 | `keyvault_key_get` | ❌ |
| 5 | 0.705038 | `keyvault_secret_get` | ❌ |

---

## Test 126

**Expected Tool:** `confidentialledger_entries_get`  
**Prompt:** Get transaction <transaction_id> from ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.794213 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.734616 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.732828 | `keyvault_key_get` | ❌ |
| 4 | 0.728510 | `keyvault_certificate_get` | ❌ |
| 5 | 0.717596 | `cosmos_database_container_item_query` | ❌ |

---

## Test 127

**Expected Tool:** `cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.911865 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.853519 | `cosmos_database_list` | ❌ |
| 3 | 0.840446 | `kusto_cluster_list` | ❌ |
| 4 | 0.823181 | `subscription_list` | ❌ |
| 5 | 0.817138 | `cosmos_database_container_list` | ❌ |

---

## Test 128

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.839227 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.837729 | `cosmos_database_list` | ❌ |
| 3 | 0.809214 | `cosmos_database_container_list` | ❌ |
| 4 | 0.793189 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.780946 | `storage_account_get` | ❌ |

---

## Test 129

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.880897 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.825658 | `cosmos_database_list` | ❌ |
| 3 | 0.811468 | `kusto_cluster_list` | ❌ |
| 4 | 0.808962 | `redis_list` | ❌ |
| 5 | 0.804846 | `subscription_list` | ❌ |

---

## Test 130

**Expected Tool:** `cosmos_database_container_item_query`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.874731 | `cosmos_database_container_item_query` | ✅ **EXPECTED** |
| 2 | 0.849178 | `cosmos_database_container_list` | ❌ |
| 3 | 0.795628 | `cosmos_database_list` | ❌ |
| 4 | 0.785297 | `kusto_query` | ❌ |
| 5 | 0.785097 | `storage_blob_container_get` | ❌ |

---

## Test 131

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.921208 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.880677 | `cosmos_database_container_item_query` | ❌ |
| 3 | 0.867385 | `cosmos_database_list` | ❌ |
| 4 | 0.824280 | `cosmos_account_list` | ❌ |
| 5 | 0.809289 | `storage_blob_container_get` | ❌ |

---

## Test 132

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.900527 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.863402 | `cosmos_database_container_item_query` | ❌ |
| 3 | 0.853449 | `cosmos_database_list` | ❌ |
| 4 | 0.818800 | `storage_blob_container_get` | ❌ |
| 5 | 0.815366 | `cosmos_account_list` | ❌ |

---

## Test 133

**Expected Tool:** `cosmos_database_list`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.920574 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.858442 | `cosmos_database_container_list` | ❌ |
| 3 | 0.847820 | `cosmos_account_list` | ❌ |
| 4 | 0.844652 | `postgres_database_list` | ❌ |
| 5 | 0.831337 | `kusto_database_list` | ❌ |

---

## Test 134

**Expected Tool:** `cosmos_database_list`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.908829 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.854802 | `cosmos_database_container_list` | ❌ |
| 3 | 0.841841 | `cosmos_account_list` | ❌ |
| 4 | 0.837447 | `postgres_database_list` | ❌ |
| 5 | 0.824852 | `kusto_database_list` | ❌ |

---

## Test 135

**Expected Tool:** `kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.835257 | `kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.819405 | `kusto_cluster_list` | ❌ |
| 3 | 0.812135 | `kusto_database_list` | ❌ |
| 4 | 0.811759 | `kusto_table_schema` | ❌ |
| 5 | 0.800748 | `aks_cluster_get` | ❌ |

---

## Test 136

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.927361 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.862525 | `aks_cluster_get` | ❌ |
| 3 | 0.852044 | `kusto_database_list` | ❌ |
| 4 | 0.846470 | `kusto_cluster_get` | ❌ |
| 5 | 0.840089 | `search_service_list` | ❌ |

---

## Test 137

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.840872 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.815379 | `kusto_database_list` | ❌ |
| 3 | 0.804825 | `kusto_table_schema` | ❌ |
| 4 | 0.803327 | `kusto_cluster_get` | ❌ |
| 5 | 0.799965 | `aks_cluster_get` | ❌ |

---

## Test 138

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.886281 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.846745 | `kusto_cluster_get` | ❌ |
| 3 | 0.835073 | `aks_cluster_get` | ❌ |
| 4 | 0.828444 | `kusto_database_list` | ❌ |
| 5 | 0.823721 | `eventgrid_subscription_list` | ❌ |

---

## Test 139

**Expected Tool:** `kusto_database_list`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.884062 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.855148 | `kusto_cluster_list` | ❌ |
| 3 | 0.855056 | `kusto_table_list` | ❌ |
| 4 | 0.851531 | `postgres_database_list` | ❌ |
| 5 | 0.835723 | `cosmos_database_list` | ❌ |

---

## Test 140

**Expected Tool:** `kusto_database_list`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868395 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.840656 | `kusto_table_list` | ❌ |
| 3 | 0.838742 | `kusto_cluster_list` | ❌ |
| 4 | 0.838492 | `postgres_database_list` | ❌ |
| 5 | 0.827136 | `kusto_table_schema` | ❌ |

---

## Test 141

**Expected Tool:** `kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793703 | `kusto_query` | ✅ **EXPECTED** |
| 2 | 0.790473 | `postgres_database_query` | ❌ |
| 3 | 0.783002 | `kusto_table_list` | ❌ |
| 4 | 0.777589 | `kusto_cluster_list` | ❌ |
| 5 | 0.775796 | `kusto_sample` | ❌ |

---

## Test 142

**Expected Tool:** `kusto_sample`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852149 | `kusto_sample` | ✅ **EXPECTED** |
| 2 | 0.824247 | `kusto_table_schema` | ❌ |
| 3 | 0.799034 | `kusto_table_list` | ❌ |
| 4 | 0.793904 | `kusto_cluster_list` | ❌ |
| 5 | 0.792562 | `kusto_database_list` | ❌ |

---

## Test 143

**Expected Tool:** `kusto_table_list`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881083 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.847942 | `kusto_database_list` | ❌ |
| 3 | 0.847927 | `postgres_table_list` | ❌ |
| 4 | 0.834679 | `mysql_table_list` | ❌ |
| 5 | 0.834263 | `kusto_table_schema` | ❌ |

---

## Test 144

**Expected Tool:** `kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.864747 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.850075 | `kusto_table_schema` | ❌ |
| 3 | 0.840143 | `postgres_table_list` | ❌ |
| 4 | 0.831394 | `kusto_database_list` | ❌ |
| 5 | 0.829153 | `mysql_table_list` | ❌ |

---

## Test 145

**Expected Tool:** `kusto_table_schema`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.889776 | `kusto_table_schema` | ✅ **EXPECTED** |
| 2 | 0.832717 | `kusto_table_list` | ❌ |
| 3 | 0.832316 | `postgres_table_schema_get` | ❌ |
| 4 | 0.831456 | `mysql_table_schema_get` | ❌ |
| 5 | 0.819940 | `postgres_table_list` | ❌ |

---

## Test 146

**Expected Tool:** `mysql_database_list`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.877674 | `postgres_database_list` | ❌ |
| 2 | 0.850027 | `mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.835616 | `mysql_table_list` | ❌ |
| 4 | 0.818887 | `postgres_table_list` | ❌ |
| 5 | 0.808188 | `mysql_server_list` | ❌ |

---

## Test 147

**Expected Tool:** `mysql_database_list`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.856156 | `postgres_database_list` | ❌ |
| 2 | 0.843299 | `mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.831006 | `mysql_table_list` | ❌ |
| 4 | 0.807422 | `mysql_server_list` | ❌ |
| 5 | 0.806940 | `postgres_table_list` | ❌ |

---

## Test 148

**Expected Tool:** `mysql_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.783082 | `postgres_database_query` | ❌ |
| 2 | 0.781748 | `mysql_table_list` | ❌ |
| 3 | 0.755838 | `mysql_database_list` | ❌ |
| 4 | 0.753655 | `mysql_server_list` | ❌ |
| 5 | 0.751427 | `postgres_database_list` | ❌ |

---

## Test 149

**Expected Tool:** `mysql_server_config_get`  
**Prompt:** Show me the configuration of MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818327 | `mysql_server_config_get` | ✅ **EXPECTED** |
| 2 | 0.802899 | `postgres_server_config_get` | ❌ |
| 3 | 0.789340 | `mysql_table_schema_get` | ❌ |
| 4 | 0.786809 | `mysql_server_param_set` | ❌ |
| 5 | 0.779135 | `postgres_server_param_set` | ❌ |

---

## Test 150

**Expected Tool:** `mysql_server_list`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.864807 | `postgres_server_list` | ❌ |
| 2 | 0.824894 | `kusto_cluster_list` | ❌ |
| 3 | 0.816137 | `mysql_server_list` | ✅ **EXPECTED** |
| 4 | 0.811819 | `mysql_database_list` | ❌ |
| 5 | 0.809186 | `mysql_table_list` | ❌ |

---

## Test 151

**Expected Tool:** `mysql_server_list`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.807672 | `mysql_database_list` | ❌ |
| 2 | 0.804885 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.803458 | `mysql_table_list` | ❌ |
| 4 | 0.793162 | `postgres_database_list` | ❌ |
| 5 | 0.785478 | `mysql_server_config_get` | ❌ |

---

## Test 152

**Expected Tool:** `mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.839670 | `postgres_server_list` | ❌ |
| 2 | 0.804401 | `redis_list` | ❌ |
| 3 | 0.804145 | `mysql_server_list` | ✅ **EXPECTED** |
| 4 | 0.794522 | `kusto_cluster_list` | ❌ |
| 5 | 0.794384 | `eventgrid_subscription_list` | ❌ |

---

## Test 153

**Expected Tool:** `mysql_server_param_get`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.796692 | `mysql_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.752874 | `mysql_server_param_set` | ❌ |
| 3 | 0.751912 | `mysql_server_config_get` | ❌ |
| 4 | 0.745646 | `mysql_table_schema_get` | ❌ |
| 5 | 0.744210 | `mysql_database_query` | ❌ |

---

## Test 154

**Expected Tool:** `mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.768062 | `mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.758349 | `mysql_server_param_get` | ❌ |
| 3 | 0.750023 | `postgres_server_param_set` | ❌ |
| 4 | 0.732120 | `mysql_database_query` | ❌ |
| 5 | 0.722239 | `mysql_server_config_get` | ❌ |

---

## Test 155

**Expected Tool:** `mysql_table_list`  
**Prompt:** List all tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.869002 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.866280 | `postgres_table_list` | ❌ |
| 3 | 0.844293 | `postgres_database_list` | ❌ |
| 4 | 0.829906 | `kusto_table_list` | ❌ |
| 5 | 0.829767 | `mysql_database_list` | ❌ |

---

## Test 156

**Expected Tool:** `mysql_table_list`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.855600 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.847666 | `postgres_table_list` | ❌ |
| 3 | 0.824616 | `postgres_database_list` | ❌ |
| 4 | 0.821093 | `mysql_database_list` | ❌ |
| 5 | 0.819615 | `mysql_table_schema_get` | ❌ |

---

## Test 157

**Expected Tool:** `mysql_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851928 | `mysql_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.830822 | `postgres_table_schema_get` | ❌ |
| 3 | 0.826168 | `kusto_table_schema` | ❌ |
| 4 | 0.826033 | `mysql_table_list` | ❌ |
| 5 | 0.818184 | `postgres_table_list` | ❌ |

---

## Test 158

**Expected Tool:** `postgres_database_list`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.943988 | `postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.886663 | `postgres_table_list` | ❌ |
| 3 | 0.852392 | `postgres_server_list` | ❌ |
| 4 | 0.810673 | `postgres_server_config_get` | ❌ |
| 5 | 0.799363 | `kusto_table_list` | ❌ |

---

## Test 159

**Expected Tool:** `postgres_database_list`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.926930 | `postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.877987 | `postgres_table_list` | ❌ |
| 3 | 0.842093 | `postgres_server_list` | ❌ |
| 4 | 0.820788 | `postgres_server_config_get` | ❌ |
| 5 | 0.809743 | `postgres_table_schema_get` | ❌ |

---

## Test 160

**Expected Tool:** `postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.829593 | `postgres_database_query` | ✅ **EXPECTED** |
| 2 | 0.808740 | `postgres_database_list` | ❌ |
| 3 | 0.806504 | `postgres_table_list` | ❌ |
| 4 | 0.785447 | `postgres_server_list` | ❌ |
| 5 | 0.756511 | `postgres_server_param_get` | ❌ |

---

## Test 161

**Expected Tool:** `postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.895026 | `postgres_server_config_get` | ✅ **EXPECTED** |
| 2 | 0.859290 | `postgres_server_param_set` | ❌ |
| 3 | 0.833239 | `postgres_database_list` | ❌ |
| 4 | 0.820952 | `postgres_table_list` | ❌ |
| 5 | 0.804261 | `postgres_server_param_get` | ❌ |

---

## Test 162

**Expected Tool:** `postgres_server_list`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.946704 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.861108 | `postgres_database_list` | ❌ |
| 3 | 0.841542 | `postgres_table_list` | ❌ |
| 4 | 0.831346 | `kusto_cluster_list` | ❌ |
| 5 | 0.803790 | `search_service_list` | ❌ |

---

## Test 163

**Expected Tool:** `postgres_server_list`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.863293 | `postgres_database_list` | ❌ |
| 2 | 0.854387 | `postgres_server_list` | ✅ **EXPECTED** |
| 3 | 0.846394 | `postgres_table_list` | ❌ |
| 4 | 0.825378 | `postgres_server_config_get` | ❌ |
| 5 | 0.812839 | `postgres_server_param_set` | ❌ |

---

## Test 164

**Expected Tool:** `postgres_server_list`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.920229 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.836549 | `postgres_database_list` | ❌ |
| 3 | 0.823609 | `postgres_table_list` | ❌ |
| 4 | 0.804581 | `kusto_cluster_list` | ❌ |
| 5 | 0.801470 | `postgres_server_config_get` | ❌ |

---

## Test 165

**Expected Tool:** `postgres_server_param_get`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.827655 | `postgres_server_param_set` | ❌ |
| 2 | 0.806053 | `postgres_server_param_get` | ✅ **EXPECTED** |
| 3 | 0.800510 | `postgres_server_config_get` | ❌ |
| 4 | 0.791725 | `postgres_server_list` | ❌ |
| 5 | 0.788745 | `postgres_database_list` | ❌ |

---

## Test 166

**Expected Tool:** `postgres_server_param_set`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.846240 | `postgres_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.797091 | `postgres_server_config_get` | ❌ |
| 3 | 0.780646 | `postgres_database_list` | ❌ |
| 4 | 0.768556 | `postgres_server_list` | ❌ |
| 5 | 0.765124 | `postgres_table_list` | ❌ |

---

## Test 167

**Expected Tool:** `postgres_table_list`  
**Prompt:** List all tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.935256 | `postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.909184 | `postgres_database_list` | ❌ |
| 3 | 0.844600 | `postgres_server_list` | ❌ |
| 4 | 0.836233 | `postgres_table_schema_get` | ❌ |
| 5 | 0.824605 | `kusto_table_list` | ❌ |

---

## Test 168

**Expected Tool:** `postgres_table_list`  
**Prompt:** Show me the tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.915594 | `postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.889063 | `postgres_database_list` | ❌ |
| 3 | 0.842322 | `postgres_table_schema_get` | ❌ |
| 4 | 0.828271 | `postgres_server_list` | ❌ |
| 5 | 0.811715 | `postgres_server_config_get` | ❌ |

---

## Test 169

**Expected Tool:** `postgres_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.898503 | `postgres_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.882606 | `postgres_table_list` | ❌ |
| 3 | 0.861496 | `postgres_database_list` | ❌ |
| 4 | 0.828578 | `kusto_table_schema` | ❌ |
| 5 | 0.817162 | `postgres_server_config_get` | ❌ |

---

## Test 170

**Expected Tool:** `deploy_app_logs_get`  
**Prompt:** Show me the log of the application in resource group <resource-group-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.854430 | `deploy_app_logs_get` | ✅ **EXPECTED** |
| 2 | 0.804046 | `monitor_activitylog_list` | ❌ |
| 3 | 0.801034 | `monitor_workspace_log_query` | ❌ |
| 4 | 0.799285 | `monitor_resource_log_query` | ❌ |
| 5 | 0.796333 | `applens_resource_diagnose` | ❌ |

---

## Test 171

**Expected Tool:** `deploy_architecture_diagram_generate`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.913195 | `deploy_architecture_diagram_generate` | ✅ **EXPECTED** |
| 2 | 0.837844 | `deploy_plan_get` | ❌ |
| 3 | 0.829720 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.823312 | `cloudarchitect_design` | ❌ |
| 5 | 0.819095 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 172

**Expected Tool:** `deploy_iac_rules_get`  
**Prompt:** Show me the rules to generate scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.766731 | `extension_cli_generate` | ❌ |
| 2 | 0.752556 | `deploy_iac_rules_get` | ✅ **EXPECTED** |
| 3 | 0.741796 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.736052 | `kusto_table_schema` | ❌ |
| 5 | 0.736016 | `extension_cli_install` | ❌ |

---

## Test 173

**Expected Tool:** `deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.904455 | `deploy_pipeline_guidance_get` | ✅ **EXPECTED** |
| 2 | 0.847580 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.836244 | `deploy_plan_get` | ❌ |
| 4 | 0.804504 | `cloudarchitect_design` | ❌ |
| 5 | 0.802719 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 174

**Expected Tool:** `deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.888872 | `deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.866662 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.850645 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.844504 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.824667 | `foundry_models_deploy` | ❌ |

---

## Test 175

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Publish an event to Event Grid topic <topic_name> using <event_schema> with the following data <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.889654 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.798699 | `eventgrid_topic_list` | ❌ |
| 3 | 0.782893 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.749106 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.736760 | `kusto_table_schema` | ❌ |

---

## Test 176

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Publish event to my Event Grid topic <topic_name> with the following events <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.859413 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.820907 | `eventgrid_topic_list` | ❌ |
| 3 | 0.804106 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.756970 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.736031 | `eventhubs_eventhub_get` | ❌ |

---

## Test 177

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Send an event to Event Grid topic <topic_name> in resource group <resource_group_name> with <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.829645 | `eventgrid_topic_list` | ❌ |
| 2 | 0.823490 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 3 | 0.800179 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.761489 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.761001 | `eventhubs_eventhub_consumergroup_update` | ❌ |

---

## Test 178

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.905660 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.892916 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.804302 | `kusto_cluster_list` | ❌ |
| 4 | 0.791512 | `redis_list` | ❌ |
| 5 | 0.790915 | `search_service_list` | ❌ |

---

## Test 179

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.898506 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.892006 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.793690 | `redis_list` | ❌ |
| 4 | 0.788307 | `kusto_cluster_list` | ❌ |
| 5 | 0.785561 | `marketplace_product_list` | ❌ |

---

## Test 180

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.913227 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.894759 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.814843 | `postgres_server_list` | ❌ |
| 4 | 0.810645 | `kusto_cluster_list` | ❌ |
| 5 | 0.802982 | `redis_list` | ❌ |

---

## Test 181

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.920089 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.882339 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.837201 | `group_list` | ❌ |
| 4 | 0.820569 | `kusto_cluster_list` | ❌ |
| 5 | 0.816757 | `loadtesting_testresource_list` | ❌ |

---

## Test 182

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.888829 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.883944 | `eventgrid_topic_list` | ❌ |
| 3 | 0.800907 | `kusto_cluster_list` | ❌ |
| 4 | 0.792277 | `postgres_server_list` | ❌ |
| 5 | 0.790827 | `redis_list` | ❌ |

---

## Test 183

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.864350 | `eventgrid_topic_list` | ❌ |
| 2 | 0.860571 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.786464 | `postgres_server_list` | ❌ |
| 4 | 0.776853 | `kusto_cluster_list` | ❌ |
| 5 | 0.767170 | `grafana_list` | ❌ |

---

## Test 184

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.886777 | `eventgrid_topic_list` | ❌ |
| 2 | 0.870652 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.804751 | `group_list` | ❌ |
| 4 | 0.800346 | `kusto_cluster_list` | ❌ |
| 5 | 0.792884 | `monitor_webtests_list` | ❌ |

---

## Test 185

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.895381 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.877596 | `eventgrid_topic_list` | ❌ |
| 3 | 0.811393 | `kusto_cluster_list` | ❌ |
| 4 | 0.806815 | `redis_list` | ❌ |
| 5 | 0.803717 | `subscription_list` | ❌ |

---

## Test 186

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.888654 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.884649 | `eventgrid_topic_list` | ❌ |
| 3 | 0.824591 | `postgres_server_list` | ❌ |
| 4 | 0.822312 | `kusto_cluster_list` | ❌ |
| 5 | 0.813453 | `group_list` | ❌ |

---

## Test 187

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.891397 | `eventgrid_topic_list` | ❌ |
| 2 | 0.884803 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.841621 | `group_list` | ❌ |
| 4 | 0.820360 | `redis_list` | ❌ |
| 5 | 0.815762 | `kusto_cluster_list` | ❌ |

---

## Test 188

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.856701 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.849284 | `eventgrid_topic_list` | ❌ |
| 3 | 0.789738 | `postgres_server_list` | ❌ |
| 4 | 0.776123 | `kusto_cluster_list` | ❌ |
| 5 | 0.773041 | `appconfig_account_list` | ❌ |

---

## Test 189

**Expected Tool:** `eventhubs_eventhub_consumergroup_delete`  
**Prompt:** Delete my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.903198 | `eventhubs_eventhub_consumergroup_delete` | ✅ **EXPECTED** |
| 2 | 0.859003 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.857360 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.849526 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.822938 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 190

**Expected Tool:** `eventhubs_eventhub_consumergroup_get`  
**Prompt:** List all consumer groups in my event hub <event_hub_name> in namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.875349 | `eventhubs_eventhub_consumergroup_get` | ✅ **EXPECTED** |
| 2 | 0.837529 | `eventhubs_eventhub_get` | ❌ |
| 3 | 0.833129 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.823667 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 5 | 0.823241 | `eventhubs_eventhub_consumergroup_update` | ❌ |

---

## Test 191

**Expected Tool:** `eventhubs_eventhub_consumergroup_get`  
**Prompt:** Get the details of my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.886891 | `eventhubs_eventhub_consumergroup_get` | ✅ **EXPECTED** |
| 2 | 0.848877 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.839500 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.835102 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.825449 | `eventhubs_eventhub_get` | ❌ |

---

## Test 192

**Expected Tool:** `eventhubs_eventhub_consumergroup_update`  
**Prompt:** Create a new consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.884826 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
| 2 | 0.869484 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 3 | 0.858082 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.821795 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.821636 | `eventhubs_namespace_delete` | ❌ |

---

## Test 193

**Expected Tool:** `eventhubs_eventhub_consumergroup_update`  
**Prompt:** Update my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881362 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
| 2 | 0.862158 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 3 | 0.853036 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.828428 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.810440 | `eventhubs_namespace_get` | ❌ |

---

## Test 194

**Expected Tool:** `eventhubs_eventhub_delete`  
**Prompt:** Delete my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.873563 | `eventhubs_namespace_delete` | ❌ |
| 2 | 0.854152 | `eventhubs_eventhub_delete` | ✅ **EXPECTED** |
| 3 | 0.840655 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.824830 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.821433 | `eventhubs_eventhub_get` | ❌ |

---

## Test 195

**Expected Tool:** `eventhubs_eventhub_get`  
**Prompt:** List all Event Hubs in my namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.884791 | `eventhubs_eventhub_get` | ✅ **EXPECTED** |
| 2 | 0.850468 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.820604 | `eventhubs_eventhub_update` | ❌ |
| 4 | 0.816239 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.815068 | `kusto_cluster_list` | ❌ |

---

## Test 196

**Expected Tool:** `eventhubs_eventhub_get`  
**Prompt:** Get the details of my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.872481 | `eventhubs_eventhub_get` | ✅ **EXPECTED** |
| 2 | 0.863325 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.828888 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.825528 | `eventgrid_topic_list` | ❌ |
| 5 | 0.823248 | `eventhubs_eventhub_update` | ❌ |

---

## Test 197

**Expected Tool:** `eventhubs_eventhub_update`  
**Prompt:** Create a new event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.858259 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.847597 | `eventhubs_namespace_delete` | ❌ |
| 3 | 0.847070 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.844468 | `eventhubs_eventhub_get` | ❌ |
| 5 | 0.819099 | `eventhubs_eventhub_consumergroup_update` | ❌ |

---

## Test 198

**Expected Tool:** `eventhubs_eventhub_update`  
**Prompt:** Update my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.861448 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.837861 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.830895 | `eventhubs_namespace_delete` | ❌ |
| 4 | 0.828681 | `eventhubs_eventhub_get` | ❌ |
| 5 | 0.821437 | `eventhubs_eventhub_consumergroup_update` | ❌ |

---

## Test 199

**Expected Tool:** `eventhubs_namespace_delete`  
**Prompt:** Delete my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.847077 | `eventhubs_namespace_delete` | ✅ **EXPECTED** |
| 2 | 0.809806 | `eventhubs_namespace_update` | ❌ |
| 3 | 0.796933 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.786281 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.774290 | `sql_server_delete` | ❌ |

---

## Test 200

**Expected Tool:** `eventhubs_namespace_get`  
**Prompt:** List all Event Hubs namespaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.872513 | `eventhubs_namespace_get` | ✅ **EXPECTED** |
| 2 | 0.864321 | `eventhubs_eventhub_get` | ❌ |
| 3 | 0.848809 | `kusto_cluster_list` | ❌ |
| 4 | 0.826985 | `eventgrid_topic_list` | ❌ |
| 5 | 0.818780 | `eventgrid_subscription_list` | ❌ |

---

## Test 201

**Expected Tool:** `eventhubs_namespace_get`  
**Prompt:** Get the details of my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.816780 | `eventhubs_namespace_get` | ✅ **EXPECTED** |
| 2 | 0.808318 | `eventhubs_namespace_update` | ❌ |
| 3 | 0.798274 | `monitor_webtests_get` | ❌ |
| 4 | 0.797139 | `storage_account_get` | ❌ |
| 5 | 0.794771 | `functionapp_get` | ❌ |

---

## Test 202

**Expected Tool:** `eventhubs_namespace_update`  
**Prompt:** Create an new namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.842150 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.811546 | `eventhubs_namespace_delete` | ❌ |
| 3 | 0.802209 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.762521 | `storage_account_create` | ❌ |
| 5 | 0.761522 | `eventhubs_eventhub_consumergroup_update` | ❌ |

---

## Test 203

**Expected Tool:** `eventhubs_namespace_update`  
**Prompt:** Update my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.861582 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.803811 | `eventhubs_namespace_delete` | ❌ |
| 3 | 0.796158 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.768230 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.765758 | `eventhubs_eventhub_consumergroup_update` | ❌ |

---

## Test 204

**Expected Tool:** `functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.864639 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.802003 | `deploy_app_logs_get` | ❌ |
| 3 | 0.779803 | `applens_resource_diagnose` | ❌ |
| 4 | 0.761400 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.759013 | `appservice_database_add` | ❌ |

---

## Test 205

**Expected Tool:** `functionapp_get`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.823429 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.772407 | `deploy_app_logs_get` | ❌ |
| 3 | 0.763699 | `postgres_server_config_get` | ❌ |
| 4 | 0.761617 | `appconfig_kv_get` | ❌ |
| 5 | 0.747188 | `appconfig_account_list` | ❌ |

---

## Test 206

**Expected Tool:** `functionapp_get`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812102 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.760593 | `deploy_app_logs_get` | ❌ |
| 3 | 0.726109 | `applens_resource_diagnose` | ❌ |
| 4 | 0.712137 | `appconfig_kv_get` | ❌ |
| 5 | 0.711637 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 207

**Expected Tool:** `functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.888340 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.801509 | `deploy_app_logs_get` | ❌ |
| 3 | 0.796455 | `applens_resource_diagnose` | ❌ |
| 4 | 0.773059 | `foundry_resource_get` | ❌ |
| 5 | 0.766532 | `monitor_webtests_get` | ❌ |

---

## Test 208

**Expected Tool:** `functionapp_get`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.800410 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.782737 | `deploy_app_logs_get` | ❌ |
| 3 | 0.746350 | `applens_resource_diagnose` | ❌ |
| 4 | 0.735675 | `sql_server_show` | ❌ |
| 5 | 0.728265 | `virtualdesktop_hostpool_host_user-list` | ❌ |

---

## Test 209

**Expected Tool:** `functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.884793 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.791441 | `deploy_app_logs_get` | ❌ |
| 3 | 0.767977 | `applens_resource_diagnose` | ❌ |
| 4 | 0.765038 | `monitor_webtests_get` | ❌ |
| 5 | 0.753194 | `foundry_resource_get` | ❌ |

---

## Test 210

**Expected Tool:** `functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.854621 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.806043 | `deploy_app_logs_get` | ❌ |
| 3 | 0.763992 | `applens_resource_diagnose` | ❌ |
| 4 | 0.751034 | `sql_server_show` | ❌ |
| 5 | 0.750748 | `mysql_table_schema_get` | ❌ |

---

## Test 211

**Expected Tool:** `functionapp_get`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.809572 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.793238 | `deploy_app_logs_get` | ❌ |
| 3 | 0.767555 | `quota_usage_check` | ❌ |
| 4 | 0.763218 | `deploy_plan_get` | ❌ |
| 5 | 0.762313 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 212

**Expected Tool:** `functionapp_get`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.822908 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.794801 | `deploy_app_logs_get` | ❌ |
| 3 | 0.758340 | `applens_resource_diagnose` | ❌ |
| 4 | 0.736890 | `extension_cli_install` | ❌ |
| 5 | 0.730025 | `cloudarchitect_design` | ❌ |

---

## Test 213

**Expected Tool:** `functionapp_get`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.855542 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.801070 | `search_service_list` | ❌ |
| 3 | 0.798947 | `deploy_app_logs_get` | ❌ |
| 4 | 0.797524 | `appconfig_account_list` | ❌ |
| 5 | 0.784876 | `eventgrid_subscription_list` | ❌ |

---

## Test 214

**Expected Tool:** `functionapp_get`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.846634 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.834459 | `deploy_app_logs_get` | ❌ |
| 3 | 0.813025 | `applens_resource_diagnose` | ❌ |
| 4 | 0.802079 | `extension_cli_install` | ❌ |
| 5 | 0.801066 | `search_service_list` | ❌ |

---

## Test 215

**Expected Tool:** `functionapp_get`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.794721 | `deploy_app_logs_get` | ❌ |
| 2 | 0.786712 | `functionapp_get` | ✅ **EXPECTED** |
| 3 | 0.763268 | `applens_resource_diagnose` | ❌ |
| 4 | 0.758227 | `extension_cli_install` | ❌ |
| 5 | 0.756720 | `cloudarchitect_design` | ❌ |

---

## Test 216

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** Get the account settings for my key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.845654 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.812440 | `storage_account_get` | ❌ |
| 3 | 0.803953 | `keyvault_key_get` | ❌ |
| 4 | 0.788578 | `keyvault_secret_get` | ❌ |
| 5 | 0.781050 | `keyvault_certificate_get` | ❌ |

---

## Test 217

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** Show me the account settings for managed HSM keyvault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879934 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.817876 | `storage_account_get` | ❌ |
| 3 | 0.793482 | `keyvault_key_get` | ❌ |
| 4 | 0.792320 | `keyvault_key_create` | ❌ |
| 5 | 0.789303 | `storage_blob_container_get` | ❌ |

---

## Test 218

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** What's the value of the <setting_name> setting in my key vault with name <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793890 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.778209 | `appconfig_kv_set` | ❌ |
| 3 | 0.777060 | `keyvault_secret_create` | ❌ |
| 4 | 0.773529 | `keyvault_secret_get` | ❌ |
| 5 | 0.771355 | `storage_account_get` | ❌ |

---

## Test 219

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.858241 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.840490 | `keyvault_certificate_import` | ❌ |
| 3 | 0.838058 | `keyvault_key_create` | ❌ |
| 4 | 0.829966 | `keyvault_certificate_get` | ❌ |
| 5 | 0.817735 | `keyvault_secret_create` | ❌ |

---

## Test 220

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Generate a certificate named <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.847433 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.827003 | `keyvault_certificate_import` | ❌ |
| 3 | 0.823322 | `keyvault_certificate_get` | ❌ |
| 4 | 0.817519 | `keyvault_key_create` | ❌ |
| 5 | 0.806741 | `keyvault_certificate_list` | ❌ |

---

## Test 221

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Request creation of certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.845848 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.828974 | `keyvault_certificate_import` | ❌ |
| 3 | 0.826442 | `keyvault_certificate_get` | ❌ |
| 4 | 0.821290 | `keyvault_key_create` | ❌ |
| 5 | 0.811771 | `keyvault_secret_create` | ❌ |

---

## Test 222

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.840745 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.827276 | `keyvault_certificate_import` | ❌ |
| 3 | 0.825962 | `keyvault_certificate_get` | ❌ |
| 4 | 0.814983 | `keyvault_key_create` | ❌ |
| 5 | 0.805347 | `keyvault_secret_create` | ❌ |

---

## Test 223

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Issue a certificate <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852018 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.834928 | `keyvault_certificate_get` | ❌ |
| 3 | 0.832155 | `keyvault_certificate_import` | ❌ |
| 4 | 0.817923 | `keyvault_key_create` | ❌ |
| 5 | 0.817680 | `keyvault_certificate_list` | ❌ |

---

## Test 224

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.849686 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.829160 | `keyvault_certificate_list` | ❌ |
| 3 | 0.822051 | `keyvault_certificate_create` | ❌ |
| 4 | 0.816352 | `keyvault_certificate_import` | ❌ |
| 5 | 0.799569 | `keyvault_key_create` | ❌ |

---

## Test 225

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.875089 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.834060 | `keyvault_certificate_list` | ❌ |
| 3 | 0.832918 | `keyvault_key_get` | ❌ |
| 4 | 0.824555 | `keyvault_certificate_create` | ❌ |
| 5 | 0.821314 | `keyvault_secret_get` | ❌ |

---

## Test 226

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Get the certificate <certificate_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.836051 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.814631 | `keyvault_certificate_create` | ❌ |
| 3 | 0.808022 | `keyvault_certificate_import` | ❌ |
| 4 | 0.803102 | `keyvault_certificate_list` | ❌ |
| 5 | 0.786034 | `keyvault_key_create` | ❌ |

---

## Test 227

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Display the certificate details for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.859029 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.813669 | `keyvault_certificate_create` | ❌ |
| 3 | 0.813552 | `keyvault_key_get` | ❌ |
| 4 | 0.807628 | `keyvault_certificate_list` | ❌ |
| 5 | 0.806605 | `keyvault_certificate_import` | ❌ |

---

## Test 228

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.822429 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.794140 | `keyvault_certificate_list` | ❌ |
| 3 | 0.790227 | `keyvault_certificate_create` | ❌ |
| 4 | 0.787864 | `keyvault_certificate_import` | ❌ |
| 5 | 0.778190 | `keyvault_key_get` | ❌ |

---

## Test 229

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.821117 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.775551 | `keyvault_certificate_get` | ❌ |
| 3 | 0.773043 | `keyvault_certificate_create` | ❌ |
| 4 | 0.764307 | `keyvault_certificate_list` | ❌ |
| 5 | 0.762211 | `keyvault_key_create` | ❌ |

---

## Test 230

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.848446 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.829118 | `keyvault_certificate_get` | ❌ |
| 3 | 0.817167 | `keyvault_certificate_create` | ❌ |
| 4 | 0.803441 | `keyvault_certificate_list` | ❌ |
| 5 | 0.799048 | `keyvault_key_create` | ❌ |

---

## Test 231

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Upload certificate file <file_path> to key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.828524 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.793637 | `keyvault_certificate_create` | ❌ |
| 3 | 0.783872 | `keyvault_certificate_get` | ❌ |
| 4 | 0.781328 | `keyvault_key_create` | ❌ |
| 5 | 0.777882 | `keyvault_secret_create` | ❌ |

---

## Test 232

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.817546 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.779310 | `keyvault_certificate_get` | ❌ |
| 3 | 0.775396 | `keyvault_certificate_create` | ❌ |
| 4 | 0.760712 | `keyvault_certificate_list` | ❌ |
| 5 | 0.753985 | `keyvault_secret_create` | ❌ |

---

## Test 233

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.825629 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.785854 | `keyvault_certificate_create` | ❌ |
| 3 | 0.777793 | `keyvault_certificate_get` | ❌ |
| 4 | 0.764990 | `keyvault_secret_create` | ❌ |
| 5 | 0.762896 | `keyvault_key_create` | ❌ |

---

## Test 234

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.886390 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.841516 | `keyvault_certificate_get` | ❌ |
| 3 | 0.822561 | `keyvault_secret_list` | ❌ |
| 4 | 0.821789 | `keyvault_key_list` | ❌ |
| 5 | 0.815023 | `keyvault_certificate_create` | ❌ |

---

## Test 235

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851648 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.840536 | `keyvault_certificate_get` | ❌ |
| 3 | 0.810598 | `keyvault_certificate_create` | ❌ |
| 4 | 0.802737 | `keyvault_certificate_import` | ❌ |
| 5 | 0.798452 | `keyvault_key_get` | ❌ |

---

## Test 236

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** What certificates are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.838086 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.827396 | `keyvault_certificate_get` | ❌ |
| 3 | 0.809956 | `keyvault_certificate_create` | ❌ |
| 4 | 0.799527 | `keyvault_certificate_import` | ❌ |
| 5 | 0.795610 | `keyvault_key_create` | ❌ |

---

## Test 237

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** List certificate names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.850566 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.823931 | `keyvault_certificate_get` | ❌ |
| 3 | 0.801728 | `keyvault_key_list` | ❌ |
| 4 | 0.797232 | `keyvault_certificate_create` | ❌ |
| 5 | 0.796135 | `keyvault_secret_list` | ❌ |

---

## Test 238

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Enumerate certificates in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.905580 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.850051 | `keyvault_key_list` | ❌ |
| 3 | 0.844701 | `keyvault_secret_list` | ❌ |
| 4 | 0.826271 | `keyvault_certificate_get` | ❌ |
| 5 | 0.807618 | `keyvault_certificate_create` | ❌ |

---

## Test 239

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show certificate names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.848376 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.836977 | `keyvault_certificate_get` | ❌ |
| 3 | 0.806944 | `keyvault_certificate_create` | ❌ |
| 4 | 0.805745 | `keyvault_key_list` | ❌ |
| 5 | 0.801511 | `keyvault_secret_list` | ❌ |

---

## Test 240

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.865691 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.779557 | `keyvault_secret_create` | ❌ |
| 3 | 0.773761 | `keyvault_certificate_create` | ❌ |
| 4 | 0.770666 | `keyvault_key_get` | ❌ |
| 5 | 0.761933 | `keyvault_certificate_import` | ❌ |

---

## Test 241

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Generate a key <key_name> with type <key_type> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.821569 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.769618 | `keyvault_secret_create` | ❌ |
| 3 | 0.765658 | `keyvault_key_get` | ❌ |
| 4 | 0.753743 | `keyvault_certificate_create` | ❌ |
| 5 | 0.748896 | `keyvault_key_list` | ❌ |

---

## Test 242

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an oct key in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.836425 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.795505 | `keyvault_secret_create` | ❌ |
| 3 | 0.786292 | `keyvault_key_get` | ❌ |
| 4 | 0.781614 | `keyvault_certificate_create` | ❌ |
| 5 | 0.772805 | `keyvault_secret_get` | ❌ |

---

## Test 243

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an RSA key in the vault <key_vault_account_name> with name <key_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.854742 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.793508 | `keyvault_secret_create` | ❌ |
| 3 | 0.786903 | `keyvault_certificate_create` | ❌ |
| 4 | 0.784100 | `keyvault_key_get` | ❌ |
| 5 | 0.778133 | `keyvault_certificate_import` | ❌ |

---

## Test 244

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an EC key with name <key_name> in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.827686 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.775423 | `keyvault_secret_create` | ❌ |
| 3 | 0.769017 | `keyvault_key_get` | ❌ |
| 4 | 0.766467 | `keyvault_certificate_create` | ❌ |
| 5 | 0.762806 | `keyvault_certificate_import` | ❌ |

---

## Test 245

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.831379 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.817128 | `keyvault_key_create` | ❌ |
| 3 | 0.815133 | `keyvault_secret_get` | ❌ |
| 4 | 0.813706 | `keyvault_certificate_get` | ❌ |
| 5 | 0.808534 | `keyvault_key_list` | ❌ |

---

## Test 246

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.858452 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.834789 | `keyvault_secret_get` | ❌ |
| 3 | 0.832103 | `keyvault_certificate_get` | ❌ |
| 4 | 0.813740 | `storage_account_get` | ❌ |
| 5 | 0.813226 | `keyvault_key_create` | ❌ |

---

## Test 247

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Get the key <key_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.791389 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.778753 | `keyvault_key_create` | ❌ |
| 3 | 0.778135 | `keyvault_secret_get` | ❌ |
| 4 | 0.769580 | `keyvault_certificate_get` | ❌ |
| 5 | 0.763332 | `keyvault_key_list` | ❌ |

---

## Test 248

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Display the key details for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.847889 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.821518 | `keyvault_secret_get` | ❌ |
| 3 | 0.815933 | `keyvault_certificate_get` | ❌ |
| 4 | 0.795505 | `storage_account_get` | ❌ |
| 5 | 0.789318 | `keyvault_key_create` | ❌ |

---

## Test 249

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Retrieve key metadata for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.794616 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.780498 | `storage_account_get` | ❌ |
| 3 | 0.778820 | `keyvault_secret_get` | ❌ |
| 4 | 0.769088 | `keyvault_certificate_get` | ❌ |
| 5 | 0.768384 | `keyvault_admin_settings_get` | ❌ |

---

## Test 250

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.872277 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.848718 | `keyvault_secret_list` | ❌ |
| 3 | 0.834942 | `keyvault_certificate_list` | ❌ |
| 4 | 0.828677 | `keyvault_key_get` | ❌ |
| 5 | 0.814524 | `keyvault_key_create` | ❌ |

---

## Test 251

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.834771 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.827778 | `keyvault_key_get` | ❌ |
| 3 | 0.817688 | `keyvault_secret_list` | ❌ |
| 4 | 0.813443 | `keyvault_key_create` | ❌ |
| 5 | 0.809918 | `keyvault_certificate_get` | ❌ |

---

## Test 252

**Expected Tool:** `keyvault_key_list`  
**Prompt:** What keys are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.822309 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.812245 | `keyvault_key_create` | ❌ |
| 3 | 0.812200 | `keyvault_key_get` | ❌ |
| 4 | 0.802969 | `keyvault_secret_list` | ❌ |
| 5 | 0.797343 | `keyvault_admin_settings_get` | ❌ |

---

## Test 253

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List key names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.842717 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.819023 | `keyvault_secret_list` | ❌ |
| 3 | 0.816590 | `keyvault_key_get` | ❌ |
| 4 | 0.808446 | `keyvault_certificate_list` | ❌ |
| 5 | 0.797875 | `keyvault_key_create` | ❌ |

---

## Test 254

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Enumerate keys in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.895928 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.864689 | `keyvault_secret_list` | ❌ |
| 3 | 0.854825 | `keyvault_certificate_list` | ❌ |
| 4 | 0.812529 | `keyvault_key_get` | ❌ |
| 5 | 0.810374 | `keyvault_key_create` | ❌ |

---

## Test 255

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Show key names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.842460 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.826003 | `keyvault_key_get` | ❌ |
| 3 | 0.823494 | `keyvault_secret_list` | ❌ |
| 4 | 0.810392 | `keyvault_certificate_list` | ❌ |
| 5 | 0.809585 | `keyvault_key_create` | ❌ |

---

## Test 256

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.862855 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.815682 | `keyvault_secret_get` | ❌ |
| 3 | 0.805459 | `keyvault_key_create` | ❌ |
| 4 | 0.785645 | `keyvault_certificate_create` | ❌ |
| 5 | 0.778963 | `keyvault_certificate_import` | ❌ |

---

## Test 257

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.860701 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.816343 | `keyvault_secret_get` | ❌ |
| 3 | 0.784856 | `keyvault_key_create` | ❌ |
| 4 | 0.783952 | `keyvault_secret_list` | ❌ |
| 5 | 0.777067 | `appconfig_kv_set` | ❌ |

---

## Test 258

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.849189 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.816193 | `keyvault_secret_get` | ❌ |
| 3 | 0.787393 | `keyvault_key_create` | ❌ |
| 4 | 0.784295 | `keyvault_secret_list` | ❌ |
| 5 | 0.778729 | `appconfig_kv_set` | ❌ |

---

## Test 259

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.847957 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.801169 | `keyvault_secret_get` | ❌ |
| 3 | 0.777110 | `keyvault_key_create` | ❌ |
| 4 | 0.774510 | `keyvault_certificate_import` | ❌ |
| 5 | 0.767067 | `keyvault_certificate_create` | ❌ |

---

## Test 260

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Update secret <secret_name> to value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.839098 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.814716 | `keyvault_secret_get` | ❌ |
| 3 | 0.773281 | `keyvault_secret_list` | ❌ |
| 4 | 0.766877 | `keyvault_key_create` | ❌ |
| 5 | 0.766012 | `keyvault_certificate_get` | ❌ |

---

## Test 261

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.847856 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.832925 | `keyvault_secret_create` | ❌ |
| 3 | 0.822559 | `keyvault_secret_list` | ❌ |
| 4 | 0.812702 | `keyvault_key_get` | ❌ |
| 5 | 0.809324 | `keyvault_certificate_get` | ❌ |

---

## Test 262

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868704 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.844900 | `keyvault_key_get` | ❌ |
| 3 | 0.834721 | `keyvault_certificate_get` | ❌ |
| 4 | 0.825751 | `keyvault_secret_list` | ❌ |
| 5 | 0.819219 | `keyvault_secret_create` | ❌ |

---

## Test 263

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Get the secret <secret_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.821182 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.814429 | `keyvault_secret_create` | ❌ |
| 3 | 0.782409 | `keyvault_secret_list` | ❌ |
| 4 | 0.781840 | `keyvault_key_get` | ❌ |
| 5 | 0.776044 | `keyvault_certificate_get` | ❌ |

---

## Test 264

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Display the secret details for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.849990 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.822671 | `keyvault_key_get` | ❌ |
| 3 | 0.812987 | `keyvault_secret_create` | ❌ |
| 4 | 0.810970 | `keyvault_certificate_get` | ❌ |
| 5 | 0.805665 | `keyvault_secret_list` | ❌ |

---

## Test 265

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Retrieve secret metadata for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.819671 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.794790 | `keyvault_secret_create` | ❌ |
| 3 | 0.784690 | `keyvault_secret_list` | ❌ |
| 4 | 0.778922 | `keyvault_key_get` | ❌ |
| 5 | 0.776819 | `storage_account_get` | ❌ |

---

## Test 266

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.878939 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.843347 | `keyvault_secret_get` | ❌ |
| 3 | 0.828204 | `keyvault_key_list` | ❌ |
| 4 | 0.823340 | `keyvault_certificate_list` | ❌ |
| 5 | 0.813526 | `keyvault_key_get` | ❌ |

---

## Test 267

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.830772 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.830679 | `keyvault_secret_get` | ❌ |
| 3 | 0.821267 | `keyvault_key_get` | ❌ |
| 4 | 0.810178 | `keyvault_certificate_get` | ❌ |
| 5 | 0.810099 | `keyvault_secret_create` | ❌ |

---

## Test 268

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** What secrets are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.826301 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.814112 | `keyvault_secret_get` | ❌ |
| 3 | 0.800208 | `keyvault_secret_create` | ❌ |
| 4 | 0.798763 | `keyvault_key_get` | ❌ |
| 5 | 0.795196 | `keyvault_admin_settings_get` | ❌ |

---

## Test 269

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** List secrets names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.840566 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.821317 | `keyvault_secret_get` | ❌ |
| 3 | 0.800144 | `keyvault_secret_create` | ❌ |
| 4 | 0.797779 | `keyvault_key_list` | ❌ |
| 5 | 0.792826 | `keyvault_key_get` | ❌ |

---

## Test 270

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Enumerate secrets in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.895927 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.852096 | `keyvault_key_list` | ❌ |
| 3 | 0.837096 | `keyvault_certificate_list` | ❌ |
| 4 | 0.827009 | `keyvault_secret_get` | ❌ |
| 5 | 0.815837 | `keyvault_secret_create` | ❌ |

---

## Test 271

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Show secrets names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.844935 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.839355 | `keyvault_secret_get` | ❌ |
| 3 | 0.824265 | `keyvault_key_get` | ❌ |
| 4 | 0.816774 | `keyvault_certificate_get` | ❌ |
| 5 | 0.815124 | `keyvault_secret_create` | ❌ |

---

## Test 272

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.854567 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.831267 | `aks_nodepool_get` | ❌ |
| 3 | 0.804847 | `kusto_cluster_get` | ❌ |
| 4 | 0.800764 | `kusto_cluster_list` | ❌ |
| 5 | 0.789156 | `mysql_server_config_get` | ❌ |

---

## Test 273

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.877322 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.854840 | `aks_nodepool_get` | ❌ |
| 3 | 0.835170 | `kusto_cluster_get` | ❌ |
| 4 | 0.817774 | `kusto_cluster_list` | ❌ |
| 5 | 0.804879 | `redis_list` | ❌ |

---

## Test 274

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.831186 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.824247 | `aks_nodepool_get` | ❌ |
| 3 | 0.787148 | `kusto_cluster_get` | ❌ |
| 4 | 0.782086 | `kusto_cluster_list` | ❌ |
| 5 | 0.777667 | `sql_server_show` | ❌ |

---

## Test 275

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.857609 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.841157 | `aks_nodepool_get` | ❌ |
| 3 | 0.811485 | `kusto_cluster_get` | ❌ |
| 4 | 0.797632 | `foundry_resource_get` | ❌ |
| 5 | 0.791661 | `storage_account_get` | ❌ |

---

## Test 276

**Expected Tool:** `aks_cluster_get`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.919011 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.898604 | `kusto_cluster_list` | ❌ |
| 3 | 0.850028 | `aks_nodepool_get` | ❌ |
| 4 | 0.841096 | `search_service_list` | ❌ |
| 5 | 0.833424 | `kusto_cluster_get` | ❌ |

---

## Test 277

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.869927 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.852885 | `kusto_cluster_list` | ❌ |
| 3 | 0.839721 | `aks_nodepool_get` | ❌ |
| 4 | 0.820677 | `search_service_list` | ❌ |
| 5 | 0.814302 | `kusto_cluster_get` | ❌ |

---

## Test 278

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.840404 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.816732 | `aks_nodepool_get` | ❌ |
| 3 | 0.778826 | `kusto_cluster_list` | ❌ |
| 4 | 0.761851 | `kusto_cluster_get` | ❌ |
| 5 | 0.757982 | `kusto_database_list` | ❌ |

---

## Test 279

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.887377 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.836887 | `aks_cluster_get` | ❌ |
| 3 | 0.811633 | `kusto_cluster_get` | ❌ |
| 4 | 0.781283 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.778652 | `foundry_resource_get` | ❌ |

---

## Test 280

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.871515 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.820539 | `aks_cluster_get` | ❌ |
| 3 | 0.790116 | `kusto_cluster_get` | ❌ |
| 4 | 0.776492 | `kusto_cluster_list` | ❌ |
| 5 | 0.774670 | `virtualdesktop_hostpool_list` | ❌ |

---

## Test 281

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.854484 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.808546 | `aks_cluster_get` | ❌ |
| 3 | 0.768289 | `kusto_cluster_get` | ❌ |
| 4 | 0.762637 | `kusto_cluster_list` | ❌ |
| 5 | 0.759113 | `eventhubs_namespace_get` | ❌ |

---

## Test 282

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.870076 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.821292 | `aks_cluster_get` | ❌ |
| 3 | 0.812351 | `kusto_cluster_list` | ❌ |
| 4 | 0.802729 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.775537 | `sql_elastic-pool_list` | ❌ |

---

## Test 283

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.886269 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.837240 | `aks_cluster_get` | ❌ |
| 3 | 0.817573 | `kusto_cluster_list` | ❌ |
| 4 | 0.801825 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.791654 | `kusto_database_list` | ❌ |

---

## Test 284

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.872738 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.815806 | `aks_cluster_get` | ❌ |
| 3 | 0.779339 | `kusto_cluster_list` | ❌ |
| 4 | 0.777183 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.762947 | `eventhubs_namespace_get` | ❌ |

---

## Test 285

**Expected Tool:** `loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.823078 | `loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.821671 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.810974 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.803358 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.781260 | `loadtesting_test_get` | ❌ |

---

## Test 286

**Expected Tool:** `loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.835853 | `loadtesting_test_get` | ✅ **EXPECTED** |
| 2 | 0.823305 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.823186 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.822609 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.808050 | `monitor_webtests_get` | ❌ |

---

## Test 287

**Expected Tool:** `loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.850829 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.848455 | `loadtesting_testresource_create` | ✅ **EXPECTED** |
| 3 | 0.825065 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.823096 | `monitor_webtests_list` | ❌ |
| 5 | 0.809276 | `loadtesting_test_create` | ❌ |

---

## Test 288

**Expected Tool:** `loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.903902 | `loadtesting_testresource_list` | ✅ **EXPECTED** |
| 2 | 0.867710 | `monitor_webtests_list` | ❌ |
| 3 | 0.849957 | `group_list` | ❌ |
| 4 | 0.832848 | `redis_list` | ❌ |
| 5 | 0.831770 | `kusto_cluster_list` | ❌ |

---

## Test 289

**Expected Tool:** `loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.877261 | `loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.831241 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.820081 | `loadtesting_test_create` | ❌ |
| 4 | 0.816240 | `loadtesting_testrun_update` | ❌ |
| 5 | 0.803680 | `loadtesting_testresource_list` | ❌ |

---

## Test 290

**Expected Tool:** `loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.845121 | `loadtesting_testrun_create` | ❌ |
| 2 | 0.831813 | `loadtesting_test_get` | ❌ |
| 3 | 0.818845 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.818014 | `monitor_webtests_get` | ❌ |
| 5 | 0.814756 | `loadtesting_testrun_list` | ❌ |

---

## Test 291

**Expected Tool:** `loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.838956 | `loadtesting_testrun_list` | ✅ **EXPECTED** |
| 2 | 0.831750 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.826910 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.812169 | `monitor_webtests_list` | ❌ |
| 5 | 0.812163 | `loadtesting_test_get` | ❌ |

---

## Test 292

**Expected Tool:** `loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.844152 | `loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.807072 | `loadtesting_testrun_create` | ❌ |
| 3 | 0.756178 | `eventhubs_namespace_update` | ❌ |
| 4 | 0.749486 | `monitor_webtests_get` | ❌ |
| 5 | 0.749281 | `loadtesting_testresource_list` | ❌ |

---

## Test 293

**Expected Tool:** `grafana_list`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.846512 | `kusto_cluster_list` | ❌ |
| 2 | 0.841949 | `search_service_list` | ❌ |
| 3 | 0.830453 | `redis_list` | ❌ |
| 4 | 0.827182 | `eventgrid_topic_list` | ❌ |
| 5 | 0.824258 | `subscription_list` | ❌ |

---

## Test 294

**Expected Tool:** `managedlustre_fs_create`  
**Prompt:** Create an Azure Managed Lustre filesystem with name <filesystem_name>, size <filesystem_size>, SKU <sku>, and subnet <subnet_id> for availability zone <zone> in location <location>. Maintenance should occur on <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.883800 | `managedlustre_fs_create` | ✅ **EXPECTED** |
| 2 | 0.872554 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 3 | 0.849347 | `managedlustre_fs_list` | ❌ |
| 4 | 0.841000 | `managedlustre_fs_sku_get` | ❌ |
| 5 | 0.834033 | `managedlustre_fs_update` | ❌ |

---

## Test 295

**Expected Tool:** `managedlustre_fs_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.890121 | `managedlustre_fs_list` | ✅ **EXPECTED** |
| 2 | 0.826604 | `kusto_cluster_list` | ❌ |
| 3 | 0.825619 | `managedlustre_fs_create` | ❌ |
| 4 | 0.821508 | `managedlustre_fs_sku_get` | ❌ |
| 5 | 0.813953 | `managedlustre_fs_subnetsize_validate` | ❌ |

---

## Test 296

**Expected Tool:** `managedlustre_fs_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881110 | `managedlustre_fs_list` | ✅ **EXPECTED** |
| 2 | 0.827276 | `managedlustre_fs_create` | ❌ |
| 3 | 0.816668 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.806393 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.793431 | `mysql_server_list` | ❌ |

---

## Test 297

**Expected Tool:** `managedlustre_fs_sku_get`  
**Prompt:** List the Azure Managed Lustre SKUs available in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.909059 | `managedlustre_fs_sku_get` | ✅ **EXPECTED** |
| 2 | 0.839200 | `managedlustre_fs_list` | ❌ |
| 3 | 0.826578 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 4 | 0.808762 | `managedlustre_fs_create` | ❌ |
| 5 | 0.800384 | `storage_account_get` | ❌ |

---

## Test 298

**Expected Tool:** `managedlustre_fs_subnetsize_ask`  
**Prompt:** Tell me how many IP addresses I need for an Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.878047 | `managedlustre_fs_subnetsize_ask` | ✅ **EXPECTED** |
| 2 | 0.865157 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 3 | 0.836563 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.806682 | `managedlustre_fs_create` | ❌ |
| 5 | 0.805986 | `managedlustre_fs_list` | ❌ |

---

## Test 299

**Expected Tool:** `managedlustre_fs_subnetsize_validate`  
**Prompt:** Validate if the network <subnet_id> can host Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.949006 | `managedlustre_fs_subnetsize_validate` | ✅ **EXPECTED** |
| 2 | 0.836190 | `managedlustre_fs_subnetsize_ask` | ❌ |
| 3 | 0.823688 | `managedlustre_fs_create` | ❌ |
| 4 | 0.815984 | `managedlustre_fs_sku_get` | ❌ |
| 5 | 0.809832 | `managedlustre_fs_list` | ❌ |

---

## Test 300

**Expected Tool:** `managedlustre_fs_update`  
**Prompt:** Update the maintenance window of the Azure Managed Lustre filesystem <filesystem_name> to <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.870692 | `managedlustre_fs_update` | ✅ **EXPECTED** |
| 2 | 0.826888 | `managedlustre_fs_create` | ❌ |
| 3 | 0.796541 | `managedlustre_fs_list` | ❌ |
| 4 | 0.766323 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 5 | 0.753852 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 301

**Expected Tool:** `marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.840110 | `marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.838677 | `marketplace_product_list` | ❌ |
| 3 | 0.760387 | `storage_account_get` | ❌ |
| 4 | 0.756842 | `foundry_resource_get` | ❌ |
| 5 | 0.755465 | `search_index_get` | ❌ |

---

## Test 302

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.853722 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.777058 | `marketplace_product_get` | ❌ |
| 3 | 0.762771 | `cloudarchitect_design` | ❌ |
| 4 | 0.756989 | `search_service_list` | ❌ |
| 5 | 0.756021 | `search_index_query` | ❌ |

---

## Test 303

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.857890 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.768864 | `marketplace_product_get` | ❌ |
| 3 | 0.750272 | `eventgrid_topic_list` | ❌ |
| 4 | 0.743658 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.741465 | `foundry_models_list` | ❌ |

---

## Test 304

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.878380 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.848039 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.827076 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.807900 | `extension_cli_generate` | ❌ |
| 5 | 0.802481 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 305

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.863870 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.836743 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.830418 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.820558 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.810741 | `cloudarchitect_design` | ❌ |

---

## Test 306

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868161 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.852755 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.820987 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.814172 | `cloudarchitect_design` | ❌ |
| 5 | 0.808502 | `quota_usage_check` | ❌ |

---

## Test 307

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868122 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.823297 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.808704 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.804954 | `extension_cli_install` | ❌ |
| 5 | 0.793827 | `extension_cli_generate` | ❌ |

---

## Test 308

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.858274 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.818959 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.810241 | `extension_cli_install` | ❌ |
| 4 | 0.808328 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.808030 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 309

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.859510 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.823757 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.808384 | `extension_cli_install` | ❌ |
| 4 | 0.800792 | `cloudarchitect_design` | ❌ |
| 5 | 0.798030 | `deploy_iac_rules_get` | ❌ |

---

## Test 310

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.846449 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.820631 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.816437 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.815223 | `cloudarchitect_design` | ❌ |
| 5 | 0.797049 | `applens_resource_diagnose` | ❌ |

---

## Test 311

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.841439 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.816198 | `cloudarchitect_design` | ❌ |
| 3 | 0.815143 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.799818 | `extension_cli_install` | ❌ |
| 5 | 0.797349 | `applens_resource_diagnose` | ❌ |

---

## Test 312

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** configure azure mcp in coding agent for my repo  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.820628 | `deploy_plan_get` | ❌ |
| 2 | 0.801789 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.799225 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.769252 | `extension_cli_install` | ❌ |
| 5 | 0.764137 | `get_bestpractices_get` | ✅ **EXPECTED** |

---

## Test 313

**Expected Tool:** `monitor_activitylog_list`  
**Prompt:** List the activity logs of the last month for <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.814080 | `monitor_activitylog_list` | ✅ **EXPECTED** |
| 2 | 0.787043 | `monitor_resource_log_query` | ❌ |
| 3 | 0.780794 | `monitor_workspace_log_query` | ❌ |
| 4 | 0.767243 | `deploy_app_logs_get` | ❌ |
| 5 | 0.747344 | `resourcehealth_health-events_list` | ❌ |

---

## Test 314

**Expected Tool:** `monitor_healthmodels_entity_get`  
**Prompt:** Show me the health status of entity <entity_id> using the health model <health_model_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.837136 | `monitor_healthmodels_entity_get` | ✅ **EXPECTED** |
| 2 | 0.798169 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.745048 | `resourcehealth_health-events_list` | ❌ |
| 4 | 0.727153 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.724203 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 315

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818978 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.769441 | `monitor_metrics_query` | ❌ |
| 3 | 0.740290 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.734430 | `applens_resource_diagnose` | ❌ |
| 5 | 0.734107 | `eventhubs_namespace_get` | ❌ |

---

## Test 316

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.844158 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.829681 | `storage_account_get` | ❌ |
| 3 | 0.818100 | `storage_blob_container_get` | ❌ |
| 4 | 0.802592 | `quota_usage_check` | ❌ |
| 5 | 0.802175 | `storage_blob_get` | ❌ |

---

## Test 317

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.864559 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.808641 | `monitor_metrics_query` | ❌ |
| 3 | 0.804435 | `applens_resource_diagnose` | ❌ |
| 4 | 0.770690 | `foundry_resource_get` | ❌ |
| 5 | 0.770012 | `bicepschema_get` | ❌ |

---

## Test 318

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.825488 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.795008 | `applens_resource_diagnose` | ❌ |
| 3 | 0.775527 | `applicationinsights_recommendation_list` | ❌ |
| 4 | 0.772063 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.771587 | `monitor_resource_log_query` | ❌ |

---

## Test 319

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.826194 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.800088 | `quota_usage_check` | ❌ |
| 3 | 0.797355 | `applens_resource_diagnose` | ❌ |
| 4 | 0.796370 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.792043 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 320

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.782679 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.722626 | `monitor_metrics_definitions` | ❌ |
| 3 | 0.707861 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.704787 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.703109 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 321

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798339 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.779818 | `applens_resource_diagnose` | ❌ |
| 3 | 0.768299 | `resourcehealth_health-events_list` | ❌ |
| 4 | 0.766578 | `monitor_activitylog_list` | ❌ |
| 5 | 0.765061 | `quota_usage_check` | ❌ |

---

## Test 322

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.811105 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.750347 | `monitor_resource_log_query` | ❌ |
| 3 | 0.741873 | `monitor_workspace_log_query` | ❌ |
| 4 | 0.730939 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.727139 | `monitor_metrics_definitions` | ❌ |

---

## Test 323

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.781554 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.751204 | `applens_resource_diagnose` | ❌ |
| 3 | 0.748179 | `applicationinsights_recommendation_list` | ❌ |
| 4 | 0.743912 | `quota_usage_check` | ❌ |
| 5 | 0.737125 | `resourcehealth_health-events_list` | ❌ |

---

## Test 324

**Expected Tool:** `monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.864572 | `monitor_workspace_log_query` | ❌ |
| 2 | 0.858333 | `monitor_resource_log_query` | ✅ **EXPECTED** |
| 3 | 0.848487 | `deploy_app_logs_get` | ❌ |
| 4 | 0.840408 | `monitor_activitylog_list` | ❌ |
| 5 | 0.805815 | `monitor_workspace_list` | ❌ |

---

## Test 325

**Expected Tool:** `monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.911959 | `monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.884489 | `monitor_table_type_list` | ❌ |
| 3 | 0.853296 | `monitor_workspace_list` | ❌ |
| 4 | 0.831372 | `postgres_table_list` | ❌ |
| 5 | 0.829730 | `monitor_workspace_log_query` | ❌ |

---

## Test 326

**Expected Tool:** `monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.882736 | `monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.866055 | `monitor_table_type_list` | ❌ |
| 3 | 0.844022 | `monitor_workspace_list` | ❌ |
| 4 | 0.840771 | `deploy_app_logs_get` | ❌ |
| 5 | 0.831257 | `monitor_workspace_log_query` | ❌ |

---

## Test 327

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.948425 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.875367 | `monitor_table_list` | ❌ |
| 3 | 0.821336 | `monitor_workspace_list` | ❌ |
| 4 | 0.806486 | `postgres_table_list` | ❌ |
| 5 | 0.796210 | `deploy_app_logs_get` | ❌ |

---

## Test 328

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.930287 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.858333 | `monitor_table_list` | ❌ |
| 3 | 0.820712 | `monitor_workspace_list` | ❌ |
| 4 | 0.814130 | `deploy_app_logs_get` | ❌ |
| 5 | 0.804596 | `monitor_workspace_log_query` | ❌ |

---

## Test 329

**Expected Tool:** `monitor_webtests_create`  
**Prompt:** Create a new Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.814184 | `monitor_webtests_list` | ❌ |
| 2 | 0.801618 | `monitor_webtests_create` | ✅ **EXPECTED** |
| 3 | 0.798315 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.797600 | `monitor_webtests_get` | ❌ |
| 5 | 0.781952 | `loadtesting_testrun_create` | ❌ |

---

## Test 330

**Expected Tool:** `monitor_webtests_get`  
**Prompt:** Get Web Test details for <webtest_resource_name> in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.891612 | `monitor_webtests_get` | ✅ **EXPECTED** |
| 2 | 0.875197 | `monitor_webtests_list` | ❌ |
| 3 | 0.832477 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.795023 | `eventgrid_topic_list` | ❌ |
| 5 | 0.793021 | `group_list` | ❌ |

---

## Test 331

**Expected Tool:** `monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.888021 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.839863 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.829709 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.821453 | `eventgrid_topic_list` | ❌ |
| 5 | 0.820228 | `redis_list` | ❌ |

---

## Test 332

**Expected Tool:** `monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.917496 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.862023 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.846033 | `group_list` | ❌ |
| 4 | 0.830941 | `monitor_webtests_get` | ❌ |
| 5 | 0.825441 | `eventgrid_topic_list` | ❌ |

---

## Test 333

**Expected Tool:** `monitor_webtests_update`  
**Prompt:** Update an existing Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.838960 | `monitor_webtests_update` | ✅ **EXPECTED** |
| 2 | 0.804943 | `monitor_webtests_get` | ❌ |
| 3 | 0.797734 | `monitor_webtests_list` | ❌ |
| 4 | 0.777279 | `monitor_webtests_create` | ❌ |
| 5 | 0.760733 | `loadtesting_testrun_create` | ❌ |

---

## Test 334

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.918591 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.850868 | `grafana_list` | ❌ |
| 3 | 0.842137 | `deploy_app_logs_get` | ❌ |
| 4 | 0.836532 | `kusto_cluster_list` | ❌ |
| 5 | 0.832656 | `monitor_table_list` | ❌ |

---

## Test 335

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.870767 | `deploy_app_logs_get` | ❌ |
| 2 | 0.861385 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 3 | 0.828604 | `monitor_workspace_log_query` | ❌ |
| 4 | 0.814130 | `monitor_table_list` | ❌ |
| 5 | 0.807481 | `monitor_table_type_list` | ❌ |

---

## Test 336

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.897758 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.857938 | `deploy_app_logs_get` | ❌ |
| 3 | 0.826091 | `monitor_workspace_log_query` | ❌ |
| 4 | 0.822058 | `grafana_list` | ❌ |
| 5 | 0.820925 | `eventgrid_subscription_list` | ❌ |

---

## Test 337

**Expected Tool:** `monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.855162 | `deploy_app_logs_get` | ❌ |
| 2 | 0.853168 | `monitor_workspace_log_query` | ✅ **EXPECTED** |
| 3 | 0.827911 | `monitor_resource_log_query` | ❌ |
| 4 | 0.823379 | `monitor_activitylog_list` | ❌ |
| 5 | 0.815845 | `monitor_workspace_list` | ❌ |

---

## Test 338

**Expected Tool:** `datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.893413 | `datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.802297 | `redis_list` | ❌ |
| 3 | 0.777619 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.776555 | `group_list` | ❌ |
| 5 | 0.774190 | `monitor_metrics_query` | ❌ |

---

## Test 339

**Expected Tool:** `datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.877580 | `datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.805340 | `redis_list` | ❌ |
| 3 | 0.790070 | `monitor_metrics_query` | ❌ |
| 4 | 0.780563 | `quota_usage_check` | ❌ |
| 5 | 0.779453 | `deploy_app_logs_get` | ❌ |

---

## Test 340

**Expected Tool:** `extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843977 | `quota_usage_check` | ❌ |
| 2 | 0.835160 | `applens_resource_diagnose` | ❌ |
| 3 | 0.818047 | `subscription_list` | ❌ |
| 4 | 0.813126 | `extension_azqr` | ✅ **EXPECTED** |
| 5 | 0.811721 | `marketplace_product_list` | ❌ |

---

## Test 341

**Expected Tool:** `extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.827239 | `quota_usage_check` | ❌ |
| 2 | 0.820824 | `applens_resource_diagnose` | ❌ |
| 3 | 0.818905 | `cloudarchitect_design` | ❌ |
| 4 | 0.814683 | `subscription_list` | ❌ |
| 5 | 0.810504 | `marketplace_product_list` | ❌ |

---

## Test 342

**Expected Tool:** `extension_azqr`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.839099 | `quota_usage_check` | ❌ |
| 2 | 0.825566 | `applens_resource_diagnose` | ❌ |
| 3 | 0.822292 | `search_service_list` | ❌ |
| 4 | 0.821240 | `extension_azqr` | ✅ **EXPECTED** |
| 5 | 0.815321 | `marketplace_product_list` | ❌ |

---

## Test 343

**Expected Tool:** `quota_region_availability_list`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.849676 | `quota_region_availability_list` | ✅ **EXPECTED** |
| 2 | 0.795915 | `quota_usage_check` | ❌ |
| 3 | 0.777354 | `redis_list` | ❌ |
| 4 | 0.759725 | `group_list` | ❌ |
| 5 | 0.756711 | `eventgrid_topic_list` | ❌ |

---

## Test 344

**Expected Tool:** `quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.858891 | `quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.793713 | `quota_region_availability_list` | ❌ |
| 3 | 0.766212 | `applens_resource_diagnose` | ❌ |
| 4 | 0.763985 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.763012 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 345

**Expected Tool:** `role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.834996 | `role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.812028 | `subscription_list` | ❌ |
| 3 | 0.808743 | `kusto_cluster_list` | ❌ |
| 4 | 0.805561 | `search_service_list` | ❌ |
| 5 | 0.804305 | `eventgrid_subscription_list` | ❌ |

---

## Test 346

**Expected Tool:** `role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.826482 | `role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.816682 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.808661 | `subscription_list` | ❌ |
| 4 | 0.804538 | `eventgrid_topic_list` | ❌ |
| 5 | 0.802028 | `redis_list` | ❌ |

---

## Test 347

**Expected Tool:** `redis_list`  
**Prompt:** List all Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.919341 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.819484 | `group_list` | ❌ |
| 3 | 0.817669 | `kusto_cluster_list` | ❌ |
| 4 | 0.813246 | `grafana_list` | ❌ |
| 5 | 0.811886 | `eventgrid_topic_list` | ❌ |

---

## Test 348

**Expected Tool:** `redis_list`  
**Prompt:** Show me my Redis resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868860 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.779446 | `quota_usage_check` | ❌ |
| 3 | 0.766882 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.754078 | `deploy_app_logs_get` | ❌ |
| 5 | 0.752675 | `eventgrid_subscription_list` | ❌ |

---

## Test 349

**Expected Tool:** `redis_list`  
**Prompt:** Show me the Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.907147 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.805796 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.805620 | `eventgrid_topic_list` | ❌ |
| 4 | 0.792343 | `group_list` | ❌ |
| 5 | 0.787606 | `grafana_list` | ❌ |

---

## Test 350

**Expected Tool:** `redis_list`  
**Prompt:** Show me my Redis caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.829540 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.761341 | `postgres_database_list` | ❌ |
| 3 | 0.749666 | `postgres_table_list` | ❌ |
| 4 | 0.743058 | `quota_usage_check` | ❌ |
| 5 | 0.736989 | `deploy_app_logs_get` | ❌ |

---

## Test 351

**Expected Tool:** `redis_list`  
**Prompt:** Get Redis clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789084 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.782061 | `kusto_cluster_list` | ❌ |
| 3 | 0.768549 | `aks_cluster_get` | ❌ |
| 4 | 0.757925 | `kusto_database_list` | ❌ |
| 5 | 0.750544 | `aks_nodepool_get` | ❌ |

---

## Test 352

**Expected Tool:** `group_list`  
**Prompt:** List all resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.912210 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.853428 | `eventgrid_topic_list` | ❌ |
| 3 | 0.846332 | `kusto_cluster_list` | ❌ |
| 4 | 0.845507 | `redis_list` | ❌ |
| 5 | 0.842132 | `loadtesting_testresource_list` | ❌ |

---

## Test 353

**Expected Tool:** `group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.831722 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.810008 | `redis_list` | ❌ |
| 3 | 0.807500 | `eventgrid_topic_list` | ❌ |
| 4 | 0.803914 | `quota_usage_check` | ❌ |
| 5 | 0.800318 | `loadtesting_testresource_list` | ❌ |

---

## Test 354

**Expected Tool:** `group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.885575 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.857088 | `eventgrid_topic_list` | ❌ |
| 3 | 0.855957 | `redis_list` | ❌ |
| 4 | 0.847026 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.829705 | `quota_region_availability_list` | ❌ |

---

## Test 355

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.840651 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.821603 | `resourcehealth_availability-status_list` | ❌ |
| 3 | 0.767782 | `quota_usage_check` | ❌ |
| 4 | 0.756824 | `monitor_metrics_definitions` | ❌ |
| 5 | 0.752753 | `monitor_healthmodels_entity_get` | ❌ |

---

## Test 356

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.844127 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.824312 | `storage_account_get` | ❌ |
| 3 | 0.821574 | `storage_blob_container_get` | ❌ |
| 4 | 0.813187 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.807901 | `quota_usage_check` | ❌ |

---

## Test 357

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.838295 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.832098 | `resourcehealth_availability-status_list` | ❌ |
| 3 | 0.791186 | `quota_usage_check` | ❌ |
| 4 | 0.771561 | `applens_resource_diagnose` | ❌ |
| 5 | 0.770795 | `managedlustre_fs_list` | ❌ |

---

## Test 358

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.909559 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.852783 | `redis_list` | ❌ |
| 3 | 0.838126 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.837929 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.826610 | `search_service_list` | ❌ |

---

## Test 359

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.869054 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.852189 | `quota_usage_check` | ❌ |
| 3 | 0.845670 | `resourcehealth_availability-status_get` | ❌ |
| 4 | 0.822780 | `applens_resource_diagnose` | ❌ |
| 5 | 0.819445 | `loadtesting_testresource_list` | ❌ |

---

## Test 360

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.804274 | `resourcehealth_availability-status_get` | ❌ |
| 2 | 0.802278 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 3 | 0.802099 | `applens_resource_diagnose` | ❌ |
| 4 | 0.785235 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.781158 | `quota_usage_check` | ❌ |

---

## Test 361

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.861712 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.835872 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.832761 | `search_service_list` | ❌ |
| 4 | 0.831846 | `eventgrid_topic_list` | ❌ |
| 5 | 0.815611 | `kusto_cluster_list` | ❌ |

---

## Test 362

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.864582 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.844059 | `search_service_list` | ❌ |
| 3 | 0.832410 | `eventgrid_topic_list` | ❌ |
| 4 | 0.825710 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.822428 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 363

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** What service issues have occurred in the last 30 days?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.791286 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.759813 | `applens_resource_diagnose` | ❌ |
| 3 | 0.737538 | `deploy_app_logs_get` | ❌ |
| 4 | 0.729654 | `quota_usage_check` | ❌ |
| 5 | 0.728369 | `cloudarchitect_design` | ❌ |

---

## Test 364

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** List active service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.833717 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.829280 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.811795 | `eventgrid_topic_list` | ❌ |
| 4 | 0.802959 | `search_service_list` | ❌ |
| 5 | 0.774852 | `kusto_cluster_list` | ❌ |

---

## Test 365

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.835592 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.821613 | `quota_usage_check` | ❌ |
| 3 | 0.817292 | `search_service_list` | ❌ |
| 4 | 0.802143 | `applens_resource_diagnose` | ❌ |
| 5 | 0.800427 | `deploy_app_logs_get` | ❌ |

---

## Test 366

**Expected Tool:** `servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.831616 | `servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.814519 | `servicebus_topic_details` | ❌ |
| 3 | 0.785992 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.783941 | `redis_list` | ❌ |
| 5 | 0.782057 | `sql_server_show` | ❌ |

---

## Test 367

**Expected Tool:** `servicebus_topic_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851454 | `servicebus_topic_details` | ✅ **EXPECTED** |
| 2 | 0.806562 | `servicebus_topic_subscription_details` | ❌ |
| 3 | 0.795960 | `eventgrid_topic_list` | ❌ |
| 4 | 0.790087 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.783687 | `redis_list` | ❌ |

---

## Test 368

**Expected Tool:** `servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.840245 | `servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.834077 | `servicebus_topic_details` | ❌ |
| 3 | 0.826540 | `redis_list` | ❌ |
| 4 | 0.823738 | `kusto_cluster_get` | ❌ |
| 5 | 0.815742 | `search_service_list` | ❌ |

---

## Test 369

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show me the details of SignalR <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.809383 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.772240 | `eventhubs_eventhub_get` | ❌ |
| 3 | 0.767936 | `sql_server_show` | ❌ |
| 4 | 0.762918 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.761571 | `redis_list` | ❌ |

---

## Test 370

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show me the network information of SignalR runtime <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.839342 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.767355 | `sql_server_show` | ❌ |
| 3 | 0.746593 | `eventhubs_eventhub_get` | ❌ |
| 4 | 0.743524 | `servicebus_topic_details` | ❌ |
| 5 | 0.735644 | `redis_list` | ❌ |

---

## Test 371

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Describe the SignalR runtime <signalr_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851664 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.768968 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.766785 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.764940 | `eventgrid_topic_list` | ❌ |
| 5 | 0.762929 | `foundry_models_deploy` | ❌ |

---

## Test 372

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Get information about my SignalR runtime <signalr_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.877625 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.778225 | `eventhubs_eventhub_get` | ❌ |
| 3 | 0.777934 | `quota_usage_check` | ❌ |
| 4 | 0.776913 | `servicebus_topic_details` | ❌ |
| 5 | 0.776107 | `eventhubs_namespace_get` | ❌ |

---

## Test 373

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show all the SignalRs information in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.801139 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.788870 | `redis_list` | ❌ |
| 3 | 0.785706 | `group_list` | ❌ |
| 4 | 0.782587 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.773956 | `loadtesting_testresource_list` | ❌ |

---

## Test 374

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** List all SignalRs in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.794619 | `eventgrid_subscription_list` | ❌ |
| 2 | 0.794190 | `redis_list` | ❌ |
| 3 | 0.792777 | `search_service_list` | ❌ |
| 4 | 0.792107 | `eventgrid_topic_list` | ❌ |
| 5 | 0.777033 | `signalr_runtime_get` | ✅ **EXPECTED** |

---

## Test 375

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a new SQL database named <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.791527 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.791206 | `postgres_database_list` | ❌ |
| 3 | 0.781417 | `mysql_table_list` | ❌ |
| 4 | 0.770893 | `mysql_database_list` | ❌ |
| 5 | 0.769424 | `sql_db_rename` | ❌ |

---

## Test 376

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a SQL database <database_name> with Basic tier in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.808905 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.778982 | `mysql_table_list` | ❌ |
| 3 | 0.775333 | `mysql_database_query` | ❌ |
| 4 | 0.774103 | `postgres_database_list` | ❌ |
| 5 | 0.771927 | `sql_server_show` | ❌ |

---

## Test 377

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.823521 | `sql_server_create` | ❌ |
| 2 | 0.817751 | `sql_db_create` | ✅ **EXPECTED** |
| 3 | 0.802433 | `sql_db_rename` | ❌ |
| 4 | 0.798566 | `mysql_server_list` | ❌ |
| 5 | 0.797166 | `appservice_database_add` | ❌ |

---

## Test 378

**Expected Tool:** `sql_db_delete`  
**Prompt:** Delete the SQL database <database_name> from server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.826667 | `sql_server_delete` | ❌ |
| 2 | 0.824872 | `sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.767857 | `postgres_database_list` | ❌ |
| 4 | 0.760605 | `mysql_table_list` | ❌ |
| 5 | 0.752777 | `mysql_database_query` | ❌ |

---

## Test 379

**Expected Tool:** `sql_db_delete`  
**Prompt:** Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.838652 | `sql_server_delete` | ❌ |
| 2 | 0.832480 | `sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.785882 | `sql_db_rename` | ❌ |
| 4 | 0.772417 | `mysql_server_list` | ❌ |
| 5 | 0.771258 | `appservice_database_add` | ❌ |

---

## Test 380

**Expected Tool:** `sql_db_delete`  
**Prompt:** Delete the database called <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.823651 | `sql_db_delete` | ✅ **EXPECTED** |
| 2 | 0.819672 | `sql_server_delete` | ❌ |
| 3 | 0.778153 | `postgres_database_list` | ❌ |
| 4 | 0.766154 | `mysql_table_list` | ❌ |
| 5 | 0.756942 | `mysql_database_list` | ❌ |

---

## Test 381

**Expected Tool:** `sql_db_list`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.872949 | `mysql_database_list` | ❌ |
| 2 | 0.869679 | `postgres_database_list` | ❌ |
| 3 | 0.852748 | `kusto_database_list` | ❌ |
| 4 | 0.850883 | `mysql_table_list` | ❌ |
| 5 | 0.848527 | `kusto_table_list` | ❌ |

---

## Test 382

**Expected Tool:** `sql_db_list`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.861464 | `sql_server_show` | ❌ |
| 2 | 0.853800 | `mysql_database_list` | ❌ |
| 3 | 0.853001 | `mysql_server_config_get` | ❌ |
| 4 | 0.845998 | `sql_db_list` | ✅ **EXPECTED** |
| 5 | 0.833247 | `postgres_database_list` | ❌ |

---

## Test 383

**Expected Tool:** `sql_db_rename`  
**Prompt:** Rename the SQL database <database_name> on server <server_name> to <new_database_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.820633 | `sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.769383 | `sql_server_delete` | ❌ |
| 3 | 0.766500 | `sql_db_delete` | ❌ |
| 4 | 0.765873 | `mysql_table_list` | ❌ |
| 5 | 0.765545 | `postgres_database_list` | ❌ |

---

## Test 384

**Expected Tool:** `sql_db_rename`  
**Prompt:** Rename my Azure SQL database <database_name> to <new_database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.867572 | `sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.818728 | `sql_db_delete` | ❌ |
| 3 | 0.816661 | `sql_server_delete` | ❌ |
| 4 | 0.808741 | `sql_db_create` | ❌ |
| 5 | 0.794511 | `mysql_table_list` | ❌ |

---

## Test 385

**Expected Tool:** `sql_db_show`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.841836 | `sql_server_show` | ❌ |
| 2 | 0.837908 | `postgres_server_config_get` | ❌ |
| 3 | 0.832807 | `mysql_server_config_get` | ❌ |
| 4 | 0.800042 | `sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.798674 | `mysql_server_param_get` | ❌ |

---

## Test 386

**Expected Tool:** `sql_db_show`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.824731 | `postgres_database_list` | ❌ |
| 2 | 0.823927 | `sql_server_show` | ❌ |
| 3 | 0.818905 | `mysql_table_schema_get` | ❌ |
| 4 | 0.810948 | `mysql_table_list` | ❌ |
| 5 | 0.808168 | `postgres_table_list` | ❌ |

---

## Test 387

**Expected Tool:** `sql_db_update`  
**Prompt:** Update the performance tier of SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818568 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.791576 | `sql_db_create` | ❌ |
| 3 | 0.767192 | `mysql_server_param_set` | ❌ |
| 4 | 0.753117 | `sql_db_rename` | ❌ |
| 5 | 0.749712 | `sql_server_show` | ❌ |

---

## Test 388

**Expected Tool:** `sql_db_update`  
**Prompt:** Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.801331 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.759453 | `mysql_database_query` | ❌ |
| 3 | 0.757619 | `sql_server_delete` | ❌ |
| 4 | 0.756779 | `mysql_table_list` | ❌ |
| 5 | 0.753532 | `kusto_table_schema` | ❌ |

---

## Test 389

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.855533 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.796709 | `aks_nodepool_get` | ❌ |
| 3 | 0.785831 | `kusto_cluster_list` | ❌ |
| 4 | 0.785251 | `mysql_database_list` | ❌ |
| 5 | 0.783243 | `mysql_table_list` | ❌ |

---

## Test 390

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.829336 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.780002 | `aks_nodepool_get` | ❌ |
| 3 | 0.773168 | `sql_server_show` | ❌ |
| 4 | 0.771375 | `mysql_server_config_get` | ❌ |
| 5 | 0.766098 | `mysql_database_list` | ❌ |

---

## Test 391

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.820544 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.779960 | `mysql_server_list` | ❌ |
| 3 | 0.779487 | `aks_nodepool_get` | ❌ |
| 4 | 0.778977 | `mysql_database_list` | ❌ |
| 5 | 0.769456 | `mysql_table_list` | ❌ |

---

## Test 392

**Expected Tool:** `sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.864677 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.811178 | `sql_db_create` | ❌ |
| 3 | 0.809841 | `storage_account_create` | ❌ |
| 4 | 0.809783 | `mysql_server_list` | ❌ |
| 5 | 0.801756 | `sql_db_rename` | ❌ |

---

## Test 393

**Expected Tool:** `sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.862810 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.809713 | `sql_server_show` | ❌ |
| 3 | 0.799231 | `sql_db_create` | ❌ |
| 4 | 0.791808 | `storage_account_create` | ❌ |
| 5 | 0.786847 | `sql_server_delete` | ❌ |

---

## Test 394

**Expected Tool:** `sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.833359 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.795509 | `mysql_server_list` | ❌ |
| 3 | 0.789083 | `sql_server_delete` | ❌ |
| 4 | 0.788264 | `sql_server_show` | ❌ |
| 5 | 0.786621 | `sql_db_create` | ❌ |

---

## Test 395

**Expected Tool:** `sql_server_delete`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.859275 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.831949 | `sql_db_delete` | ❌ |
| 3 | 0.793028 | `sql_server_create` | ❌ |
| 4 | 0.784608 | `workbooks_delete` | ❌ |
| 5 | 0.782799 | `sql_server_show` | ❌ |

---

## Test 396

**Expected Tool:** `sql_server_delete`  
**Prompt:** Remove the SQL server <server_name> from my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852797 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.783340 | `postgres_server_list` | ❌ |
| 3 | 0.777521 | `sql_db_delete` | ❌ |
| 4 | 0.751897 | `sql_server_show` | ❌ |
| 5 | 0.746917 | `kusto_cluster_list` | ❌ |

---

## Test 397

**Expected Tool:** `sql_server_delete`  
**Prompt:** Delete SQL server <server_name> permanently  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851952 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.797903 | `sql_db_delete` | ❌ |
| 3 | 0.757697 | `sql_server_show` | ❌ |
| 4 | 0.750805 | `mysql_database_query` | ❌ |
| 5 | 0.737303 | `sql_server_firewall-rule_delete` | ❌ |

---

## Test 398

**Expected Tool:** `sql_server_entra-admin_list`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.898488 | `sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.778346 | `sql_server_show` | ❌ |
| 3 | 0.735783 | `mysql_table_list` | ❌ |
| 4 | 0.735595 | `sql_server_list` | ❌ |
| 5 | 0.734118 | `mysql_database_list` | ❌ |

---

## Test 399

**Expected Tool:** `sql_server_entra-admin_list`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.865818 | `sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.776424 | `sql_server_show` | ❌ |
| 3 | 0.745369 | `postgres_server_config_get` | ❌ |
| 4 | 0.742073 | `mysql_server_config_get` | ❌ |
| 5 | 0.737477 | `mysql_table_list` | ❌ |

---

## Test 400

**Expected Tool:** `sql_server_entra-admin_list`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.854495 | `sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.797624 | `sql_server_show` | ❌ |
| 3 | 0.755084 | `mysql_table_schema_get` | ❌ |
| 4 | 0.752474 | `mysql_database_query` | ❌ |
| 5 | 0.750405 | `cloudarchitect_design` | ❌ |

---

## Test 401

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.831247 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.815312 | `sql_server_firewall-rule_delete` | ❌ |
| 3 | 0.804633 | `sql_server_firewall-rule_list` | ❌ |
| 4 | 0.797793 | `sql_server_show` | ❌ |
| 5 | 0.795087 | `mysql_database_query` | ❌ |

---

## Test 402

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.876187 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.802577 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.793211 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.738543 | `mysql_database_query` | ❌ |
| 5 | 0.722857 | `postgres_database_query` | ❌ |

---

## Test 403

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.831960 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.814822 | `sql_server_firewall-rule_delete` | ❌ |
| 3 | 0.806122 | `sql_server_firewall-rule_list` | ❌ |
| 4 | 0.756460 | `mysql_database_query` | ❌ |
| 5 | 0.746974 | `sql_server_create` | ❌ |

---

## Test 404

**Expected Tool:** `sql_server_firewall-rule_delete`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.876376 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.818141 | `sql_server_delete` | ❌ |
| 3 | 0.801269 | `sql_db_delete` | ❌ |
| 4 | 0.800536 | `sql_server_firewall-rule_list` | ❌ |
| 5 | 0.791225 | `sql_server_firewall-rule_create` | ❌ |

---

## Test 405

**Expected Tool:** `sql_server_firewall-rule_delete`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852353 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.784204 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.782995 | `sql_server_delete` | ❌ |
| 4 | 0.777908 | `sql_server_firewall-rule_create` | ❌ |
| 5 | 0.748831 | `mysql_database_query` | ❌ |

---

## Test 406

**Expected Tool:** `sql_server_firewall-rule_delete`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.846622 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.787512 | `sql_server_delete` | ❌ |
| 3 | 0.786045 | `sql_server_firewall-rule_list` | ❌ |
| 4 | 0.771804 | `sql_server_firewall-rule_create` | ❌ |
| 5 | 0.761894 | `sql_db_delete` | ❌ |

---

## Test 407

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.872104 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.819716 | `sql_server_firewall-rule_delete` | ❌ |
| 3 | 0.815955 | `sql_server_firewall-rule_create` | ❌ |
| 4 | 0.782384 | `mysql_table_list` | ❌ |
| 5 | 0.778905 | `sql_server_show` | ❌ |

---

## Test 408

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.836402 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.811356 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.802883 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.787151 | `sql_server_show` | ❌ |
| 5 | 0.778071 | `mysql_database_query` | ❌ |

---

## Test 409

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.810427 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.801505 | `sql_server_show` | ❌ |
| 3 | 0.796004 | `sql_server_firewall-rule_create` | ❌ |
| 4 | 0.783020 | `sql_server_firewall-rule_delete` | ❌ |
| 5 | 0.776939 | `mysql_database_query` | ❌ |

---

## Test 410

**Expected Tool:** `sql_server_list`  
**Prompt:** List all Azure SQL servers in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852366 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.849387 | `mysql_server_list` | ❌ |
| 3 | 0.828202 | `kusto_cluster_list` | ❌ |
| 4 | 0.826725 | `search_service_list` | ❌ |
| 5 | 0.811977 | `kusto_table_list` | ❌ |

---

## Test 411

**Expected Tool:** `sql_server_list`  
**Prompt:** Show me every SQL server available in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.833980 | `mysql_server_list` | ❌ |
| 2 | 0.809229 | `sql_server_list` | ✅ **EXPECTED** |
| 3 | 0.803620 | `kusto_cluster_list` | ❌ |
| 4 | 0.793686 | `kusto_database_list` | ❌ |
| 5 | 0.793317 | `redis_list` | ❌ |

---

## Test 412

**Expected Tool:** `sql_server_show`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.849971 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.833702 | `sql_db_show` | ❌ |
| 3 | 0.831025 | `mysql_server_list` | ❌ |
| 4 | 0.824924 | `sql_server_list` | ❌ |
| 5 | 0.817213 | `sql_server_create` | ❌ |

---

## Test 413

**Expected Tool:** `sql_server_show`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.864055 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.842651 | `postgres_server_config_get` | ❌ |
| 3 | 0.840563 | `mysql_server_config_get` | ❌ |
| 4 | 0.801786 | `mysql_server_param_get` | ❌ |
| 5 | 0.786885 | `sql_db_show` | ❌ |

---

## Test 414

**Expected Tool:** `sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.849154 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.798635 | `mysql_server_config_get` | ❌ |
| 3 | 0.789879 | `mysql_table_schema_get` | ❌ |
| 4 | 0.783357 | `postgres_database_list` | ❌ |
| 5 | 0.778578 | `mysql_table_list` | ❌ |

---

## Test 415

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.810126 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.803393 | `quota_usage_check` | ❌ |
| 3 | 0.798147 | `storage_blob_container_create` | ❌ |
| 4 | 0.793527 | `storage_account_get` | ❌ |
| 5 | 0.779202 | `loadtesting_testresource_create` | ❌ |

---

## Test 416

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.800424 | `storage_blob_container_create` | ❌ |
| 2 | 0.795116 | `managedlustre_fs_list` | ❌ |
| 3 | 0.794841 | `managedlustre_fs_create` | ❌ |
| 4 | 0.794145 | `storage_account_get` | ❌ |
| 5 | 0.792234 | `storage_account_create` | ✅ **EXPECTED** |

---

## Test 417

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.792725 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.784383 | `storage_account_get` | ❌ |
| 3 | 0.783429 | `storage_blob_container_create` | ❌ |
| 4 | 0.771109 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.770299 | `managedlustre_fs_list` | ❌ |

---

## Test 418

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.864705 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.852899 | `storage_blob_container_get` | ❌ |
| 3 | 0.833180 | `storage_blob_get` | ❌ |
| 4 | 0.809586 | `sql_server_show` | ❌ |
| 5 | 0.808785 | `quota_usage_check` | ❌ |

---

## Test 419

**Expected Tool:** `storage_account_get`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.887388 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.848471 | `storage_blob_container_get` | ❌ |
| 3 | 0.839562 | `storage_account_create` | ❌ |
| 4 | 0.838349 | `storage_blob_get` | ❌ |
| 5 | 0.829422 | `sql_server_show` | ❌ |

---

## Test 420

**Expected Tool:** `storage_account_get`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.883045 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.847833 | `kusto_cluster_list` | ❌ |
| 3 | 0.846985 | `subscription_list` | ❌ |
| 4 | 0.842619 | `search_service_list` | ❌ |
| 5 | 0.834655 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 421

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.824986 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.783882 | `storage_blob_container_get` | ❌ |
| 3 | 0.782774 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.782649 | `eventhubs_eventhub_get` | ❌ |
| 5 | 0.770657 | `keyvault_admin_settings_get` | ❌ |

---

## Test 422

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.865927 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.860765 | `storage_blob_container_get` | ❌ |
| 3 | 0.839171 | `storage_blob_get` | ❌ |
| 4 | 0.822019 | `subscription_list` | ❌ |
| 5 | 0.816924 | `storage_blob_container_create` | ❌ |

---

## Test 423

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.850639 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.834053 | `storage_blob_container_get` | ❌ |
| 3 | 0.802218 | `storage_blob_get` | ❌ |
| 4 | 0.795190 | `storage_account_create` | ❌ |
| 5 | 0.791078 | `cosmos_database_container_list` | ❌ |

---

## Test 424

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.882125 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.864153 | `storage_blob_container_get` | ❌ |
| 3 | 0.839007 | `storage_blob_get` | ❌ |
| 4 | 0.806190 | `storage_account_get` | ❌ |
| 5 | 0.796376 | `cosmos_database_container_item_query` | ❌ |

---

## Test 425

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.876258 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.849822 | `storage_blob_container_get` | ❌ |
| 3 | 0.826988 | `storage_blob_get` | ❌ |
| 4 | 0.805862 | `storage_account_get` | ❌ |
| 5 | 0.800129 | `cosmos_database_container_item_query` | ❌ |

---

## Test 426

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.891728 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.857128 | `storage_blob_get` | ❌ |
| 3 | 0.832367 | `storage_account_get` | ❌ |
| 4 | 0.829749 | `storage_blob_container_create` | ❌ |
| 5 | 0.814505 | `cosmos_database_container_list` | ❌ |

---

## Test 427

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.892410 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.879298 | `storage_blob_get` | ❌ |
| 3 | 0.854342 | `storage_blob_container_create` | ❌ |
| 4 | 0.844712 | `cosmos_database_container_list` | ❌ |
| 5 | 0.829854 | `storage_account_get` | ❌ |

---

## Test 428

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879623 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.847456 | `cosmos_database_container_list` | ❌ |
| 3 | 0.833656 | `storage_blob_get` | ❌ |
| 4 | 0.825182 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.818749 | `storage_account_get` | ❌ |

---

## Test 429

**Expected Tool:** `storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.894876 | `storage_blob_container_get` | ❌ |
| 2 | 0.891593 | `storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.837119 | `storage_blob_container_create` | ❌ |
| 4 | 0.829013 | `storage_account_get` | ❌ |
| 5 | 0.787061 | `cosmos_database_container_list` | ❌ |

---

## Test 430

**Expected Tool:** `storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.893390 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.883276 | `storage_blob_container_get` | ❌ |
| 3 | 0.861351 | `storage_account_get` | ❌ |
| 4 | 0.849169 | `storage_blob_container_create` | ❌ |
| 5 | 0.805667 | `cosmos_database_container_item_query` | ❌ |

---

## Test 431

**Expected Tool:** `storage_blob_get`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.893070 | `storage_blob_container_get` | ❌ |
| 2 | 0.890783 | `storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.861377 | `storage_blob_container_create` | ❌ |
| 4 | 0.844922 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.841280 | `cosmos_database_container_list` | ❌ |

---

## Test 432

**Expected Tool:** `storage_blob_get`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.895199 | `storage_blob_container_get` | ❌ |
| 2 | 0.884449 | `storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.847241 | `storage_blob_container_create` | ❌ |
| 4 | 0.827470 | `storage_account_get` | ❌ |
| 5 | 0.818979 | `cosmos_database_container_list` | ❌ |

---

## Test 433

**Expected Tool:** `storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.837264 | `storage_blob_container_create` | ❌ |
| 2 | 0.834800 | `storage_blob_upload` | ✅ **EXPECTED** |
| 3 | 0.822190 | `storage_blob_get` | ❌ |
| 4 | 0.809272 | `storage_blob_container_get` | ❌ |
| 5 | 0.768544 | `storage_account_get` | ❌ |

---

## Test 434

**Expected Tool:** `subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.856770 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.842356 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.831559 | `eventgrid_topic_list` | ❌ |
| 4 | 0.825601 | `cosmos_account_list` | ❌ |
| 5 | 0.825060 | `search_service_list` | ❌ |

---

## Test 435

**Expected Tool:** `subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.819054 | `eventgrid_subscription_list` | ❌ |
| 2 | 0.803164 | `redis_list` | ❌ |
| 3 | 0.800703 | `eventgrid_topic_list` | ❌ |
| 4 | 0.795662 | `subscription_list` | ✅ **EXPECTED** |
| 5 | 0.785142 | `search_service_list` | ❌ |

---

## Test 436

**Expected Tool:** `subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.791653 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.784686 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.777850 | `eventgrid_topic_list` | ❌ |
| 4 | 0.770929 | `redis_list` | ❌ |
| 5 | 0.768711 | `marketplace_product_list` | ❌ |

---

## Test 437

**Expected Tool:** `subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.805756 | `eventgrid_subscription_list` | ❌ |
| 2 | 0.799051 | `subscription_list` | ✅ **EXPECTED** |
| 3 | 0.792453 | `redis_list` | ❌ |
| 4 | 0.792011 | `eventgrid_topic_list` | ❌ |
| 5 | 0.775511 | `search_service_list` | ❌ |

---

## Test 438

**Expected Tool:** `azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879147 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.861340 | `get_bestpractices_get` | ❌ |
| 3 | 0.814913 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.804409 | `cloudarchitect_design` | ❌ |
| 5 | 0.800976 | `quota_usage_check` | ❌ |

---

## Test 439

**Expected Tool:** `azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.821345 | `keyvault_secret_get` | ❌ |
| 2 | 0.812086 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 3 | 0.808736 | `keyvault_secret_create` | ❌ |
| 4 | 0.807316 | `get_bestpractices_get` | ❌ |
| 5 | 0.795954 | `keyvault_secret_list` | ❌ |

---

## Test 440

**Expected Tool:** `virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.875443 | `virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
| 2 | 0.849081 | `virtualdesktop_hostpool_host_list` | ❌ |
| 3 | 0.811656 | `kusto_cluster_list` | ❌ |
| 4 | 0.804105 | `postgres_server_list` | ❌ |
| 5 | 0.802689 | `virtualdesktop_hostpool_host_user-list` | ❌ |

---

## Test 441

**Expected Tool:** `virtualdesktop_hostpool_host_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.877241 | `virtualdesktop_hostpool_host_list` | ✅ **EXPECTED** |
| 2 | 0.872758 | `virtualdesktop_hostpool_host_user-list` | ❌ |
| 3 | 0.814023 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.768581 | `aks_nodepool_get` | ❌ |
| 5 | 0.745038 | `postgres_server_list` | ❌ |

---

## Test 442

**Expected Tool:** `virtualdesktop_hostpool_host_user-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.903056 | `virtualdesktop_hostpool_host_user-list` | ✅ **EXPECTED** |
| 2 | 0.852891 | `virtualdesktop_hostpool_host_list` | ❌ |
| 3 | 0.792108 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.754193 | `aks_nodepool_get` | ❌ |
| 5 | 0.740602 | `postgres_database_list` | ❌ |

---

## Test 443

**Expected Tool:** `workbooks_create`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.814549 | `workbooks_create` | ✅ **EXPECTED** |
| 2 | 0.749741 | `workbooks_update` | ❌ |
| 3 | 0.746779 | `workbooks_delete` | ❌ |
| 4 | 0.740905 | `workbooks_list` | ❌ |
| 5 | 0.734078 | `workbooks_show` | ❌ |

---

## Test 444

**Expected Tool:** `workbooks_delete`  
**Prompt:** Delete the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.871020 | `workbooks_delete` | ✅ **EXPECTED** |
| 2 | 0.800176 | `workbooks_show` | ❌ |
| 3 | 0.775506 | `workbooks_list` | ❌ |
| 4 | 0.767917 | `workbooks_create` | ❌ |
| 5 | 0.760547 | `workbooks_update` | ❌ |

---

## Test 445

**Expected Tool:** `workbooks_list`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.882445 | `workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.795826 | `workbooks_create` | ❌ |
| 3 | 0.794634 | `group_list` | ❌ |
| 4 | 0.785093 | `workbooks_delete` | ❌ |
| 5 | 0.784826 | `loadtesting_testresource_list` | ❌ |

---

## Test 446

**Expected Tool:** `workbooks_list`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.831874 | `workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.786614 | `workbooks_create` | ❌ |
| 3 | 0.777631 | `workbooks_delete` | ❌ |
| 4 | 0.770929 | `redis_list` | ❌ |
| 5 | 0.770039 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 447

**Expected Tool:** `workbooks_show`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.885740 | `workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.821996 | `workbooks_create` | ❌ |
| 3 | 0.802502 | `workbooks_list` | ❌ |
| 4 | 0.799816 | `workbooks_delete` | ❌ |
| 5 | 0.792377 | `workbooks_update` | ❌ |

---

## Test 448

**Expected Tool:** `workbooks_show`  
**Prompt:** Show me the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.825558 | `workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.812208 | `workbooks_list` | ❌ |
| 3 | 0.809751 | `workbooks_delete` | ❌ |
| 4 | 0.801816 | `workbooks_create` | ❌ |
| 5 | 0.779042 | `workbooks_update` | ❌ |

---

## Test 449

**Expected Tool:** `workbooks_update`  
**Prompt:** Update the workbook <workbook_resource_id> with a new text step  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818557 | `workbooks_update` | ✅ **EXPECTED** |
| 2 | 0.730313 | `workbooks_delete` | ❌ |
| 3 | 0.725172 | `workbooks_create` | ❌ |
| 4 | 0.724545 | `loadtesting_testrun_update` | ❌ |
| 5 | 0.719208 | `workbooks_show` | ❌ |

---

## Test 450

**Expected Tool:** `bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.829298 | `bicepschema_get` | ✅ **EXPECTED** |
| 2 | 0.819148 | `foundry_models_deploy` | ❌ |
| 3 | 0.804941 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.803211 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.798585 | `speech_stt_recognize` | ❌ |

---

## Test 451

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.839646 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.781875 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.777336 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.768970 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.753659 | `deploy_plan_get` | ❌ |

---

## Test 452

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Help me design an Azure cloud service that will serve as an ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.831567 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.800906 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.797772 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.794105 | `deploy_plan_get` | ❌ |
| 5 | 0.790057 | `quota_usage_check` | ❌ |

---

## Test 453

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.805150 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.775187 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.773639 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.768590 | `deploy_app_logs_get` | ❌ |
| 5 | 0.764537 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 454

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.833130 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.796093 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.795801 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.780336 | `speech_stt_recognize` | ❌ |
| 5 | 0.777460 | `quota_usage_check` | ❌ |

---

## Summary

**Total Prompts Tested:** 454  
**Analysis Execution Time:** 222.5577789s  

### Success Rate Metrics

**Top Choice Success:** 85.0% (386/454 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 92.5% (420/454 tests)  
**🎯 High Confidence (≥0.7):** 99.6% (452/454 tests)  
**✅ Good Confidence (≥0.6):** 99.6% (452/454 tests)  
**👍 Fair Confidence (≥0.5):** 99.6% (452/454 tests)  
**👌 Acceptable Confidence (≥0.4):** 99.6% (452/454 tests)  
**❌ Low Confidence (<0.4):** 0.4% (2/454 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 80.4% (365/454 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 85.0% (386/454 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 85.0% (386/454 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 85.0% (386/454 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 85.0% (386/454 tests)  

### Success Rate Analysis

🟡 **Good** - The tool selection system is performing well.

🎯 **Recommendation:** Tool descriptions are highly optimized for user intent matching.

