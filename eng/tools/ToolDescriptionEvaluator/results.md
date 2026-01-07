# Tool Selection Analysis Setup

**Setup completed:** 2026-01-07 16:04:28  
**Tool count:** 200  
**Database setup time:** 1.9081623s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2026-01-07 16:04:28  
**Tool count:** 200  

## Table of Contents

- [Test 1: search_knowledge_base_get](#test-1)
- [Test 2: search_knowledge_base_get](#test-2)
- [Test 3: search_knowledge_base_get](#test-3)
- [Test 4: search_knowledge_base_get](#test-4)
- [Test 5: search_knowledge_base_get](#test-5)
- [Test 6: search_knowledge_base_get](#test-6)
- [Test 7: search_knowledge_base_retrieve](#test-7)
- [Test 8: search_knowledge_base_retrieve](#test-8)
- [Test 9: search_knowledge_base_retrieve](#test-9)
- [Test 10: search_knowledge_base_retrieve](#test-10)
- [Test 11: search_knowledge_base_retrieve](#test-11)
- [Test 12: search_knowledge_base_retrieve](#test-12)
- [Test 13: search_knowledge_base_retrieve](#test-13)
- [Test 14: search_knowledge_base_retrieve](#test-14)
- [Test 15: search_knowledge_source_get](#test-15)
- [Test 16: search_knowledge_source_get](#test-16)
- [Test 17: search_knowledge_source_get](#test-17)
- [Test 18: search_knowledge_source_get](#test-18)
- [Test 19: search_knowledge_source_get](#test-19)
- [Test 20: search_knowledge_source_get](#test-20)
- [Test 21: search_index_get](#test-21)
- [Test 22: search_index_get](#test-22)
- [Test 23: search_index_get](#test-23)
- [Test 24: search_index_query](#test-24)
- [Test 25: search_service_list](#test-25)
- [Test 26: search_service_list](#test-26)
- [Test 27: search_service_list](#test-27)
- [Test 28: speech_stt_recognize](#test-28)
- [Test 29: speech_stt_recognize](#test-29)
- [Test 30: speech_stt_recognize](#test-30)
- [Test 31: speech_stt_recognize](#test-31)
- [Test 32: speech_stt_recognize](#test-32)
- [Test 33: speech_stt_recognize](#test-33)
- [Test 34: speech_stt_recognize](#test-34)
- [Test 35: speech_stt_recognize](#test-35)
- [Test 36: speech_stt_recognize](#test-36)
- [Test 37: speech_stt_recognize](#test-37)
- [Test 38: speech_tts_synthesize](#test-38)
- [Test 39: speech_tts_synthesize](#test-39)
- [Test 40: speech_tts_synthesize](#test-40)
- [Test 41: speech_tts_synthesize](#test-41)
- [Test 42: speech_tts_synthesize](#test-42)
- [Test 43: speech_tts_synthesize](#test-43)
- [Test 44: speech_tts_synthesize](#test-44)
- [Test 45: speech_tts_synthesize](#test-45)
- [Test 46: speech_tts_synthesize](#test-46)
- [Test 47: speech_tts_synthesize](#test-47)
- [Test 48: appconfig_account_list](#test-48)
- [Test 49: appconfig_account_list](#test-49)
- [Test 50: appconfig_account_list](#test-50)
- [Test 51: appconfig_kv_delete](#test-51)
- [Test 52: appconfig_kv_get](#test-52)
- [Test 53: appconfig_kv_get](#test-53)
- [Test 54: appconfig_kv_get](#test-54)
- [Test 55: appconfig_kv_get](#test-55)
- [Test 56: appconfig_kv_lock_set](#test-56)
- [Test 57: appconfig_kv_lock_set](#test-57)
- [Test 58: appconfig_kv_set](#test-58)
- [Test 59: applens_resource_diagnose](#test-59)
- [Test 60: applens_resource_diagnose](#test-60)
- [Test 61: applens_resource_diagnose](#test-61)
- [Test 62: appservice_database_add](#test-62)
- [Test 63: appservice_database_add](#test-63)
- [Test 64: appservice_database_add](#test-64)
- [Test 65: appservice_database_add](#test-65)
- [Test 66: appservice_database_add](#test-66)
- [Test 67: appservice_database_add](#test-67)
- [Test 68: appservice_database_add](#test-68)
- [Test 69: appservice_database_add](#test-69)
- [Test 70: appservice_database_add](#test-70)
- [Test 71: appservice_database_add](#test-71)
- [Test 72: applicationinsights_recommendation_list](#test-72)
- [Test 73: applicationinsights_recommendation_list](#test-73)
- [Test 74: applicationinsights_recommendation_list](#test-74)
- [Test 75: applicationinsights_recommendation_list](#test-75)
- [Test 76: extension_cli_generate](#test-76)
- [Test 77: extension_cli_generate](#test-77)
- [Test 78: extension_cli_generate](#test-78)
- [Test 79: extension_cli_install](#test-79)
- [Test 80: extension_cli_install](#test-80)
- [Test 81: extension_cli_install](#test-81)
- [Test 82: acr_registry_list](#test-82)
- [Test 83: acr_registry_list](#test-83)
- [Test 84: acr_registry_list](#test-84)
- [Test 85: acr_registry_list](#test-85)
- [Test 86: acr_registry_list](#test-86)
- [Test 87: acr_registry_repository_list](#test-87)
- [Test 88: acr_registry_repository_list](#test-88)
- [Test 89: acr_registry_repository_list](#test-89)
- [Test 90: acr_registry_repository_list](#test-90)
- [Test 91: communication_email_send](#test-91)
- [Test 92: communication_email_send](#test-92)
- [Test 93: communication_email_send](#test-93)
- [Test 94: communication_email_send](#test-94)
- [Test 95: communication_email_send](#test-95)
- [Test 96: communication_email_send](#test-96)
- [Test 97: communication_email_send](#test-97)
- [Test 98: communication_email_send](#test-98)
- [Test 99: communication_sms_send](#test-99)
- [Test 100: communication_sms_send](#test-100)
- [Test 101: communication_sms_send](#test-101)
- [Test 102: communication_sms_send](#test-102)
- [Test 103: communication_sms_send](#test-103)
- [Test 104: communication_sms_send](#test-104)
- [Test 105: communication_sms_send](#test-105)
- [Test 106: communication_sms_send](#test-106)
- [Test 107: confidentialledger_entries_append](#test-107)
- [Test 108: confidentialledger_entries_append](#test-108)
- [Test 109: confidentialledger_entries_append](#test-109)
- [Test 110: confidentialledger_entries_append](#test-110)
- [Test 111: confidentialledger_entries_append](#test-111)
- [Test 112: confidentialledger_entries_get](#test-112)
- [Test 113: confidentialledger_entries_get](#test-113)
- [Test 114: cosmos_account_list](#test-114)
- [Test 115: cosmos_account_list](#test-115)
- [Test 116: cosmos_account_list](#test-116)
- [Test 117: cosmos_database_container_item_query](#test-117)
- [Test 118: cosmos_database_container_list](#test-118)
- [Test 119: cosmos_database_container_list](#test-119)
- [Test 120: cosmos_database_list](#test-120)
- [Test 121: cosmos_database_list](#test-121)
- [Test 122: kusto_cluster_get](#test-122)
- [Test 123: kusto_cluster_list](#test-123)
- [Test 124: kusto_cluster_list](#test-124)
- [Test 125: kusto_cluster_list](#test-125)
- [Test 126: kusto_database_list](#test-126)
- [Test 127: kusto_database_list](#test-127)
- [Test 128: kusto_query](#test-128)
- [Test 129: kusto_sample](#test-129)
- [Test 130: kusto_table_list](#test-130)
- [Test 131: kusto_table_list](#test-131)
- [Test 132: kusto_table_schema](#test-132)
- [Test 133: mysql_database_list](#test-133)
- [Test 134: mysql_database_list](#test-134)
- [Test 135: mysql_database_query](#test-135)
- [Test 136: mysql_server_config_get](#test-136)
- [Test 137: mysql_server_list](#test-137)
- [Test 138: mysql_server_list](#test-138)
- [Test 139: mysql_server_list](#test-139)
- [Test 140: mysql_server_param_get](#test-140)
- [Test 141: mysql_server_param_set](#test-141)
- [Test 142: mysql_table_list](#test-142)
- [Test 143: mysql_table_list](#test-143)
- [Test 144: mysql_table_schema_get](#test-144)
- [Test 145: postgres_database_list](#test-145)
- [Test 146: postgres_database_list](#test-146)
- [Test 147: postgres_database_query](#test-147)
- [Test 148: postgres_server_config_get](#test-148)
- [Test 149: postgres_server_list](#test-149)
- [Test 150: postgres_server_list](#test-150)
- [Test 151: postgres_server_list](#test-151)
- [Test 152: postgres_server_param_get](#test-152)
- [Test 153: postgres_server_param_set](#test-153)
- [Test 154: postgres_table_list](#test-154)
- [Test 155: postgres_table_list](#test-155)
- [Test 156: postgres_table_schema_get](#test-156)
- [Test 157: deploy_app_logs_get](#test-157)
- [Test 158: deploy_architecture_diagram_generate](#test-158)
- [Test 159: deploy_iac_rules_get](#test-159)
- [Test 160: deploy_pipeline_guidance_get](#test-160)
- [Test 161: deploy_plan_get](#test-161)
- [Test 162: eventgrid_events_publish](#test-162)
- [Test 163: eventgrid_events_publish](#test-163)
- [Test 164: eventgrid_events_publish](#test-164)
- [Test 165: eventgrid_topic_list](#test-165)
- [Test 166: eventgrid_topic_list](#test-166)
- [Test 167: eventgrid_topic_list](#test-167)
- [Test 168: eventgrid_topic_list](#test-168)
- [Test 169: eventgrid_subscription_list](#test-169)
- [Test 170: eventgrid_subscription_list](#test-170)
- [Test 171: eventgrid_subscription_list](#test-171)
- [Test 172: eventgrid_subscription_list](#test-172)
- [Test 173: eventgrid_subscription_list](#test-173)
- [Test 174: eventgrid_subscription_list](#test-174)
- [Test 175: eventgrid_subscription_list](#test-175)
- [Test 176: eventhubs_eventhub_consumergroup_delete](#test-176)
- [Test 177: eventhubs_eventhub_consumergroup_get](#test-177)
- [Test 178: eventhubs_eventhub_consumergroup_get](#test-178)
- [Test 179: eventhubs_eventhub_consumergroup_update](#test-179)
- [Test 180: eventhubs_eventhub_consumergroup_update](#test-180)
- [Test 181: eventhubs_eventhub_delete](#test-181)
- [Test 182: eventhubs_eventhub_get](#test-182)
- [Test 183: eventhubs_eventhub_get](#test-183)
- [Test 184: eventhubs_eventhub_update](#test-184)
- [Test 185: eventhubs_eventhub_update](#test-185)
- [Test 186: eventhubs_namespace_delete](#test-186)
- [Test 187: eventhubs_namespace_get](#test-187)
- [Test 188: eventhubs_namespace_get](#test-188)
- [Test 189: eventhubs_namespace_update](#test-189)
- [Test 190: eventhubs_namespace_update](#test-190)
- [Test 191: functionapp_get](#test-191)
- [Test 192: functionapp_get](#test-192)
- [Test 193: functionapp_get](#test-193)
- [Test 194: functionapp_get](#test-194)
- [Test 195: functionapp_get](#test-195)
- [Test 196: functionapp_get](#test-196)
- [Test 197: functionapp_get](#test-197)
- [Test 198: functionapp_get](#test-198)
- [Test 199: functionapp_get](#test-199)
- [Test 200: functionapp_get](#test-200)
- [Test 201: functionapp_get](#test-201)
- [Test 202: functionapp_get](#test-202)
- [Test 203: keyvault_admin_settings_get](#test-203)
- [Test 204: keyvault_admin_settings_get](#test-204)
- [Test 205: keyvault_admin_settings_get](#test-205)
- [Test 206: keyvault_certificate_create](#test-206)
- [Test 207: keyvault_certificate_create](#test-207)
- [Test 208: keyvault_certificate_create](#test-208)
- [Test 209: keyvault_certificate_create](#test-209)
- [Test 210: keyvault_certificate_create](#test-210)
- [Test 211: keyvault_certificate_get](#test-211)
- [Test 212: keyvault_certificate_get](#test-212)
- [Test 213: keyvault_certificate_get](#test-213)
- [Test 214: keyvault_certificate_get](#test-214)
- [Test 215: keyvault_certificate_get](#test-215)
- [Test 216: keyvault_certificate_import](#test-216)
- [Test 217: keyvault_certificate_import](#test-217)
- [Test 218: keyvault_certificate_import](#test-218)
- [Test 219: keyvault_certificate_import](#test-219)
- [Test 220: keyvault_certificate_import](#test-220)
- [Test 221: keyvault_certificate_list](#test-221)
- [Test 222: keyvault_certificate_list](#test-222)
- [Test 223: keyvault_certificate_list](#test-223)
- [Test 224: keyvault_certificate_list](#test-224)
- [Test 225: keyvault_certificate_list](#test-225)
- [Test 226: keyvault_certificate_list](#test-226)
- [Test 227: keyvault_key_create](#test-227)
- [Test 228: keyvault_key_create](#test-228)
- [Test 229: keyvault_key_create](#test-229)
- [Test 230: keyvault_key_create](#test-230)
- [Test 231: keyvault_key_create](#test-231)
- [Test 232: keyvault_key_get](#test-232)
- [Test 233: keyvault_key_get](#test-233)
- [Test 234: keyvault_key_get](#test-234)
- [Test 235: keyvault_key_get](#test-235)
- [Test 236: keyvault_key_get](#test-236)
- [Test 237: keyvault_key_list](#test-237)
- [Test 238: keyvault_key_list](#test-238)
- [Test 239: keyvault_key_list](#test-239)
- [Test 240: keyvault_key_list](#test-240)
- [Test 241: keyvault_key_list](#test-241)
- [Test 242: keyvault_key_list](#test-242)
- [Test 243: keyvault_secret_create](#test-243)
- [Test 244: keyvault_secret_create](#test-244)
- [Test 245: keyvault_secret_create](#test-245)
- [Test 246: keyvault_secret_create](#test-246)
- [Test 247: keyvault_secret_create](#test-247)
- [Test 248: keyvault_secret_get](#test-248)
- [Test 249: keyvault_secret_get](#test-249)
- [Test 250: keyvault_secret_get](#test-250)
- [Test 251: keyvault_secret_get](#test-251)
- [Test 252: keyvault_secret_get](#test-252)
- [Test 253: keyvault_secret_list](#test-253)
- [Test 254: keyvault_secret_list](#test-254)
- [Test 255: keyvault_secret_list](#test-255)
- [Test 256: keyvault_secret_list](#test-256)
- [Test 257: keyvault_secret_list](#test-257)
- [Test 258: keyvault_secret_list](#test-258)
- [Test 259: aks_cluster_get](#test-259)
- [Test 260: aks_cluster_get](#test-260)
- [Test 261: aks_cluster_get](#test-261)
- [Test 262: aks_cluster_get](#test-262)
- [Test 263: aks_cluster_get](#test-263)
- [Test 264: aks_cluster_get](#test-264)
- [Test 265: aks_cluster_get](#test-265)
- [Test 266: aks_nodepool_get](#test-266)
- [Test 267: aks_nodepool_get](#test-267)
- [Test 268: aks_nodepool_get](#test-268)
- [Test 269: aks_nodepool_get](#test-269)
- [Test 270: aks_nodepool_get](#test-270)
- [Test 271: aks_nodepool_get](#test-271)
- [Test 272: loadtesting_test_create](#test-272)
- [Test 273: loadtesting_test_get](#test-273)
- [Test 274: loadtesting_testresource_create](#test-274)
- [Test 275: loadtesting_testresource_list](#test-275)
- [Test 276: loadtesting_testrun_create](#test-276)
- [Test 277: loadtesting_testrun_get](#test-277)
- [Test 278: loadtesting_testrun_list](#test-278)
- [Test 279: loadtesting_testrun_update](#test-279)
- [Test 280: grafana_list](#test-280)
- [Test 281: managedlustre_fs_create](#test-281)
- [Test 282: managedlustre_fs_list](#test-282)
- [Test 283: managedlustre_fs_list](#test-283)
- [Test 284: managedlustre_fs_sku_get](#test-284)
- [Test 285: managedlustre_fs_subnetsize_ask](#test-285)
- [Test 286: managedlustre_fs_subnetsize_validate](#test-286)
- [Test 287: managedlustre_fs_update](#test-287)
- [Test 288: marketplace_product_get](#test-288)
- [Test 289: marketplace_product_list](#test-289)
- [Test 290: marketplace_product_list](#test-290)
- [Test 291: get_bestpractices_get](#test-291)
- [Test 292: get_bestpractices_get](#test-292)
- [Test 293: get_bestpractices_get](#test-293)
- [Test 294: get_bestpractices_get](#test-294)
- [Test 295: get_bestpractices_get](#test-295)
- [Test 296: get_bestpractices_get](#test-296)
- [Test 297: get_bestpractices_get](#test-297)
- [Test 298: get_bestpractices_get](#test-298)
- [Test 299: get_bestpractices_get](#test-299)
- [Test 300: get_bestpractices_ai_app](#test-300)
- [Test 301: get_bestpractices_ai_app](#test-301)
- [Test 302: get_bestpractices_ai_app](#test-302)
- [Test 303: get_bestpractices_ai_app](#test-303)
- [Test 304: get_bestpractices_ai_app](#test-304)
- [Test 305: monitor_activitylog_list](#test-305)
- [Test 306: monitor_healthmodels_entity_get](#test-306)
- [Test 307: monitor_metrics_definitions](#test-307)
- [Test 308: monitor_metrics_definitions](#test-308)
- [Test 309: monitor_metrics_definitions](#test-309)
- [Test 310: monitor_metrics_query](#test-310)
- [Test 311: monitor_metrics_query](#test-311)
- [Test 312: monitor_metrics_query](#test-312)
- [Test 313: monitor_metrics_query](#test-313)
- [Test 314: monitor_metrics_query](#test-314)
- [Test 315: monitor_metrics_query](#test-315)
- [Test 316: monitor_resource_log_query](#test-316)
- [Test 317: monitor_table_list](#test-317)
- [Test 318: monitor_table_list](#test-318)
- [Test 319: monitor_table_type_list](#test-319)
- [Test 320: monitor_table_type_list](#test-320)
- [Test 321: monitor_webtests_create](#test-321)
- [Test 322: monitor_webtests_get](#test-322)
- [Test 323: monitor_webtests_list](#test-323)
- [Test 324: monitor_webtests_list](#test-324)
- [Test 325: monitor_webtests_update](#test-325)
- [Test 326: monitor_workspace_list](#test-326)
- [Test 327: monitor_workspace_list](#test-327)
- [Test 328: monitor_workspace_list](#test-328)
- [Test 329: monitor_workspace_log_query](#test-329)
- [Test 330: datadog_monitoredresources_list](#test-330)
- [Test 331: datadog_monitoredresources_list](#test-331)
- [Test 332: extension_azqr](#test-332)
- [Test 333: extension_azqr](#test-333)
- [Test 334: extension_azqr](#test-334)
- [Test 335: quota_region_availability_list](#test-335)
- [Test 336: quota_usage_check](#test-336)
- [Test 337: role_assignment_list](#test-337)
- [Test 338: role_assignment_list](#test-338)
- [Test 339: redis_create](#test-339)
- [Test 340: redis_create](#test-340)
- [Test 341: redis_create](#test-341)
- [Test 342: redis_create](#test-342)
- [Test 343: redis_list](#test-343)
- [Test 344: redis_list](#test-344)
- [Test 345: redis_list](#test-345)
- [Test 346: redis_list](#test-346)
- [Test 347: redis_list](#test-347)
- [Test 348: group_list](#test-348)
- [Test 349: group_list](#test-349)
- [Test 350: group_list](#test-350)
- [Test 351: resourcehealth_availability-status_get](#test-351)
- [Test 352: resourcehealth_availability-status_get](#test-352)
- [Test 353: resourcehealth_availability-status_get](#test-353)
- [Test 354: resourcehealth_availability-status_list](#test-354)
- [Test 355: resourcehealth_availability-status_list](#test-355)
- [Test 356: resourcehealth_availability-status_list](#test-356)
- [Test 357: resourcehealth_health-events_list](#test-357)
- [Test 358: resourcehealth_health-events_list](#test-358)
- [Test 359: resourcehealth_health-events_list](#test-359)
- [Test 360: resourcehealth_health-events_list](#test-360)
- [Test 361: resourcehealth_health-events_list](#test-361)
- [Test 362: servicebus_queue_details](#test-362)
- [Test 363: servicebus_topic_details](#test-363)
- [Test 364: servicebus_topic_subscription_details](#test-364)
- [Test 365: signalr_runtime_get](#test-365)
- [Test 366: signalr_runtime_get](#test-366)
- [Test 367: signalr_runtime_get](#test-367)
- [Test 368: signalr_runtime_get](#test-368)
- [Test 369: signalr_runtime_get](#test-369)
- [Test 370: signalr_runtime_get](#test-370)
- [Test 371: sql_db_create](#test-371)
- [Test 372: sql_db_create](#test-372)
- [Test 373: sql_db_create](#test-373)
- [Test 374: sql_db_delete](#test-374)
- [Test 375: sql_db_delete](#test-375)
- [Test 376: sql_db_delete](#test-376)
- [Test 377: sql_db_list](#test-377)
- [Test 378: sql_db_list](#test-378)
- [Test 379: sql_db_rename](#test-379)
- [Test 380: sql_db_rename](#test-380)
- [Test 381: sql_db_show](#test-381)
- [Test 382: sql_db_show](#test-382)
- [Test 383: sql_db_update](#test-383)
- [Test 384: sql_db_update](#test-384)
- [Test 385: sql_elastic-pool_list](#test-385)
- [Test 386: sql_elastic-pool_list](#test-386)
- [Test 387: sql_elastic-pool_list](#test-387)
- [Test 388: sql_server_create](#test-388)
- [Test 389: sql_server_create](#test-389)
- [Test 390: sql_server_create](#test-390)
- [Test 391: sql_server_delete](#test-391)
- [Test 392: sql_server_delete](#test-392)
- [Test 393: sql_server_delete](#test-393)
- [Test 394: sql_server_entra-admin_list](#test-394)
- [Test 395: sql_server_entra-admin_list](#test-395)
- [Test 396: sql_server_entra-admin_list](#test-396)
- [Test 397: sql_server_firewall-rule_create](#test-397)
- [Test 398: sql_server_firewall-rule_create](#test-398)
- [Test 399: sql_server_firewall-rule_create](#test-399)
- [Test 400: sql_server_firewall-rule_delete](#test-400)
- [Test 401: sql_server_firewall-rule_delete](#test-401)
- [Test 402: sql_server_firewall-rule_delete](#test-402)
- [Test 403: sql_server_firewall-rule_list](#test-403)
- [Test 404: sql_server_firewall-rule_list](#test-404)
- [Test 405: sql_server_firewall-rule_list](#test-405)
- [Test 406: sql_server_list](#test-406)
- [Test 407: sql_server_list](#test-407)
- [Test 408: sql_server_show](#test-408)
- [Test 409: sql_server_show](#test-409)
- [Test 410: sql_server_show](#test-410)
- [Test 411: storage_account_create](#test-411)
- [Test 412: storage_account_create](#test-412)
- [Test 413: storage_account_create](#test-413)
- [Test 414: storage_account_get](#test-414)
- [Test 415: storage_account_get](#test-415)
- [Test 416: storage_account_get](#test-416)
- [Test 417: storage_account_get](#test-417)
- [Test 418: storage_account_get](#test-418)
- [Test 419: storage_blob_container_create](#test-419)
- [Test 420: storage_blob_container_create](#test-420)
- [Test 421: storage_blob_container_create](#test-421)
- [Test 422: storage_blob_container_get](#test-422)
- [Test 423: storage_blob_container_get](#test-423)
- [Test 424: storage_blob_container_get](#test-424)
- [Test 425: storage_blob_get](#test-425)
- [Test 426: storage_blob_get](#test-426)
- [Test 427: storage_blob_get](#test-427)
- [Test 428: storage_blob_get](#test-428)
- [Test 429: storage_blob_upload](#test-429)
- [Test 430: storage_table_list](#test-430)
- [Test 431: storage_table_list](#test-431)
- [Test 432: storagesync_service_create](#test-432)
- [Test 433: storagesync_service_delete](#test-433)
- [Test 434: storagesync_service_get](#test-434)
- [Test 435: storagesync_service_get](#test-435)
- [Test 436: storagesync_service_update](#test-436)
- [Test 437: storagesync_registeredserver_get](#test-437)
- [Test 438: storagesync_registeredserver_get](#test-438)
- [Test 439: storagesync_registeredserver_unregister](#test-439)
- [Test 440: storagesync_registeredserver_update](#test-440)
- [Test 441: storagesync_syncgroup_create](#test-441)
- [Test 442: storagesync_syncgroup_delete](#test-442)
- [Test 443: storagesync_syncgroup_get](#test-443)
- [Test 444: storagesync_cloudendpoint_changedetection](#test-444)
- [Test 445: storagesync_cloudendpoint_create](#test-445)
- [Test 446: storagesync_cloudendpoint_delete](#test-446)
- [Test 447: storagesync_cloudendpoint_get](#test-447)
- [Test 448: storagesync_cloudendpoint_get](#test-448)
- [Test 449: storagesync_serverendpoint_create](#test-449)
- [Test 450: storagesync_serverendpoint_delete](#test-450)
- [Test 451: storagesync_serverendpoint_get](#test-451)
- [Test 452: storagesync_serverendpoint_get](#test-452)
- [Test 453: storagesync_serverendpoint_update](#test-453)
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
- [Test 475: foundry_agents_connect](#test-475)
- [Test 476: foundry_agents_create](#test-476)
- [Test 477: foundry_agents_evaluate](#test-477)
- [Test 478: foundry_agents_get-sdk-sample](#test-478)
- [Test 479: foundry_agents_list](#test-479)
- [Test 480: foundry_agents_list](#test-480)
- [Test 481: foundry_agents_query-and-evaluate](#test-481)
- [Test 482: foundry_knowledge_index_list](#test-482)
- [Test 483: foundry_knowledge_index_list](#test-483)
- [Test 484: foundry_knowledge_index_schema](#test-484)
- [Test 485: foundry_knowledge_index_schema](#test-485)
- [Test 486: foundry_models_deploy](#test-486)
- [Test 487: foundry_models_deployments_list](#test-487)
- [Test 488: foundry_models_deployments_list](#test-488)
- [Test 489: foundry_models_list](#test-489)
- [Test 490: foundry_models_list](#test-490)
- [Test 491: foundry_openai_chat-completions-create](#test-491)
- [Test 492: foundry_openai_create-completion](#test-492)
- [Test 493: foundry_openai_embeddings-create](#test-493)
- [Test 494: foundry_openai_embeddings-create](#test-494)
- [Test 495: foundry_openai_models-list](#test-495)
- [Test 496: foundry_openai_models-list](#test-496)
- [Test 497: foundry_resource_get](#test-497)
- [Test 498: foundry_resource_get](#test-498)
- [Test 499: foundry_resource_get](#test-499)
- [Test 500: foundry_threads_create](#test-500)
- [Test 501: foundry_threads_get-messages](#test-501)
- [Test 502: foundry_threads_list](#test-502)

---

## Test 1

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.785967 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.700665 | `search_knowledge_source_get` | ❌ |
| 3 | 0.692653 | `search_service_list` | ❌ |
| 4 | 0.635863 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.586575 | `search_index_get` | ❌ |

---

## Test 2

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.748213 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.668325 | `search_knowledge_source_get` | ❌ |
| 3 | 0.628582 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.623653 | `search_service_list` | ❌ |
| 5 | 0.566618 | `search_index_get` | ❌ |

---

## Test 3

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702942 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.605701 | `search_knowledge_source_get` | ❌ |
| 3 | 0.583234 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.512878 | `search_service_list` | ❌ |
| 5 | 0.471301 | `foundry_knowledge_index_list` | ❌ |

---

## Test 4

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688051 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.599032 | `search_knowledge_source_get` | ❌ |
| 3 | 0.578499 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.456729 | `search_service_list` | ❌ |
| 5 | 0.439996 | `foundry_knowledge_index_list` | ❌ |

---

## Test 5

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Get the details of knowledge base <agent-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769383 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.685523 | `search_knowledge_source_get` | ❌ |
| 3 | 0.636958 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.585949 | `search_index_get` | ❌ |
| 5 | 0.533235 | `search_service_list` | ❌ |

---

## Test 6

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595585 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.551922 | `search_knowledge_base_retrieve` | ❌ |
| 3 | 0.515284 | `search_knowledge_source_get` | ❌ |
| 4 | 0.366191 | `search_service_list` | ❌ |
| 5 | 0.365633 | `search_index_get` | ❌ |

---

## Test 7

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in Azure AI Search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.724846 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.650590 | `search_knowledge_base_get` | ❌ |
| 3 | 0.575330 | `search_index_query` | ❌ |
| 4 | 0.567222 | `search_knowledge_source_get` | ❌ |
| 5 | 0.480475 | `search_service_list` | ❌ |

---

## Test 8

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633766 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.589869 | `search_knowledge_base_get` | ❌ |
| 3 | 0.501903 | `search_knowledge_source_get` | ❌ |
| 4 | 0.422671 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.399569 | `search_index_query` | ❌ |

---

## Test 9

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657866 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.557206 | `search_knowledge_base_get` | ❌ |
| 3 | 0.463397 | `search_knowledge_source_get` | ❌ |
| 4 | 0.436739 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.399079 | `foundry_agents_connect` | ❌ |

---

## Test 10

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633766 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.589869 | `search_knowledge_base_get` | ❌ |
| 3 | 0.501903 | `search_knowledge_source_get` | ❌ |
| 4 | 0.422671 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.399569 | `search_index_query` | ❌ |

---

## Test 11

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Query knowledge base <agent-name> in search service <service-name> about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.598868 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.547862 | `search_knowledge_base_get` | ❌ |
| 3 | 0.467907 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.464709 | `search_knowledge_source_get` | ❌ |
| 5 | 0.414323 | `foundry_agents_connect` | ❌ |

---

## Test 12

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Search knowledge base <agent-name> in Azure AI Search service <service-name> for <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649767 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.631435 | `search_knowledge_base_get` | ❌ |
| 3 | 0.581438 | `search_index_query` | ❌ |
| 4 | 0.571021 | `search_knowledge_source_get` | ❌ |
| 5 | 0.544527 | `search_service_list` | ❌ |

---

## Test 13

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** What does knowledge base <agent-name> in search service <service-name> know about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579716 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.560688 | `search_knowledge_base_get` | ❌ |
| 3 | 0.477711 | `search_knowledge_source_get` | ❌ |
| 4 | 0.402582 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.365204 | `foundry_agents_connect` | ❌ |

---

## Test 14

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Find information about <query> using knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582662 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.528610 | `search_knowledge_base_get` | ❌ |
| 3 | 0.449154 | `search_knowledge_source_get` | ❌ |
| 4 | 0.447780 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.401030 | `foundry_agents_connect` | ❌ |

---

## Test 15

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.760200 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.690791 | `search_service_list` | ❌ |
| 3 | 0.665922 | `search_knowledge_base_get` | ❌ |
| 4 | 0.573012 | `search_index_get` | ❌ |
| 5 | 0.560779 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 16

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737657 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.659160 | `search_service_list` | ❌ |
| 3 | 0.652969 | `search_knowledge_base_get` | ❌ |
| 4 | 0.578836 | `search_index_get` | ❌ |
| 5 | 0.560568 | `search_index_query` | ❌ |

---

## Test 17

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657642 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.558516 | `search_knowledge_base_get` | ❌ |
| 3 | 0.510379 | `search_service_list` | ❌ |
| 4 | 0.470560 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.435554 | `foundry_knowledge_index_list` | ❌ |

---

## Test 18

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.652696 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.563270 | `search_knowledge_base_get` | ❌ |
| 3 | 0.485935 | `search_service_list` | ❌ |
| 4 | 0.477636 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.430518 | `search_index_get` | ❌ |

---

## Test 19

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Get the details of knowledge source <source-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.825478 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.693438 | `search_knowledge_base_get` | ❌ |
| 3 | 0.595643 | `search_index_get` | ❌ |
| 4 | 0.540550 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.531033 | `search_service_list` | ❌ |

---

## Test 20

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge source <source-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630629 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.523643 | `search_knowledge_base_get` | ❌ |
| 3 | 0.459923 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.371465 | `search_index_get` | ❌ |
| 5 | 0.370622 | `search_service_list` | ❌ |

---

## Test 21

**Expected Tool:** `search_index_get`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680769 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.527833 | `search_knowledge_base_get` | ❌ |
| 3 | 0.521387 | `search_knowledge_source_get` | ❌ |
| 4 | 0.509549 | `foundry_knowledge_index_schema` | ❌ |
| 5 | 0.490089 | `search_service_list` | ❌ |

---

## Test 22

**Expected Tool:** `search_index_get`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640256 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.619988 | `search_service_list` | ❌ |
| 3 | 0.538456 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.511485 | `search_knowledge_base_get` | ❌ |
| 5 | 0.495961 | `search_knowledge_source_get` | ❌ |

---

## Test 23

**Expected Tool:** `search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620759 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.562508 | `search_service_list` | ❌ |
| 3 | 0.543811 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.500365 | `search_knowledge_base_get` | ❌ |
| 5 | 0.489910 | `search_knowledge_source_get` | ❌ |

---

## Test 24

**Expected Tool:** `search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522826 | `search_index_get` | ❌ |
| 2 | 0.515889 | `search_index_query` | ✅ **EXPECTED** |
| 3 | 0.498515 | `search_service_list` | ❌ |
| 4 | 0.447977 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.437715 | `postgres_database_query` | ❌ |

---

## Test 25

**Expected Tool:** `search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.791861 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.553012 | `kusto_cluster_list` | ❌ |
| 3 | 0.509460 | `subscription_list` | ❌ |
| 4 | 0.505971 | `search_index_get` | ❌ |
| 5 | 0.504693 | `marketplace_product_list` | ❌ |

---

## Test 26

**Expected Tool:** `search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684866 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.484092 | `marketplace_product_list` | ❌ |
| 3 | 0.479898 | `search_index_get` | ❌ |
| 4 | 0.462337 | `search_knowledge_base_get` | ❌ |
| 5 | 0.461786 | `kusto_cluster_list` | ❌ |

---

## Test 27

**Expected Tool:** `search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551249 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.436230 | `search_index_get` | ❌ |
| 3 | 0.415277 | `search_knowledge_base_get` | ❌ |
| 4 | 0.410297 | `search_knowledge_source_get` | ❌ |
| 5 | 0.404800 | `search_index_query` | ❌ |

---

## Test 28

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert this audio file to text using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682099 | `speech_tts_synthesize` | ❌ |
| 2 | 0.666038 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 3 | 0.380659 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.351127 | `deploy_plan_get` | ❌ |
| 5 | 0.338137 | `extension_cli_generate` | ❌ |

---

## Test 29

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Recognize speech from my audio file with language detection  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.511324 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.344367 | `speech_tts_synthesize` | ❌ |
| 3 | 0.201341 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.181312 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.177025 | `storagesync_cloudendpoint_triggerchangedetection` | ❌ |

---

## Test 30

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe speech from audio file <file_path> with profanity filtering  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.486489 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.335124 | `speech_tts_synthesize` | ❌ |
| 3 | 0.167783 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.164530 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.156850 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 31

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text from audio file <file_path> using endpoint <endpoint>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611992 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.573240 | `speech_tts_synthesize` | ❌ |
| 3 | 0.319617 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.300426 | `storagesync_cloudendpoint_get` | ❌ |
| 5 | 0.297430 | `storagesync_cloudendpoint_triggerchangedetection` | ❌ |

---

## Test 32

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe the audio file <file_path> in Spanish language  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.410533 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.353797 | `speech_tts_synthesize` | ❌ |
| 3 | 0.159785 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.158411 | `foundry_models_deploy` | ❌ |
| 5 | 0.151632 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 33

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text with detailed output format from audio file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546259 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.480211 | `speech_tts_synthesize` | ❌ |
| 3 | 0.217699 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.200334 | `foundry_resource_get` | ❌ |
| 5 | 0.183927 | `extension_azqr` | ❌ |

---

## Test 34

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Recognize speech from <file_path> with phrase hints for better accuracy  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.539963 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.367375 | `speech_tts_synthesize` | ❌ |
| 3 | 0.240278 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.210474 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.205136 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 35

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe audio using multiple phrase hints: "Azure", "cognitive services", "machine learning"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549202 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.468249 | `speech_tts_synthesize` | ❌ |
| 3 | 0.342577 | `extension_cli_generate` | ❌ |
| 4 | 0.337392 | `cloudarchitect_design` | ❌ |
| 5 | 0.333112 | `get_bestpractices_get` | ❌ |

---

## Test 36

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text with comma-separated phrase hints: "Azure, cognitive services, API"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532536 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.515568 | `speech_tts_synthesize` | ❌ |
| 3 | 0.340575 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.335215 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.334238 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 37

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe audio with raw profanity output from file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.453396 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.322709 | `speech_tts_synthesize` | ❌ |
| 3 | 0.173875 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.173205 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.168275 | `foundry_openai_create-completion` | ❌ |

---

## Test 38

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Convert text to speech and save to output.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521817 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.422457 | `speech_stt_recognize` | ❌ |
| 3 | 0.207687 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.194603 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.181208 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 39

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Synthesize speech from "Hello, welcome to Azure" and save to welcome.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.517016 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.486019 | `speech_stt_recognize` | ❌ |
| 3 | 0.329765 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.323728 | `extension_cli_generate` | ❌ |
| 5 | 0.317502 | `azureterraformbestpractices_get` | ❌ |

---

## Test 40

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Generate speech audio from text "Hello world" using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592222 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.534001 | `speech_stt_recognize` | ❌ |
| 3 | 0.341014 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.326670 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.318460 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 41

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Convert text to speech with Spanish language and save to spanish-audio.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.501089 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.452648 | `speech_stt_recognize` | ❌ |
| 3 | 0.214844 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.193771 | `foundry_models_deploy` | ❌ |
| 5 | 0.192473 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 42

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Synthesize speech with voice en-US-JennyNeural from text "Azure AI Services"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604898 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.496715 | `speech_stt_recognize` | ❌ |
| 3 | 0.407053 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.369146 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.347857 | `search_service_list` | ❌ |

---

## Test 43

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Create MP3 audio file from text "Welcome to Azure" with high quality format  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.561352 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.510909 | `speech_stt_recognize` | ❌ |
| 3 | 0.350715 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.347597 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.345073 | `deploy_iac_rules_get` | ❌ |

---

## Test 44

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Generate speech with custom voice model using endpoint ID <endpoint-id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527286 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.455734 | `speech_stt_recognize` | ❌ |
| 3 | 0.349837 | `storagesync_serverendpoint_update` | ❌ |
| 4 | 0.348553 | `foundry_models_deploy` | ❌ |
| 5 | 0.339510 | `foundry_resource_get` | ❌ |

---

## Test 45

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Convert text to OGG/Opus format audio file  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.432821 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.410086 | `speech_stt_recognize` | ❌ |
| 3 | 0.251319 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.196153 | `extension_cli_generate` | ❌ |
| 5 | 0.183982 | `foundry_openai_create-completion` | ❌ |

---

## Test 46

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Synthesize long text content to audio file with streaming  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.428067 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.369045 | `speech_stt_recognize` | ❌ |
| 3 | 0.235202 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.220111 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.217152 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 47

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Create audio file from text in French language with appropriate voice  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.444403 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.385267 | `speech_stt_recognize` | ❌ |
| 3 | 0.236455 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.232269 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.219091 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 48

**Expected Tool:** `appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786360 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.530613 | `appconfig_kv_get` | ❌ |
| 3 | 0.491406 | `postgres_server_list` | ❌ |
| 4 | 0.481223 | `kusto_cluster_list` | ❌ |
| 5 | 0.479988 | `subscription_list` | ❌ |

---

## Test 49

**Expected Tool:** `appconfig_account_list`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634978 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.464865 | `appconfig_kv_get` | ❌ |
| 3 | 0.398495 | `subscription_list` | ❌ |
| 4 | 0.391291 | `redis_list` | ❌ |
| 5 | 0.372482 | `postgres_server_list` | ❌ |

---

## Test 50

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

## Test 51

**Expected Tool:** `appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618276 | `appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.464358 | `appconfig_kv_get` | ❌ |
| 3 | 0.424344 | `appconfig_kv_set` | ❌ |
| 4 | 0.422912 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.392016 | `appconfig_account_list` | ❌ |

---

## Test 52

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.632687 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.557810 | `appconfig_account_list` | ❌ |
| 3 | 0.530884 | `appconfig_kv_set` | ❌ |
| 4 | 0.464635 | `appconfig_kv_delete` | ❌ |
| 5 | 0.439270 | `appconfig_kv_lock_set` | ❌ |

---

## Test 53

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612555 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.522426 | `appconfig_account_list` | ❌ |
| 3 | 0.512945 | `appconfig_kv_set` | ❌ |
| 4 | 0.468503 | `appconfig_kv_delete` | ❌ |
| 5 | 0.458141 | `appconfig_kv_lock_set` | ❌ |

---

## Test 54

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** List all key-value settings with key name starting with 'prod-' in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512883 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.449905 | `appconfig_account_list` | ❌ |
| 3 | 0.398684 | `appconfig_kv_set` | ❌ |
| 4 | 0.380614 | `appconfig_kv_delete` | ❌ |
| 5 | 0.346178 | `appconfig_kv_lock_set` | ❌ |

---

## Test 55

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552300 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.448912 | `appconfig_kv_set` | ❌ |
| 3 | 0.441713 | `appconfig_kv_delete` | ❌ |
| 4 | 0.437432 | `appconfig_account_list` | ❌ |
| 5 | 0.416442 | `appconfig_kv_lock_set` | ❌ |

---

## Test 56

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591169 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.487174 | `appconfig_kv_get` | ❌ |
| 3 | 0.445551 | `appconfig_kv_set` | ❌ |
| 4 | 0.431516 | `appconfig_kv_delete` | ❌ |
| 5 | 0.373656 | `appconfig_account_list` | ❌ |

---

## Test 57

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555896 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.506565 | `appconfig_kv_get` | ❌ |
| 3 | 0.476729 | `appconfig_kv_delete` | ❌ |
| 4 | 0.426843 | `appconfig_kv_set` | ❌ |
| 5 | 0.410215 | `appconfig_account_list` | ❌ |

---

## Test 58

**Expected Tool:** `appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609635 | `appconfig_kv_set` | ✅ **EXPECTED** |
| 2 | 0.536971 | `appconfig_kv_lock_set` | ❌ |
| 3 | 0.512707 | `appconfig_kv_get` | ❌ |
| 4 | 0.505571 | `appconfig_kv_delete` | ❌ |
| 5 | 0.377919 | `appconfig_account_list` | ❌ |

---

## Test 59

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595632 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.336090 | `deploy_app_logs_get` | ❌ |
| 3 | 0.300703 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.273083 | `cloudarchitect_design` | ❌ |
| 5 | 0.254473 | `monitor_resource_log_query` | ❌ |

---

## Test 60

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** Use app lens to check why my app is slow?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502361 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.316297 | `deploy_app_logs_get` | ❌ |
| 3 | 0.255885 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.249583 | `monitor_resource_log_query` | ❌ |
| 5 | 0.225972 | `quota_usage_check` | ❌ |

---

## Test 61

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492820 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.257080 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.242574 | `cloudarchitect_design` | ❌ |
| 4 | 0.225608 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.211565 | `deploy_app_logs_get` | ❌ |

---

## Test 62

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection <connection_string> to my app service <app_name> for database <database_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.717885 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.401350 | `sql_db_rename` | ❌ |
| 3 | 0.399744 | `sql_db_create` | ❌ |
| 4 | 0.378277 | `storagesync_service_create` | ❌ |
| 5 | 0.362875 | `sql_db_show` | ❌ |

---

## Test 63

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure SQL Server database <database_name> for app service <app_name> with connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688325 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.498165 | `sql_db_rename` | ❌ |
| 3 | 0.496970 | `sql_db_create` | ❌ |
| 4 | 0.469482 | `sql_db_show` | ❌ |
| 5 | 0.453070 | `sql_db_list` | ❌ |

---

## Test 64

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add MySQL database <database_name> to app service <app_name> using connection <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.675509 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.465070 | `sql_db_create` | ❌ |
| 3 | 0.452630 | `sql_db_rename` | ❌ |
| 4 | 0.433163 | `mysql_server_list` | ❌ |
| 5 | 0.410315 | `sql_db_show` | ❌ |

---

## Test 65

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add PostgreSQL database <database_name> to app service <app_name> using connection <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627784 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.444274 | `sql_db_create` | ❌ |
| 3 | 0.404826 | `postgres_database_query` | ❌ |
| 4 | 0.401084 | `postgres_database_list` | ❌ |
| 5 | 0.400754 | `sql_db_rename` | ❌ |

---

## Test 66

**Expected Tool:** `appservice_database_add`  
**Prompt:** Connect CosmosDB database <database_name> using connection string <connection_string> to app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662835 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.446275 | `cosmos_database_list` | ❌ |
| 3 | 0.441797 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.427140 | `cosmos_database_container_list` | ❌ |
| 5 | 0.420319 | `sql_db_rename` | ❌ |

---

## Test 67

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection <connection_string> for database <database_name> on server <database_server> to app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.733775 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.454055 | `sql_db_create` | ❌ |
| 3 | 0.415274 | `sql_db_rename` | ❌ |
| 4 | 0.414144 | `sql_server_create` | ❌ |
| 5 | 0.410260 | `sql_db_list` | ❌ |

---

## Test 68

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection string for <database_name> to app service <app_name> using connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.746370 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.441616 | `sql_db_rename` | ❌ |
| 3 | 0.433982 | `sql_db_create` | ❌ |
| 4 | 0.391303 | `sql_db_list` | ❌ |
| 5 | 0.390203 | `sql_db_show` | ❌ |

---

## Test 69

**Expected Tool:** `appservice_database_add`  
**Prompt:** Connect database <database_name> to my app service <app_name> using connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680378 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.429322 | `sql_db_rename` | ❌ |
| 3 | 0.406205 | `sql_db_create` | ❌ |
| 4 | 0.396606 | `sql_db_show` | ❌ |
| 5 | 0.391484 | `sql_db_list` | ❌ |

---

## Test 70

**Expected Tool:** `appservice_database_add`  
**Prompt:** Set up database <database_name> for app service <app_name> with connection string <connection_string> under resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640708 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.456283 | `sql_db_create` | ❌ |
| 3 | 0.419340 | `storagesync_service_create` | ❌ |
| 4 | 0.402791 | `sql_db_rename` | ❌ |
| 5 | 0.401976 | `sql_db_show` | ❌ |

---

## Test 71

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure database <database_name> for app service <app_name> with the connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688347 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.449170 | `sql_db_rename` | ❌ |
| 3 | 0.448098 | `sql_db_create` | ❌ |
| 4 | 0.414329 | `sql_db_show` | ❌ |
| 5 | 0.411782 | `sql_db_list` | ❌ |

---

## Test 72

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572473 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.445157 | `get_bestpractices_get` | ❌ |
| 3 | 0.403349 | `get_bestpractices_ai_app` | ❌ |
| 4 | 0.390470 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.383948 | `applens_resource_diagnose` | ❌ |

---

## Test 73

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** Show me code optimization recommendations for all Application Insights resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696531 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.472323 | `get_bestpractices_ai_app` | ❌ |
| 3 | 0.468384 | `get_bestpractices_get` | ❌ |
| 4 | 0.452231 | `applens_resource_diagnose` | ❌ |
| 5 | 0.435222 | `azureterraformbestpractices_get` | ❌ |

---

## Test 74

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626722 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.488002 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.479335 | `mysql_server_list` | ❌ |
| 4 | 0.477396 | `applens_resource_diagnose` | ❌ |
| 5 | 0.468847 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 75

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509502 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.419670 | `applens_resource_diagnose` | ❌ |
| 3 | 0.393920 | `get_bestpractices_ai_app` | ❌ |
| 4 | 0.383767 | `get_bestpractices_get` | ❌ |
| 5 | 0.366758 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 76

**Expected Tool:** `extension_cli_generate`  
**Prompt:** Create a Storage account with name <storage_account_name> using Azure CLI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593241 | `storage_account_create` | ❌ |
| 2 | 0.564940 | `storage_blob_container_create` | ❌ |
| 3 | 0.493684 | `storage_account_get` | ❌ |
| 4 | 0.474398 | `storage_blob_container_get` | ❌ |
| 5 | 0.470575 | `redis_create` | ❌ |

---

## Test 77

**Expected Tool:** `extension_cli_generate`  
**Prompt:** List all virtual machines in my subscription using Azure CLI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592167 | `search_service_list` | ❌ |
| 2 | 0.575274 | `kusto_cluster_list` | ❌ |
| 3 | 0.549965 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.544412 | `monitor_workspace_list` | ❌ |
| 5 | 0.536252 | `subscription_list` | ❌ |

---

## Test 78

**Expected Tool:** `extension_cli_generate`  
**Prompt:** Show me the details of the storage account <account_name> with Azure CLI commands  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710307 | `storage_account_get` | ❌ |
| 2 | 0.602173 | `storage_blob_container_get` | ❌ |
| 3 | 0.565127 | `storagesync_service_get` | ❌ |
| 4 | 0.543268 | `storage_blob_get` | ❌ |
| 5 | 0.524045 | `storage_table_list` | ❌ |

---

## Test 79

**Expected Tool:** `extension_cli_install`  
**Prompt:** <Ask the MCP host to uninstall az cli on your machine and run test prompts for extension_cli_generate>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.479590 | `extension_cli_install` | ✅ **EXPECTED** |
| 2 | 0.473250 | `extension_cli_generate` | ❌ |
| 3 | 0.389292 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.382389 | `deploy_plan_get` | ❌ |
| 5 | 0.366012 | `get_bestpractices_get` | ❌ |

---

## Test 80

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

## Test 81

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

## Test 82

**Expected Tool:** `acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743568 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.711580 | `acr_registry_repository_list` | ❌ |
| 3 | 0.585675 | `kusto_cluster_list` | ❌ |
| 4 | 0.540310 | `search_service_list` | ❌ |
| 5 | 0.531536 | `storage_table_list` | ❌ |

---

## Test 83

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586014 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563636 | `acr_registry_repository_list` | ❌ |
| 3 | 0.460544 | `storage_blob_container_get` | ❌ |
| 4 | 0.415520 | `cosmos_database_container_list` | ❌ |
| 5 | 0.402247 | `redis_list` | ❌ |

---

## Test 84

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

## Test 85

**Expected Tool:** `acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654318 | `acr_registry_repository_list` | ❌ |
| 2 | 0.633938 | `acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.475991 | `mysql_server_list` | ❌ |
| 4 | 0.454929 | `group_list` | ❌ |
| 5 | 0.453926 | `datadog_monitoredresources_list` | ❌ |

---

## Test 86

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639391 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.637972 | `acr_registry_repository_list` | ❌ |
| 3 | 0.468015 | `mysql_server_list` | ❌ |
| 4 | 0.449545 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.445741 | `group_list` | ❌ |

---

## Test 87

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626482 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.617504 | `acr_registry_list` | ❌ |
| 3 | 0.544172 | `kusto_cluster_list` | ❌ |
| 4 | 0.508483 | `storage_blob_container_get` | ❌ |
| 5 | 0.495610 | `postgres_server_list` | ❌ |

---

## Test 88

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546334 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.469295 | `acr_registry_list` | ❌ |
| 3 | 0.451083 | `storage_blob_container_get` | ❌ |
| 4 | 0.407934 | `cosmos_database_container_list` | ❌ |
| 5 | 0.373464 | `storage_blob_get` | ❌ |

---

## Test 89

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674234 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.541834 | `acr_registry_list` | ❌ |
| 3 | 0.437485 | `storage_blob_container_get` | ❌ |
| 4 | 0.433861 | `cosmos_database_container_list` | ❌ |
| 5 | 0.383187 | `kusto_database_list` | ❌ |

---

## Test 90

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600780 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.501842 | `acr_registry_list` | ❌ |
| 3 | 0.430880 | `storage_blob_container_get` | ❌ |
| 4 | 0.418582 | `cosmos_database_container_list` | ❌ |
| 5 | 0.378151 | `redis_list` | ❌ |

---

## Test 91

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498119 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.229062 | `communication_sms_send` | ❌ |
| 3 | 0.188973 | `eventgrid_events_publish` | ❌ |
| 4 | 0.149092 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.145951 | `servicebus_topic_details` | ❌ |

---

## Test 92

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email from my communication service to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498745 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.314418 | `communication_sms_send` | ❌ |
| 3 | 0.239044 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.215375 | `speech_tts_synthesize` | ❌ |
| 5 | 0.211154 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 93

**Expected Tool:** `communication_email_send`  
**Prompt:** Send HTML-formatted email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.519864 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.207648 | `communication_sms_send` | ❌ |
| 3 | 0.152409 | `eventgrid_events_publish` | ❌ |
| 4 | 0.152013 | `servicebus_topic_details` | ❌ |
| 5 | 0.147540 | `foundry_agents_connect` | ❌ |

---

## Test 94

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with CC to <email-address-1> and <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533653 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.219564 | `communication_sms_send` | ❌ |
| 3 | 0.118017 | `storagesync_cloudendpoint_create` | ❌ |
| 4 | 0.106189 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.106026 | `foundry_agents_query-and-evaluate` | ❌ |

---

## Test 95

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email to multiple recipients: <email-address-1>, <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.541232 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.244508 | `communication_sms_send` | ❌ |
| 3 | 0.138567 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.114324 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.111404 | `storagesync_cloudendpoint_create` | ❌ |

---

## Test 96

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with reply-to address set to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512233 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.200170 | `communication_sms_send` | ❌ |
| 3 | 0.196090 | `storagesync_serverendpoint_update` | ❌ |
| 4 | 0.166386 | `storagesync_registeredserver_update` | ❌ |
| 5 | 0.164115 | `mysql_server_param_set` | ❌ |

---

## Test 97

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with custom sender name <sender-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.472887 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.255168 | `communication_sms_send` | ❌ |
| 3 | 0.175566 | `storagesync_serverendpoint_get` | ❌ |
| 4 | 0.174157 | `storagesync_serverendpoint_update` | ❌ |
| 5 | 0.168394 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 98

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email with BCC recipients  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.528971 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.241077 | `communication_sms_send` | ❌ |
| 3 | 0.137538 | `confidentialledger_entries_append` | ❌ |
| 4 | 0.126336 | `storagesync_cloudendpoint_triggerchangedetection` | ❌ |
| 5 | 0.122349 | `storagesync_cloudendpoint_create` | ❌ |

---

## Test 99

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send an SMS message to <phone-number> saying "Hello"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533798 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.251333 | `communication_email_send` | ❌ |
| 3 | 0.221055 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.166041 | `speech_tts_synthesize` | ❌ |
| 5 | 0.154795 | `foundry_threads_create` | ❌ |

---

## Test 100

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS to <phone-number-2> from <phone-number-1> with message "Test message"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545965 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.294699 | `communication_email_send` | ❌ |
| 3 | 0.204588 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.200481 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.139240 | `speech_tts_synthesize` | ❌ |

---

## Test 101

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS to multiple recipients: <phone-number-1>, <phone-number-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545726 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.422414 | `communication_email_send` | ❌ |
| 3 | 0.189219 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.142029 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.106261 | `foundry_threads_get-messages` | ❌ |

---

## Test 102

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS with delivery reporting enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.554764 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.268972 | `communication_email_send` | ❌ |
| 3 | 0.192340 | `extension_azqr` | ❌ |
| 4 | 0.186765 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.170726 | `foundry_agents_query-and-evaluate` | ❌ |

---

## Test 103

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS message with custom tracking tag "campaign1"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.538762 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.270002 | `communication_email_send` | ❌ |
| 3 | 0.192486 | `foundry_agents_create` | ❌ |
| 4 | 0.188153 | `loadtesting_testrun_create` | ❌ |
| 5 | 0.185542 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 104

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send broadcast SMS to <phone-number-1> and <phone-number-2> saying "Urgent notification"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.474757 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.286460 | `communication_email_send` | ❌ |
| 3 | 0.164289 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.150225 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.132512 | `storagesync_cloudendpoint_triggerchangedetection` | ❌ |

---

## Test 105

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS from my communication service to <phone-number-1>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564057 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.302666 | `communication_email_send` | ❌ |
| 3 | 0.240744 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.183651 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.173726 | `foundry_openai_create-completion` | ❌ |

---

## Test 106

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send an SMS with delivery receipt tracking  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.598091 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.314411 | `communication_email_send` | ❌ |
| 3 | 0.206916 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.201433 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.187824 | `confidentialledger_entries_append` | ❌ |

---

## Test 107

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Append an entry to my ledger <ledger_name> with data {"key": "value"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510650 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.294802 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.292014 | `appconfig_kv_set` | ❌ |
| 4 | 0.259271 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.249908 | `keyvault_certificate_import` | ❌ |

---

## Test 108

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Write a tamper-proof entry to ledger <ledger_name> containing {"transaction": "data"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602247 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.356925 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.212221 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.195471 | `keyvault_secret_create` | ❌ |
| 5 | 0.184077 | `keyvault_certificate_import` | ❌ |

---

## Test 109

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Append {"hello": "from mcp"} to my confidential ledger <ledger_name> in collection <collection_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546660 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.452137 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.225275 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.215932 | `appconfig_kv_set` | ❌ |
| 5 | 0.203262 | `keyvault_certificate_import` | ❌ |

---

## Test 110

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Create an immutable ledger entry in <ledger_name> with content {"audit": "log"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496023 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.338865 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.218473 | `monitor_activitylog_list` | ❌ |
| 4 | 0.215229 | `storage_blob_container_create` | ❌ |
| 5 | 0.204925 | `monitor_resource_log_query` | ❌ |

---

## Test 111

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Write an entry to confidential ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622138 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.524374 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.252847 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.240252 | `keyvault_secret_create` | ❌ |
| 5 | 0.186890 | `appconfig_kv_set` | ❌ |

---

## Test 112

**Expected Tool:** `confidentialledger_entries_get`  
**Prompt:** Get entry from Confidential Ledger for transaction <transaction_id> on ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.707098 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.551953 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.245541 | `keyvault_secret_get` | ❌ |
| 4 | 0.229943 | `keyvault_key_get` | ❌ |
| 5 | 0.211839 | `loadtesting_testrun_get` | ❌ |

---

## Test 113

**Expected Tool:** `confidentialledger_entries_get`  
**Prompt:** Get transaction <transaction_id> from ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510186 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.416580 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.223959 | `loadtesting_testrun_get` | ❌ |
| 4 | 0.218412 | `monitor_resource_log_query` | ❌ |
| 5 | 0.217671 | `loadtesting_testrun_list` | ❌ |

---

## Test 114

**Expected Tool:** `cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818321 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.668473 | `cosmos_database_list` | ❌ |
| 3 | 0.635976 | `subscription_list` | ❌ |
| 4 | 0.615162 | `cosmos_database_container_list` | ❌ |
| 5 | 0.601437 | `kusto_cluster_list` | ❌ |

---

## Test 115

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665364 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605357 | `cosmos_database_list` | ❌ |
| 3 | 0.571550 | `cosmos_database_container_list` | ❌ |
| 4 | 0.549423 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.503830 | `storage_account_get` | ❌ |

---

## Test 116

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752361 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.606937 | `subscription_list` | ❌ |
| 3 | 0.605196 | `cosmos_database_list` | ❌ |
| 4 | 0.566321 | `cosmos_database_container_list` | ❌ |
| 5 | 0.564057 | `cosmos_database_container_item_query` | ❌ |

---

## Test 117

**Expected Tool:** `cosmos_database_container_item_query`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658697 | `cosmos_database_container_item_query` | ✅ **EXPECTED** |
| 2 | 0.605198 | `cosmos_database_container_list` | ❌ |
| 3 | 0.487612 | `storage_blob_container_get` | ❌ |
| 4 | 0.477874 | `cosmos_database_list` | ❌ |
| 5 | 0.447717 | `cosmos_account_list` | ❌ |

---

## Test 118

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852734 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.680631 | `cosmos_database_list` | ❌ |
| 3 | 0.680453 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.632499 | `storage_blob_container_get` | ❌ |
| 5 | 0.630210 | `cosmos_account_list` | ❌ |

---

## Test 119

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789324 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.648094 | `cosmos_database_container_item_query` | ❌ |
| 3 | 0.614155 | `cosmos_database_list` | ❌ |
| 4 | 0.591352 | `storage_blob_container_get` | ❌ |
| 5 | 0.561909 | `cosmos_account_list` | ❌ |

---

## Test 120

**Expected Tool:** `cosmos_database_list`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815683 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.668464 | `cosmos_account_list` | ❌ |
| 3 | 0.665229 | `cosmos_database_container_list` | ❌ |
| 4 | 0.606398 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.583361 | `kusto_database_list` | ❌ |

---

## Test 121

**Expected Tool:** `cosmos_database_list`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.749370 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.624681 | `cosmos_database_container_list` | ❌ |
| 3 | 0.614522 | `cosmos_account_list` | ❌ |
| 4 | 0.579877 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.538097 | `mysql_database_list` | ❌ |

---

## Test 122

**Expected Tool:** `kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590264 | `kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.463832 | `kusto_cluster_list` | ❌ |
| 3 | 0.429100 | `kusto_query` | ❌ |
| 4 | 0.425664 | `kusto_database_list` | ❌ |
| 5 | 0.422577 | `kusto_table_schema` | ❌ |

---

## Test 123

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793744 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.630469 | `kusto_database_list` | ❌ |
| 3 | 0.573395 | `kusto_cluster_get` | ❌ |
| 4 | 0.525025 | `aks_cluster_get` | ❌ |
| 5 | 0.509397 | `grafana_list` | ❌ |

---

## Test 124

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531307 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.465277 | `kusto_cluster_get` | ❌ |
| 3 | 0.432277 | `kusto_database_list` | ❌ |
| 4 | 0.369596 | `aks_cluster_get` | ❌ |
| 5 | 0.363119 | `kusto_table_schema` | ❌ |

---

## Test 125

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701484 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.571191 | `kusto_cluster_get` | ❌ |
| 3 | 0.548668 | `kusto_database_list` | ❌ |
| 4 | 0.498909 | `aks_cluster_get` | ❌ |
| 5 | 0.474201 | `redis_list` | ❌ |

---

## Test 126

**Expected Tool:** `kusto_database_list`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676920 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.560592 | `kusto_cluster_list` | ❌ |
| 3 | 0.556689 | `kusto_table_list` | ❌ |
| 4 | 0.553166 | `postgres_database_list` | ❌ |
| 5 | 0.549673 | `cosmos_database_list` | ❌ |

---

## Test 127

**Expected Tool:** `kusto_database_list`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623390 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.509952 | `kusto_cluster_list` | ❌ |
| 3 | 0.506969 | `kusto_table_list` | ❌ |
| 4 | 0.497144 | `cosmos_database_list` | ❌ |
| 5 | 0.491298 | `mysql_database_list` | ❌ |

---

## Test 128

**Expected Tool:** `kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.423821 | `kusto_query` | ✅ **EXPECTED** |
| 2 | 0.409558 | `postgres_database_query` | ❌ |
| 3 | 0.408178 | `kusto_table_schema` | ❌ |
| 4 | 0.407740 | `kusto_sample` | ❌ |
| 5 | 0.403989 | `kusto_cluster_list` | ❌ |

---

## Test 129

**Expected Tool:** `kusto_sample`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595554 | `kusto_sample` | ✅ **EXPECTED** |
| 2 | 0.510233 | `kusto_table_schema` | ❌ |
| 3 | 0.424217 | `kusto_table_list` | ❌ |
| 4 | 0.400924 | `kusto_cluster_list` | ❌ |
| 5 | 0.399525 | `kusto_cluster_get` | ❌ |

---

## Test 130

**Expected Tool:** `kusto_table_list`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.679645 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.585237 | `postgres_table_list` | ❌ |
| 3 | 0.581122 | `kusto_database_list` | ❌ |
| 4 | 0.556496 | `mysql_table_list` | ❌ |
| 5 | 0.549900 | `monitor_table_list` | ❌ |

---

## Test 131

**Expected Tool:** `kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619236 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.554330 | `kusto_table_schema` | ❌ |
| 3 | 0.527531 | `kusto_database_list` | ❌ |
| 4 | 0.524449 | `mysql_table_list` | ❌ |
| 5 | 0.523433 | `postgres_table_list` | ❌ |

---

## Test 132

**Expected Tool:** `kusto_table_schema`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.667052 | `kusto_table_schema` | ✅ **EXPECTED** |
| 2 | 0.564311 | `postgres_table_schema_get` | ❌ |
| 3 | 0.527917 | `mysql_table_schema_get` | ❌ |
| 4 | 0.490904 | `kusto_sample` | ❌ |
| 5 | 0.489624 | `kusto_table_list` | ❌ |

---

## Test 133

**Expected Tool:** `mysql_database_list`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633993 | `postgres_database_list` | ❌ |
| 2 | 0.623425 | `mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.534121 | `mysql_table_list` | ❌ |
| 4 | 0.498881 | `mysql_server_list` | ❌ |
| 5 | 0.490148 | `sql_db_list` | ❌ |

---

## Test 134

**Expected Tool:** `mysql_database_list`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588252 | `mysql_database_list` | ✅ **EXPECTED** |
| 2 | 0.573988 | `postgres_database_list` | ❌ |
| 3 | 0.483579 | `mysql_table_list` | ❌ |
| 4 | 0.463207 | `mysql_server_list` | ❌ |
| 5 | 0.444547 | `sql_db_list` | ❌ |

---

## Test 135

**Expected Tool:** `mysql_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476173 | `mysql_table_list` | ❌ |
| 2 | 0.456293 | `mysql_database_list` | ❌ |
| 3 | 0.433579 | `mysql_database_query` | ✅ **EXPECTED** |
| 4 | 0.419905 | `mysql_server_list` | ❌ |
| 5 | 0.409486 | `mysql_table_schema_get` | ❌ |

---

## Test 136

**Expected Tool:** `mysql_server_config_get`  
**Prompt:** Show me the configuration of MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531964 | `postgres_server_config_get` | ❌ |
| 2 | 0.516900 | `mysql_server_param_set` | ❌ |
| 3 | 0.490463 | `mysql_server_config_get` | ✅ **EXPECTED** |
| 4 | 0.476861 | `mysql_server_param_get` | ❌ |
| 5 | 0.426596 | `mysql_table_schema_get` | ❌ |

---

## Test 137

**Expected Tool:** `mysql_server_list`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678516 | `postgres_server_list` | ❌ |
| 2 | 0.558063 | `mysql_database_list` | ❌ |
| 3 | 0.554767 | `mysql_server_list` | ✅ **EXPECTED** |
| 4 | 0.513706 | `kusto_cluster_list` | ❌ |
| 5 | 0.501240 | `mysql_table_list` | ❌ |

---

## Test 138

**Expected Tool:** `mysql_server_list`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478476 | `mysql_database_list` | ❌ |
| 2 | 0.474544 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.435646 | `postgres_server_list` | ❌ |
| 4 | 0.412155 | `mysql_table_list` | ❌ |
| 5 | 0.389895 | `postgres_database_list` | ❌ |

---

## Test 139

**Expected Tool:** `mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636491 | `postgres_server_list` | ❌ |
| 2 | 0.534205 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.530155 | `mysql_database_list` | ❌ |
| 4 | 0.475052 | `kusto_cluster_list` | ❌ |
| 5 | 0.470468 | `redis_list` | ❌ |

---

## Test 140

**Expected Tool:** `mysql_server_param_get`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.494856 | `mysql_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.438075 | `mysql_server_param_set` | ❌ |
| 3 | 0.333841 | `mysql_database_query` | ❌ |
| 4 | 0.313150 | `mysql_table_schema_get` | ❌ |
| 5 | 0.310834 | `postgres_server_param_get` | ❌ |

---

## Test 141

**Expected Tool:** `mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.449419 | `mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.380956 | `mysql_server_param_get` | ❌ |
| 3 | 0.311784 | `storagesync_registeredserver_update` | ❌ |
| 4 | 0.303499 | `postgres_server_param_set` | ❌ |
| 5 | 0.298911 | `mysql_database_query` | ❌ |

---

## Test 142

**Expected Tool:** `mysql_table_list`  
**Prompt:** List all tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633465 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.573844 | `postgres_table_list` | ❌ |
| 3 | 0.550861 | `postgres_database_list` | ❌ |
| 4 | 0.546950 | `mysql_database_list` | ❌ |
| 5 | 0.511874 | `kusto_table_list` | ❌ |

---

## Test 143

**Expected Tool:** `mysql_table_list`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609075 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.526236 | `postgres_table_list` | ❌ |
| 3 | 0.525694 | `mysql_database_list` | ❌ |
| 4 | 0.507258 | `mysql_table_schema_get` | ❌ |
| 5 | 0.497981 | `postgres_database_list` | ❌ |

---

## Test 144

**Expected Tool:** `mysql_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630623 | `mysql_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.558306 | `postgres_table_schema_get` | ❌ |
| 3 | 0.544354 | `mysql_table_list` | ❌ |
| 4 | 0.517419 | `kusto_table_schema` | ❌ |
| 5 | 0.457722 | `mysql_database_list` | ❌ |

---

## Test 145

**Expected Tool:** `postgres_database_list`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815564 | `postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.644014 | `postgres_table_list` | ❌ |
| 3 | 0.622815 | `postgres_server_list` | ❌ |
| 4 | 0.542685 | `postgres_server_config_get` | ❌ |
| 5 | 0.490904 | `postgres_server_param_get` | ❌ |

---

## Test 146

**Expected Tool:** `postgres_database_list`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759942 | `postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.589807 | `postgres_server_list` | ❌ |
| 3 | 0.585891 | `postgres_table_list` | ❌ |
| 4 | 0.552660 | `postgres_server_config_get` | ❌ |
| 5 | 0.495629 | `postgres_server_param_get` | ❌ |

---

## Test 147

**Expected Tool:** `postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546153 | `postgres_database_list` | ❌ |
| 2 | 0.523142 | `postgres_database_query` | ✅ **EXPECTED** |
| 3 | 0.503267 | `postgres_table_list` | ❌ |
| 4 | 0.466625 | `postgres_server_list` | ❌ |
| 5 | 0.403969 | `postgres_server_param_get` | ❌ |

---

## Test 148

**Expected Tool:** `postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756593 | `postgres_server_config_get` | ✅ **EXPECTED** |
| 2 | 0.615429 | `postgres_server_param_set` | ❌ |
| 3 | 0.599471 | `postgres_server_param_get` | ❌ |
| 4 | 0.534957 | `postgres_database_list` | ❌ |
| 5 | 0.518562 | `postgres_server_list` | ❌ |

---

## Test 149

**Expected Tool:** `postgres_server_list`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.900030 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.640665 | `postgres_database_list` | ❌ |
| 3 | 0.565914 | `postgres_table_list` | ❌ |
| 4 | 0.538997 | `postgres_server_config_get` | ❌ |
| 5 | 0.534239 | `kusto_cluster_list` | ❌ |

---

## Test 150

**Expected Tool:** `postgres_server_list`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674306 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.606977 | `postgres_database_list` | ❌ |
| 3 | 0.576348 | `postgres_server_config_get` | ❌ |
| 4 | 0.522995 | `postgres_table_list` | ❌ |
| 5 | 0.506171 | `postgres_server_param_get` | ❌ |

---

## Test 151

**Expected Tool:** `postgres_server_list`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.832176 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.579149 | `postgres_database_list` | ❌ |
| 3 | 0.531804 | `postgres_server_config_get` | ❌ |
| 4 | 0.514445 | `postgres_table_list` | ❌ |
| 5 | 0.505869 | `postgres_server_param_get` | ❌ |

---

## Test 152

**Expected Tool:** `postgres_server_param_get`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594753 | `postgres_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.552678 | `postgres_server_param_set` | ❌ |
| 3 | 0.539671 | `postgres_server_config_get` | ❌ |
| 4 | 0.489659 | `postgres_server_list` | ❌ |
| 5 | 0.451779 | `postgres_database_list` | ❌ |

---

## Test 153

**Expected Tool:** `postgres_server_param_set`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579873 | `postgres_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.488474 | `postgres_server_config_get` | ❌ |
| 3 | 0.469790 | `postgres_server_list` | ❌ |
| 4 | 0.447011 | `postgres_server_param_get` | ❌ |
| 5 | 0.440673 | `postgres_database_list` | ❌ |

---

## Test 154

**Expected Tool:** `postgres_table_list`  
**Prompt:** List all tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789883 | `postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.750558 | `postgres_database_list` | ❌ |
| 3 | 0.574943 | `postgres_server_list` | ❌ |
| 4 | 0.519820 | `postgres_table_schema_get` | ❌ |
| 5 | 0.501400 | `postgres_server_config_get` | ❌ |

---

## Test 155

**Expected Tool:** `postgres_table_list`  
**Prompt:** Show me the tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736083 | `postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.690057 | `postgres_database_list` | ❌ |
| 3 | 0.558357 | `postgres_table_schema_get` | ❌ |
| 4 | 0.543337 | `postgres_server_list` | ❌ |
| 5 | 0.521570 | `postgres_server_config_get` | ❌ |

---

## Test 156

**Expected Tool:** `postgres_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.714877 | `postgres_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.597846 | `postgres_table_list` | ❌ |
| 3 | 0.574188 | `postgres_database_list` | ❌ |
| 4 | 0.508082 | `postgres_server_config_get` | ❌ |
| 5 | 0.502626 | `kusto_table_schema` | ❌ |

---

## Test 157

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

## Test 158

**Expected Tool:** `deploy_architecture_diagram_generate`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680035 | `deploy_architecture_diagram_generate` | ✅ **EXPECTED** |
| 2 | 0.562521 | `deploy_plan_get` | ❌ |
| 3 | 0.497193 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.489344 | `cloudarchitect_design` | ❌ |
| 5 | 0.435921 | `deploy_iac_rules_get` | ❌ |

---

## Test 159

**Expected Tool:** `deploy_iac_rules_get`  
**Prompt:** Show me the rules to generate bicep scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529092 | `deploy_iac_rules_get` | ✅ **EXPECTED** |
| 2 | 0.479903 | `bicepschema_get` | ❌ |
| 3 | 0.391965 | `get_bestpractices_get` | ❌ |
| 4 | 0.383173 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.375561 | `extension_cli_generate` | ❌ |

---

## Test 160

**Expected Tool:** `deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638841 | `deploy_pipeline_guidance_get` | ✅ **EXPECTED** |
| 2 | 0.499242 | `deploy_plan_get` | ❌ |
| 3 | 0.448917 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.388442 | `foundry_models_deploy` | ❌ |
| 5 | 0.385920 | `deploy_app_logs_get` | ❌ |

---

## Test 161

**Expected Tool:** `deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688055 | `deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.587903 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.499385 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.497470 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.448692 | `loadtesting_test_create` | ❌ |

---

## Test 162

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Publish an event to Event Grid topic <topic_name> using <event_schema> with the following data <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755307 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.483000 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.466066 | `eventgrid_topic_list` | ❌ |
| 4 | 0.360713 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.355584 | `servicebus_topic_details` | ❌ |

---

## Test 163

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Publish event to my Event Grid topic <topic_name> with the following events <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654571 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.524503 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.510144 | `eventgrid_topic_list` | ❌ |
| 4 | 0.373718 | `servicebus_topic_details` | ❌ |
| 5 | 0.359908 | `eventhubs_eventhub_update` | ❌ |

---

## Test 164

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Send an event to Event Grid topic <topic_name> in resource group <resource_group_name> with <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600229 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.521226 | `eventgrid_topic_list` | ❌ |
| 3 | 0.504808 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.411095 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.389439 | `eventhubs_eventhub_consumergroup_get` | ❌ |

---

## Test 165

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.770184 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.745471 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.561862 | `kusto_cluster_list` | ❌ |
| 4 | 0.543940 | `search_service_list` | ❌ |
| 5 | 0.526138 | `subscription_list` | ❌ |

---

## Test 166

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738320 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.737486 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.492592 | `kusto_cluster_list` | ❌ |
| 4 | 0.480287 | `subscription_list` | ❌ |
| 5 | 0.473493 | `search_service_list` | ❌ |

---

## Test 167

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.770178 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.721362 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.535326 | `kusto_cluster_list` | ❌ |
| 4 | 0.513958 | `search_service_list` | ❌ |
| 5 | 0.495987 | `subscription_list` | ❌ |

---

## Test 168

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.758681 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.704462 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.609175 | `group_list` | ❌ |
| 4 | 0.544809 | `monitor_webtests_list` | ❌ |
| 5 | 0.524209 | `eventhubs_namespace_get` | ❌ |

---

## Test 169

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.768402 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.719445 | `eventgrid_topic_list` | ❌ |
| 3 | 0.497333 | `servicebus_topic_details` | ❌ |
| 4 | 0.485553 | `eventgrid_events_publish` | ❌ |
| 5 | 0.483693 | `servicebus_topic_subscription_details` | ❌ |

---

## Test 170

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.718109 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.709966 | `eventgrid_topic_list` | ❌ |
| 3 | 0.539977 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.529286 | `servicebus_topic_details` | ❌ |
| 5 | 0.477790 | `eventgrid_events_publish` | ❌ |

---

## Test 171

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.746815 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.746219 | `eventgrid_topic_list` | ❌ |
| 3 | 0.535569 | `monitor_webtests_list` | ❌ |
| 4 | 0.524919 | `group_list` | ❌ |
| 5 | 0.503158 | `servicebus_topic_details` | ❌ |

---

## Test 172

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736436 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.659784 | `eventgrid_topic_list` | ❌ |
| 3 | 0.569254 | `subscription_list` | ❌ |
| 4 | 0.537922 | `kusto_cluster_list` | ❌ |
| 5 | 0.517379 | `search_service_list` | ❌ |

---

## Test 173

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684543 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.656343 | `eventgrid_topic_list` | ❌ |
| 3 | 0.542388 | `subscription_list` | ❌ |
| 4 | 0.521053 | `kusto_cluster_list` | ❌ |
| 5 | 0.510115 | `group_list` | ❌ |

---

## Test 174

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.697861 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.692115 | `eventgrid_topic_list` | ❌ |
| 3 | 0.559238 | `group_list` | ❌ |
| 4 | 0.512547 | `monitor_webtests_list` | ❌ |
| 5 | 0.507701 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 175

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.709910 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.642408 | `eventgrid_topic_list` | ❌ |
| 3 | 0.506739 | `subscription_list` | ❌ |
| 4 | 0.476298 | `search_service_list` | ❌ |
| 5 | 0.475681 | `kusto_cluster_list` | ❌ |

---

## Test 176

**Expected Tool:** `eventhubs_eventhub_consumergroup_delete`  
**Prompt:** Delete my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.767515 | `eventhubs_eventhub_consumergroup_delete` | ✅ **EXPECTED** |
| 2 | 0.676285 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.641222 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.633842 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.605610 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 177

**Expected Tool:** `eventhubs_eventhub_consumergroup_get`  
**Prompt:** List all consumer groups in my event hub <event_hub_name> in namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.740745 | `eventhubs_eventhub_consumergroup_get` | ✅ **EXPECTED** |
| 2 | 0.635229 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.625898 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.613933 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.601794 | `eventhubs_eventhub_get` | ❌ |

---

## Test 178

**Expected Tool:** `eventhubs_eventhub_consumergroup_get`  
**Prompt:** Get the details of my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712861 | `eventhubs_eventhub_consumergroup_get` | ✅ **EXPECTED** |
| 2 | 0.637190 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.625099 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.576800 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.529940 | `eventhubs_eventhub_get` | ❌ |

---

## Test 179

**Expected Tool:** `eventhubs_eventhub_consumergroup_update`  
**Prompt:** Create a new consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.757323 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
| 2 | 0.688923 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 3 | 0.668801 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.554315 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.545003 | `eventhubs_namespace_get` | ❌ |

---

## Test 180

**Expected Tool:** `eventhubs_eventhub_consumergroup_update`  
**Prompt:** Update my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738518 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
| 2 | 0.654951 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 3 | 0.642179 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.552245 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.524386 | `eventhubs_namespace_delete` | ❌ |

---

## Test 181

**Expected Tool:** `eventhubs_eventhub_delete`  
**Prompt:** Delete my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.699213 | `eventhubs_namespace_delete` | ❌ |
| 2 | 0.688502 | `eventhubs_eventhub_delete` | ✅ **EXPECTED** |
| 3 | 0.629498 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.578687 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.552954 | `eventhubs_eventhub_get` | ❌ |

---

## Test 182

**Expected Tool:** `eventhubs_eventhub_get`  
**Prompt:** List all Event Hubs in my namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.773242 | `eventhubs_eventhub_get` | ✅ **EXPECTED** |
| 2 | 0.687582 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.578689 | `eventhubs_eventhub_update` | ❌ |
| 4 | 0.561544 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.545474 | `eventhubs_eventhub_consumergroup_get` | ❌ |

---

## Test 183

**Expected Tool:** `eventhubs_eventhub_get`  
**Prompt:** Get the details of my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638083 | `eventhubs_namespace_get` | ❌ |
| 2 | 0.627638 | `eventhubs_eventhub_get` | ✅ **EXPECTED** |
| 3 | 0.570904 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.527646 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.521919 | `eventhubs_namespace_delete` | ❌ |

---

## Test 184

**Expected Tool:** `eventhubs_eventhub_update`  
**Prompt:** Create a new event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645968 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.605908 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.574439 | `eventhubs_eventhub_get` | ❌ |
| 4 | 0.571544 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.557631 | `eventhubs_namespace_delete` | ❌ |

---

## Test 185

**Expected Tool:** `eventhubs_eventhub_update`  
**Prompt:** Update my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655283 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.571661 | `eventhubs_eventhub_delete` | ❌ |
| 3 | 0.568556 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 4 | 0.568396 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.565977 | `eventhubs_namespace_delete` | ❌ |

---

## Test 186

**Expected Tool:** `eventhubs_namespace_delete`  
**Prompt:** Delete my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623995 | `eventhubs_namespace_delete` | ✅ **EXPECTED** |
| 2 | 0.525820 | `eventhubs_namespace_update` | ❌ |
| 3 | 0.506829 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.480722 | `storagesync_syncgroup_delete` | ❌ |
| 5 | 0.477743 | `storagesync_service_delete` | ❌ |

---

## Test 187

**Expected Tool:** `eventhubs_namespace_get`  
**Prompt:** List all Event Hubs namespaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659838 | `eventhubs_eventhub_get` | ❌ |
| 2 | 0.658827 | `eventhubs_namespace_get` | ✅ **EXPECTED** |
| 3 | 0.607372 | `kusto_cluster_list` | ❌ |
| 4 | 0.557162 | `eventgrid_topic_list` | ❌ |
| 5 | 0.556126 | `eventgrid_subscription_list` | ❌ |

---

## Test 188

**Expected Tool:** `eventhubs_namespace_get`  
**Prompt:** Get the details of my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509762 | `eventhubs_namespace_get` | ✅ **EXPECTED** |
| 2 | 0.509611 | `monitor_webtests_get` | ❌ |
| 3 | 0.497493 | `servicebus_queue_details` | ❌ |
| 4 | 0.489930 | `eventhubs_namespace_update` | ❌ |
| 5 | 0.478468 | `storagesync_syncgroup_get` | ❌ |

---

## Test 189

**Expected Tool:** `eventhubs_namespace_update`  
**Prompt:** Create an new namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610228 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.498949 | `storagesync_service_create` | ❌ |
| 3 | 0.466721 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.461833 | `storagesync_syncgroup_create` | ❌ |
| 5 | 0.458458 | `eventhubs_namespace_delete` | ❌ |

---

## Test 190

**Expected Tool:** `eventhubs_namespace_update`  
**Prompt:** Update my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622067 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.474098 | `eventhubs_namespace_delete` | ❌ |
| 3 | 0.448723 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.436572 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.408853 | `storagesync_service_update` | ❌ |

---

## Test 191

**Expected Tool:** `functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659383 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.451613 | `deploy_app_logs_get` | ❌ |
| 3 | 0.450457 | `applens_resource_diagnose` | ❌ |
| 4 | 0.390063 | `mysql_server_list` | ❌ |
| 5 | 0.380314 | `get_bestpractices_get` | ❌ |

---

## Test 192

**Expected Tool:** `functionapp_get`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607500 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.447355 | `mysql_server_config_get` | ❌ |
| 3 | 0.424693 | `appconfig_account_list` | ❌ |
| 4 | 0.411267 | `appconfig_kv_get` | ❌ |
| 5 | 0.400402 | `deploy_app_logs_get` | ❌ |

---

## Test 193

**Expected Tool:** `functionapp_get`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623176 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.413388 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.390708 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.383533 | `deploy_app_logs_get` | ❌ |
| 5 | 0.360665 | `storage_account_get` | ❌ |

---

## Test 194

**Expected Tool:** `functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690728 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.440320 | `foundry_resource_get` | ❌ |
| 3 | 0.432376 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.431843 | `applens_resource_diagnose` | ❌ |
| 5 | 0.429127 | `storage_account_get` | ❌ |

---

## Test 195

**Expected Tool:** `functionapp_get`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593151 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.417670 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.409712 | `deploy_app_logs_get` | ❌ |
| 4 | 0.399953 | `storage_account_get` | ❌ |
| 5 | 0.392237 | `applens_resource_diagnose` | ❌ |

---

## Test 196

**Expected Tool:** `functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687118 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.449314 | `deploy_app_logs_get` | ❌ |
| 3 | 0.428552 | `applens_resource_diagnose` | ❌ |
| 4 | 0.421694 | `foundry_resource_get` | ❌ |
| 5 | 0.391525 | `monitor_webtests_get` | ❌ |

---

## Test 197

**Expected Tool:** `functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645168 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.430189 | `deploy_app_logs_get` | ❌ |
| 3 | 0.421082 | `storage_account_get` | ❌ |
| 4 | 0.403311 | `signalr_runtime_get` | ❌ |
| 5 | 0.398991 | `storagesync_service_get` | ❌ |

---

## Test 198

**Expected Tool:** `functionapp_get`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555416 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.426703 | `quota_usage_check` | ❌ |
| 3 | 0.424610 | `deploy_app_logs_get` | ❌ |
| 4 | 0.408011 | `deploy_plan_get` | ❌ |
| 5 | 0.381067 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 199

**Expected Tool:** `functionapp_get`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566436 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.403665 | `deploy_app_logs_get` | ❌ |
| 3 | 0.384159 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.369868 | `applens_resource_diagnose` | ❌ |
| 5 | 0.354787 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 200

**Expected Tool:** `functionapp_get`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645417 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.557615 | `search_service_list` | ❌ |
| 3 | 0.534930 | `subscription_list` | ❌ |
| 4 | 0.529031 | `kusto_cluster_list` | ❌ |
| 5 | 0.516629 | `cosmos_account_list` | ❌ |

---

## Test 201

**Expected Tool:** `functionapp_get`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560150 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.464985 | `deploy_app_logs_get` | ❌ |
| 3 | 0.411323 | `get_bestpractices_get` | ❌ |
| 4 | 0.410459 | `search_service_list` | ❌ |
| 5 | 0.398503 | `extension_cli_install` | ❌ |

---

## Test 202

**Expected Tool:** `functionapp_get`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433095 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.346251 | `deploy_app_logs_get` | ❌ |
| 3 | 0.337647 | `applens_resource_diagnose` | ❌ |
| 4 | 0.316244 | `extension_cli_install` | ❌ |
| 5 | 0.283924 | `get_bestpractices_get` | ❌ |

---

## Test 203

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** Get the account settings for my key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604961 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.532282 | `storage_account_get` | ❌ |
| 3 | 0.496754 | `keyvault_key_get` | ❌ |
| 4 | 0.452386 | `appconfig_kv_set` | ❌ |
| 5 | 0.448143 | `keyvault_secret_get` | ❌ |

---

## Test 204

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** Show me the account settings for managed HSM keyvault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671474 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.455561 | `storage_account_get` | ❌ |
| 3 | 0.441224 | `keyvault_key_get` | ❌ |
| 4 | 0.404666 | `appconfig_kv_set` | ❌ |
| 5 | 0.395274 | `keyvault_secret_get` | ❌ |

---

## Test 205

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** What's the value of the <setting_name> setting in my key vault with name <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.505805 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.496540 | `appconfig_kv_set` | ❌ |
| 3 | 0.420195 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.419126 | `keyvault_key_get` | ❌ |
| 5 | 0.410215 | `keyvault_secret_get` | ❌ |

---

## Test 206

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627727 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.570319 | `keyvault_certificate_import` | ❌ |
| 3 | 0.540191 | `keyvault_key_create` | ❌ |
| 4 | 0.519218 | `keyvault_certificate_get` | ❌ |
| 5 | 0.500027 | `keyvault_certificate_list` | ❌ |

---

## Test 207

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Generate a certificate named <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599990 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.561445 | `keyvault_certificate_import` | ❌ |
| 3 | 0.522706 | `keyvault_certificate_get` | ❌ |
| 4 | 0.502133 | `keyvault_key_create` | ❌ |
| 5 | 0.497145 | `keyvault_certificate_list` | ❌ |

---

## Test 208

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Request creation of certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573998 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.527759 | `keyvault_certificate_import` | ❌ |
| 3 | 0.498278 | `keyvault_certificate_get` | ❌ |
| 4 | 0.481532 | `keyvault_key_create` | ❌ |
| 5 | 0.469601 | `keyvault_certificate_list` | ❌ |

---

## Test 209

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591697 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.562265 | `keyvault_certificate_import` | ❌ |
| 3 | 0.522147 | `keyvault_certificate_get` | ❌ |
| 4 | 0.502517 | `keyvault_key_create` | ❌ |
| 5 | 0.479992 | `keyvault_certificate_list` | ❌ |

---

## Test 210

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Issue a certificate <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622788 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.558532 | `keyvault_certificate_import` | ❌ |
| 3 | 0.534503 | `keyvault_certificate_get` | ❌ |
| 4 | 0.521316 | `keyvault_certificate_list` | ❌ |
| 5 | 0.465055 | `keyvault_key_create` | ❌ |

---

## Test 211

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600625 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.528405 | `keyvault_certificate_list` | ❌ |
| 3 | 0.519037 | `keyvault_certificate_import` | ❌ |
| 4 | 0.499293 | `keyvault_certificate_create` | ❌ |
| 5 | 0.486608 | `keyvault_key_get` | ❌ |

---

## Test 212

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646098 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.562988 | `keyvault_key_get` | ❌ |
| 3 | 0.514170 | `keyvault_secret_get` | ❌ |
| 4 | 0.509446 | `keyvault_certificate_list` | ❌ |
| 5 | 0.507738 | `keyvault_certificate_import` | ❌ |

---

## Test 213

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Get the certificate <certificate_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609523 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.515570 | `keyvault_certificate_list` | ❌ |
| 3 | 0.511197 | `keyvault_certificate_create` | ❌ |
| 4 | 0.507768 | `keyvault_certificate_import` | ❌ |
| 5 | 0.474394 | `keyvault_key_get` | ❌ |

---

## Test 214

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Display the certificate details for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.647632 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527383 | `keyvault_key_get` | ❌ |
| 3 | 0.521542 | `keyvault_certificate_list` | ❌ |
| 4 | 0.509816 | `keyvault_certificate_import` | ❌ |
| 5 | 0.501960 | `keyvault_secret_get` | ❌ |

---

## Test 215

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595959 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527404 | `keyvault_certificate_list` | ❌ |
| 3 | 0.519059 | `keyvault_certificate_import` | ❌ |
| 4 | 0.501138 | `keyvault_certificate_create` | ❌ |
| 5 | 0.465174 | `keyvault_key_get` | ❌ |

---

## Test 216

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585481 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.420747 | `keyvault_certificate_get` | ❌ |
| 3 | 0.402595 | `keyvault_certificate_create` | ❌ |
| 4 | 0.399342 | `keyvault_certificate_list` | ❌ |
| 5 | 0.352919 | `keyvault_key_create` | ❌ |

---

## Test 217

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622182 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.504511 | `keyvault_certificate_get` | ❌ |
| 3 | 0.498800 | `keyvault_certificate_create` | ❌ |
| 4 | 0.448370 | `keyvault_certificate_list` | ❌ |
| 5 | 0.419820 | `keyvault_key_create` | ❌ |

---

## Test 218

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Upload certificate file <file_path> to key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595707 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.453929 | `keyvault_certificate_create` | ❌ |
| 3 | 0.452551 | `keyvault_certificate_get` | ❌ |
| 4 | 0.418203 | `keyvault_certificate_list` | ❌ |
| 5 | 0.413395 | `keyvault_key_create` | ❌ |

---

## Test 219

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619555 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.517900 | `keyvault_certificate_get` | ❌ |
| 3 | 0.480847 | `keyvault_certificate_create` | ❌ |
| 4 | 0.444397 | `keyvault_certificate_list` | ❌ |
| 5 | 0.381859 | `keyvault_key_create` | ❌ |

---

## Test 220

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595418 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.452490 | `keyvault_certificate_create` | ❌ |
| 3 | 0.441616 | `keyvault_certificate_get` | ❌ |
| 4 | 0.408040 | `keyvault_key_create` | ❌ |
| 5 | 0.392244 | `keyvault_secret_create` | ❌ |

---

## Test 221

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726059 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.582961 | `keyvault_key_list` | ❌ |
| 3 | 0.531892 | `keyvault_secret_list` | ❌ |
| 4 | 0.515202 | `keyvault_certificate_get` | ❌ |
| 5 | 0.485732 | `keyvault_certificate_create` | ❌ |

---

## Test 222

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615541 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.522453 | `keyvault_certificate_get` | ❌ |
| 3 | 0.475078 | `keyvault_key_list` | ❌ |
| 4 | 0.460973 | `keyvault_certificate_create` | ❌ |
| 5 | 0.448139 | `keyvault_key_get` | ❌ |

---

## Test 223

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** What certificates are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624710 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.519739 | `keyvault_certificate_get` | ❌ |
| 3 | 0.510048 | `keyvault_certificate_create` | ❌ |
| 4 | 0.505534 | `keyvault_certificate_import` | ❌ |
| 5 | 0.497273 | `keyvault_key_list` | ❌ |

---

## Test 224

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** List certificate names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672622 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.553876 | `keyvault_key_list` | ❌ |
| 3 | 0.511907 | `keyvault_secret_list` | ❌ |
| 4 | 0.507062 | `keyvault_certificate_get` | ❌ |
| 5 | 0.492357 | `keyvault_certificate_create` | ❌ |

---

## Test 225

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Enumerate certificates in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747408 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.594182 | `keyvault_key_list` | ❌ |
| 3 | 0.558641 | `keyvault_secret_list` | ❌ |
| 4 | 0.515568 | `keyvault_certificate_get` | ❌ |
| 5 | 0.490876 | `keyvault_certificate_create` | ❌ |

---

## Test 226

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show certificate names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639711 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.512475 | `keyvault_certificate_get` | ❌ |
| 3 | 0.507474 | `keyvault_key_list` | ❌ |
| 4 | 0.482583 | `keyvault_certificate_create` | ❌ |
| 5 | 0.464740 | `keyvault_secret_list` | ❌ |

---

## Test 227

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661452 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.456580 | `keyvault_secret_create` | ❌ |
| 3 | 0.451741 | `keyvault_certificate_create` | ❌ |
| 4 | 0.429574 | `keyvault_certificate_import` | ❌ |
| 5 | 0.399324 | `keyvault_key_get` | ❌ |

---

## Test 228

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Generate a key <key_name> with type <key_type> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641153 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.428555 | `keyvault_key_get` | ❌ |
| 3 | 0.422801 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420068 | `keyvault_secret_create` | ❌ |
| 5 | 0.405622 | `appconfig_kv_set` | ❌ |

---

## Test 229

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an oct key in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547460 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.462869 | `keyvault_secret_create` | ❌ |
| 3 | 0.447168 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420696 | `keyvault_key_get` | ❌ |
| 5 | 0.403903 | `keyvault_certificate_import` | ❌ |

---

## Test 230

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an RSA key in the vault <key_vault_account_name> with name <key_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641361 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.501636 | `keyvault_secret_create` | ❌ |
| 3 | 0.491735 | `keyvault_certificate_create` | ❌ |
| 4 | 0.464557 | `keyvault_certificate_import` | ❌ |
| 5 | 0.451016 | `keyvault_key_get` | ❌ |

---

## Test 231

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an EC key with name <key_name> in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571741 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.443369 | `keyvault_certificate_create` | ❌ |
| 3 | 0.434675 | `keyvault_secret_create` | ❌ |
| 4 | 0.421721 | `keyvault_key_get` | ❌ |
| 5 | 0.400533 | `keyvault_certificate_import` | ❌ |

---

## Test 232

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549488 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.468165 | `keyvault_secret_get` | ❌ |
| 3 | 0.452835 | `keyvault_key_create` | ❌ |
| 4 | 0.439843 | `keyvault_key_list` | ❌ |
| 5 | 0.426545 | `keyvault_certificate_get` | ❌ |

---

## Test 233

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629691 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.532749 | `keyvault_secret_get` | ❌ |
| 3 | 0.512411 | `storage_account_get` | ❌ |
| 4 | 0.496184 | `keyvault_certificate_get` | ❌ |
| 5 | 0.456992 | `keyvault_key_create` | ❌ |

---

## Test 234

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Get the key <key_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.484645 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.443231 | `keyvault_key_create` | ❌ |
| 3 | 0.409388 | `keyvault_secret_get` | ❌ |
| 4 | 0.395550 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.383744 | `appconfig_kv_lock_set` | ❌ |

---

## Test 235

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Display the key details for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590303 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.488213 | `keyvault_secret_get` | ❌ |
| 3 | 0.476498 | `storage_account_get` | ❌ |
| 4 | 0.460796 | `keyvault_certificate_get` | ❌ |
| 5 | 0.436550 | `keyvault_admin_settings_get` | ❌ |

---

## Test 236

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Retrieve key metadata for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518886 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.432950 | `storage_account_get` | ❌ |
| 3 | 0.432828 | `keyvault_admin_settings_get` | ❌ |
| 4 | 0.429175 | `keyvault_key_create` | ❌ |
| 5 | 0.422536 | `keyvault_secret_get` | ❌ |

---

## Test 237

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701321 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.601513 | `keyvault_certificate_list` | ❌ |
| 3 | 0.587415 | `keyvault_secret_list` | ❌ |
| 4 | 0.505496 | `storage_table_list` | ❌ |
| 5 | 0.498712 | `cosmos_account_list` | ❌ |

---

## Test 238

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549361 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.506815 | `keyvault_key_get` | ❌ |
| 3 | 0.475507 | `keyvault_certificate_list` | ❌ |
| 4 | 0.472542 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.455683 | `keyvault_secret_get` | ❌ |

---

## Test 239

**Expected Tool:** `keyvault_key_list`  
**Prompt:** What keys are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581861 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.502317 | `keyvault_admin_settings_get` | ❌ |
| 3 | 0.501481 | `keyvault_certificate_list` | ❌ |
| 4 | 0.476470 | `keyvault_key_get` | ❌ |
| 5 | 0.472352 | `keyvault_secret_list` | ❌ |

---

## Test 240

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List key names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641141 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.559550 | `keyvault_certificate_list` | ❌ |
| 3 | 0.553568 | `keyvault_secret_list` | ❌ |
| 4 | 0.491260 | `storage_table_list` | ❌ |
| 5 | 0.486467 | `keyvault_admin_settings_get` | ❌ |

---

## Test 241

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Enumerate keys in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.723305 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.611406 | `keyvault_certificate_list` | ❌ |
| 3 | 0.611136 | `keyvault_secret_list` | ❌ |
| 4 | 0.473916 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.441830 | `keyvault_key_get` | ❌ |

---

## Test 242

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Show key names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570295 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.501073 | `keyvault_key_get` | ❌ |
| 3 | 0.500103 | `keyvault_certificate_list` | ❌ |
| 4 | 0.496817 | `storage_account_get` | ❌ |
| 5 | 0.490384 | `keyvault_secret_list` | ❌ |

---

## Test 243

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678482 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.553016 | `keyvault_key_create` | ❌ |
| 3 | 0.512856 | `keyvault_secret_get` | ❌ |
| 4 | 0.475097 | `keyvault_certificate_create` | ❌ |
| 5 | 0.461437 | `appconfig_kv_set` | ❌ |

---

## Test 244

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663094 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.519601 | `keyvault_secret_get` | ❌ |
| 3 | 0.512233 | `appconfig_kv_set` | ❌ |
| 4 | 0.458505 | `keyvault_key_create` | ❌ |
| 5 | 0.430444 | `appconfig_kv_lock_set` | ❌ |

---

## Test 245

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640580 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.510017 | `keyvault_secret_get` | ❌ |
| 3 | 0.485405 | `appconfig_kv_set` | ❌ |
| 4 | 0.485165 | `keyvault_key_create` | ❌ |
| 5 | 0.449513 | `appconfig_kv_lock_set` | ❌ |

---

## Test 246

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.675145 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.499612 | `keyvault_secret_get` | ❌ |
| 3 | 0.498300 | `keyvault_key_create` | ❌ |
| 4 | 0.479174 | `keyvault_certificate_import` | ❌ |
| 5 | 0.458574 | `appconfig_kv_set` | ❌ |

---

## Test 247

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Update secret <secret_name> to value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571395 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.514037 | `keyvault_secret_get` | ❌ |
| 3 | 0.440656 | `appconfig_kv_set` | ❌ |
| 4 | 0.418518 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.408019 | `keyvault_key_get` | ❌ |

---

## Test 248

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602769 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.504212 | `keyvault_key_get` | ❌ |
| 3 | 0.501397 | `keyvault_secret_create` | ❌ |
| 4 | 0.478697 | `keyvault_secret_list` | ❌ |
| 5 | 0.439521 | `keyvault_certificate_get` | ❌ |

---

## Test 249

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653871 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.566786 | `keyvault_key_get` | ❌ |
| 3 | 0.517547 | `storage_account_get` | ❌ |
| 4 | 0.496050 | `keyvault_certificate_get` | ❌ |
| 5 | 0.485177 | `keyvault_secret_list` | ❌ |

---

## Test 250

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Get the secret <secret_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578479 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.492213 | `keyvault_key_get` | ❌ |
| 3 | 0.488705 | `keyvault_secret_create` | ❌ |
| 4 | 0.443591 | `keyvault_secret_list` | ❌ |
| 5 | 0.424201 | `keyvault_admin_settings_get` | ❌ |

---

## Test 251

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Display the secret details for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649267 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.546992 | `keyvault_key_get` | ❌ |
| 3 | 0.497402 | `storage_account_get` | ❌ |
| 4 | 0.492583 | `keyvault_certificate_get` | ❌ |
| 5 | 0.491539 | `keyvault_secret_list` | ❌ |

---

## Test 252

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Retrieve secret metadata for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577477 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.475443 | `keyvault_key_get` | ❌ |
| 3 | 0.466890 | `keyvault_secret_create` | ❌ |
| 4 | 0.447547 | `keyvault_secret_list` | ❌ |
| 5 | 0.439583 | `storage_account_get` | ❌ |

---

## Test 253

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701158 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.563623 | `keyvault_key_list` | ❌ |
| 3 | 0.538337 | `keyvault_certificate_list` | ❌ |
| 4 | 0.499642 | `keyvault_secret_get` | ❌ |
| 5 | 0.455447 | `cosmos_account_list` | ❌ |

---

## Test 254

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555609 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.543861 | `keyvault_secret_get` | ❌ |
| 3 | 0.497525 | `keyvault_key_get` | ❌ |
| 4 | 0.464581 | `keyvault_key_list` | ❌ |
| 5 | 0.453215 | `keyvault_admin_settings_get` | ❌ |

---

## Test 255

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** What secrets are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572428 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.529219 | `keyvault_secret_get` | ❌ |
| 3 | 0.493521 | `keyvault_key_list` | ❌ |
| 4 | 0.487620 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.475190 | `keyvault_key_get` | ❌ |

---

## Test 256

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** List secrets names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624275 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.559536 | `keyvault_key_list` | ❌ |
| 3 | 0.517516 | `keyvault_certificate_list` | ❌ |
| 4 | 0.479547 | `keyvault_secret_get` | ❌ |
| 5 | 0.454288 | `storage_blob_container_get` | ❌ |

---

## Test 257

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Enumerate secrets in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.742213 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.601114 | `keyvault_key_list` | ❌ |
| 3 | 0.567827 | `keyvault_certificate_list` | ❌ |
| 4 | 0.496127 | `keyvault_secret_get` | ❌ |
| 5 | 0.437655 | `keyvault_admin_settings_get` | ❌ |

---

## Test 258

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Show secrets names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567105 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.522399 | `keyvault_secret_get` | ❌ |
| 3 | 0.476204 | `keyvault_key_list` | ❌ |
| 4 | 0.462677 | `keyvault_secret_create` | ❌ |
| 5 | 0.461326 | `keyvault_key_get` | ❌ |

---

## Test 259

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588300 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.544465 | `aks_nodepool_get` | ❌ |
| 3 | 0.517279 | `kusto_cluster_get` | ❌ |
| 4 | 0.481540 | `mysql_server_config_get` | ❌ |
| 5 | 0.430976 | `postgres_server_config_get` | ❌ |

---

## Test 260

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621751 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.575678 | `aks_nodepool_get` | ❌ |
| 3 | 0.567910 | `kusto_cluster_get` | ❌ |
| 4 | 0.461439 | `sql_db_show` | ❌ |
| 5 | 0.447064 | `storagesync_service_get` | ❌ |

---

## Test 261

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522525 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.483337 | `aks_nodepool_get` | ❌ |
| 3 | 0.434684 | `kusto_cluster_get` | ❌ |
| 4 | 0.380412 | `mysql_server_config_get` | ❌ |
| 5 | 0.366689 | `kusto_cluster_list` | ❌ |

---

## Test 262

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588634 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.550650 | `aks_nodepool_get` | ❌ |
| 3 | 0.527511 | `kusto_cluster_get` | ❌ |
| 4 | 0.448818 | `storagesync_service_get` | ❌ |
| 5 | 0.445722 | `storage_account_get` | ❌ |

---

## Test 263

**Expected Tool:** `aks_cluster_get`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756471 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.749416 | `kusto_cluster_list` | ❌ |
| 3 | 0.590256 | `aks_nodepool_get` | ❌ |
| 4 | 0.568341 | `kusto_database_list` | ❌ |
| 5 | 0.560571 | `search_service_list` | ❌ |

---

## Test 264

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612123 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.586661 | `kusto_cluster_list` | ❌ |
| 3 | 0.507801 | `aks_nodepool_get` | ❌ |
| 4 | 0.489724 | `kusto_cluster_get` | ❌ |
| 5 | 0.462820 | `kusto_database_list` | ❌ |

---

## Test 265

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628429 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.563358 | `aks_nodepool_get` | ❌ |
| 3 | 0.526756 | `kusto_cluster_list` | ❌ |
| 4 | 0.426157 | `kusto_cluster_get` | ❌ |
| 5 | 0.409087 | `kusto_database_list` | ❌ |

---

## Test 266

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728855 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.516987 | `kusto_cluster_get` | ❌ |
| 3 | 0.509775 | `aks_cluster_get` | ❌ |
| 4 | 0.468395 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.463196 | `sql_elastic-pool_list` | ❌ |

---

## Test 267

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654132 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.458320 | `sql_elastic-pool_list` | ❌ |
| 3 | 0.445942 | `aks_cluster_get` | ❌ |
| 4 | 0.439786 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.413692 | `kusto_cluster_get` | ❌ |

---

## Test 268

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592887 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.402556 | `aks_cluster_get` | ❌ |
| 3 | 0.385173 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.383120 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.355090 | `kusto_cluster_get` | ❌ |

---

## Test 269

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692332 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.519037 | `aks_cluster_get` | ❌ |
| 3 | 0.506624 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.500749 | `kusto_cluster_list` | ❌ |
| 5 | 0.487704 | `sql_elastic-pool_list` | ❌ |

---

## Test 270

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732178 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.561829 | `aks_cluster_get` | ❌ |
| 3 | 0.510323 | `sql_elastic-pool_list` | ❌ |
| 4 | 0.509732 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.486700 | `kusto_cluster_list` | ❌ |

---

## Test 271

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629476 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.456911 | `aks_cluster_get` | ❌ |
| 3 | 0.443902 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.433006 | `kusto_cluster_list` | ❌ |
| 5 | 0.425421 | `sql_elastic-pool_list` | ❌ |

---

## Test 272

**Expected Tool:** `loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577811 | `loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.519844 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.512099 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.472753 | `monitor_webtests_create` | ❌ |
| 5 | 0.460717 | `loadtesting_testresource_list` | ❌ |

---

## Test 273

**Expected Tool:** `loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626226 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.619944 | `loadtesting_test_get` | ✅ **EXPECTED** |
| 3 | 0.595020 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.590873 | `monitor_webtests_get` | ❌ |
| 5 | 0.536024 | `monitor_webtests_list` | ❌ |

---

## Test 274

**Expected Tool:** `loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646200 | `loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.618644 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.542011 | `loadtesting_test_create` | ❌ |
| 4 | 0.540112 | `loadtesting_testrun_create` | ❌ |
| 5 | 0.526387 | `monitor_webtests_list` | ❌ |

---

## Test 275

**Expected Tool:** `loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.794326 | `loadtesting_testresource_list` | ✅ **EXPECTED** |
| 2 | 0.653165 | `monitor_webtests_list` | ❌ |
| 3 | 0.577408 | `group_list` | ❌ |
| 4 | 0.575652 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.565435 | `datadog_monitoredresources_list` | ❌ |

---

## Test 276

**Expected Tool:** `loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688976 | `loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.594926 | `loadtesting_testrun_update` | ❌ |
| 3 | 0.558636 | `loadtesting_test_create` | ❌ |
| 4 | 0.547546 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.496224 | `loadtesting_testresource_list` | ❌ |

---

## Test 277

**Expected Tool:** `loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619125 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.601314 | `loadtesting_test_get` | ❌ |
| 3 | 0.597853 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.577717 | `monitor_webtests_get` | ❌ |
| 5 | 0.565324 | `loadtesting_testrun_list` | ❌ |

---

## Test 278

**Expected Tool:** `loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669180 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.640360 | `loadtesting_testrun_list` | ✅ **EXPECTED** |
| 3 | 0.601075 | `loadtesting_test_get` | ❌ |
| 4 | 0.577811 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.569584 | `monitor_webtests_get` | ❌ |

---

## Test 279

**Expected Tool:** `loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.706737 | `loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.514428 | `loadtesting_testrun_create` | ❌ |
| 3 | 0.486980 | `monitor_webtests_update` | ❌ |
| 4 | 0.470337 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.468450 | `monitor_webtests_get` | ❌ |

---

## Test 280

**Expected Tool:** `grafana_list`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599427 | `kusto_cluster_list` | ❌ |
| 2 | 0.578892 | `grafana_list` | ✅ **EXPECTED** |
| 3 | 0.550372 | `subscription_list` | ❌ |
| 4 | 0.550013 | `search_service_list` | ❌ |
| 5 | 0.531259 | `redis_list` | ❌ |

---

## Test 281

**Expected Tool:** `managedlustre_fs_create`  
**Prompt:** Create an Azure Managed Lustre filesystem with name <filesystem_name>, size <filesystem_size>, SKU <sku>, and subnet <subnet_id> for availability zone <zone> in location <location>. Maintenance should occur on <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728053 | `managedlustre_fs_create` | ✅ **EXPECTED** |
| 2 | 0.616164 | `managedlustre_fs_list` | ❌ |
| 3 | 0.605775 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.598255 | `managedlustre_fs_update` | ❌ |
| 5 | 0.557720 | `managedlustre_fs_subnetsize_validate` | ❌ |

---

## Test 282

**Expected Tool:** `managedlustre_fs_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.750675 | `managedlustre_fs_list` | ✅ **EXPECTED** |
| 2 | 0.631770 | `managedlustre_fs_sku_get` | ❌ |
| 3 | 0.582489 | `managedlustre_fs_create` | ❌ |
| 4 | 0.562377 | `kusto_cluster_list` | ❌ |
| 5 | 0.516175 | `storagesync_service_get` | ❌ |

---

## Test 283

**Expected Tool:** `managedlustre_fs_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743903 | `managedlustre_fs_list` | ✅ **EXPECTED** |
| 2 | 0.613217 | `managedlustre_fs_sku_get` | ❌ |
| 3 | 0.565631 | `managedlustre_fs_create` | ❌ |
| 4 | 0.519908 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.515433 | `loadtesting_testresource_list` | ❌ |

---

## Test 284

**Expected Tool:** `managedlustre_fs_sku_get`  
**Prompt:** List the Azure Managed Lustre SKUs available in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.827381 | `managedlustre_fs_sku_get` | ✅ **EXPECTED** |
| 2 | 0.613674 | `managedlustre_fs_list` | ❌ |
| 3 | 0.512663 | `managedlustre_fs_create` | ❌ |
| 4 | 0.496242 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 5 | 0.484015 | `storagesync_service_get` | ❌ |

---

## Test 285

**Expected Tool:** `managedlustre_fs_subnetsize_ask`  
**Prompt:** Tell me how many IP addresses I need for an Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739707 | `managedlustre_fs_subnetsize_ask` | ✅ **EXPECTED** |
| 2 | 0.651572 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 3 | 0.594531 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.559501 | `managedlustre_fs_list` | ❌ |
| 5 | 0.533380 | `managedlustre_fs_create` | ❌ |

---

## Test 286

**Expected Tool:** `managedlustre_fs_subnetsize_validate`  
**Prompt:** Validate if the network <subnet_id> can host Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879323 | `managedlustre_fs_subnetsize_validate` | ✅ **EXPECTED** |
| 2 | 0.622375 | `managedlustre_fs_subnetsize_ask` | ❌ |
| 3 | 0.542768 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.515635 | `managedlustre_fs_create` | ❌ |
| 5 | 0.480709 | `managedlustre_fs_list` | ❌ |

---

## Test 287

**Expected Tool:** `managedlustre_fs_update`  
**Prompt:** Update the maintenance window of the Azure Managed Lustre filesystem <filesystem_name> to <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739000 | `managedlustre_fs_update` | ✅ **EXPECTED** |
| 2 | 0.527554 | `managedlustre_fs_create` | ❌ |
| 3 | 0.487193 | `managedlustre_fs_list` | ❌ |
| 4 | 0.385349 | `managedlustre_fs_sku_get` | ❌ |
| 5 | 0.358486 | `storagesync_service_update` | ❌ |

---

## Test 288

**Expected Tool:** `marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570145 | `marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.499184 | `marketplace_product_list` | ❌ |
| 3 | 0.353256 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.338943 | `foundry_resource_get` | ❌ |
| 5 | 0.333160 | `servicebus_topic_details` | ❌ |

---

## Test 289

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607917 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.443133 | `marketplace_product_get` | ❌ |
| 3 | 0.380880 | `foundry_models_list` | ❌ |
| 4 | 0.341327 | `search_service_list` | ❌ |
| 5 | 0.338190 | `foundry_threads_list` | ❌ |

---

## Test 290

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537726 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.385167 | `marketplace_product_get` | ❌ |
| 3 | 0.341241 | `foundry_models_list` | ❌ |
| 4 | 0.288006 | `redis_list` | ❌ |
| 5 | 0.260387 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 291

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646844 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.635385 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.586907 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.577676 | `get_bestpractices_ai_app` | ❌ |
| 5 | 0.531728 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 292

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600903 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.548531 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.541091 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.516852 | `deploy_plan_get` | ❌ |
| 5 | 0.516443 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 293

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625259 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.594305 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.530278 | `get_bestpractices_ai_app` | ❌ |
| 4 | 0.518643 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.465573 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 294

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624273 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.570463 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.522998 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.516112 | `get_bestpractices_ai_app` | ❌ |
| 5 | 0.493998 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 295

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581850 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.497350 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.495659 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.486878 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.474511 | `deploy_plan_get` | ❌ |

---

## Test 296

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610986 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.532767 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.487322 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.481527 | `get_bestpractices_ai_app` | ❌ |
| 5 | 0.458060 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 297

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557862 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.513258 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.505883 | `get_bestpractices_ai_app` | ❌ |
| 4 | 0.505123 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.483705 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 298

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582541 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.500289 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.472112 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.434631 | `get_bestpractices_ai_app` | ❌ |
| 5 | 0.433134 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 299

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** configure azure mcp in coding agent for my repo  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488855 | `deploy_plan_get` | ❌ |
| 2 | 0.460956 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.411825 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.390270 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.370266 | `azureterraformbestpractices_get` | ❌ |

---

## Test 300

**Expected Tool:** `get_bestpractices_ai_app`  
**Prompt:** Get best practices for building AI applications in Azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.631345 | `get_bestpractices_ai_app` | ✅ **EXPECTED** |
| 2 | 0.555579 | `get_bestpractices_get` | ❌ |
| 3 | 0.501213 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.480235 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.477592 | `cloudarchitect_design` | ❌ |

---

## Test 301

**Expected Tool:** `get_bestpractices_ai_app`  
**Prompt:** Show me the best practices for Microsoft Foundry agents code generation  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657782 | `foundry_agents_get-sdk-sample` | ❌ |
| 2 | 0.507311 | `foundry_agents_create` | ❌ |
| 3 | 0.483323 | `foundry_threads_list` | ❌ |
| 4 | 0.478446 | `get_bestpractices_ai_app` | ✅ **EXPECTED** |
| 5 | 0.471413 | `foundry_threads_create` | ❌ |

---

## Test 302

**Expected Tool:** `get_bestpractices_ai_app`  
**Prompt:** Get guidance for building agents with Microsoft Foundry  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.667712 | `foundry_agents_get-sdk-sample` | ❌ |
| 2 | 0.605036 | `foundry_agents_create` | ❌ |
| 3 | 0.512941 | `foundry_threads_create` | ❌ |
| 4 | 0.499185 | `foundry_agents_list` | ❌ |
| 5 | 0.498635 | `foundry_threads_list` | ❌ |

---

## Test 303

**Expected Tool:** `get_bestpractices_ai_app`  
**Prompt:** Create an AI app that helps me to manage travel queries.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.402696 | `get_bestpractices_ai_app` | ✅ **EXPECTED** |
| 2 | 0.314523 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.309426 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.297481 | `applens_resource_diagnose` | ❌ |
| 5 | 0.294646 | `cloudarchitect_design` | ❌ |

---

## Test 304

**Expected Tool:** `get_bestpractices_ai_app`  
**Prompt:** Create an AI app that helps me to manage travel queries in Microsoft Foundry  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476439 | `foundry_openai_create-completion` | ❌ |
| 2 | 0.473301 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.464656 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.442919 | `get_bestpractices_ai_app` | ✅ **EXPECTED** |
| 5 | 0.440000 | `foundry_agents_list` | ❌ |

---

## Test 305

**Expected Tool:** `monitor_activitylog_list`  
**Prompt:** List the activity logs of the last month for <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537893 | `monitor_activitylog_list` | ✅ **EXPECTED** |
| 2 | 0.506212 | `monitor_resource_log_query` | ❌ |
| 3 | 0.371728 | `monitor_workspace_log_query` | ❌ |
| 4 | 0.363798 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.344578 | `datadog_monitoredresources_list` | ❌ |

---

## Test 306

**Expected Tool:** `monitor_healthmodels_entity_get`  
**Prompt:** Show me the health status of entity <entity_id> using the health model <health_model_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660947 | `monitor_healthmodels_entity_get` | ✅ **EXPECTED** |
| 2 | 0.608988 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.351697 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.328321 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.283143 | `foundry_models_deployments_list` | ❌ |

---

## Test 307

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592660 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424141 | `monitor_metrics_query` | ❌ |
| 3 | 0.368319 | `bicepschema_get` | ❌ |
| 4 | 0.333167 | `foundry_resource_get` | ❌ |
| 5 | 0.332356 | `monitor_table_type_list` | ❌ |

---

## Test 308

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607588 | `storage_account_get` | ❌ |
| 2 | 0.587678 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.546670 | `storage_table_list` | ❌ |
| 4 | 0.544724 | `storage_blob_container_get` | ❌ |
| 5 | 0.495790 | `storage_blob_get` | ❌ |

---

## Test 309

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633139 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495513 | `monitor_metrics_query` | ❌ |
| 3 | 0.433945 | `monitor_resource_log_query` | ❌ |
| 4 | 0.392960 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.388750 | `bicepschema_get` | ❌ |

---

## Test 310

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

## Test 311

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557830 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.476671 | `monitor_resource_log_query` | ❌ |
| 3 | 0.460611 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.455904 | `quota_usage_check` | ❌ |
| 5 | 0.438174 | `monitor_metrics_definitions` | ❌ |

---

## Test 312

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461249 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.389970 | `monitor_metrics_definitions` | ❌ |
| 3 | 0.338557 | `monitor_resource_log_query` | ❌ |
| 4 | 0.334768 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.306338 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 313

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

## Test 314

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525585 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.406185 | `monitor_resource_log_query` | ❌ |
| 3 | 0.384470 | `monitor_metrics_definitions` | ❌ |
| 4 | 0.347723 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.330869 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 315

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480140 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.444779 | `monitor_resource_log_query` | ❌ |
| 3 | 0.388382 | `applens_resource_diagnose` | ❌ |
| 4 | 0.363411 | `quota_usage_check` | ❌ |
| 5 | 0.350076 | `resourcehealth_health-events_list` | ❌ |

---

## Test 316

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

## Test 317

**Expected Tool:** `monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.850698 | `monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725738 | `monitor_table_type_list` | ❌ |
| 3 | 0.645028 | `storage_table_list` | ❌ |
| 4 | 0.620445 | `monitor_workspace_list` | ❌ |
| 5 | 0.541893 | `kusto_table_list` | ❌ |

---

## Test 318

**Expected Tool:** `monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798120 | `monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.701122 | `monitor_table_type_list` | ❌ |
| 3 | 0.599916 | `monitor_workspace_list` | ❌ |
| 4 | 0.571611 | `storage_table_list` | ❌ |
| 5 | 0.542820 | `monitor_workspace_log_query` | ❌ |

---

## Test 319

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881524 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.765523 | `monitor_table_list` | ❌ |
| 3 | 0.569921 | `monitor_workspace_list` | ❌ |
| 4 | 0.562068 | `storage_table_list` | ❌ |
| 5 | 0.504666 | `mysql_table_list` | ❌ |

---

## Test 320

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843139 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.736700 | `monitor_table_list` | ❌ |
| 3 | 0.576731 | `monitor_workspace_list` | ❌ |
| 4 | 0.516593 | `storage_table_list` | ❌ |
| 5 | 0.509598 | `monitor_workspace_log_query` | ❌ |

---

## Test 321

**Expected Tool:** `monitor_webtests_create`  
**Prompt:** Create a new Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650734 | `monitor_webtests_create` | ✅ **EXPECTED** |
| 2 | 0.570207 | `monitor_webtests_list` | ❌ |
| 3 | 0.550072 | `monitor_webtests_update` | ❌ |
| 4 | 0.533466 | `monitor_webtests_get` | ❌ |
| 5 | 0.482446 | `loadtesting_testresource_create` | ❌ |

---

## Test 322

**Expected Tool:** `monitor_webtests_get`  
**Prompt:** Get Web Test details for <webtest_resource_name> in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759087 | `monitor_webtests_get` | ✅ **EXPECTED** |
| 2 | 0.725528 | `monitor_webtests_list` | ❌ |
| 3 | 0.583790 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.562950 | `monitor_webtests_update` | ❌ |
| 5 | 0.530715 | `monitor_webtests_create` | ❌ |

---

## Test 323

**Expected Tool:** `monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.730616 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.610160 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.547708 | `grafana_list` | ❌ |
| 4 | 0.520828 | `redis_list` | ❌ |
| 5 | 0.496208 | `monitor_webtests_get` | ❌ |

---

## Test 324

**Expected Tool:** `monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793807 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.675965 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.584695 | `monitor_webtests_get` | ❌ |
| 4 | 0.573602 | `group_list` | ❌ |
| 5 | 0.546088 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 325

**Expected Tool:** `monitor_webtests_update`  
**Prompt:** Update an existing Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686390 | `monitor_webtests_update` | ✅ **EXPECTED** |
| 2 | 0.559148 | `monitor_webtests_get` | ❌ |
| 3 | 0.558148 | `monitor_webtests_create` | ❌ |
| 4 | 0.553533 | `monitor_webtests_list` | ❌ |
| 5 | 0.508766 | `loadtesting_testrun_update` | ❌ |

---

## Test 326

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813902 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.680201 | `grafana_list` | ❌ |
| 3 | 0.659468 | `monitor_table_list` | ❌ |
| 4 | 0.610623 | `kusto_cluster_list` | ❌ |
| 5 | 0.599717 | `search_service_list` | ❌ |

---

## Test 327

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656194 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.584708 | `monitor_table_list` | ❌ |
| 3 | 0.531083 | `monitor_table_type_list` | ❌ |
| 4 | 0.518254 | `grafana_list` | ❌ |
| 5 | 0.506772 | `monitor_workspace_log_query` | ❌ |

---

## Test 328

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732962 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.601481 | `grafana_list` | ❌ |
| 3 | 0.579635 | `monitor_table_list` | ❌ |
| 4 | 0.523782 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.522749 | `kusto_cluster_list` | ❌ |

---

## Test 329

**Expected Tool:** `monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610115 | `monitor_workspace_log_query` | ✅ **EXPECTED** |
| 2 | 0.587614 | `monitor_resource_log_query` | ❌ |
| 3 | 0.527733 | `monitor_activitylog_list` | ❌ |
| 4 | 0.498269 | `deploy_app_logs_get` | ❌ |
| 5 | 0.485456 | `monitor_table_list` | ❌ |

---

## Test 330

**Expected Tool:** `datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.668933 | `datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.454270 | `redis_list` | ❌ |
| 3 | 0.413661 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.413173 | `monitor_metrics_query` | ❌ |
| 5 | 0.401731 | `grafana_list` | ❌ |

---

## Test 331

**Expected Tool:** `datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624125 | `datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.443481 | `monitor_metrics_query` | ❌ |
| 3 | 0.440052 | `redis_list` | ❌ |
| 4 | 0.424391 | `monitor_resource_log_query` | ❌ |
| 5 | 0.385122 | `loadtesting_testresource_list` | ❌ |

---

## Test 332

**Expected Tool:** `extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533164 | `quota_usage_check` | ❌ |
| 2 | 0.481146 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.476762 | `extension_azqr` | ✅ **EXPECTED** |
| 4 | 0.471499 | `subscription_list` | ❌ |
| 5 | 0.468404 | `applens_resource_diagnose` | ❌ |

---

## Test 333

**Expected Tool:** `extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532788 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.492863 | `get_bestpractices_get` | ❌ |
| 3 | 0.476164 | `applicationinsights_recommendation_list` | ❌ |
| 4 | 0.473365 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.464604 | `cloudarchitect_design` | ❌ |

---

## Test 334

**Expected Tool:** `extension_azqr`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536934 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.516810 | `extension_azqr` | ✅ **EXPECTED** |
| 3 | 0.514978 | `applicationinsights_recommendation_list` | ❌ |
| 4 | 0.504673 | `quota_usage_check` | ❌ |
| 5 | 0.494872 | `deploy_plan_get` | ❌ |

---

## Test 335

**Expected Tool:** `quota_region_availability_list`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590964 | `quota_region_availability_list` | ✅ **EXPECTED** |
| 2 | 0.413274 | `quota_usage_check` | ❌ |
| 3 | 0.391332 | `redis_list` | ❌ |
| 4 | 0.372940 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.369855 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 336

**Expected Tool:** `quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609211 | `quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.491069 | `quota_region_availability_list` | ❌ |
| 3 | 0.386959 | `foundry_resource_get` | ❌ |
| 4 | 0.384335 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.376348 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 337

**Expected Tool:** `role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645214 | `role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.539760 | `subscription_list` | ❌ |
| 3 | 0.483988 | `group_list` | ❌ |
| 4 | 0.478700 | `grafana_list` | ❌ |
| 5 | 0.471367 | `cosmos_account_list` | ❌ |

---

## Test 338

**Expected Tool:** `role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609674 | `role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.514696 | `subscription_list` | ❌ |
| 3 | 0.456956 | `grafana_list` | ❌ |
| 4 | 0.449210 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.445149 | `redis_list` | ❌ |

---

## Test 339

**Expected Tool:** `redis_create`  
**Prompt:** Create a new Redis resource named <resource_name> with SKU <sku_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684993 | `redis_create` | ✅ **EXPECTED** |
| 2 | 0.507766 | `storagesync_service_create` | ❌ |
| 3 | 0.491883 | `redis_list` | ❌ |
| 4 | 0.489599 | `storage_account_create` | ❌ |
| 5 | 0.457201 | `workbooks_create` | ❌ |

---

## Test 340

**Expected Tool:** `redis_create`  
**Prompt:** Create a new Redis resource for me  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639083 | `redis_create` | ✅ **EXPECTED** |
| 2 | 0.479139 | `redis_list` | ❌ |
| 3 | 0.374515 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.321830 | `storagesync_service_create` | ❌ |
| 5 | 0.318545 | `loadtesting_testrun_create` | ❌ |

---

## Test 341

**Expected Tool:** `redis_create`  
**Prompt:** Create a Redis cache named <resource_name> with SKU <sku_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622359 | `redis_create` | ✅ **EXPECTED** |
| 2 | 0.476078 | `storage_account_create` | ❌ |
| 3 | 0.464158 | `redis_list` | ❌ |
| 4 | 0.459098 | `storagesync_service_create` | ❌ |
| 5 | 0.419243 | `eventhubs_namespace_update` | ❌ |

---

## Test 342

**Expected Tool:** `redis_create`  
**Prompt:** Create a new Redis cluster with name <resource_name>, SKU <sku_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595128 | `redis_create` | ✅ **EXPECTED** |
| 2 | 0.425864 | `redis_list` | ❌ |
| 3 | 0.403540 | `kusto_cluster_get` | ❌ |
| 4 | 0.383277 | `storagesync_service_create` | ❌ |
| 5 | 0.377190 | `eventhubs_namespace_update` | ❌ |

---

## Test 343

**Expected Tool:** `redis_list`  
**Prompt:** List all Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.810506 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.587858 | `grafana_list` | ❌ |
| 3 | 0.512988 | `kusto_cluster_list` | ❌ |
| 4 | 0.508561 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.501227 | `postgres_server_list` | ❌ |

---

## Test 344

**Expected Tool:** `redis_list`  
**Prompt:** Show me my Redis resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685128 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.470281 | `redis_create` | ❌ |
| 3 | 0.374327 | `grafana_list` | ❌ |
| 4 | 0.364255 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.359645 | `mysql_server_list` | ❌ |

---

## Test 345

**Expected Tool:** `redis_list`  
**Prompt:** Show me the Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.781228 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.539177 | `grafana_list` | ❌ |
| 3 | 0.519685 | `redis_create` | ❌ |
| 4 | 0.449258 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.449035 | `postgres_server_list` | ❌ |

---

## Test 346

**Expected Tool:** `redis_list`  
**Prompt:** Show me my Redis caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572767 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.379546 | `redis_create` | ❌ |
| 3 | 0.316566 | `mysql_database_list` | ❌ |
| 4 | 0.301662 | `postgres_database_list` | ❌ |
| 5 | 0.286518 | `mysql_server_list` | ❌ |

---

## Test 347

**Expected Tool:** `redis_list`  
**Prompt:** Get Redis clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478070 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.456308 | `kusto_cluster_list` | ❌ |
| 3 | 0.425849 | `redis_create` | ❌ |
| 4 | 0.384630 | `kusto_cluster_get` | ❌ |
| 5 | 0.359421 | `kusto_database_list` | ❌ |

---

## Test 348

**Expected Tool:** `group_list`  
**Prompt:** List all resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755935 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.566552 | `workbooks_list` | ❌ |
| 3 | 0.564566 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.552456 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.549477 | `monitor_webtests_list` | ❌ |

---

## Test 349

**Expected Tool:** `group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529504 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.464690 | `redis_list` | ❌ |
| 3 | 0.463589 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.462340 | `mysql_server_list` | ❌ |
| 5 | 0.460280 | `loadtesting_testresource_list` | ❌ |

---

## Test 350

**Expected Tool:** `group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665772 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.532505 | `redis_list` | ❌ |
| 3 | 0.532469 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.531947 | `eventgrid_topic_list` | ❌ |
| 5 | 0.531920 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 351

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.556622 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.538273 | `resourcehealth_availability-status_list` | ❌ |
| 3 | 0.377586 | `quota_usage_check` | ❌ |
| 4 | 0.373112 | `monitor_healthmodels_entity_get` | ❌ |
| 5 | 0.360140 | `foundry_resource_get` | ❌ |

---

## Test 352

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576591 | `storage_account_get` | ❌ |
| 2 | 0.564088 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.556167 | `storage_blob_container_get` | ❌ |
| 4 | 0.509114 | `storage_table_list` | ❌ |
| 5 | 0.487207 | `storage_blob_get` | ❌ |

---

## Test 353

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577398 | `resourcehealth_availability-status_list` | ❌ |
| 2 | 0.501257 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.424946 | `mysql_server_list` | ❌ |
| 4 | 0.412025 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.393479 | `managedlustre_fs_list` | ❌ |

---

## Test 354

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

## Test 355

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644982 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.544962 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.509740 | `resourcehealth_health-events_list` | ❌ |
| 4 | 0.508252 | `quota_usage_check` | ❌ |
| 5 | 0.505776 | `redis_list` | ❌ |

---

## Test 356

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.596890 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.549726 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.496640 | `resourcehealth_health-events_list` | ❌ |
| 4 | 0.441921 | `applens_resource_diagnose` | ❌ |
| 5 | 0.433614 | `loadtesting_testresource_list` | ❌ |

---

## Test 357

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690720 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.553542 | `search_service_list` | ❌ |
| 3 | 0.534400 | `eventgrid_topic_list` | ❌ |
| 4 | 0.529761 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.518372 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 358

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686448 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.534556 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.513418 | `eventgrid_topic_list` | ❌ |
| 4 | 0.513313 | `search_service_list` | ❌ |
| 5 | 0.501135 | `subscription_list` | ❌ |

---

## Test 359

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** What service issues have occurred in the last 30 days?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.450841 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.267663 | `applens_resource_diagnose` | ❌ |
| 3 | 0.245720 | `cloudarchitect_design` | ❌ |
| 4 | 0.216847 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.211092 | `search_service_list` | ❌ |

---

## Test 360

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** List active service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685391 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.527905 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.524195 | `eventgrid_topic_list` | ❌ |
| 4 | 0.518714 | `search_service_list` | ❌ |
| 5 | 0.502064 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 361

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565851 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.436320 | `search_service_list` | ❌ |
| 3 | 0.403665 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.402493 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.402208 | `storagesync_service_get` | ❌ |

---

## Test 362

**Expected Tool:** `servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642876 | `servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.460932 | `servicebus_topic_subscription_details` | ❌ |
| 3 | 0.436980 | `servicebus_topic_details` | ❌ |
| 4 | 0.424926 | `storagesync_service_get` | ❌ |
| 5 | 0.385812 | `search_knowledge_base_get` | ❌ |

---

## Test 363

**Expected Tool:** `servicebus_topic_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642952 | `servicebus_topic_details` | ✅ **EXPECTED** |
| 2 | 0.571860 | `servicebus_topic_subscription_details` | ❌ |
| 3 | 0.483976 | `servicebus_queue_details` | ❌ |
| 4 | 0.483108 | `eventgrid_topic_list` | ❌ |
| 5 | 0.458712 | `eventgrid_subscription_list` | ❌ |

---

## Test 364

**Expected Tool:** `servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633148 | `servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.517617 | `servicebus_topic_details` | ❌ |
| 3 | 0.494504 | `servicebus_queue_details` | ❌ |
| 4 | 0.493980 | `eventgrid_topic_list` | ❌ |
| 5 | 0.475089 | `storagesync_service_get` | ❌ |

---

## Test 365

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show me the details of SignalR <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532544 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.355028 | `redis_list` | ❌ |
| 3 | 0.345880 | `storagesync_service_get` | ❌ |
| 4 | 0.336067 | `foundry_resource_get` | ❌ |
| 5 | 0.319981 | `sql_server_show` | ❌ |

---

## Test 366

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show me the network information of SignalR runtime <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573400 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.337313 | `sql_server_show` | ❌ |
| 3 | 0.323029 | `foundry_resource_get` | ❌ |
| 4 | 0.318184 | `storagesync_service_get` | ❌ |
| 5 | 0.308008 | `storagesync_serverendpoint_get` | ❌ |

---

## Test 367

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Describe the SignalR runtime <signalr_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710353 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.422040 | `storagesync_service_create` | ❌ |
| 3 | 0.419280 | `foundry_resource_get` | ❌ |
| 4 | 0.411396 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.406031 | `storagesync_service_get` | ❌ |

---

## Test 368

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Get information about my SignalR runtime <signalr_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.716012 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.469341 | `foundry_resource_get` | ❌ |
| 3 | 0.447229 | `storagesync_service_get` | ❌ |
| 4 | 0.430824 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.430699 | `loadtesting_testresource_list` | ❌ |

---

## Test 369

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show all the SignalRs information in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564071 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.501077 | `redis_list` | ❌ |
| 3 | 0.494478 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.481428 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.462067 | `mysql_server_list` | ❌ |

---

## Test 370

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** List all SignalRs in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530646 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.507647 | `postgres_server_list` | ❌ |
| 3 | 0.495157 | `redis_list` | ❌ |
| 4 | 0.494498 | `kusto_cluster_list` | ❌ |
| 5 | 0.487856 | `subscription_list` | ❌ |

---

## Test 371

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a new SQL database named <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.514763 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.471080 | `sql_server_create` | ❌ |
| 3 | 0.420504 | `sql_db_rename` | ❌ |
| 4 | 0.408515 | `sql_db_delete` | ❌ |
| 5 | 0.404860 | `sql_server_delete` | ❌ |

---

## Test 372

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a SQL database <database_name> with Basic tier in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570143 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.459849 | `sql_server_create` | ❌ |
| 3 | 0.437525 | `sql_server_delete` | ❌ |
| 4 | 0.420843 | `sql_db_show` | ❌ |
| 5 | 0.417661 | `sql_db_delete` | ❌ |

---

## Test 373

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603490 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.546055 | `sql_server_create` | ❌ |
| 3 | 0.504013 | `sql_db_rename` | ❌ |
| 4 | 0.496146 | `storagesync_service_create` | ❌ |
| 5 | 0.494377 | `sql_db_show` | ❌ |

---

## Test 374

**Expected Tool:** `sql_db_delete`  
**Prompt:** Delete the SQL database <database_name> from server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.568196 | `sql_db_delete` | ✅ **EXPECTED** |
| 2 | 0.567412 | `sql_server_delete` | ❌ |
| 3 | 0.391509 | `sql_db_rename` | ❌ |
| 4 | 0.386564 | `sql_server_firewall-rule_delete` | ❌ |
| 5 | 0.381339 | `storagesync_serverendpoint_delete` | ❌ |

---

## Test 375

**Expected Tool:** `sql_db_delete`  
**Prompt:** Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567560 | `sql_server_delete` | ❌ |
| 2 | 0.543465 | `sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.500771 | `sql_db_show` | ❌ |
| 4 | 0.481098 | `sql_db_rename` | ❌ |
| 5 | 0.478705 | `sql_db_list` | ❌ |

---

## Test 376

**Expected Tool:** `sql_db_delete`  
**Prompt:** Delete the database called <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509916 | `sql_db_delete` | ✅ **EXPECTED** |
| 2 | 0.490893 | `sql_server_delete` | ❌ |
| 3 | 0.364416 | `postgres_database_list` | ❌ |
| 4 | 0.359040 | `storagesync_serverendpoint_delete` | ❌ |
| 5 | 0.354801 | `mysql_database_list` | ❌ |

---

## Test 377

**Expected Tool:** `sql_db_list`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643186 | `sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.639228 | `mysql_database_list` | ❌ |
| 3 | 0.609125 | `postgres_database_list` | ❌ |
| 4 | 0.602889 | `cosmos_database_list` | ❌ |
| 5 | 0.569977 | `kusto_database_list` | ❌ |

---

## Test 378

**Expected Tool:** `sql_db_list`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.617746 | `sql_server_show` | ❌ |
| 2 | 0.609322 | `sql_db_list` | ✅ **EXPECTED** |
| 3 | 0.557166 | `mysql_database_list` | ❌ |
| 4 | 0.554080 | `mysql_server_config_get` | ❌ |
| 5 | 0.524274 | `sql_db_show` | ❌ |

---

## Test 379

**Expected Tool:** `sql_db_rename`  
**Prompt:** Rename the SQL database <database_name> on server <server_name> to <new_database_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593348 | `sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.425282 | `sql_server_delete` | ❌ |
| 3 | 0.416207 | `sql_db_delete` | ❌ |
| 4 | 0.395295 | `sql_db_create` | ❌ |
| 5 | 0.390545 | `storagesync_registeredserver_update` | ❌ |

---

## Test 380

**Expected Tool:** `sql_db_rename`  
**Prompt:** Rename my Azure SQL database <database_name> to <new_database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711063 | `sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.516485 | `sql_server_delete` | ❌ |
| 3 | 0.506499 | `sql_db_delete` | ❌ |
| 4 | 0.500344 | `sql_db_create` | ❌ |
| 5 | 0.433898 | `sql_server_show` | ❌ |

---

## Test 381

**Expected Tool:** `sql_db_show`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610991 | `sql_server_show` | ❌ |
| 2 | 0.593150 | `postgres_server_config_get` | ❌ |
| 3 | 0.530858 | `mysql_server_config_get` | ❌ |
| 4 | 0.528136 | `sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.465693 | `sql_db_list` | ❌ |

---

## Test 382

**Expected Tool:** `sql_db_show`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530095 | `sql_db_show` | ✅ **EXPECTED** |
| 2 | 0.503681 | `sql_server_show` | ❌ |
| 3 | 0.440073 | `sql_db_list` | ❌ |
| 4 | 0.438622 | `mysql_table_schema_get` | ❌ |
| 5 | 0.433017 | `mysql_database_list` | ❌ |

---

## Test 383

**Expected Tool:** `sql_db_update`  
**Prompt:** Update the performance tier of SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603366 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.466244 | `sql_db_create` | ❌ |
| 3 | 0.440493 | `sql_db_rename` | ❌ |
| 4 | 0.427621 | `sql_db_show` | ❌ |
| 5 | 0.413941 | `sql_server_delete` | ❌ |

---

## Test 384

**Expected Tool:** `sql_db_update`  
**Prompt:** Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550457 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.418426 | `sql_server_delete` | ❌ |
| 3 | 0.401804 | `sql_db_list` | ❌ |
| 4 | 0.395499 | `sql_db_rename` | ❌ |
| 5 | 0.394808 | `sql_db_show` | ❌ |

---

## Test 385

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678055 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502376 | `sql_db_list` | ❌ |
| 3 | 0.498083 | `mysql_database_list` | ❌ |
| 4 | 0.485233 | `aks_nodepool_get` | ❌ |
| 5 | 0.479044 | `sql_server_show` | ❌ |

---

## Test 386

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606339 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502877 | `sql_server_show` | ❌ |
| 3 | 0.457164 | `sql_db_list` | ❌ |
| 4 | 0.450761 | `aks_nodepool_get` | ❌ |
| 5 | 0.432725 | `mysql_database_list` | ❌ |

---

## Test 387

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592641 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.420282 | `mysql_database_list` | ❌ |
| 3 | 0.407160 | `aks_nodepool_get` | ❌ |
| 4 | 0.402592 | `mysql_server_list` | ❌ |
| 5 | 0.397670 | `sql_db_list` | ❌ |

---

## Test 388

**Expected Tool:** `sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682781 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.562721 | `sql_db_create` | ❌ |
| 3 | 0.551614 | `storagesync_service_create` | ❌ |
| 4 | 0.529198 | `sql_server_list` | ❌ |
| 5 | 0.482102 | `storage_account_create` | ❌ |

---

## Test 389

**Expected Tool:** `sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618495 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.509476 | `sql_db_create` | ❌ |
| 3 | 0.472463 | `sql_server_show` | ❌ |
| 4 | 0.441174 | `sql_server_delete` | ❌ |
| 5 | 0.417881 | `redis_create` | ❌ |

---

## Test 390

**Expected Tool:** `sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589956 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.519462 | `storagesync_service_create` | ❌ |
| 3 | 0.499949 | `sql_db_create` | ❌ |
| 4 | 0.497890 | `sql_server_list` | ❌ |
| 5 | 0.461181 | `sql_db_rename` | ❌ |

---

## Test 391

**Expected Tool:** `sql_server_delete`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656593 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.548064 | `sql_db_delete` | ❌ |
| 3 | 0.534639 | `storagesync_service_delete` | ❌ |
| 4 | 0.518037 | `sql_server_list` | ❌ |
| 5 | 0.495702 | `sql_server_create` | ❌ |

---

## Test 392

**Expected Tool:** `sql_server_delete`  
**Prompt:** Remove the SQL server <server_name> from my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615073 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.495735 | `storagesync_registeredserver_unregister` | ❌ |
| 3 | 0.407936 | `storagesync_serverendpoint_delete` | ❌ |
| 4 | 0.393924 | `postgres_server_list` | ❌ |
| 5 | 0.379760 | `sql_db_delete` | ❌ |

---

## Test 393

**Expected Tool:** `sql_server_delete`  
**Prompt:** Delete SQL server <server_name> permanently  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624310 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.454892 | `sql_db_delete` | ❌ |
| 3 | 0.406951 | `storagesync_registeredserver_unregister` | ❌ |
| 4 | 0.390823 | `storagesync_service_delete` | ❌ |
| 5 | 0.389258 | `storagesync_serverendpoint_delete` | ❌ |

---

## Test 394

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

## Test 395

**Expected Tool:** `sql_server_entra-admin_list`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713306 | `sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.413144 | `sql_server_show` | ❌ |
| 3 | 0.368082 | `sql_server_list` | ❌ |
| 4 | 0.325584 | `storagesync_registeredserver_get` | ❌ |
| 5 | 0.315966 | `sql_db_list` | ❌ |

---

## Test 396

**Expected Tool:** `sql_server_entra-admin_list`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646419 | `sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.356025 | `sql_server_show` | ❌ |
| 3 | 0.322155 | `sql_server_list` | ❌ |
| 4 | 0.307880 | `sql_server_create` | ❌ |
| 5 | 0.269788 | `sql_server_delete` | ❌ |

---

## Test 397

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.635467 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.532712 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.522184 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.448936 | `sql_server_create` | ❌ |
| 5 | 0.440845 | `sql_server_delete` | ❌ |

---

## Test 398

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670208 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533503 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503709 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.316578 | `sql_server_list` | ❌ |
| 5 | 0.302214 | `sql_server_delete` | ❌ |

---

## Test 399

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685062 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.574384 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539567 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.429042 | `sql_server_create` | ❌ |
| 5 | 0.393504 | `sql_db_create` | ❌ |

---

## Test 400

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

## Test 401

**Expected Tool:** `sql_server_firewall-rule_delete`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670179 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.574340 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.530419 | `sql_server_firewall-rule_create` | ❌ |
| 4 | 0.488418 | `sql_server_delete` | ❌ |
| 5 | 0.360381 | `sql_db_delete` | ❌ |

---

## Test 402

**Expected Tool:** `sql_server_firewall-rule_delete`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671211 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.601231 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.577330 | `sql_server_firewall-rule_create` | ❌ |
| 4 | 0.499272 | `sql_server_delete` | ❌ |
| 5 | 0.378586 | `sql_db_delete` | ❌ |

---

## Test 403

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729372 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.549667 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.513114 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.468812 | `sql_server_show` | ❌ |
| 5 | 0.418817 | `sql_server_list` | ❌ |

---

## Test 404

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630731 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.524126 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.476757 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.410680 | `sql_server_show` | ❌ |
| 5 | 0.348100 | `sql_server_list` | ❌ |

---

## Test 405

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630546 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.532454 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.473501 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.412957 | `sql_server_show` | ❌ |
| 5 | 0.350513 | `sql_server_list` | ❌ |

---

## Test 406

**Expected Tool:** `sql_server_list`  
**Prompt:** List all Azure SQL servers in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694404 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.596675 | `mysql_server_list` | ❌ |
| 3 | 0.578238 | `sql_db_list` | ❌ |
| 4 | 0.515809 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.509789 | `sql_db_show` | ❌ |

---

## Test 407

**Expected Tool:** `sql_server_list`  
**Prompt:** Show me every SQL server available in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618218 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.593804 | `mysql_server_list` | ❌ |
| 3 | 0.542398 | `sql_db_list` | ❌ |
| 4 | 0.507404 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.496200 | `group_list` | ❌ |

---

## Test 408

**Expected Tool:** `sql_server_show`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629672 | `sql_db_show` | ❌ |
| 2 | 0.595184 | `sql_server_show` | ✅ **EXPECTED** |
| 3 | 0.587728 | `sql_server_list` | ❌ |
| 4 | 0.559879 | `mysql_server_list` | ❌ |
| 5 | 0.540218 | `sql_db_list` | ❌ |

---

## Test 409

**Expected Tool:** `sql_server_show`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658817 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.610507 | `postgres_server_config_get` | ❌ |
| 3 | 0.538466 | `mysql_server_config_get` | ❌ |
| 4 | 0.471541 | `sql_db_show` | ❌ |
| 5 | 0.445430 | `postgres_server_param_get` | ❌ |

---

## Test 410

**Expected Tool:** `sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563130 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.424550 | `storagesync_registeredserver_get` | ❌ |
| 3 | 0.395151 | `storagesync_registeredserver_update` | ❌ |
| 4 | 0.392481 | `postgres_server_config_get` | ❌ |
| 5 | 0.379988 | `postgres_server_param_get` | ❌ |

---

## Test 411

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533552 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.438046 | `storage_blob_container_create` | ❌ |
| 3 | 0.418191 | `storage_account_get` | ❌ |
| 4 | 0.414518 | `storage_blob_container_get` | ❌ |
| 5 | 0.370654 | `managedlustre_fs_create` | ❌ |

---

## Test 412

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500638 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.482719 | `managedlustre_fs_create` | ❌ |
| 3 | 0.482690 | `storagesync_service_create` | ❌ |
| 4 | 0.407222 | `storage_account_get` | ❌ |
| 5 | 0.406804 | `storage_blob_container_create` | ❌ |

---

## Test 413

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589002 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.535296 | `managedlustre_fs_create` | ❌ |
| 3 | 0.509731 | `storage_blob_container_create` | ❌ |
| 4 | 0.497478 | `storagesync_service_create` | ❌ |
| 5 | 0.462519 | `storage_account_get` | ❌ |

---

## Test 414

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.673750 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.608256 | `storage_blob_container_get` | ❌ |
| 3 | 0.556457 | `storage_blob_get` | ❌ |
| 4 | 0.494746 | `storage_table_list` | ❌ |
| 5 | 0.483435 | `storage_account_create` | ❌ |

---

## Test 415

**Expected Tool:** `storage_account_get`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692687 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.577547 | `storage_blob_container_get` | ❌ |
| 3 | 0.529205 | `storage_blob_get` | ❌ |
| 4 | 0.518215 | `storage_account_create` | ❌ |
| 5 | 0.462024 | `storage_table_list` | ❌ |

---

## Test 416

**Expected Tool:** `storage_account_get`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649151 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.563911 | `storage_table_list` | ❌ |
| 3 | 0.556979 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.550110 | `storage_blob_container_get` | ❌ |
| 5 | 0.547557 | `subscription_list` | ❌ |

---

## Test 417

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.556860 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.482418 | `storage_blob_container_get` | ❌ |
| 3 | 0.478618 | `storage_table_list` | ❌ |
| 4 | 0.461284 | `managedlustre_fs_list` | ❌ |
| 5 | 0.426253 | `storagesync_service_get` | ❌ |

---

## Test 418

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619462 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.556436 | `storage_blob_container_get` | ❌ |
| 3 | 0.518229 | `storage_blob_get` | ❌ |
| 4 | 0.516371 | `storage_table_list` | ❌ |
| 5 | 0.473560 | `cosmos_account_list` | ❌ |

---

## Test 419

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649793 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.583896 | `storage_blob_container_get` | ❌ |
| 3 | 0.524779 | `storage_account_create` | ❌ |
| 4 | 0.496679 | `storage_blob_get` | ❌ |
| 5 | 0.447742 | `cosmos_database_container_list` | ❌ |

---

## Test 420

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682161 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.590160 | `storage_blob_container_get` | ❌ |
| 3 | 0.559264 | `storage_blob_get` | ❌ |
| 4 | 0.500625 | `storage_account_create` | ❌ |
| 5 | 0.420514 | `storage_account_get` | ❌ |

---

## Test 421

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625397 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.543503 | `storage_blob_container_get` | ❌ |
| 3 | 0.497804 | `storage_blob_get` | ❌ |
| 4 | 0.463198 | `storage_account_create` | ❌ |
| 5 | 0.435070 | `cosmos_database_container_list` | ❌ |

---

## Test 422

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701642 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.623681 | `storage_blob_get` | ❌ |
| 3 | 0.577921 | `storage_account_get` | ❌ |
| 4 | 0.549804 | `storage_blob_container_create` | ❌ |
| 5 | 0.523234 | `cosmos_database_container_list` | ❌ |

---

## Test 423

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712037 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.680802 | `storage_blob_get` | ❌ |
| 3 | 0.613885 | `cosmos_database_container_list` | ❌ |
| 4 | 0.598086 | `storage_table_list` | ❌ |
| 5 | 0.556319 | `storage_blob_container_create` | ❌ |

---

## Test 424

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713527 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.592323 | `cosmos_database_container_list` | ❌ |
| 3 | 0.586169 | `storage_blob_get` | ❌ |
| 4 | 0.580835 | `storage_table_list` | ❌ |
| 5 | 0.523322 | `storage_account_get` | ❌ |

---

## Test 425

**Expected Tool:** `storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.700972 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.646973 | `storage_blob_container_get` | ❌ |
| 3 | 0.541019 | `storage_blob_container_create` | ❌ |
| 4 | 0.527427 | `storage_account_get` | ❌ |
| 5 | 0.477900 | `cosmos_database_container_list` | ❌ |

---

## Test 426

**Expected Tool:** `storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.695024 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.631052 | `storage_blob_container_get` | ❌ |
| 3 | 0.589041 | `storage_blob_container_create` | ❌ |
| 4 | 0.580129 | `storage_account_get` | ❌ |
| 5 | 0.456882 | `storage_account_create` | ❌ |

---

## Test 427

**Expected Tool:** `storage_blob_get`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.733586 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.700891 | `storage_blob_container_get` | ❌ |
| 3 | 0.605993 | `storage_blob_container_create` | ❌ |
| 4 | 0.579018 | `cosmos_database_container_list` | ❌ |
| 5 | 0.561320 | `storage_table_list` | ❌ |

---

## Test 428

**Expected Tool:** `storage_blob_get`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.704426 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.664940 | `storage_blob_container_get` | ❌ |
| 3 | 0.561557 | `storage_blob_container_create` | ❌ |
| 4 | 0.533462 | `cosmos_database_container_list` | ❌ |
| 5 | 0.492652 | `storage_table_list` | ❌ |

---

## Test 429

**Expected Tool:** `storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566115 | `storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.525984 | `storage_blob_container_create` | ❌ |
| 3 | 0.517917 | `storage_blob_get` | ❌ |
| 4 | 0.473840 | `storage_blob_container_get` | ❌ |
| 5 | 0.382194 | `storage_account_create` | ❌ |

---

## Test 430

**Expected Tool:** `storage_table_list`  
**Prompt:** List all tables in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.867010 | `storage_table_list` | ✅ **EXPECTED** |
| 2 | 0.584789 | `storage_blob_container_get` | ❌ |
| 3 | 0.574314 | `monitor_table_list` | ❌ |
| 4 | 0.552988 | `mysql_table_list` | ❌ |
| 5 | 0.530551 | `kusto_table_list` | ❌ |

---

## Test 431

**Expected Tool:** `storage_table_list`  
**Prompt:** Show me the tables in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.803289 | `storage_table_list` | ✅ **EXPECTED** |
| 2 | 0.582290 | `storage_blob_container_get` | ❌ |
| 3 | 0.529801 | `storage_account_get` | ❌ |
| 4 | 0.521319 | `mysql_table_list` | ❌ |
| 5 | 0.521245 | `monitor_table_list` | ❌ |

---

## Test 432

**Expected Tool:** `storagesync_service_create`  
**Prompt:** Create a new Storage Sync Service named <service-name> in resource group <resource-group-name> at location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.764173 | `storagesync_service_create` | ✅ **EXPECTED** |
| 2 | 0.648844 | `storagesync_syncgroup_create` | ❌ |
| 3 | 0.647734 | `storagesync_service_get` | ❌ |
| 4 | 0.602176 | `storage_account_create` | ❌ |
| 5 | 0.598929 | `storagesync_service_update` | ❌ |

---

## Test 433

**Expected Tool:** `storagesync_service_delete`  
**Prompt:** Delete the Storage Sync Service <service-name> from resource group <resource-group-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769767 | `storagesync_service_delete` | ✅ **EXPECTED** |
| 2 | 0.690441 | `storagesync_syncgroup_delete` | ❌ |
| 3 | 0.639404 | `storagesync_service_create` | ❌ |
| 4 | 0.615988 | `storagesync_service_get` | ❌ |
| 5 | 0.573839 | `storagesync_registeredserver_unregister` | ❌ |

---

## Test 434

**Expected Tool:** `storagesync_service_get`  
**Prompt:** Get the details of Storage Sync Service <service-name> in resource group <resource-group-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755903 | `storagesync_service_get` | ✅ **EXPECTED** |
| 2 | 0.691442 | `storagesync_syncgroup_get` | ❌ |
| 3 | 0.669189 | `storagesync_service_create` | ❌ |
| 4 | 0.582225 | `storagesync_service_update` | ❌ |
| 5 | 0.564225 | `storagesync_service_delete` | ❌ |

---

## Test 435

**Expected Tool:** `storagesync_service_get`  
**Prompt:** List all Storage Sync Services in resource group <resource-group-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.709915 | `storagesync_service_get` | ✅ **EXPECTED** |
| 2 | 0.659816 | `storagesync_service_create` | ❌ |
| 3 | 0.632257 | `storagesync_syncgroup_get` | ❌ |
| 4 | 0.548477 | `storagesync_service_delete` | ❌ |
| 5 | 0.527054 | `storagesync_syncgroup_create` | ❌ |

---

## Test 436

**Expected Tool:** `storagesync_service_update`  
**Prompt:** Update Storage Sync Service <service-name> with new tags  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592857 | `storagesync_service_update` | ✅ **EXPECTED** |
| 2 | 0.474366 | `storagesync_service_get` | ❌ |
| 3 | 0.470338 | `storagesync_service_create` | ❌ |
| 4 | 0.467103 | `storagesync_service_delete` | ❌ |
| 5 | 0.446942 | `storagesync_registeredserver_unregister` | ❌ |

---

## Test 437

**Expected Tool:** `storagesync_registeredserver_get`  
**Prompt:** Get the details of registered server <server-name> in service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646356 | `storagesync_registeredserver_get` | ✅ **EXPECTED** |
| 2 | 0.529573 | `storagesync_registeredserver_update` | ❌ |
| 3 | 0.517582 | `sql_server_show` | ❌ |
| 4 | 0.446197 | `storagesync_serverendpoint_get` | ❌ |
| 5 | 0.425869 | `storagesync_registeredserver_unregister` | ❌ |

---

## Test 438

**Expected Tool:** `storagesync_registeredserver_get`  
**Prompt:** List all registered servers in service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643980 | `storagesync_registeredserver_get` | ✅ **EXPECTED** |
| 2 | 0.537441 | `postgres_server_list` | ❌ |
| 3 | 0.470818 | `storagesync_registeredserver_update` | ❌ |
| 4 | 0.451773 | `storagesync_serverendpoint_get` | ❌ |
| 5 | 0.444159 | `search_service_list` | ❌ |

---

## Test 439

**Expected Tool:** `storagesync_registeredserver_unregister`  
**Prompt:** Unregister server <server-name> from service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710240 | `storagesync_registeredserver_unregister` | ✅ **EXPECTED** |
| 2 | 0.463416 | `storagesync_registeredserver_update` | ❌ |
| 3 | 0.451024 | `storagesync_registeredserver_get` | ❌ |
| 4 | 0.440446 | `sql_server_delete` | ❌ |
| 5 | 0.405996 | `storagesync_serverendpoint_delete` | ❌ |

---

## Test 440

**Expected Tool:** `storagesync_registeredserver_update`  
**Prompt:** Update registered server <server-name> configuration in service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655852 | `storagesync_registeredserver_update` | ✅ **EXPECTED** |
| 2 | 0.484312 | `storagesync_serverendpoint_update` | ❌ |
| 3 | 0.438526 | `storagesync_registeredserver_get` | ❌ |
| 4 | 0.430657 | `mysql_server_param_set` | ❌ |
| 5 | 0.416635 | `storagesync_registeredserver_unregister` | ❌ |

---

## Test 441

**Expected Tool:** `storagesync_syncgroup_create`  
**Prompt:** Create a new sync group named <syncgroup-name> in service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.706482 | `storagesync_syncgroup_create` | ✅ **EXPECTED** |
| 2 | 0.620903 | `storagesync_syncgroup_get` | ❌ |
| 3 | 0.613434 | `storagesync_service_create` | ❌ |
| 4 | 0.533577 | `storagesync_syncgroup_delete` | ❌ |
| 5 | 0.463424 | `storagesync_serverendpoint_create` | ❌ |

---

## Test 442

**Expected Tool:** `storagesync_syncgroup_delete`  
**Prompt:** Delete the sync group <syncgroup-name> from service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710432 | `storagesync_syncgroup_delete` | ✅ **EXPECTED** |
| 2 | 0.616065 | `storagesync_serverendpoint_delete` | ❌ |
| 3 | 0.611600 | `storagesync_cloudendpoint_delete` | ❌ |
| 4 | 0.571036 | `storagesync_syncgroup_get` | ❌ |
| 5 | 0.557686 | `storagesync_service_delete` | ❌ |

---

## Test 443

**Expected Tool:** `storagesync_syncgroup_get`  
**Prompt:** Get the details of sync group <syncgroup-name> in service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.722755 | `storagesync_syncgroup_get` | ✅ **EXPECTED** |
| 2 | 0.568475 | `storagesync_service_get` | ❌ |
| 3 | 0.533609 | `storagesync_serverendpoint_get` | ❌ |
| 4 | 0.532683 | `storagesync_syncgroup_create` | ❌ |
| 5 | 0.524189 | `storagesync_syncgroup_delete` | ❌ |

---

## Test 444

**Expected Tool:** `storagesync_cloudendpoint_changedetection`  
**Prompt:** Trigger change detection on cloud endpoint <endpoint-name> in sync group <syncgroup-name> in service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.774483 | `storagesync_cloudendpoint_triggerchangedetection` | ❌ |
| 2 | 0.557928 | `storagesync_cloudendpoint_delete` | ❌ |
| 3 | 0.522043 | `storagesync_serverendpoint_get` | ❌ |
| 4 | 0.513200 | `storagesync_cloudendpoint_get` | ❌ |
| 5 | 0.509450 | `storagesync_serverendpoint_delete` | ❌ |

---

## Test 445

**Expected Tool:** `storagesync_cloudendpoint_create`  
**Prompt:** Create a new cloud endpoint named <endpoint-name> for Azure file share <share-name> in storage account <storage-account-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696788 | `storagesync_cloudendpoint_create` | ✅ **EXPECTED** |
| 2 | 0.628509 | `storagesync_cloudendpoint_get` | ❌ |
| 3 | 0.512189 | `storagesync_syncgroup_create` | ❌ |
| 4 | 0.476576 | `storage_account_create` | ❌ |
| 5 | 0.452275 | `managedlustre_fs_create` | ❌ |

---

## Test 446

**Expected Tool:** `storagesync_cloudendpoint_delete`  
**Prompt:** Delete the cloud endpoint <endpoint-name> from sync group <syncgroup-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.892267 | `storagesync_cloudendpoint_delete` | ✅ **EXPECTED** |
| 2 | 0.783649 | `storagesync_serverendpoint_delete` | ❌ |
| 3 | 0.695820 | `storagesync_syncgroup_delete` | ❌ |
| 4 | 0.612755 | `storagesync_cloudendpoint_get` | ❌ |
| 5 | 0.581681 | `storagesync_cloudendpoint_create` | ❌ |

---

## Test 447

**Expected Tool:** `storagesync_cloudendpoint_get`  
**Prompt:** Get the details of cloud endpoint <endpoint-name> in sync group <syncgroup-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.733337 | `storagesync_cloudendpoint_get` | ✅ **EXPECTED** |
| 2 | 0.722096 | `storagesync_serverendpoint_get` | ❌ |
| 3 | 0.677767 | `storagesync_cloudendpoint_delete` | ❌ |
| 4 | 0.614565 | `storagesync_serverendpoint_delete` | ❌ |
| 5 | 0.604450 | `storagesync_syncgroup_get` | ❌ |

---

## Test 448

**Expected Tool:** `storagesync_cloudendpoint_get`  
**Prompt:** List all cloud endpoints in sync group <syncgroup-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.717514 | `storagesync_cloudendpoint_get` | ✅ **EXPECTED** |
| 2 | 0.700162 | `storagesync_serverendpoint_get` | ❌ |
| 3 | 0.650925 | `storagesync_cloudendpoint_delete` | ❌ |
| 4 | 0.575774 | `storagesync_syncgroup_get` | ❌ |
| 5 | 0.569566 | `storagesync_serverendpoint_delete` | ❌ |

---

## Test 449

**Expected Tool:** `storagesync_serverendpoint_create`  
**Prompt:** Create a new server endpoint on server <server-name> pointing to local path <local-path> in sync group <syncgroup-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.808800 | `storagesync_serverendpoint_create` | ✅ **EXPECTED** |
| 2 | 0.644260 | `storagesync_serverendpoint_delete` | ❌ |
| 3 | 0.635204 | `storagesync_serverendpoint_get` | ❌ |
| 4 | 0.606998 | `storagesync_syncgroup_create` | ❌ |
| 5 | 0.558866 | `storagesync_cloudendpoint_create` | ❌ |

---

## Test 450

**Expected Tool:** `storagesync_serverendpoint_delete`  
**Prompt:** Delete the server endpoint <endpoint-name> from sync group <syncgroup-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879845 | `storagesync_serverendpoint_delete` | ✅ **EXPECTED** |
| 2 | 0.804262 | `storagesync_cloudendpoint_delete` | ❌ |
| 3 | 0.698278 | `storagesync_syncgroup_delete` | ❌ |
| 4 | 0.618990 | `storagesync_serverendpoint_get` | ❌ |
| 5 | 0.592153 | `storagesync_serverendpoint_create` | ❌ |

---

## Test 451

**Expected Tool:** `storagesync_serverendpoint_get`  
**Prompt:** Get the details of server endpoint <endpoint-name> in sync group <syncgroup-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.764539 | `storagesync_serverendpoint_get` | ✅ **EXPECTED** |
| 2 | 0.681634 | `storagesync_serverendpoint_delete` | ❌ |
| 3 | 0.640096 | `storagesync_cloudendpoint_get` | ❌ |
| 4 | 0.604306 | `storagesync_serverendpoint_create` | ❌ |
| 5 | 0.588022 | `storagesync_cloudendpoint_delete` | ❌ |

---

## Test 452

**Expected Tool:** `storagesync_serverendpoint_get`  
**Prompt:** List all server endpoints in sync group <syncgroup-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739939 | `storagesync_serverendpoint_get` | ✅ **EXPECTED** |
| 2 | 0.649644 | `storagesync_serverendpoint_delete` | ❌ |
| 3 | 0.581866 | `storagesync_cloudendpoint_get` | ❌ |
| 4 | 0.576332 | `storagesync_serverendpoint_create` | ❌ |
| 5 | 0.549697 | `storagesync_syncgroup_delete` | ❌ |

---

## Test 453

**Expected Tool:** `storagesync_serverendpoint_update`  
**Prompt:** Update server endpoint <endpoint-name> with cloud tiering enabled and tiering policy in sync group <syncgroup-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682409 | `storagesync_serverendpoint_create` | ❌ |
| 2 | 0.633528 | `storagesync_serverendpoint_get` | ❌ |
| 3 | 0.581526 | `storagesync_serverendpoint_delete` | ❌ |
| 4 | 0.543031 | `storagesync_cloudendpoint_delete` | ❌ |
| 5 | 0.530706 | `storagesync_cloudendpoint_get` | ❌ |

---

## Test 454

**Expected Tool:** `subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654071 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512901 | `cosmos_account_list` | ❌ |
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
| 3 | 0.393822 | `eventgrid_topic_list` | ❌ |
| 4 | 0.391555 | `redis_list` | ❌ |
| 5 | 0.381216 | `postgres_server_list` | ❌ |

---

## Test 456

**Expected Tool:** `subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433196 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.319579 | `marketplace_product_list` | ❌ |
| 3 | 0.315547 | `marketplace_product_get` | ❌ |
| 4 | 0.293772 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.289509 | `eventgrid_topic_list` | ❌ |

---

## Test 457

**Expected Tool:** `subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.477194 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.357944 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.354423 | `marketplace_product_list` | ❌ |
| 4 | 0.344863 | `redis_list` | ❌ |
| 5 | 0.341510 | `eventgrid_topic_list` | ❌ |

---

## Test 458

**Expected Tool:** `azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686887 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.625256 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.605164 | `get_bestpractices_get` | ❌ |
| 4 | 0.482958 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.473048 | `get_bestpractices_ai_app` | ❌ |

---

## Test 459

**Expected Tool:** `azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581274 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.512141 | `get_bestpractices_get` | ❌ |
| 3 | 0.510005 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.473597 | `keyvault_secret_get` | ❌ |
| 5 | 0.453190 | `get_bestpractices_ai_app` | ❌ |

---

## Test 460

**Expected Tool:** `virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711969 | `virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
| 2 | 0.659649 | `virtualdesktop_hostpool_host_list` | ❌ |
| 3 | 0.620665 | `kusto_cluster_list` | ❌ |
| 4 | 0.546780 | `search_service_list` | ❌ |
| 5 | 0.535777 | `virtualdesktop_hostpool_host_user-list` | ❌ |

---

## Test 461

**Expected Tool:** `virtualdesktop_hostpool_host_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.727020 | `virtualdesktop_hostpool_host_list` | ✅ **EXPECTED** |
| 2 | 0.714552 | `virtualdesktop_hostpool_host_user-list` | ❌ |
| 3 | 0.573352 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.438687 | `aks_nodepool_get` | ❌ |
| 5 | 0.393699 | `sql_elastic-pool_list` | ❌ |

---

## Test 462

**Expected Tool:** `virtualdesktop_hostpool_host_user-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812787 | `virtualdesktop_hostpool_host_user-list` | ✅ **EXPECTED** |
| 2 | 0.659164 | `virtualdesktop_hostpool_host_list` | ❌ |
| 3 | 0.501168 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.357614 | `aks_nodepool_get` | ❌ |
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
| 1 | 0.772430 | `workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.562485 | `workbooks_create` | ❌ |
| 3 | 0.516739 | `grafana_list` | ❌ |
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
| 2 | 0.570260 | `workbooks_create` | ❌ |
| 3 | 0.499716 | `workbooks_show` | ❌ |
| 4 | 0.485504 | `workbooks_delete` | ❌ |
| 5 | 0.472378 | `grafana_list` | ❌ |

---

## Test 467

**Expected Tool:** `workbooks_show`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686034 | `workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.498345 | `workbooks_create` | ❌ |
| 3 | 0.494637 | `workbooks_list` | ❌ |
| 4 | 0.463154 | `workbooks_update` | ❌ |
| 5 | 0.452341 | `workbooks_delete` | ❌ |

---

## Test 468

**Expected Tool:** `workbooks_show`  
**Prompt:** Show me the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581501 | `workbooks_show` | ✅ **EXPECTED** |
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
| 4 | 0.347944 | `workbooks_show` | ❌ |
| 5 | 0.292870 | `loadtesting_testrun_update` | ❌ |

---

## Test 470

**Expected Tool:** `bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543154 | `bicepschema_get` | ✅ **EXPECTED** |
| 2 | 0.485889 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.478288 | `foundry_models_deploy` | ❌ |
| 4 | 0.466635 | `get_bestpractices_ai_app` | ❌ |
| 5 | 0.448373 | `get_bestpractices_get` | ❌ |

---

## Test 471

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502125 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.290788 | `storage_blob_upload` | ❌ |
| 3 | 0.259212 | `managedlustre_fs_create` | ❌ |
| 4 | 0.254656 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.254119 | `storagesync_cloudendpoint_triggerchangedetection` | ❌ |

---

## Test 472

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Help me design an Azure cloud service that will serve as an ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.508153 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.377543 | `deploy_architecture_diagram_generate` | ❌ |
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
| 1 | 0.423577 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.271943 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.265274 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.242581 | `deploy_plan_get` | ❌ |
| 5 | 0.229074 | `extension_cli_generate` | ❌ |

---

## Test 474

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.535092 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.369407 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.357157 | `storagesync_service_create` | ❌ |
| 4 | 0.356788 | `managedlustre_fs_create` | ❌ |
| 5 | 0.352848 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 475

**Expected Tool:** `foundry_agents_connect`  
**Prompt:** Query an agent in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653761 | `foundry_agents_connect` | ✅ **EXPECTED** |
| 2 | 0.593439 | `foundry_agents_get-sdk-sample` | ❌ |
| 3 | 0.587483 | `foundry_threads_list` | ❌ |
| 4 | 0.562854 | `foundry_agents_list` | ❌ |
| 5 | 0.553874 | `foundry_threads_get-messages` | ❌ |

---

## Test 476

**Expected Tool:** `foundry_agents_create`  
**Prompt:** Create a new Microsoft Foundry agent using instructions in the active editor  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.651699 | `foundry_agents_create` | ✅ **EXPECTED** |
| 2 | 0.605400 | `foundry_threads_create` | ❌ |
| 3 | 0.592591 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.507379 | `foundry_threads_list` | ❌ |
| 5 | 0.450138 | `foundry_threads_get-messages` | ❌ |

---

## Test 477

**Expected Tool:** `foundry_agents_evaluate`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.544099 | `foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.497672 | `foundry_agents_connect` | ❌ |
| 3 | 0.469428 | `foundry_agents_evaluate` | ✅ **EXPECTED** |
| 4 | 0.283175 | `foundry_agents_list` | ❌ |
| 5 | 0.268553 | `foundry_threads_get-messages` | ❌ |

---

## Test 478

**Expected Tool:** `foundry_agents_get-sdk-sample`  
**Prompt:** Create a CLI app that can talk to a Microsoft Foundry Agent using Python SDK  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640177 | `foundry_agents_get-sdk-sample` | ✅ **EXPECTED** |
| 2 | 0.542657 | `foundry_threads_create` | ❌ |
| 3 | 0.542300 | `foundry_agents_create` | ❌ |
| 4 | 0.473155 | `foundry_agents_connect` | ❌ |
| 5 | 0.464149 | `foundry_threads_list` | ❌ |

---

## Test 479

**Expected Tool:** `foundry_agents_list`  
**Prompt:** List all agents in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.681576 | `foundry_threads_list` | ❌ |
| 2 | 0.681254 | `foundry_agents_list` | ✅ **EXPECTED** |
| 3 | 0.574383 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.565382 | `foundry_resource_get` | ❌ |
| 5 | 0.553438 | `foundry_threads_get-messages` | ❌ |

---

## Test 480

**Expected Tool:** `foundry_agents_list`  
**Prompt:** Show me the available agents in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657082 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.639619 | `foundry_threads_list` | ❌ |
| 3 | 0.613719 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.546827 | `foundry_resource_get` | ❌ |
| 5 | 0.538991 | `foundry_threads_get-messages` | ❌ |

---

## Test 481

**Expected Tool:** `foundry_agents_query-and-evaluate`  
**Prompt:** Query and evaluate an agent in my Microsoft Foundry resource for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623476 | `foundry_agents_connect` | ❌ |
| 2 | 0.585061 | `foundry_agents_query-and-evaluate` | ✅ **EXPECTED** |
| 3 | 0.508685 | `foundry_agents_evaluate` | ❌ |
| 4 | 0.502086 | `foundry_agents_list` | ❌ |
| 5 | 0.468929 | `foundry_agents_get-sdk-sample` | ❌ |

---

## Test 482

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my Microsoft Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.709443 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.549219 | `foundry_knowledge_index_schema` | ❌ |
| 3 | 0.499697 | `foundry_agents_list` | ❌ |
| 4 | 0.462128 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.449142 | `foundry_threads_list` | ❌ |

---

## Test 483

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** Show me the knowledge indexes in my Microsoft Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.597932 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.499788 | `foundry_knowledge_index_schema` | ❌ |
| 3 | 0.425795 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.417816 | `foundry_agents_list` | ❌ |
| 5 | 0.411250 | `foundry_resource_get` | ❌ |

---

## Test 484

**Expected Tool:** `foundry_knowledge_index_schema`  
**Prompt:** Show me the schema for knowledge index <index-name> in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.716936 | `foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.564618 | `foundry_knowledge_index_list` | ❌ |
| 3 | 0.442929 | `kusto_table_schema` | ❌ |
| 4 | 0.440366 | `foundry_resource_get` | ❌ |
| 5 | 0.439018 | `bicepschema_get` | ❌ |

---

## Test 485

**Expected Tool:** `foundry_knowledge_index_schema`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.652246 | `foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.432758 | `postgres_table_schema_get` | ❌ |
| 3 | 0.417421 | `kusto_table_schema` | ❌ |
| 4 | 0.398186 | `mysql_table_schema_get` | ❌ |
| 5 | 0.393541 | `search_knowledge_base_get` | ❌ |

---

## Test 486

**Expected Tool:** `foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565477 | `foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.310121 | `foundry_openai_models-list` | ❌ |
| 3 | 0.298490 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.293459 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.282406 | `mysql_server_list` | ❌ |

---

## Test 487

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** List all Microsoft Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672277 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.647107 | `foundry_openai_models-list` | ❌ |
| 3 | 0.590083 | `foundry_resource_get` | ❌ |
| 4 | 0.579375 | `foundry_threads_list` | ❌ |
| 5 | 0.566003 | `foundry_models_list` | ❌ |

---

## Test 488

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** Show me all Microsoft Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602765 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.582767 | `foundry_openai_models-list` | ❌ |
| 3 | 0.550358 | `foundry_resource_get` | ❌ |
| 4 | 0.532100 | `foundry_threads_list` | ❌ |
| 5 | 0.527865 | `foundry_models_deploy` | ❌ |

---

## Test 489

**Expected Tool:** `foundry_models_list`  
**Prompt:** List all Microsoft Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572716 | `foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.549981 | `foundry_threads_list` | ❌ |
| 3 | 0.519108 | `foundry_openai_models-list` | ❌ |
| 4 | 0.483640 | `foundry_resource_get` | ❌ |
| 5 | 0.478754 | `foundry_models_deployments_list` | ❌ |

---

## Test 490

**Expected Tool:** `foundry_models_list`  
**Prompt:** Show me the available Microsoft Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.524253 | `foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.480785 | `foundry_threads_list` | ❌ |
| 3 | 0.458633 | `foundry_openai_models-list` | ❌ |
| 4 | 0.450464 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.448242 | `foundry_resource_get` | ❌ |

---

## Test 491

**Expected Tool:** `foundry_openai_chat-completions-create`  
**Prompt:** Create a chat completion with the message "Hello, how are you today?" using my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571040 | `foundry_openai_chat-completions-create` | ✅ **EXPECTED** |
| 2 | 0.471693 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.405280 | `foundry_threads_create` | ❌ |
| 4 | 0.349571 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.346911 | `foundry_agents_create` | ❌ |

---

## Test 492

**Expected Tool:** `foundry_openai_create-completion`  
**Prompt:** Create a completion with the prompt "What is Azure?" using my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.652748 | `foundry_openai_create-completion` | ✅ **EXPECTED** |
| 2 | 0.527216 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.439706 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.434029 | `extension_cli_generate` | ❌ |
| 5 | 0.411037 | `foundry_models_deploy` | ❌ |

---

## Test 493

**Expected Tool:** `foundry_openai_embeddings-create`  
**Prompt:** Generate embeddings for the text "Azure OpenAI Service" using my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.751956 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.532528 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.532498 | `foundry_models_deploy` | ❌ |
| 4 | 0.521037 | `foundry_openai_models-list` | ❌ |
| 5 | 0.494233 | `foundry_resource_get` | ❌ |

---

## Test 494

**Expected Tool:** `foundry_openai_embeddings-create`  
**Prompt:** Create vector embeddings for my text using my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650480 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.425667 | `foundry_resource_get` | ❌ |
| 3 | 0.413294 | `foundry_models_deploy` | ❌ |
| 4 | 0.411815 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.405960 | `foundry_agents_create` | ❌ |

---

## Test 495

**Expected Tool:** `foundry_openai_models-list`  
**Prompt:** List all available OpenAI models in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.783808 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.642953 | `foundry_models_deployments_list` | ❌ |
| 3 | 0.634098 | `foundry_resource_get` | ❌ |
| 4 | 0.630646 | `foundry_agents_list` | ❌ |
| 5 | 0.622537 | `foundry_models_list` | ❌ |

---

## Test 496

**Expected Tool:** `foundry_openai_models-list`  
**Prompt:** Show me the OpenAI model deployments in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729842 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.655585 | `foundry_models_deploy` | ❌ |
| 3 | 0.639571 | `foundry_models_deployments_list` | ❌ |
| 4 | 0.617691 | `foundry_resource_get` | ❌ |
| 5 | 0.604207 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 497

**Expected Tool:** `foundry_resource_get`  
**Prompt:** List all Microsoft Foundry resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630611 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.596863 | `foundry_threads_list` | ❌ |
| 3 | 0.558609 | `foundry_openai_models-list` | ❌ |
| 4 | 0.542902 | `redis_list` | ❌ |
| 5 | 0.526981 | `foundry_agents_list` | ❌ |

---

## Test 498

**Expected Tool:** `foundry_resource_get`  
**Prompt:** Show me the Microsoft Foundry resources in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634884 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.527760 | `foundry_openai_models-list` | ❌ |
| 3 | 0.524809 | `foundry_threads_list` | ❌ |
| 4 | 0.488496 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.483183 | `foundry_agents_list` | ❌ |

---

## Test 499

**Expected Tool:** `foundry_resource_get`  
**Prompt:** Get details for Microsoft Foundry resource <resource_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728275 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.544746 | `foundry_openai_models-list` | ❌ |
| 3 | 0.506756 | `monitor_webtests_get` | ❌ |
| 4 | 0.481305 | `functionapp_get` | ❌ |
| 5 | 0.478900 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 500

**Expected Tool:** `foundry_threads_create`  
**Prompt:** Create a Microsoft Foundry thread to hold the conversation  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671596 | `foundry_threads_create` | ✅ **EXPECTED** |
| 2 | 0.551672 | `foundry_threads_get-messages` | ❌ |
| 3 | 0.545637 | `foundry_threads_list` | ❌ |
| 4 | 0.493505 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.487117 | `foundry_agents_create` | ❌ |

---

## Test 501

**Expected Tool:** `foundry_threads_get-messages`  
**Prompt:** Show me the messages in the Microsoft Foundry thread with id <thread_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662733 | `foundry_threads_get-messages` | ✅ **EXPECTED** |
| 2 | 0.553371 | `foundry_threads_create` | ❌ |
| 3 | 0.538595 | `foundry_threads_list` | ❌ |
| 4 | 0.419581 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.404242 | `foundry_agents_create` | ❌ |

---

## Test 502

**Expected Tool:** `foundry_threads_list`  
**Prompt:** List my Microsoft Foundry threads  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.720787 | `foundry_threads_list` | ✅ **EXPECTED** |
| 2 | 0.598835 | `foundry_threads_get-messages` | ❌ |
| 3 | 0.572987 | `foundry_threads_create` | ❌ |
| 4 | 0.479893 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.396690 | `foundry_resource_get` | ❌ |

---

## Summary

**Total Prompts Tested:** 502  
**Analysis Execution Time:** 104.5064622s  

### Success Rate Metrics

**Top Choice Success:** 92.0% (462/502 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 4.0% (20/502 tests)  
**🎯 High Confidence (≥0.7):** 23.9% (120/502 tests)  
**✅ Good Confidence (≥0.6):** 62.5% (314/502 tests)  
**👍 Fair Confidence (≥0.5):** 91.4% (459/502 tests)  
**👌 Acceptable Confidence (≥0.4):** 99.2% (498/502 tests)  
**❌ Low Confidence (<0.4):** 0.8% (4/502 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 4.0% (20/502 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 23.9% (120/502 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 60.6% (304/502 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 86.5% (434/502 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 92.0% (462/502 tests)  

### Success Rate Analysis

🟢 **Excellent** - The tool selection system is performing exceptionally well.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

