# Tool Selection Analysis Setup

**Setup completed:** 2025-11-25 11:10:49  
**Tool count:** 181  
**Database setup time:** 20.1270870s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-11-25 11:10:49  
**Tool count:** 181  

## Table of Contents

- [Test 1: azureaibestpractices_get](#test-1)
- [Test 2: azureaibestpractices_get](#test-2)
- [Test 3: azureaibestpractices_get](#test-3)
- [Test 4: azureaibestpractices_get](#test-4)
- [Test 5: azureaibestpractices_get](#test-5)
- [Test 6: search_knowledge_base_get](#test-6)
- [Test 7: search_knowledge_base_get](#test-7)
- [Test 8: search_knowledge_base_get](#test-8)
- [Test 9: search_knowledge_base_get](#test-9)
- [Test 10: search_knowledge_base_get](#test-10)
- [Test 11: search_knowledge_base_get](#test-11)
- [Test 12: search_knowledge_base_retrieve](#test-12)
- [Test 13: search_knowledge_base_retrieve](#test-13)
- [Test 14: search_knowledge_base_retrieve](#test-14)
- [Test 15: search_knowledge_base_retrieve](#test-15)
- [Test 16: search_knowledge_base_retrieve](#test-16)
- [Test 17: search_knowledge_base_retrieve](#test-17)
- [Test 18: search_knowledge_base_retrieve](#test-18)
- [Test 19: search_knowledge_base_retrieve](#test-19)
- [Test 20: search_knowledge_source_get](#test-20)
- [Test 21: search_knowledge_source_get](#test-21)
- [Test 22: search_knowledge_source_get](#test-22)
- [Test 23: search_knowledge_source_get](#test-23)
- [Test 24: search_knowledge_source_get](#test-24)
- [Test 25: search_knowledge_source_get](#test-25)
- [Test 26: search_index_get](#test-26)
- [Test 27: search_index_get](#test-27)
- [Test 28: search_index_get](#test-28)
- [Test 29: search_index_query](#test-29)
- [Test 30: search_service_list](#test-30)
- [Test 31: search_service_list](#test-31)
- [Test 32: search_service_list](#test-32)
- [Test 33: speech_stt_recognize](#test-33)
- [Test 34: speech_stt_recognize](#test-34)
- [Test 35: speech_stt_recognize](#test-35)
- [Test 36: speech_stt_recognize](#test-36)
- [Test 37: speech_stt_recognize](#test-37)
- [Test 38: speech_stt_recognize](#test-38)
- [Test 39: speech_stt_recognize](#test-39)
- [Test 40: speech_stt_recognize](#test-40)
- [Test 41: speech_stt_recognize](#test-41)
- [Test 42: speech_stt_recognize](#test-42)
- [Test 43: speech_tts_synthesize](#test-43)
- [Test 44: speech_tts_synthesize](#test-44)
- [Test 45: speech_tts_synthesize](#test-45)
- [Test 46: speech_tts_synthesize](#test-46)
- [Test 47: speech_tts_synthesize](#test-47)
- [Test 48: speech_tts_synthesize](#test-48)
- [Test 49: speech_tts_synthesize](#test-49)
- [Test 50: speech_tts_synthesize](#test-50)
- [Test 51: speech_tts_synthesize](#test-51)
- [Test 52: speech_tts_synthesize](#test-52)
- [Test 53: appconfig_account_list](#test-53)
- [Test 54: appconfig_account_list](#test-54)
- [Test 55: appconfig_account_list](#test-55)
- [Test 56: appconfig_kv_delete](#test-56)
- [Test 57: appconfig_kv_get](#test-57)
- [Test 58: appconfig_kv_get](#test-58)
- [Test 59: appconfig_kv_get](#test-59)
- [Test 60: appconfig_kv_get](#test-60)
- [Test 61: appconfig_kv_lock_set](#test-61)
- [Test 62: appconfig_kv_lock_set](#test-62)
- [Test 63: appconfig_kv_set](#test-63)
- [Test 64: applens_resource_diagnose](#test-64)
- [Test 65: applens_resource_diagnose](#test-65)
- [Test 66: applens_resource_diagnose](#test-66)
- [Test 67: appservice_database_add](#test-67)
- [Test 68: appservice_database_add](#test-68)
- [Test 69: appservice_database_add](#test-69)
- [Test 70: appservice_database_add](#test-70)
- [Test 71: appservice_database_add](#test-71)
- [Test 72: appservice_database_add](#test-72)
- [Test 73: appservice_database_add](#test-73)
- [Test 74: appservice_database_add](#test-74)
- [Test 75: appservice_database_add](#test-75)
- [Test 76: appservice_database_add](#test-76)
- [Test 77: applicationinsights_recommendation_list](#test-77)
- [Test 78: applicationinsights_recommendation_list](#test-78)
- [Test 79: applicationinsights_recommendation_list](#test-79)
- [Test 80: applicationinsights_recommendation_list](#test-80)
- [Test 81: extension_cli_generate](#test-81)
- [Test 82: extension_cli_generate](#test-82)
- [Test 83: extension_cli_generate](#test-83)
- [Test 84: extension_cli_install](#test-84)
- [Test 85: extension_cli_install](#test-85)
- [Test 86: extension_cli_install](#test-86)
- [Test 87: acr_registry_list](#test-87)
- [Test 88: acr_registry_list](#test-88)
- [Test 89: acr_registry_list](#test-89)
- [Test 90: acr_registry_list](#test-90)
- [Test 91: acr_registry_list](#test-91)
- [Test 92: acr_registry_repository_list](#test-92)
- [Test 93: acr_registry_repository_list](#test-93)
- [Test 94: acr_registry_repository_list](#test-94)
- [Test 95: acr_registry_repository_list](#test-95)
- [Test 96: communication_email_send](#test-96)
- [Test 97: communication_email_send](#test-97)
- [Test 98: communication_email_send](#test-98)
- [Test 99: communication_email_send](#test-99)
- [Test 100: communication_email_send](#test-100)
- [Test 101: communication_email_send](#test-101)
- [Test 102: communication_email_send](#test-102)
- [Test 103: communication_email_send](#test-103)
- [Test 104: communication_sms_send](#test-104)
- [Test 105: communication_sms_send](#test-105)
- [Test 106: communication_sms_send](#test-106)
- [Test 107: communication_sms_send](#test-107)
- [Test 108: communication_sms_send](#test-108)
- [Test 109: communication_sms_send](#test-109)
- [Test 110: communication_sms_send](#test-110)
- [Test 111: communication_sms_send](#test-111)
- [Test 112: confidentialledger_entries_append](#test-112)
- [Test 113: confidentialledger_entries_append](#test-113)
- [Test 114: confidentialledger_entries_append](#test-114)
- [Test 115: confidentialledger_entries_append](#test-115)
- [Test 116: confidentialledger_entries_append](#test-116)
- [Test 117: confidentialledger_entries_get](#test-117)
- [Test 118: confidentialledger_entries_get](#test-118)
- [Test 119: cosmos_account_list](#test-119)
- [Test 120: cosmos_account_list](#test-120)
- [Test 121: cosmos_account_list](#test-121)
- [Test 122: cosmos_database_container_item_query](#test-122)
- [Test 123: cosmos_database_container_list](#test-123)
- [Test 124: cosmos_database_container_list](#test-124)
- [Test 125: cosmos_database_list](#test-125)
- [Test 126: cosmos_database_list](#test-126)
- [Test 127: kusto_cluster_get](#test-127)
- [Test 128: kusto_cluster_list](#test-128)
- [Test 129: kusto_cluster_list](#test-129)
- [Test 130: kusto_cluster_list](#test-130)
- [Test 131: kusto_database_list](#test-131)
- [Test 132: kusto_database_list](#test-132)
- [Test 133: kusto_query](#test-133)
- [Test 134: kusto_sample](#test-134)
- [Test 135: kusto_table_list](#test-135)
- [Test 136: kusto_table_list](#test-136)
- [Test 137: kusto_table_schema](#test-137)
- [Test 138: mysql_database_list](#test-138)
- [Test 139: mysql_database_list](#test-139)
- [Test 140: mysql_database_query](#test-140)
- [Test 141: mysql_server_config_get](#test-141)
- [Test 142: mysql_server_list](#test-142)
- [Test 143: mysql_server_list](#test-143)
- [Test 144: mysql_server_list](#test-144)
- [Test 145: mysql_server_param_get](#test-145)
- [Test 146: mysql_server_param_set](#test-146)
- [Test 147: mysql_table_list](#test-147)
- [Test 148: mysql_table_list](#test-148)
- [Test 149: mysql_table_schema_get](#test-149)
- [Test 150: postgres_database_list](#test-150)
- [Test 151: postgres_database_list](#test-151)
- [Test 152: postgres_database_query](#test-152)
- [Test 153: postgres_server_config_get](#test-153)
- [Test 154: postgres_server_list](#test-154)
- [Test 155: postgres_server_list](#test-155)
- [Test 156: postgres_server_list](#test-156)
- [Test 157: postgres_server_param_get](#test-157)
- [Test 158: postgres_server_param_set](#test-158)
- [Test 159: postgres_table_list](#test-159)
- [Test 160: postgres_table_list](#test-160)
- [Test 161: postgres_table_schema_get](#test-161)
- [Test 162: deploy_app_logs_get](#test-162)
- [Test 163: deploy_architecture_diagram_generate](#test-163)
- [Test 164: deploy_iac_rules_get](#test-164)
- [Test 165: deploy_pipeline_guidance_get](#test-165)
- [Test 166: deploy_plan_get](#test-166)
- [Test 167: eventgrid_events_publish](#test-167)
- [Test 168: eventgrid_events_publish](#test-168)
- [Test 169: eventgrid_events_publish](#test-169)
- [Test 170: eventgrid_topic_list](#test-170)
- [Test 171: eventgrid_topic_list](#test-171)
- [Test 172: eventgrid_topic_list](#test-172)
- [Test 173: eventgrid_topic_list](#test-173)
- [Test 174: eventgrid_subscription_list](#test-174)
- [Test 175: eventgrid_subscription_list](#test-175)
- [Test 176: eventgrid_subscription_list](#test-176)
- [Test 177: eventgrid_subscription_list](#test-177)
- [Test 178: eventgrid_subscription_list](#test-178)
- [Test 179: eventgrid_subscription_list](#test-179)
- [Test 180: eventgrid_subscription_list](#test-180)
- [Test 181: eventhubs_eventhub_consumergroup_delete](#test-181)
- [Test 182: eventhubs_eventhub_consumergroup_get](#test-182)
- [Test 183: eventhubs_eventhub_consumergroup_get](#test-183)
- [Test 184: eventhubs_eventhub_consumergroup_update](#test-184)
- [Test 185: eventhubs_eventhub_consumergroup_update](#test-185)
- [Test 186: eventhubs_eventhub_delete](#test-186)
- [Test 187: eventhubs_eventhub_get](#test-187)
- [Test 188: eventhubs_eventhub_get](#test-188)
- [Test 189: eventhubs_eventhub_update](#test-189)
- [Test 190: eventhubs_eventhub_update](#test-190)
- [Test 191: eventhubs_namespace_delete](#test-191)
- [Test 192: eventhubs_namespace_get](#test-192)
- [Test 193: eventhubs_namespace_get](#test-193)
- [Test 194: eventhubs_namespace_update](#test-194)
- [Test 195: eventhubs_namespace_update](#test-195)
- [Test 196: functionapp_get](#test-196)
- [Test 197: functionapp_get](#test-197)
- [Test 198: functionapp_get](#test-198)
- [Test 199: functionapp_get](#test-199)
- [Test 200: functionapp_get](#test-200)
- [Test 201: functionapp_get](#test-201)
- [Test 202: functionapp_get](#test-202)
- [Test 203: functionapp_get](#test-203)
- [Test 204: functionapp_get](#test-204)
- [Test 205: functionapp_get](#test-205)
- [Test 206: functionapp_get](#test-206)
- [Test 207: functionapp_get](#test-207)
- [Test 208: keyvault_admin_settings_get](#test-208)
- [Test 209: keyvault_admin_settings_get](#test-209)
- [Test 210: keyvault_admin_settings_get](#test-210)
- [Test 211: keyvault_certificate_create](#test-211)
- [Test 212: keyvault_certificate_create](#test-212)
- [Test 213: keyvault_certificate_create](#test-213)
- [Test 214: keyvault_certificate_create](#test-214)
- [Test 215: keyvault_certificate_create](#test-215)
- [Test 216: keyvault_certificate_get](#test-216)
- [Test 217: keyvault_certificate_get](#test-217)
- [Test 218: keyvault_certificate_get](#test-218)
- [Test 219: keyvault_certificate_get](#test-219)
- [Test 220: keyvault_certificate_get](#test-220)
- [Test 221: keyvault_certificate_import](#test-221)
- [Test 222: keyvault_certificate_import](#test-222)
- [Test 223: keyvault_certificate_import](#test-223)
- [Test 224: keyvault_certificate_import](#test-224)
- [Test 225: keyvault_certificate_import](#test-225)
- [Test 226: keyvault_certificate_list](#test-226)
- [Test 227: keyvault_certificate_list](#test-227)
- [Test 228: keyvault_certificate_list](#test-228)
- [Test 229: keyvault_certificate_list](#test-229)
- [Test 230: keyvault_certificate_list](#test-230)
- [Test 231: keyvault_certificate_list](#test-231)
- [Test 232: keyvault_key_create](#test-232)
- [Test 233: keyvault_key_create](#test-233)
- [Test 234: keyvault_key_create](#test-234)
- [Test 235: keyvault_key_create](#test-235)
- [Test 236: keyvault_key_create](#test-236)
- [Test 237: keyvault_key_get](#test-237)
- [Test 238: keyvault_key_get](#test-238)
- [Test 239: keyvault_key_get](#test-239)
- [Test 240: keyvault_key_get](#test-240)
- [Test 241: keyvault_key_get](#test-241)
- [Test 242: keyvault_key_list](#test-242)
- [Test 243: keyvault_key_list](#test-243)
- [Test 244: keyvault_key_list](#test-244)
- [Test 245: keyvault_key_list](#test-245)
- [Test 246: keyvault_key_list](#test-246)
- [Test 247: keyvault_key_list](#test-247)
- [Test 248: keyvault_secret_create](#test-248)
- [Test 249: keyvault_secret_create](#test-249)
- [Test 250: keyvault_secret_create](#test-250)
- [Test 251: keyvault_secret_create](#test-251)
- [Test 252: keyvault_secret_create](#test-252)
- [Test 253: keyvault_secret_get](#test-253)
- [Test 254: keyvault_secret_get](#test-254)
- [Test 255: keyvault_secret_get](#test-255)
- [Test 256: keyvault_secret_get](#test-256)
- [Test 257: keyvault_secret_get](#test-257)
- [Test 258: keyvault_secret_list](#test-258)
- [Test 259: keyvault_secret_list](#test-259)
- [Test 260: keyvault_secret_list](#test-260)
- [Test 261: keyvault_secret_list](#test-261)
- [Test 262: keyvault_secret_list](#test-262)
- [Test 263: keyvault_secret_list](#test-263)
- [Test 264: aks_cluster_get](#test-264)
- [Test 265: aks_cluster_get](#test-265)
- [Test 266: aks_cluster_get](#test-266)
- [Test 267: aks_cluster_get](#test-267)
- [Test 268: aks_cluster_get](#test-268)
- [Test 269: aks_cluster_get](#test-269)
- [Test 270: aks_cluster_get](#test-270)
- [Test 271: aks_nodepool_get](#test-271)
- [Test 272: aks_nodepool_get](#test-272)
- [Test 273: aks_nodepool_get](#test-273)
- [Test 274: aks_nodepool_get](#test-274)
- [Test 275: aks_nodepool_get](#test-275)
- [Test 276: aks_nodepool_get](#test-276)
- [Test 277: loadtesting_test_create](#test-277)
- [Test 278: loadtesting_test_get](#test-278)
- [Test 279: loadtesting_testresource_create](#test-279)
- [Test 280: loadtesting_testresource_list](#test-280)
- [Test 281: loadtesting_testrun_create](#test-281)
- [Test 282: loadtesting_testrun_get](#test-282)
- [Test 283: loadtesting_testrun_list](#test-283)
- [Test 284: loadtesting_testrun_update](#test-284)
- [Test 285: grafana_list](#test-285)
- [Test 286: managedlustre_fs_create](#test-286)
- [Test 287: managedlustre_fs_list](#test-287)
- [Test 288: managedlustre_fs_list](#test-288)
- [Test 289: managedlustre_fs_sku_get](#test-289)
- [Test 290: managedlustre_fs_subnetsize_ask](#test-290)
- [Test 291: managedlustre_fs_subnetsize_validate](#test-291)
- [Test 292: managedlustre_fs_update](#test-292)
- [Test 293: marketplace_product_get](#test-293)
- [Test 294: marketplace_product_list](#test-294)
- [Test 295: marketplace_product_list](#test-295)
- [Test 296: get_bestpractices_get](#test-296)
- [Test 297: get_bestpractices_get](#test-297)
- [Test 298: get_bestpractices_get](#test-298)
- [Test 299: get_bestpractices_get](#test-299)
- [Test 300: get_bestpractices_get](#test-300)
- [Test 301: get_bestpractices_get](#test-301)
- [Test 302: get_bestpractices_get](#test-302)
- [Test 303: get_bestpractices_get](#test-303)
- [Test 304: get_bestpractices_get](#test-304)
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
- [Test 430: subscription_list](#test-430)
- [Test 431: subscription_list](#test-431)
- [Test 432: subscription_list](#test-432)
- [Test 433: subscription_list](#test-433)
- [Test 434: azureterraformbestpractices_get](#test-434)
- [Test 435: azureterraformbestpractices_get](#test-435)
- [Test 436: virtualdesktop_hostpool_list](#test-436)
- [Test 437: virtualdesktop_hostpool_host_list](#test-437)
- [Test 438: virtualdesktop_hostpool_host_user-list](#test-438)
- [Test 439: workbooks_create](#test-439)
- [Test 440: workbooks_delete](#test-440)
- [Test 441: workbooks_list](#test-441)
- [Test 442: workbooks_list](#test-442)
- [Test 443: workbooks_show](#test-443)
- [Test 444: workbooks_show](#test-444)
- [Test 445: workbooks_update](#test-445)
- [Test 446: bicepschema_get](#test-446)
- [Test 447: cloudarchitect_design](#test-447)
- [Test 448: cloudarchitect_design](#test-448)
- [Test 449: cloudarchitect_design](#test-449)
- [Test 450: cloudarchitect_design](#test-450)
- [Test 451: foundry_agents_connect](#test-451)
- [Test 452: foundry_agents_create](#test-452)
- [Test 453: foundry_agents_evaluate](#test-453)
- [Test 454: foundry_agents_get-sdk-sample](#test-454)
- [Test 455: foundry_agents_list](#test-455)
- [Test 456: foundry_agents_list](#test-456)
- [Test 457: foundry_agents_query-and-evaluate](#test-457)
- [Test 458: foundry_knowledge_index_list](#test-458)
- [Test 459: foundry_knowledge_index_list](#test-459)
- [Test 460: foundry_knowledge_index_schema](#test-460)
- [Test 461: foundry_knowledge_index_schema](#test-461)
- [Test 462: foundry_models_deploy](#test-462)
- [Test 463: foundry_models_deployments_list](#test-463)
- [Test 464: foundry_models_deployments_list](#test-464)
- [Test 465: foundry_models_list](#test-465)
- [Test 466: foundry_models_list](#test-466)
- [Test 467: foundry_openai_chat-completions-create](#test-467)
- [Test 468: foundry_openai_create-completion](#test-468)
- [Test 469: foundry_openai_embeddings-create](#test-469)
- [Test 470: foundry_openai_embeddings-create](#test-470)
- [Test 471: foundry_openai_models-list](#test-471)
- [Test 472: foundry_openai_models-list](#test-472)
- [Test 473: foundry_resource_get](#test-473)
- [Test 474: foundry_resource_get](#test-474)
- [Test 475: foundry_resource_get](#test-475)
- [Test 476: foundry_threads_create](#test-476)
- [Test 477: foundry_threads_get-messages](#test-477)
- [Test 478: foundry_threads_list](#test-478)

---

## Test 1

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Get best practices for building AI applications in Azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.901707 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.849188 | `get_bestpractices_get` | ❌ |
| 3 | 0.837780 | `cloudarchitect_design` | ❌ |
| 4 | 0.837172 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.828440 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 2

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Show me the best practices for Microsoft Foundry agents code generation  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.902039 | `foundry_agents_get-sdk-sample` | ❌ |
| 2 | 0.852729 | `foundry_threads_list` | ❌ |
| 3 | 0.840573 | `foundry_threads_get-messages` | ❌ |
| 4 | 0.839788 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 5 | 0.826004 | `foundry_agents_create` | ❌ |

---

## Test 3

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Get guidance for building agents with Microsoft Foundry  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881602 | `foundry_agents_get-sdk-sample` | ❌ |
| 2 | 0.847658 | `foundry_threads_list` | ❌ |
| 3 | 0.844244 | `foundry_agents_create` | ❌ |
| 4 | 0.838805 | `foundry_threads_get-messages` | ❌ |
| 5 | 0.832386 | `azureaibestpractices_get` | ✅ **EXPECTED** |

---

## Test 4

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Create an AI app that helps me to manage travel queries.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.814525 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.782747 | `cloudarchitect_design` | ❌ |
| 3 | 0.782239 | `deploy_app_logs_get` | ❌ |
| 4 | 0.776789 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.774939 | `search_index_query` | ❌ |

---

## Test 5

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Create an AI app that helps me to manage travel queries in Microsoft Foundry  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851597 | `foundry_agents_get-sdk-sample` | ❌ |
| 2 | 0.847705 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 3 | 0.845440 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.843237 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.833031 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 6

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.883581 | `search_service_list` | ❌ |
| 2 | 0.874295 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 3 | 0.845190 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.835427 | `search_index_query` | ❌ |
| 5 | 0.831530 | `search_knowledge_source_get` | ❌ |

---

## Test 7

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.861872 | `search_service_list` | ❌ |
| 2 | 0.856137 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 3 | 0.843258 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.839597 | `search_index_query` | ❌ |
| 5 | 0.823722 | `search_index_get` | ❌ |

---

## Test 8

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

## Test 9

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

## Test 10

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Get the details of knowledge base <agent-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.857497 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.855167 | `search_service_list` | ❌ |
| 3 | 0.842705 | `search_index_query` | ❌ |
| 4 | 0.840920 | `search_index_get` | ❌ |
| 5 | 0.838583 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 11

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818518 | `search_knowledge_base_retrieve` | ❌ |
| 2 | 0.805246 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 3 | 0.799337 | `search_service_list` | ❌ |
| 4 | 0.778408 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.771652 | `foundry_agents_connect` | ❌ |

---

## Test 12

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in Azure AI Search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.867527 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.845382 | `search_index_query` | ❌ |
| 3 | 0.817868 | `foundry_agents_connect` | ❌ |
| 4 | 0.810555 | `search_service_list` | ❌ |
| 5 | 0.804040 | `postgres_database_query` | ❌ |

---

## Test 13

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.834849 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.805058 | `foundry_agents_connect` | ❌ |
| 3 | 0.794166 | `search_knowledge_base_get` | ❌ |
| 4 | 0.783138 | `search_index_query` | ❌ |
| 5 | 0.779341 | `foundry_agents_query-and-evaluate` | ❌ |

---

## Test 14

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.855967 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.807722 | `foundry_agents_connect` | ❌ |
| 3 | 0.789843 | `foundry_agents_evaluate` | ❌ |
| 4 | 0.786866 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.783063 | `postgres_database_query` | ❌ |

---

## Test 15

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.834862 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.805012 | `foundry_agents_connect` | ❌ |
| 3 | 0.794203 | `search_knowledge_base_get` | ❌ |
| 4 | 0.783073 | `search_index_query` | ❌ |
| 5 | 0.779336 | `foundry_agents_query-and-evaluate` | ❌ |

---

## Test 16

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Query knowledge base <agent-name> in search service <service-name> about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.826851 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.808859 | `foundry_agents_connect` | ❌ |
| 3 | 0.795991 | `search_index_query` | ❌ |
| 4 | 0.791363 | `kusto_query` | ❌ |
| 5 | 0.790295 | `postgres_database_query` | ❌ |

---

## Test 17

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Search knowledge base <agent-name> in Azure AI Search service <service-name> for <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.855299 | `search_index_query` | ❌ |
| 2 | 0.837748 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 3 | 0.836542 | `search_service_list` | ❌ |
| 4 | 0.807468 | `azureaibestpractices_get` | ❌ |
| 5 | 0.803245 | `postgres_database_query` | ❌ |

---

## Test 18

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** What does knowledge base <agent-name> in search service <service-name> know about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.810631 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.789111 | `foundry_agents_connect` | ❌ |
| 3 | 0.777486 | `search_knowledge_base_get` | ❌ |
| 4 | 0.773482 | `search_index_query` | ❌ |
| 5 | 0.764773 | `foundry_agents_query-and-evaluate` | ❌ |

---

## Test 19

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Find information about <query> using knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.828871 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.796520 | `foundry_agents_connect` | ❌ |
| 3 | 0.789486 | `search_knowledge_base_get` | ❌ |
| 4 | 0.782309 | `kusto_query` | ❌ |
| 5 | 0.781076 | `search_index_query` | ❌ |

---

## Test 20

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.888833 | `search_service_list` | ❌ |
| 2 | 0.870413 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 3 | 0.842855 | `search_knowledge_base_get` | ❌ |
| 4 | 0.842165 | `search_index_query` | ❌ |
| 5 | 0.829867 | `search_index_get` | ❌ |

---

## Test 21

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868552 | `search_service_list` | ❌ |
| 2 | 0.851623 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 3 | 0.842769 | `search_index_query` | ❌ |
| 4 | 0.830612 | `search_knowledge_base_get` | ❌ |
| 5 | 0.827743 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 22

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

## Test 23

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

## Test 24

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Get the details of knowledge source <source-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.880082 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.849831 | `search_service_list` | ❌ |
| 3 | 0.842576 | `search_index_get` | ❌ |
| 4 | 0.841171 | `search_knowledge_base_get` | ❌ |
| 5 | 0.834825 | `search_index_query` | ❌ |

---

## Test 25

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

## Test 26

**Expected Tool:** `search_index_get`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.842818 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.819586 | `search_service_list` | ❌ |
| 3 | 0.818564 | `search_index_query` | ❌ |
| 4 | 0.801176 | `foundry_resource_get` | ❌ |
| 5 | 0.778269 | `kusto_table_schema` | ❌ |

---

## Test 27

**Expected Tool:** `search_index_get`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.835503 | `search_service_list` | ❌ |
| 2 | 0.820775 | `search_index_get` | ✅ **EXPECTED** |
| 3 | 0.794226 | `search_index_query` | ❌ |
| 4 | 0.783625 | `kusto_cluster_list` | ❌ |
| 5 | 0.772750 | `foundry_resource_get` | ❌ |

---

## Test 28

**Expected Tool:** `search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.822042 | `search_service_list` | ❌ |
| 2 | 0.816723 | `search_index_get` | ✅ **EXPECTED** |
| 3 | 0.800532 | `search_index_query` | ❌ |
| 4 | 0.779781 | `foundry_resource_get` | ❌ |
| 5 | 0.776571 | `foundry_models_deployments_list` | ❌ |

---

## Test 29

**Expected Tool:** `search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.811322 | `search_index_query` | ✅ **EXPECTED** |
| 2 | 0.803180 | `search_service_list` | ❌ |
| 3 | 0.782806 | `postgres_database_query` | ❌ |
| 4 | 0.779808 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.779115 | `kusto_query` | ❌ |

---

## Test 30

**Expected Tool:** `search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.893047 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.816139 | `kusto_cluster_list` | ❌ |
| 3 | 0.805669 | `redis_list` | ❌ |
| 4 | 0.805532 | `foundry_resource_get` | ❌ |
| 5 | 0.792097 | `marketplace_product_list` | ❌ |

---

## Test 31

**Expected Tool:** `search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.863446 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.806040 | `redis_list` | ❌ |
| 3 | 0.804274 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.804187 | `foundry_resource_get` | ❌ |
| 5 | 0.795736 | `marketplace_product_list` | ❌ |

---

## Test 32

**Expected Tool:** `search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813442 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.795308 | `foundry_resource_get` | ❌ |
| 3 | 0.789965 | `search_index_query` | ❌ |
| 4 | 0.786346 | `deploy_app_logs_get` | ❌ |
| 5 | 0.780307 | `foundry_agents_get-sdk-sample` | ❌ |

---

## Test 33

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert this audio file to text using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.912779 | `speech_tts_synthesize` | ❌ |
| 2 | 0.907718 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 3 | 0.791230 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.787746 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.785739 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 34

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Recognize speech from my audio file with language detection  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.871430 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.810741 | `speech_tts_synthesize` | ❌ |
| 3 | 0.755382 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.733954 | `azureaibestpractices_get` | ❌ |
| 5 | 0.733863 | `foundry_openai_create-completion` | ❌ |

---

## Test 35

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe speech from audio file <file_path> with profanity filtering  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.800249 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.762297 | `speech_tts_synthesize` | ❌ |
| 3 | 0.679904 | `azureaibestpractices_get` | ❌ |
| 4 | 0.674115 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.672941 | `mysql_database_query` | ❌ |

---

## Test 36

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text from audio file <file_path> using endpoint <endpoint>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.845048 | `speech_tts_synthesize` | ❌ |
| 2 | 0.833839 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 3 | 0.712860 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.706974 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.704105 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 37

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe the audio file <file_path> in Spanish language  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.801396 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.781128 | `speech_tts_synthesize` | ❌ |
| 3 | 0.709994 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.704874 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.700876 | `deploy_iac_rules_get` | ❌ |

---

## Test 38

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text with detailed output format from audio file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.820196 | `speech_tts_synthesize` | ❌ |
| 2 | 0.812918 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 3 | 0.691471 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.686186 | `azureaibestpractices_get` | ❌ |
| 5 | 0.681825 | `extension_cli_generate` | ❌ |

---

## Test 39

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Recognize speech from <file_path> with phrase hints for better accuracy  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.820376 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.762572 | `speech_tts_synthesize` | ❌ |
| 3 | 0.720218 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.719858 | `azureaibestpractices_get` | ❌ |
| 5 | 0.713171 | `extension_cli_generate` | ❌ |

---

## Test 40

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe audio using multiple phrase hints: "Azure", "cognitive services", "machine learning"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851789 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.836435 | `speech_tts_synthesize` | ❌ |
| 3 | 0.795157 | `cloudarchitect_design` | ❌ |
| 4 | 0.794858 | `azureaibestpractices_get` | ❌ |
| 5 | 0.793442 | `foundry_openai_create-completion` | ❌ |

---

## Test 41

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text with comma-separated phrase hints: "Azure, cognitive services, API"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.855506 | `speech_tts_synthesize` | ❌ |
| 2 | 0.849995 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 3 | 0.811326 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.801933 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.796301 | `azureaibestpractices_get` | ❌ |

---

## Test 42

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe audio with raw profanity output from file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.782061 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.756244 | `speech_tts_synthesize` | ❌ |
| 3 | 0.675683 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.673177 | `foundry_agents_connect` | ❌ |
| 5 | 0.670173 | `extension_cli_generate` | ❌ |

---

## Test 43

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Convert text to speech and save to output.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852732 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.829449 | `speech_stt_recognize` | ❌ |
| 3 | 0.737252 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.728115 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.727432 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 44

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Synthesize speech from "Hello, welcome to Azure" and save to welcome.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.847909 | `speech_stt_recognize` | ❌ |
| 2 | 0.832691 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 3 | 0.781259 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.778955 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.760548 | `foundry_agents_get-sdk-sample` | ❌ |

---

## Test 45

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Generate speech audio from text "Hello world" using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.882853 | `speech_stt_recognize` | ❌ |
| 2 | 0.881965 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 3 | 0.798395 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.792187 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.787775 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 46

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Convert text to speech with Spanish language and save to spanish-audio.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.839992 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.825812 | `speech_stt_recognize` | ❌ |
| 3 | 0.741351 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.728882 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.724734 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 47

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Synthesize speech with voice en-US-JennyNeural from text "Azure AI Services"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.884680 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.867037 | `speech_stt_recognize` | ❌ |
| 3 | 0.819734 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.818966 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.813921 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 48

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Create MP3 audio file from text "Welcome to Azure" with high quality format  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.846572 | `speech_stt_recognize` | ❌ |
| 2 | 0.834861 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 3 | 0.765231 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.758524 | `azureaibestpractices_get` | ❌ |
| 5 | 0.754444 | `deploy_iac_rules_get` | ❌ |

---

## Test 49

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Generate speech with custom voice model using endpoint ID <endpoint-id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.829757 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.808370 | `speech_stt_recognize` | ❌ |
| 3 | 0.756112 | `foundry_resource_get` | ❌ |
| 4 | 0.754856 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.746172 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 50

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Convert text to OGG/Opus format audio file  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.796950 | `speech_stt_recognize` | ❌ |
| 2 | 0.784945 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 3 | 0.712920 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.711991 | `communication_email_send` | ❌ |
| 5 | 0.705540 | `extension_cli_generate` | ❌ |

---

## Test 51

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Synthesize long text content to audio file with streaming  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786826 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.781695 | `speech_stt_recognize` | ❌ |
| 3 | 0.725620 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.722756 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.721370 | `communication_email_send` | ❌ |

---

## Test 52

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Create audio file from text in French language with appropriate voice  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818795 | `speech_stt_recognize` | ❌ |
| 2 | 0.816038 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 3 | 0.739962 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.734955 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.732513 | `communication_email_send` | ❌ |

---

## Test 53

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

## Test 54

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

## Test 55

**Expected Tool:** `appconfig_account_list`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.838378 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.806416 | `appconfig_kv_get` | ❌ |
| 3 | 0.771104 | `deploy_app_logs_get` | ❌ |
| 4 | 0.762362 | `appconfig_kv_set` | ❌ |
| 5 | 0.754373 | `postgres_server_config_get` | ❌ |

---

## Test 56

**Expected Tool:** `appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.828986 | `appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.786676 | `appconfig_kv_set` | ❌ |
| 3 | 0.786255 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.782200 | `appconfig_kv_get` | ❌ |
| 5 | 0.738970 | `appconfig_account_list` | ❌ |

---

## Test 57

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.871372 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.838423 | `appconfig_account_list` | ❌ |
| 3 | 0.822388 | `appconfig_kv_set` | ❌ |
| 4 | 0.792702 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.788942 | `appconfig_kv_delete` | ❌ |

---

## Test 58

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.860199 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.829010 | `appconfig_kv_set` | ❌ |
| 3 | 0.811705 | `appconfig_account_list` | ❌ |
| 4 | 0.802906 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.795144 | `appconfig_kv_delete` | ❌ |

---

## Test 59

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** List all key-value settings with key name starting with 'prod-' in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.822577 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.798993 | `appconfig_account_list` | ❌ |
| 3 | 0.773761 | `appconfig_kv_set` | ❌ |
| 4 | 0.751419 | `appconfig_kv_delete` | ❌ |
| 5 | 0.747206 | `appconfig_kv_lock_set` | ❌ |

---

## Test 60

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.821011 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.815211 | `appconfig_kv_set` | ❌ |
| 3 | 0.776588 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.776106 | `appconfig_account_list` | ❌ |
| 5 | 0.762513 | `appconfig_kv_delete` | ❌ |

---

## Test 61

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.838018 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.798669 | `appconfig_kv_set` | ❌ |
| 3 | 0.784101 | `appconfig_kv_get` | ❌ |
| 4 | 0.782583 | `appconfig_kv_delete` | ❌ |
| 5 | 0.738791 | `appconfig_account_list` | ❌ |

---

## Test 62

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.834195 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.801717 | `appconfig_kv_set` | ❌ |
| 3 | 0.798289 | `appconfig_kv_get` | ❌ |
| 4 | 0.783738 | `appconfig_kv_delete` | ❌ |
| 5 | 0.751469 | `appconfig_account_list` | ❌ |

---

## Test 63

**Expected Tool:** `appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843448 | `appconfig_kv_set` | ✅ **EXPECTED** |
| 2 | 0.814112 | `appconfig_kv_lock_set` | ❌ |
| 3 | 0.790744 | `appconfig_kv_get` | ❌ |
| 4 | 0.788584 | `appconfig_kv_delete` | ❌ |
| 5 | 0.751340 | `mysql_server_param_set` | ❌ |

---

## Test 64

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.860204 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.808555 | `deploy_app_logs_get` | ❌ |
| 3 | 0.763654 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.755670 | `cloudarchitect_design` | ❌ |
| 5 | 0.743357 | `foundry_agents_get-sdk-sample` | ❌ |

---

## Test 65

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

## Test 66

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

## Test 67

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

## Test 68

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

## Test 69

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

## Test 70

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

## Test 71

**Expected Tool:** `appservice_database_add`  
**Prompt:** Connect CosmosDB database <database_name> using connection string <connection_string> to app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.869715 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.793050 | `cosmos_database_container_list` | ❌ |
| 3 | 0.792432 | `kusto_query` | ❌ |
| 4 | 0.791978 | `cosmos_database_list` | ❌ |
| 5 | 0.788846 | `cosmos_database_container_item_query` | ❌ |

---

## Test 72

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

## Test 73

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

## Test 74

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

## Test 75

**Expected Tool:** `appservice_database_add`  
**Prompt:** Set up database <database_name> for app service <app_name> with connection string <connection_string> under resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.889735 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.778608 | `sql_db_create` | ❌ |
| 3 | 0.771983 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.771033 | `kusto_table_list` | ❌ |
| 5 | 0.767668 | `redis_create` | ❌ |

---

## Test 76

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

## Test 77

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.841405 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.795617 | `azureaibestpractices_get` | ❌ |
| 3 | 0.791605 | `applens_resource_diagnose` | ❌ |
| 4 | 0.782883 | `deploy_app_logs_get` | ❌ |
| 5 | 0.781903 | `get_bestpractices_get` | ❌ |

---

## Test 78

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

## Test 79

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.878587 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.808693 | `applens_resource_diagnose` | ❌ |
| 3 | 0.786629 | `deploy_app_logs_get` | ❌ |
| 4 | 0.782837 | `azureaibestpractices_get` | ❌ |
| 5 | 0.780562 | `get_bestpractices_get` | ❌ |

---

## Test 80

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815200 | `applens_resource_diagnose` | ❌ |
| 2 | 0.814016 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 3 | 0.798283 | `deploy_app_logs_get` | ❌ |
| 4 | 0.793547 | `azureaibestpractices_get` | ❌ |
| 5 | 0.788448 | `cloudarchitect_design` | ❌ |

---

## Test 81

**Expected Tool:** `extension_cli_generate`  
**Prompt:** Create a Storage account with name <storage_account_name> using Azure CLI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.845006 | `storage_account_create` | ❌ |
| 2 | 0.840701 | `storage_blob_container_create` | ❌ |
| 3 | 0.832012 | `storage_account_get` | ❌ |
| 4 | 0.799995 | `redis_create` | ❌ |
| 5 | 0.796441 | `storage_blob_container_get` | ❌ |

---

## Test 82

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

## Test 83

**Expected Tool:** `extension_cli_generate`  
**Prompt:** Show me the details of the storage account <account_name> with Azure CLI commands  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.887952 | `storage_account_get` | ❌ |
| 2 | 0.855618 | `storage_blob_container_get` | ❌ |
| 3 | 0.838257 | `storage_blob_get` | ❌ |
| 4 | 0.823911 | `storage_account_create` | ❌ |
| 5 | 0.821905 | `storage_blob_container_create` | ❌ |

---

## Test 84

**Expected Tool:** `extension_cli_install`  
**Prompt:** <Ask the MCP host to uninstall az cli on your machine and run test prompts for extension_cli_generate>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.783226 | `extension_cli_generate` | ❌ |
| 2 | 0.781459 | `extension_cli_install` | ✅ **EXPECTED** |
| 3 | 0.753667 | `sql_server_delete` | ❌ |
| 4 | 0.750484 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.741619 | `deploy_plan_get` | ❌ |

---

## Test 85

**Expected Tool:** `extension_cli_install`  
**Prompt:** How to install azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.790601 | `extension_cli_install` | ✅ **EXPECTED** |
| 2 | 0.728892 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.721054 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.717446 | `extension_cli_generate` | ❌ |
| 5 | 0.714625 | `extension_azqr` | ❌ |

---

## Test 86

**Expected Tool:** `extension_cli_install`  
**Prompt:** What is Azure Functions Core tools and how to install it  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.892433 | `extension_cli_install` | ✅ **EXPECTED** |
| 2 | 0.812380 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.811207 | `extension_cli_generate` | ❌ |
| 4 | 0.803697 | `azureaibestpractices_get` | ❌ |
| 5 | 0.803642 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 87

**Expected Tool:** `acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.893468 | `acr_registry_repository_list` | ❌ |
| 2 | 0.872986 | `acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.862416 | `kusto_cluster_list` | ❌ |
| 4 | 0.856360 | `search_service_list` | ❌ |
| 5 | 0.837896 | `redis_list` | ❌ |

---

## Test 88

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.863518 | `acr_registry_repository_list` | ❌ |
| 2 | 0.828738 | `acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.819051 | `storage_blob_container_get` | ❌ |
| 4 | 0.813630 | `quota_usage_check` | ❌ |
| 5 | 0.803314 | `redis_list` | ❌ |

---

## Test 89

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.845363 | `acr_registry_repository_list` | ❌ |
| 2 | 0.823566 | `acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.814685 | `redis_list` | ❌ |
| 4 | 0.805737 | `storage_blob_container_get` | ❌ |
| 5 | 0.804288 | `eventgrid_subscription_list` | ❌ |

---

## Test 90

**Expected Tool:** `acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.865925 | `acr_registry_repository_list` | ❌ |
| 2 | 0.833004 | `acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.788833 | `kusto_cluster_list` | ❌ |
| 4 | 0.781741 | `group_list` | ❌ |
| 5 | 0.778240 | `storage_blob_container_get` | ❌ |

---

## Test 91

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.867299 | `acr_registry_repository_list` | ❌ |
| 2 | 0.824300 | `acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.793830 | `storage_blob_container_get` | ❌ |
| 4 | 0.786488 | `redis_list` | ❌ |
| 5 | 0.776447 | `eventgrid_topic_list` | ❌ |

---

## Test 92

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.886397 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.837955 | `acr_registry_list` | ❌ |
| 3 | 0.826558 | `kusto_cluster_list` | ❌ |
| 4 | 0.813168 | `search_service_list` | ❌ |
| 5 | 0.808497 | `redis_list` | ❌ |

---

## Test 93

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.846337 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.787159 | `storage_blob_container_get` | ❌ |
| 3 | 0.771812 | `acr_registry_list` | ❌ |
| 4 | 0.768761 | `deploy_app_logs_get` | ❌ |
| 5 | 0.762557 | `redis_list` | ❌ |

---

## Test 94

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868138 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.793332 | `acr_registry_list` | ❌ |
| 3 | 0.773097 | `storage_blob_container_get` | ❌ |
| 4 | 0.759270 | `cosmos_database_container_list` | ❌ |
| 5 | 0.755409 | `kusto_database_list` | ❌ |

---

## Test 95

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.853934 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.783735 | `storage_blob_container_get` | ❌ |
| 3 | 0.779666 | `acr_registry_list` | ❌ |
| 4 | 0.753688 | `redis_list` | ❌ |
| 5 | 0.750924 | `storage_blob_get` | ❌ |

---

## Test 96

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868273 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.742303 | `communication_sms_send` | ❌ |
| 3 | 0.731491 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.718726 | `eventgrid_topic_list` | ❌ |
| 5 | 0.711620 | `kusto_query` | ❌ |

---

## Test 97

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email from my communication service to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852579 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.744727 | `communication_sms_send` | ❌ |
| 3 | 0.738864 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.723001 | `speech_stt_recognize` | ❌ |
| 5 | 0.721946 | `speech_tts_synthesize` | ❌ |

---

## Test 98

**Expected Tool:** `communication_email_send`  
**Prompt:** Send HTML-formatted email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.872349 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.741165 | `communication_sms_send` | ❌ |
| 3 | 0.729801 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.709321 | `eventgrid_topic_list` | ❌ |
| 5 | 0.704821 | `extension_cli_generate` | ❌ |

---

## Test 99

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with CC to <email-address-1> and <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.880319 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.749198 | `communication_sms_send` | ❌ |
| 3 | 0.715848 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.713742 | `cloudarchitect_design` | ❌ |
| 5 | 0.705096 | `foundry_agents_get-sdk-sample` | ❌ |

---

## Test 100

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email to multiple recipients: <email-address-1>, <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.904716 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.782326 | `communication_sms_send` | ❌ |
| 3 | 0.710614 | `cloudarchitect_design` | ❌ |
| 4 | 0.709656 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.705415 | `eventgrid_topic_list` | ❌ |

---

## Test 101

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with reply-to address set to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.866825 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.736970 | `communication_sms_send` | ❌ |
| 3 | 0.708399 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.696529 | `foundry_threads_get-messages` | ❌ |
| 5 | 0.695710 | `postgres_server_param_set` | ❌ |

---

## Test 102

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with custom sender name <sender-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.837896 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.721563 | `communication_sms_send` | ❌ |
| 3 | 0.717084 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.696428 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.693472 | `foundry_threads_get-messages` | ❌ |

---

## Test 103

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email with BCC recipients  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879273 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.755289 | `communication_sms_send` | ❌ |
| 3 | 0.707649 | `foundry_threads_get-messages` | ❌ |
| 4 | 0.704448 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.704119 | `extension_cli_generate` | ❌ |

---

## Test 104

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send an SMS message to <phone-number> saying "Hello"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.820770 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.769285 | `communication_email_send` | ❌ |
| 3 | 0.725443 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.709364 | `speech_tts_synthesize` | ❌ |
| 5 | 0.708279 | `speech_stt_recognize` | ❌ |

---

## Test 105

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS to <phone-number-2> from <phone-number-1> with message "Test message"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.821947 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.775770 | `communication_email_send` | ❌ |
| 3 | 0.712315 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.696385 | `foundry_threads_get-messages` | ❌ |
| 5 | 0.693367 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 106

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS to multiple recipients: <phone-number-1>, <phone-number-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.870041 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.844795 | `communication_email_send` | ❌ |
| 3 | 0.714595 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.704269 | `eventgrid_topic_list` | ❌ |
| 5 | 0.701691 | `speech_tts_synthesize` | ❌ |

---

## Test 107

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS with delivery reporting enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851135 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.775737 | `communication_email_send` | ❌ |
| 3 | 0.733053 | `deploy_app_logs_get` | ❌ |
| 4 | 0.726200 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.719715 | `foundry_agents_get-sdk-sample` | ❌ |

---

## Test 108

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS message with custom tracking tag "campaign1"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.797352 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.766617 | `communication_email_send` | ❌ |
| 3 | 0.727646 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.711044 | `foundry_threads_get-messages` | ❌ |
| 5 | 0.704560 | `deploy_app_logs_get` | ❌ |

---

## Test 109

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send broadcast SMS to <phone-number-1> and <phone-number-2> saying "Urgent notification"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.828175 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.767957 | `communication_email_send` | ❌ |
| 3 | 0.701228 | `foundry_threads_get-messages` | ❌ |
| 4 | 0.697810 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.696027 | `eventgrid_topic_list` | ❌ |

---

## Test 110

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS from my communication service to <phone-number-1>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.824706 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.788074 | `communication_email_send` | ❌ |
| 3 | 0.722517 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.713771 | `speech_tts_synthesize` | ❌ |
| 5 | 0.707065 | `speech_stt_recognize` | ❌ |

---

## Test 111

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send an SMS with delivery receipt tracking  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852271 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.784496 | `communication_email_send` | ❌ |
| 3 | 0.736170 | `deploy_app_logs_get` | ❌ |
| 4 | 0.734240 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.730021 | `foundry_threads_get-messages` | ❌ |

---

## Test 112

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Append an entry to my ledger <ledger_name> with data {"key": "value"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.777179 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.737938 | `appconfig_kv_set` | ❌ |
| 3 | 0.717614 | `keyvault_secret_create` | ❌ |
| 4 | 0.714455 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.710804 | `keyvault_key_create` | ❌ |

---

## Test 113

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Write a tamper-proof entry to ledger <ledger_name> containing {"transaction": "data"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.827993 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.743736 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.697179 | `eventgrid_events_publish` | ❌ |
| 4 | 0.693981 | `keyvault_secret_create` | ❌ |
| 5 | 0.691925 | `keyvault_certificate_import` | ❌ |

---

## Test 114

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Append {"hello": "from mcp"} to my confidential ledger <ledger_name> in collection <collection_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.782923 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.750702 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.695460 | `redis_create` | ❌ |
| 4 | 0.693021 | `appconfig_kv_set` | ❌ |
| 5 | 0.686606 | `cosmos_database_container_item_query` | ❌ |

---

## Test 115

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Create an immutable ledger entry in <ledger_name> with content {"audit": "log"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.746159 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.721142 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.708151 | `keyvault_secret_create` | ❌ |
| 4 | 0.701706 | `redis_create` | ❌ |
| 5 | 0.700351 | `appconfig_kv_set` | ❌ |

---

## Test 116

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Write an entry to confidential ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.835313 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.787289 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.732012 | `keyvault_secret_create` | ❌ |
| 4 | 0.718759 | `keyvault_certificate_import` | ❌ |
| 5 | 0.718445 | `appconfig_kv_lock_set` | ❌ |

---

## Test 117

**Expected Tool:** `confidentialledger_entries_get`  
**Prompt:** Get entry from Confidential Ledger for transaction <transaction_id> on ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.873510 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.817697 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.717577 | `keyvault_certificate_get` | ❌ |
| 4 | 0.704764 | `keyvault_key_get` | ❌ |
| 5 | 0.704575 | `keyvault_secret_get` | ❌ |

---

## Test 118

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

## Test 119

**Expected Tool:** `cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.911865 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.853519 | `cosmos_database_list` | ❌ |
| 3 | 0.840446 | `kusto_cluster_list` | ❌ |
| 4 | 0.823181 | `subscription_list` | ❌ |
| 5 | 0.817476 | `cosmos_database_container_list` | ❌ |

---

## Test 120

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.839227 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.837729 | `cosmos_database_list` | ❌ |
| 3 | 0.809637 | `cosmos_database_container_list` | ❌ |
| 4 | 0.793189 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.780946 | `storage_account_get` | ❌ |

---

## Test 121

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

## Test 122

**Expected Tool:** `cosmos_database_container_item_query`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.874731 | `cosmos_database_container_item_query` | ✅ **EXPECTED** |
| 2 | 0.849399 | `cosmos_database_container_list` | ❌ |
| 3 | 0.795628 | `cosmos_database_list` | ❌ |
| 4 | 0.785297 | `kusto_query` | ❌ |
| 5 | 0.785097 | `storage_blob_container_get` | ❌ |

---

## Test 123

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.921300 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.880697 | `cosmos_database_container_item_query` | ❌ |
| 3 | 0.867517 | `cosmos_database_list` | ❌ |
| 4 | 0.824383 | `cosmos_account_list` | ❌ |
| 5 | 0.809411 | `storage_blob_container_get` | ❌ |

---

## Test 124

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.900700 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.863402 | `cosmos_database_container_item_query` | ❌ |
| 3 | 0.853449 | `cosmos_database_list` | ❌ |
| 4 | 0.818800 | `storage_blob_container_get` | ❌ |
| 5 | 0.815366 | `cosmos_account_list` | ❌ |

---

## Test 125

**Expected Tool:** `cosmos_database_list`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.920574 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.858775 | `cosmos_database_container_list` | ❌ |
| 3 | 0.847820 | `cosmos_account_list` | ❌ |
| 4 | 0.844652 | `postgres_database_list` | ❌ |
| 5 | 0.831337 | `kusto_database_list` | ❌ |

---

## Test 126

**Expected Tool:** `cosmos_database_list`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.908829 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.855188 | `cosmos_database_container_list` | ❌ |
| 3 | 0.841841 | `cosmos_account_list` | ❌ |
| 4 | 0.837447 | `postgres_database_list` | ❌ |
| 5 | 0.824852 | `kusto_database_list` | ❌ |

---

## Test 127

**Expected Tool:** `kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.835197 | `kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.819379 | `kusto_cluster_list` | ❌ |
| 3 | 0.812131 | `kusto_database_list` | ❌ |
| 4 | 0.811714 | `kusto_table_schema` | ❌ |
| 5 | 0.800691 | `aks_cluster_get` | ❌ |

---

## Test 128

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

## Test 129

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

## Test 130

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

## Test 131

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

## Test 132

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

## Test 133

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

## Test 134

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

## Test 135

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

## Test 136

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

## Test 137

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

## Test 138

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

## Test 139

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

## Test 140

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

## Test 141

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

## Test 142

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

## Test 143

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

## Test 144

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

## Test 145

**Expected Tool:** `mysql_server_param_get`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.796653 | `mysql_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.752874 | `mysql_server_param_set` | ❌ |
| 3 | 0.751912 | `mysql_server_config_get` | ❌ |
| 4 | 0.745646 | `mysql_table_schema_get` | ❌ |
| 5 | 0.744210 | `mysql_database_query` | ❌ |

---

## Test 146

**Expected Tool:** `mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.768077 | `mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.758285 | `mysql_server_param_get` | ❌ |
| 3 | 0.749990 | `postgres_server_param_set` | ❌ |
| 4 | 0.732204 | `mysql_database_query` | ❌ |
| 5 | 0.722285 | `mysql_server_config_get` | ❌ |

---

## Test 147

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

## Test 148

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

## Test 149

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

## Test 150

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

## Test 151

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

## Test 152

**Expected Tool:** `postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.829593 | `postgres_database_query` | ✅ **EXPECTED** |
| 2 | 0.808740 | `postgres_database_list` | ❌ |
| 3 | 0.806504 | `postgres_table_list` | ❌ |
| 4 | 0.785447 | `postgres_server_list` | ❌ |
| 5 | 0.756521 | `postgres_server_param_get` | ❌ |

---

## Test 153

**Expected Tool:** `postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.895026 | `postgres_server_config_get` | ✅ **EXPECTED** |
| 2 | 0.859290 | `postgres_server_param_set` | ❌ |
| 3 | 0.833239 | `postgres_database_list` | ❌ |
| 4 | 0.820952 | `postgres_table_list` | ❌ |
| 5 | 0.804190 | `postgres_server_param_get` | ❌ |

---

## Test 154

**Expected Tool:** `postgres_server_list`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.946732 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.861266 | `postgres_database_list` | ❌ |
| 3 | 0.841747 | `postgres_table_list` | ❌ |
| 4 | 0.831264 | `kusto_cluster_list` | ❌ |
| 5 | 0.803690 | `search_service_list` | ❌ |

---

## Test 155

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

## Test 156

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

## Test 157

**Expected Tool:** `postgres_server_param_get`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.827655 | `postgres_server_param_set` | ❌ |
| 2 | 0.806040 | `postgres_server_param_get` | ✅ **EXPECTED** |
| 3 | 0.800510 | `postgres_server_config_get` | ❌ |
| 4 | 0.791725 | `postgres_server_list` | ❌ |
| 5 | 0.788745 | `postgres_database_list` | ❌ |

---

## Test 158

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

## Test 159

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

## Test 160

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

## Test 161

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

## Test 162

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

## Test 163

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

## Test 164

**Expected Tool:** `deploy_iac_rules_get`  
**Prompt:** Show me the rules to generate scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786372 | `foundry_agents_get-sdk-sample` | ❌ |
| 2 | 0.766716 | `extension_cli_generate` | ❌ |
| 3 | 0.752556 | `deploy_iac_rules_get` | ✅ **EXPECTED** |
| 4 | 0.747679 | `azureaibestpractices_get` | ❌ |
| 5 | 0.741796 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 165

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

## Test 166

**Expected Tool:** `deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.888872 | `deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.866662 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.850645 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.844504 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.820138 | `foundry_models_deploy` | ❌ |

---

## Test 167

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

## Test 168

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Publish event to my Event Grid topic <topic_name> with the following events <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.859537 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.820994 | `eventgrid_topic_list` | ❌ |
| 3 | 0.804214 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.757091 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.736109 | `eventhubs_eventhub_get` | ❌ |

---

## Test 169

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

## Test 170

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

## Test 171

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

## Test 172

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.913184 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.894709 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.814853 | `postgres_server_list` | ❌ |
| 4 | 0.810667 | `kusto_cluster_list` | ❌ |
| 5 | 0.802949 | `redis_list` | ❌ |

---

## Test 173

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

## Test 174

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

## Test 175

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

## Test 176

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.886646 | `eventgrid_topic_list` | ❌ |
| 2 | 0.870517 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.804615 | `group_list` | ❌ |
| 4 | 0.800191 | `kusto_cluster_list` | ❌ |
| 5 | 0.792609 | `monitor_webtests_list` | ❌ |

---

## Test 177

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

## Test 178

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

## Test 179

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

## Test 180

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

## Test 181

**Expected Tool:** `eventhubs_eventhub_consumergroup_delete`  
**Prompt:** Delete my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.903198 | `eventhubs_eventhub_consumergroup_delete` | ✅ **EXPECTED** |
| 2 | 0.859003 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.857360 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.849651 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.822938 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 182

**Expected Tool:** `eventhubs_eventhub_consumergroup_get`  
**Prompt:** List all consumer groups in my event hub <event_hub_name> in namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.875460 | `eventhubs_eventhub_consumergroup_get` | ✅ **EXPECTED** |
| 2 | 0.837769 | `eventhubs_eventhub_get` | ❌ |
| 3 | 0.833309 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.823753 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 5 | 0.823385 | `eventhubs_eventhub_consumergroup_update` | ❌ |

---

## Test 183

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

## Test 184

**Expected Tool:** `eventhubs_eventhub_consumergroup_update`  
**Prompt:** Create a new consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.884826 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
| 2 | 0.869484 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 3 | 0.858082 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.822265 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.821795 | `eventhubs_eventhub_update` | ❌ |

---

## Test 185

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

## Test 186

**Expected Tool:** `eventhubs_eventhub_delete`  
**Prompt:** Delete my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.873644 | `eventhubs_namespace_delete` | ❌ |
| 2 | 0.854152 | `eventhubs_eventhub_delete` | ✅ **EXPECTED** |
| 3 | 0.840655 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.824830 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.821433 | `eventhubs_eventhub_get` | ❌ |

---

## Test 187

**Expected Tool:** `eventhubs_eventhub_get`  
**Prompt:** List all Event Hubs in my namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.884791 | `eventhubs_eventhub_get` | ✅ **EXPECTED** |
| 2 | 0.850468 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.820604 | `eventhubs_eventhub_update` | ❌ |
| 4 | 0.816794 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.815068 | `kusto_cluster_list` | ❌ |

---

## Test 188

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

## Test 189

**Expected Tool:** `eventhubs_eventhub_update`  
**Prompt:** Create a new event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.858259 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.848236 | `eventhubs_namespace_delete` | ❌ |
| 3 | 0.847070 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.844468 | `eventhubs_eventhub_get` | ❌ |
| 5 | 0.819099 | `eventhubs_eventhub_consumergroup_update` | ❌ |

---

## Test 190

**Expected Tool:** `eventhubs_eventhub_update`  
**Prompt:** Update my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.861448 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.837861 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.831253 | `eventhubs_namespace_delete` | ❌ |
| 4 | 0.828681 | `eventhubs_eventhub_get` | ❌ |
| 5 | 0.821437 | `eventhubs_eventhub_consumergroup_update` | ❌ |

---

## Test 191

**Expected Tool:** `eventhubs_namespace_delete`  
**Prompt:** Delete my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.847128 | `eventhubs_namespace_delete` | ✅ **EXPECTED** |
| 2 | 0.809806 | `eventhubs_namespace_update` | ❌ |
| 3 | 0.796933 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.786281 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.774290 | `sql_server_delete` | ❌ |

---

## Test 192

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

## Test 193

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

## Test 194

**Expected Tool:** `eventhubs_namespace_update`  
**Prompt:** Create an new namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.842150 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.812118 | `eventhubs_namespace_delete` | ❌ |
| 3 | 0.802209 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.789301 | `redis_create` | ❌ |
| 5 | 0.762777 | `foundry_models_deploy` | ❌ |

---

## Test 195

**Expected Tool:** `eventhubs_namespace_update`  
**Prompt:** Update my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.861629 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.804084 | `eventhubs_namespace_delete` | ❌ |
| 3 | 0.796062 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.768136 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.765585 | `eventhubs_eventhub_consumergroup_update` | ❌ |

---

## Test 196

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

## Test 197

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

## Test 198

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

## Test 199

**Expected Tool:** `functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.888340 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.801509 | `deploy_app_logs_get` | ❌ |
| 3 | 0.796455 | `applens_resource_diagnose` | ❌ |
| 4 | 0.775717 | `foundry_resource_get` | ❌ |
| 5 | 0.766532 | `monitor_webtests_get` | ❌ |

---

## Test 200

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

## Test 201

**Expected Tool:** `functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.884793 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.791441 | `deploy_app_logs_get` | ❌ |
| 3 | 0.767977 | `applens_resource_diagnose` | ❌ |
| 4 | 0.765038 | `monitor_webtests_get` | ❌ |
| 5 | 0.757883 | `foundry_resource_get` | ❌ |

---

## Test 202

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

## Test 203

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

## Test 204

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

## Test 205

**Expected Tool:** `functionapp_get`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.855510 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.801000 | `search_service_list` | ❌ |
| 3 | 0.798885 | `deploy_app_logs_get` | ❌ |
| 4 | 0.797428 | `appconfig_account_list` | ❌ |
| 5 | 0.784778 | `eventgrid_subscription_list` | ❌ |

---

## Test 206

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

## Test 207

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

## Test 208

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

## Test 209

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** Show me the account settings for managed HSM keyvault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879992 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.817886 | `storage_account_get` | ❌ |
| 3 | 0.793266 | `keyvault_key_get` | ❌ |
| 4 | 0.792142 | `keyvault_key_create` | ❌ |
| 5 | 0.789252 | `storage_blob_container_get` | ❌ |

---

## Test 210

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** What's the value of the <setting_name> setting in my key vault with name <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793890 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.778320 | `appconfig_kv_set` | ❌ |
| 3 | 0.777060 | `keyvault_secret_create` | ❌ |
| 4 | 0.773529 | `keyvault_secret_get` | ❌ |
| 5 | 0.771355 | `storage_account_get` | ❌ |

---

## Test 211

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.858241 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.840150 | `keyvault_certificate_import` | ❌ |
| 3 | 0.838058 | `keyvault_key_create` | ❌ |
| 4 | 0.829966 | `keyvault_certificate_get` | ❌ |
| 5 | 0.817735 | `keyvault_secret_create` | ❌ |

---

## Test 212

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Generate a certificate named <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.847433 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.826699 | `keyvault_certificate_import` | ❌ |
| 3 | 0.823322 | `keyvault_certificate_get` | ❌ |
| 4 | 0.817519 | `keyvault_key_create` | ❌ |
| 5 | 0.806741 | `keyvault_certificate_list` | ❌ |

---

## Test 213

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Request creation of certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.845848 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.828698 | `keyvault_certificate_import` | ❌ |
| 3 | 0.826442 | `keyvault_certificate_get` | ❌ |
| 4 | 0.821290 | `keyvault_key_create` | ❌ |
| 5 | 0.811771 | `keyvault_secret_create` | ❌ |

---

## Test 214

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.840745 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.827065 | `keyvault_certificate_import` | ❌ |
| 3 | 0.825962 | `keyvault_certificate_get` | ❌ |
| 4 | 0.814983 | `keyvault_key_create` | ❌ |
| 5 | 0.805347 | `keyvault_secret_create` | ❌ |

---

## Test 215

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Issue a certificate <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851949 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.834756 | `keyvault_certificate_get` | ❌ |
| 3 | 0.831743 | `keyvault_certificate_import` | ❌ |
| 4 | 0.817855 | `keyvault_key_create` | ❌ |
| 5 | 0.817641 | `keyvault_certificate_list` | ❌ |

---

## Test 216

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.849686 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.829160 | `keyvault_certificate_list` | ❌ |
| 3 | 0.822051 | `keyvault_certificate_create` | ❌ |
| 4 | 0.816073 | `keyvault_certificate_import` | ❌ |
| 5 | 0.799569 | `keyvault_key_create` | ❌ |

---

## Test 217

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

## Test 218

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Get the certificate <certificate_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.836168 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.814736 | `keyvault_certificate_create` | ❌ |
| 3 | 0.807860 | `keyvault_certificate_import` | ❌ |
| 4 | 0.803158 | `keyvault_certificate_list` | ❌ |
| 5 | 0.786042 | `keyvault_key_create` | ❌ |

---

## Test 219

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Display the certificate details for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.859098 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.813786 | `keyvault_certificate_create` | ❌ |
| 3 | 0.813564 | `keyvault_key_get` | ❌ |
| 4 | 0.807689 | `keyvault_certificate_list` | ❌ |
| 5 | 0.806574 | `keyvault_secret_get` | ❌ |

---

## Test 220

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.822429 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.794140 | `keyvault_certificate_list` | ❌ |
| 3 | 0.790227 | `keyvault_certificate_create` | ❌ |
| 4 | 0.787678 | `keyvault_certificate_import` | ❌ |
| 5 | 0.778190 | `keyvault_key_get` | ❌ |

---

## Test 221

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.820998 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.775551 | `keyvault_certificate_get` | ❌ |
| 3 | 0.773043 | `keyvault_certificate_create` | ❌ |
| 4 | 0.764307 | `keyvault_certificate_list` | ❌ |
| 5 | 0.762211 | `keyvault_key_create` | ❌ |

---

## Test 222

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.848227 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.829118 | `keyvault_certificate_get` | ❌ |
| 3 | 0.817167 | `keyvault_certificate_create` | ❌ |
| 4 | 0.803441 | `keyvault_certificate_list` | ❌ |
| 5 | 0.799048 | `keyvault_key_create` | ❌ |

---

## Test 223

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Upload certificate file <file_path> to key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.828397 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.793637 | `keyvault_certificate_create` | ❌ |
| 3 | 0.783872 | `keyvault_certificate_get` | ❌ |
| 4 | 0.781328 | `keyvault_key_create` | ❌ |
| 5 | 0.777882 | `keyvault_secret_create` | ❌ |

---

## Test 224

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.817359 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.779378 | `keyvault_certificate_get` | ❌ |
| 3 | 0.775460 | `keyvault_certificate_create` | ❌ |
| 4 | 0.760767 | `keyvault_certificate_list` | ❌ |
| 5 | 0.753999 | `keyvault_secret_create` | ❌ |

---

## Test 225

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.825556 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.785854 | `keyvault_certificate_create` | ❌ |
| 3 | 0.777793 | `keyvault_certificate_get` | ❌ |
| 4 | 0.764990 | `keyvault_secret_create` | ❌ |
| 5 | 0.762896 | `keyvault_key_create` | ❌ |

---

## Test 226

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

## Test 227

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851648 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.840536 | `keyvault_certificate_get` | ❌ |
| 3 | 0.810598 | `keyvault_certificate_create` | ❌ |
| 4 | 0.802496 | `keyvault_certificate_import` | ❌ |
| 5 | 0.798452 | `keyvault_key_get` | ❌ |

---

## Test 228

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** What certificates are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.838086 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.827396 | `keyvault_certificate_get` | ❌ |
| 3 | 0.809956 | `keyvault_certificate_create` | ❌ |
| 4 | 0.799348 | `keyvault_certificate_import` | ❌ |
| 5 | 0.795610 | `keyvault_key_create` | ❌ |

---

## Test 229

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

## Test 230

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Enumerate certificates in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.905894 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.850109 | `keyvault_key_list` | ❌ |
| 3 | 0.844790 | `keyvault_secret_list` | ❌ |
| 4 | 0.825989 | `keyvault_certificate_get` | ❌ |
| 5 | 0.807584 | `keyvault_certificate_create` | ❌ |

---

## Test 231

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

## Test 232

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.865691 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.779557 | `keyvault_secret_create` | ❌ |
| 3 | 0.773761 | `keyvault_certificate_create` | ❌ |
| 4 | 0.770666 | `keyvault_key_get` | ❌ |
| 5 | 0.761625 | `keyvault_certificate_import` | ❌ |

---

## Test 233

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

## Test 234

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

## Test 235

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an RSA key in the vault <key_vault_account_name> with name <key_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.854742 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.793508 | `keyvault_secret_create` | ❌ |
| 3 | 0.786903 | `keyvault_certificate_create` | ❌ |
| 4 | 0.784100 | `keyvault_key_get` | ❌ |
| 5 | 0.777868 | `keyvault_certificate_import` | ❌ |

---

## Test 236

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an EC key with name <key_name> in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.827686 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.775423 | `keyvault_secret_create` | ❌ |
| 3 | 0.769017 | `keyvault_key_get` | ❌ |
| 4 | 0.766467 | `keyvault_certificate_create` | ❌ |
| 5 | 0.762525 | `keyvault_certificate_import` | ❌ |

---

## Test 237

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

## Test 238

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.858223 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.834786 | `keyvault_secret_get` | ❌ |
| 3 | 0.832105 | `keyvault_certificate_get` | ❌ |
| 4 | 0.814015 | `storage_account_get` | ❌ |
| 5 | 0.813369 | `keyvault_key_create` | ❌ |

---

## Test 239

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

## Test 240

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

## Test 241

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

## Test 242

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

## Test 243

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

## Test 244

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

## Test 245

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List key names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.842759 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.818982 | `keyvault_secret_list` | ❌ |
| 3 | 0.816682 | `keyvault_key_get` | ❌ |
| 4 | 0.808507 | `keyvault_certificate_list` | ❌ |
| 5 | 0.797995 | `keyvault_key_create` | ❌ |

---

## Test 246

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

## Test 247

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

## Test 248

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.862855 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.815682 | `keyvault_secret_get` | ❌ |
| 3 | 0.805459 | `keyvault_key_create` | ❌ |
| 4 | 0.785645 | `keyvault_certificate_create` | ❌ |
| 5 | 0.778660 | `keyvault_certificate_import` | ❌ |

---

## Test 249

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.860701 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.816343 | `keyvault_secret_get` | ❌ |
| 3 | 0.784856 | `keyvault_key_create` | ❌ |
| 4 | 0.783952 | `keyvault_secret_list` | ❌ |
| 5 | 0.777104 | `appconfig_kv_set` | ❌ |

---

## Test 250

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.849189 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.816193 | `keyvault_secret_get` | ❌ |
| 3 | 0.787393 | `keyvault_key_create` | ❌ |
| 4 | 0.784295 | `keyvault_secret_list` | ❌ |
| 5 | 0.778744 | `appconfig_kv_set` | ❌ |

---

## Test 251

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.847957 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.801169 | `keyvault_secret_get` | ❌ |
| 3 | 0.777110 | `keyvault_key_create` | ❌ |
| 4 | 0.774292 | `keyvault_certificate_import` | ❌ |
| 5 | 0.767067 | `keyvault_certificate_create` | ❌ |

---

## Test 252

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

## Test 253

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

## Test 254

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868759 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.844743 | `keyvault_key_get` | ❌ |
| 3 | 0.834545 | `keyvault_certificate_get` | ❌ |
| 4 | 0.825237 | `keyvault_secret_list` | ❌ |
| 5 | 0.819269 | `keyvault_secret_create` | ❌ |

---

## Test 255

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

## Test 256

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

## Test 257

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

## Test 258

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

## Test 259

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

## Test 260

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

## Test 261

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

## Test 262

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

## Test 263

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

## Test 264

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

## Test 265

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.877317 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.854822 | `aks_nodepool_get` | ❌ |
| 3 | 0.835183 | `kusto_cluster_get` | ❌ |
| 4 | 0.817776 | `kusto_cluster_list` | ❌ |
| 5 | 0.804872 | `redis_list` | ❌ |

---

## Test 266

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

## Test 267

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.857609 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.841157 | `aks_nodepool_get` | ❌ |
| 3 | 0.811485 | `kusto_cluster_get` | ❌ |
| 4 | 0.795363 | `foundry_resource_get` | ❌ |
| 5 | 0.791661 | `storage_account_get` | ❌ |

---

## Test 268

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

## Test 269

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

## Test 270

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.840306 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.816615 | `aks_nodepool_get` | ❌ |
| 3 | 0.778774 | `kusto_cluster_list` | ❌ |
| 4 | 0.761782 | `kusto_cluster_get` | ❌ |
| 5 | 0.757916 | `kusto_database_list` | ❌ |

---

## Test 271

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.887377 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.836887 | `aks_cluster_get` | ❌ |
| 3 | 0.811633 | `kusto_cluster_get` | ❌ |
| 4 | 0.781283 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.778385 | `kusto_cluster_list` | ❌ |

---

## Test 272

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

## Test 273

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

## Test 274

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

## Test 275

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.886269 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.837311 | `aks_cluster_get` | ❌ |
| 3 | 0.817541 | `kusto_cluster_list` | ❌ |
| 4 | 0.801824 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.791620 | `kusto_database_list` | ❌ |

---

## Test 276

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

## Test 277

**Expected Tool:** `loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.823045 | `loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.821709 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.810933 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.803467 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.782025 | `loadtesting_test_get` | ❌ |

---

## Test 278

**Expected Tool:** `loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.836205 | `loadtesting_test_get` | ✅ **EXPECTED** |
| 2 | 0.823359 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.823186 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.822609 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.808050 | `monitor_webtests_get` | ❌ |

---

## Test 279

**Expected Tool:** `loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.850885 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.848438 | `loadtesting_testresource_create` | ✅ **EXPECTED** |
| 3 | 0.825059 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.823651 | `redis_create` | ❌ |
| 5 | 0.823016 | `monitor_webtests_list` | ❌ |

---

## Test 280

**Expected Tool:** `loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.903902 | `loadtesting_testresource_list` | ✅ **EXPECTED** |
| 2 | 0.867615 | `monitor_webtests_list` | ❌ |
| 3 | 0.849957 | `group_list` | ❌ |
| 4 | 0.832848 | `redis_list` | ❌ |
| 5 | 0.831770 | `kusto_cluster_list` | ❌ |

---

## Test 281

**Expected Tool:** `loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.877261 | `loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.831327 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.820081 | `loadtesting_test_create` | ❌ |
| 4 | 0.816240 | `loadtesting_testrun_update` | ❌ |
| 5 | 0.803680 | `loadtesting_testresource_list` | ❌ |

---

## Test 282

**Expected Tool:** `loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.845121 | `loadtesting_testrun_create` | ❌ |
| 2 | 0.832314 | `loadtesting_test_get` | ❌ |
| 3 | 0.818933 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.818014 | `monitor_webtests_get` | ❌ |
| 5 | 0.814756 | `loadtesting_testrun_list` | ❌ |

---

## Test 283

**Expected Tool:** `loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.838956 | `loadtesting_testrun_list` | ✅ **EXPECTED** |
| 2 | 0.831750 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.826910 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.812738 | `loadtesting_test_get` | ❌ |
| 5 | 0.812212 | `monitor_webtests_list` | ❌ |

---

## Test 284

**Expected Tool:** `loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.844152 | `loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.807072 | `loadtesting_testrun_create` | ❌ |
| 3 | 0.756178 | `eventhubs_namespace_update` | ❌ |
| 4 | 0.749486 | `monitor_webtests_get` | ❌ |
| 5 | 0.749363 | `loadtesting_test_get` | ❌ |

---

## Test 285

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

## Test 286

**Expected Tool:** `managedlustre_fs_create`  
**Prompt:** Create an Azure Managed Lustre filesystem with name <filesystem_name>, size <filesystem_size>, SKU <sku>, and subnet <subnet_id> for availability zone <zone> in location <location>. Maintenance should occur on <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.883688 | `managedlustre_fs_create` | ✅ **EXPECTED** |
| 2 | 0.872554 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 3 | 0.849380 | `managedlustre_fs_list` | ❌ |
| 4 | 0.841000 | `managedlustre_fs_sku_get` | ❌ |
| 5 | 0.834033 | `managedlustre_fs_update` | ❌ |

---

## Test 287

**Expected Tool:** `managedlustre_fs_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.890120 | `managedlustre_fs_list` | ✅ **EXPECTED** |
| 2 | 0.826604 | `kusto_cluster_list` | ❌ |
| 3 | 0.825453 | `managedlustre_fs_create` | ❌ |
| 4 | 0.821508 | `managedlustre_fs_sku_get` | ❌ |
| 5 | 0.813953 | `managedlustre_fs_subnetsize_validate` | ❌ |

---

## Test 288

**Expected Tool:** `managedlustre_fs_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881088 | `managedlustre_fs_list` | ✅ **EXPECTED** |
| 2 | 0.827106 | `managedlustre_fs_create` | ❌ |
| 3 | 0.816668 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.806393 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.793431 | `mysql_server_list` | ❌ |

---

## Test 289

**Expected Tool:** `managedlustre_fs_sku_get`  
**Prompt:** List the Azure Managed Lustre SKUs available in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.909059 | `managedlustre_fs_sku_get` | ✅ **EXPECTED** |
| 2 | 0.839205 | `managedlustre_fs_list` | ❌ |
| 3 | 0.826578 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 4 | 0.808577 | `managedlustre_fs_create` | ❌ |
| 5 | 0.800384 | `storage_account_get` | ❌ |

---

## Test 290

**Expected Tool:** `managedlustre_fs_subnetsize_ask`  
**Prompt:** Tell me how many IP addresses I need for an Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.878047 | `managedlustre_fs_subnetsize_ask` | ✅ **EXPECTED** |
| 2 | 0.865157 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 3 | 0.836563 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.806537 | `managedlustre_fs_create` | ❌ |
| 5 | 0.805975 | `managedlustre_fs_list` | ❌ |

---

## Test 291

**Expected Tool:** `managedlustre_fs_subnetsize_validate`  
**Prompt:** Validate if the network <subnet_id> can host Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.949006 | `managedlustre_fs_subnetsize_validate` | ✅ **EXPECTED** |
| 2 | 0.836190 | `managedlustre_fs_subnetsize_ask` | ❌ |
| 3 | 0.823608 | `managedlustre_fs_create` | ❌ |
| 4 | 0.815984 | `managedlustre_fs_sku_get` | ❌ |
| 5 | 0.809808 | `managedlustre_fs_list` | ❌ |

---

## Test 292

**Expected Tool:** `managedlustre_fs_update`  
**Prompt:** Update the maintenance window of the Azure Managed Lustre filesystem <filesystem_name> to <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.870692 | `managedlustre_fs_update` | ✅ **EXPECTED** |
| 2 | 0.826744 | `managedlustre_fs_create` | ❌ |
| 3 | 0.796605 | `managedlustre_fs_list` | ❌ |
| 4 | 0.766323 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 5 | 0.753852 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 293

**Expected Tool:** `marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.840110 | `marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.838677 | `marketplace_product_list` | ❌ |
| 3 | 0.767924 | `foundry_resource_get` | ❌ |
| 4 | 0.760387 | `storage_account_get` | ❌ |
| 5 | 0.755465 | `search_index_get` | ❌ |

---

## Test 294

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.853722 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.795533 | `foundry_agents_get-sdk-sample` | ❌ |
| 3 | 0.783121 | `foundry_threads_list` | ❌ |
| 4 | 0.777058 | `marketplace_product_get` | ❌ |
| 5 | 0.762771 | `cloudarchitect_design` | ❌ |

---

## Test 295

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.857997 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.768986 | `marketplace_product_get` | ❌ |
| 3 | 0.767593 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.752001 | `foundry_models_list` | ❌ |
| 5 | 0.750294 | `eventgrid_topic_list` | ❌ |

---

## Test 296

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.878380 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.867833 | `azureaibestpractices_get` | ❌ |
| 3 | 0.848190 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.827076 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.809418 | `foundry_agents_get-sdk-sample` | ❌ |

---

## Test 297

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.863870 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.836840 | `azureaibestpractices_get` | ❌ |
| 3 | 0.836743 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.830549 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.820558 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 298

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868178 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.853941 | `azureaibestpractices_get` | ❌ |
| 3 | 0.852993 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.820981 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.814134 | `cloudarchitect_design` | ❌ |

---

## Test 299

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868122 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.849932 | `azureaibestpractices_get` | ❌ |
| 3 | 0.823458 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.812991 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.808704 | `deploy_iac_rules_get` | ❌ |

---

## Test 300

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.858253 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.825094 | `azureaibestpractices_get` | ❌ |
| 3 | 0.819017 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.810336 | `extension_cli_install` | ❌ |
| 5 | 0.808484 | `azureterraformbestpractices_get` | ❌ |

---

## Test 301

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.859503 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.837761 | `azureaibestpractices_get` | ❌ |
| 3 | 0.823916 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.808443 | `extension_cli_install` | ❌ |
| 5 | 0.800786 | `cloudarchitect_design` | ❌ |

---

## Test 302

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.847052 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.834727 | `azureaibestpractices_get` | ❌ |
| 3 | 0.819317 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.817462 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.815950 | `cloudarchitect_design` | ❌ |

---

## Test 303

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.841439 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.822991 | `azureaibestpractices_get` | ❌ |
| 3 | 0.816198 | `cloudarchitect_design` | ❌ |
| 4 | 0.815282 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.799818 | `extension_cli_install` | ❌ |

---

## Test 304

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** configure azure mcp in coding agent for my repo  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.820628 | `deploy_plan_get` | ❌ |
| 2 | 0.801789 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.800343 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.799225 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.776799 | `azureaibestpractices_get` | ❌ |

---

## Test 305

**Expected Tool:** `monitor_activitylog_list`  
**Prompt:** List the activity logs of the last month for <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813939 | `monitor_activitylog_list` | ✅ **EXPECTED** |
| 2 | 0.787050 | `monitor_resource_log_query` | ❌ |
| 3 | 0.780778 | `monitor_workspace_log_query` | ❌ |
| 4 | 0.767093 | `deploy_app_logs_get` | ❌ |
| 5 | 0.747243 | `resourcehealth_health-events_list` | ❌ |

---

## Test 306

**Expected Tool:** `monitor_healthmodels_entity_get`  
**Prompt:** Show me the health status of entity <entity_id> using the health model <health_model_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.836867 | `monitor_healthmodels_entity_get` | ✅ **EXPECTED** |
| 2 | 0.798169 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.745048 | `resourcehealth_health-events_list` | ❌ |
| 4 | 0.724226 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.721744 | `foundry_models_deployments_list` | ❌ |

---

## Test 307

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

## Test 308

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

## Test 309

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.864559 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.808641 | `monitor_metrics_query` | ❌ |
| 3 | 0.804435 | `applens_resource_diagnose` | ❌ |
| 4 | 0.770012 | `bicepschema_get` | ❌ |
| 5 | 0.769714 | `quota_usage_check` | ❌ |

---

## Test 310

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.825544 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.795088 | `applens_resource_diagnose` | ❌ |
| 3 | 0.775598 | `applicationinsights_recommendation_list` | ❌ |
| 4 | 0.772098 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.771591 | `monitor_resource_log_query` | ❌ |

---

## Test 311

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.826194 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.800088 | `quota_usage_check` | ❌ |
| 3 | 0.797355 | `applens_resource_diagnose` | ❌ |
| 4 | 0.796373 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.792043 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 312

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

## Test 313

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

## Test 314

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

## Test 315

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

## Test 316

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

## Test 317

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

## Test 318

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

## Test 319

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

## Test 320

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.930297 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.858293 | `monitor_table_list` | ❌ |
| 3 | 0.820683 | `monitor_workspace_list` | ❌ |
| 4 | 0.814090 | `deploy_app_logs_get` | ❌ |
| 5 | 0.804498 | `monitor_workspace_log_query` | ❌ |

---

## Test 321

**Expected Tool:** `monitor_webtests_create`  
**Prompt:** Create a new Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.814130 | `monitor_webtests_list` | ❌ |
| 2 | 0.801618 | `monitor_webtests_create` | ✅ **EXPECTED** |
| 3 | 0.798365 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.797600 | `monitor_webtests_get` | ❌ |
| 5 | 0.781952 | `loadtesting_testrun_create` | ❌ |

---

## Test 322

**Expected Tool:** `monitor_webtests_get`  
**Prompt:** Get Web Test details for <webtest_resource_name> in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.891564 | `monitor_webtests_get` | ✅ **EXPECTED** |
| 2 | 0.875106 | `monitor_webtests_list` | ❌ |
| 3 | 0.832489 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.795049 | `eventgrid_topic_list` | ❌ |
| 5 | 0.793058 | `group_list` | ❌ |

---

## Test 323

**Expected Tool:** `monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.887937 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.839863 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.829709 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.821453 | `eventgrid_topic_list` | ❌ |
| 5 | 0.820228 | `redis_list` | ❌ |

---

## Test 324

**Expected Tool:** `monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.917395 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.862023 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.846033 | `group_list` | ❌ |
| 4 | 0.830941 | `monitor_webtests_get` | ❌ |
| 5 | 0.825441 | `eventgrid_topic_list` | ❌ |

---

## Test 325

**Expected Tool:** `monitor_webtests_update`  
**Prompt:** Update an existing Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.838960 | `monitor_webtests_update` | ✅ **EXPECTED** |
| 2 | 0.804943 | `monitor_webtests_get` | ❌ |
| 3 | 0.797730 | `monitor_webtests_list` | ❌ |
| 4 | 0.777279 | `monitor_webtests_create` | ❌ |
| 5 | 0.760733 | `loadtesting_testrun_create` | ❌ |

---

## Test 326

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

## Test 327

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

## Test 328

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

## Test 329

**Expected Tool:** `monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.855186 | `deploy_app_logs_get` | ❌ |
| 2 | 0.853231 | `monitor_workspace_log_query` | ✅ **EXPECTED** |
| 3 | 0.827999 | `monitor_resource_log_query` | ❌ |
| 4 | 0.823474 | `monitor_activitylog_list` | ❌ |
| 5 | 0.815853 | `monitor_workspace_list` | ❌ |

---

## Test 330

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

## Test 331

**Expected Tool:** `datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.877462 | `datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.805276 | `redis_list` | ❌ |
| 3 | 0.790049 | `monitor_metrics_query` | ❌ |
| 4 | 0.780457 | `quota_usage_check` | ❌ |
| 5 | 0.779392 | `deploy_app_logs_get` | ❌ |

---

## Test 332

**Expected Tool:** `extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843977 | `quota_usage_check` | ❌ |
| 2 | 0.835160 | `applens_resource_diagnose` | ❌ |
| 3 | 0.818047 | `subscription_list` | ❌ |
| 4 | 0.813051 | `extension_azqr` | ✅ **EXPECTED** |
| 5 | 0.811721 | `marketplace_product_list` | ❌ |

---

## Test 333

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

## Test 334

**Expected Tool:** `extension_azqr`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.839099 | `quota_usage_check` | ❌ |
| 2 | 0.825566 | `applens_resource_diagnose` | ❌ |
| 3 | 0.822292 | `search_service_list` | ❌ |
| 4 | 0.821161 | `extension_azqr` | ✅ **EXPECTED** |
| 5 | 0.815321 | `marketplace_product_list` | ❌ |

---

## Test 335

**Expected Tool:** `quota_region_availability_list`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.849752 | `quota_region_availability_list` | ✅ **EXPECTED** |
| 2 | 0.795933 | `quota_usage_check` | ❌ |
| 3 | 0.777360 | `redis_list` | ❌ |
| 4 | 0.759715 | `group_list` | ❌ |
| 5 | 0.756750 | `eventgrid_topic_list` | ❌ |

---

## Test 336

**Expected Tool:** `quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.858891 | `quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.793713 | `quota_region_availability_list` | ❌ |
| 3 | 0.766212 | `applens_resource_diagnose` | ❌ |
| 4 | 0.763981 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.763012 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 337

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

## Test 338

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

## Test 339

**Expected Tool:** `redis_create`  
**Prompt:** Create a new Redis resource named <resource_name> with SKU <sku_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.844397 | `redis_create` | ✅ **EXPECTED** |
| 2 | 0.801319 | `storage_account_create` | ❌ |
| 3 | 0.785885 | `eventhubs_namespace_update` | ❌ |
| 4 | 0.775091 | `redis_list` | ❌ |
| 5 | 0.761478 | `workbooks_create` | ❌ |

---

## Test 340

**Expected Tool:** `redis_create`  
**Prompt:** Create a new Redis resource for me  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.858770 | `redis_create` | ✅ **EXPECTED** |
| 2 | 0.796574 | `redis_list` | ❌ |
| 3 | 0.763828 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.734660 | `loadtesting_testrun_create` | ❌ |
| 5 | 0.732287 | `foundry_models_deploy` | ❌ |

---

## Test 341

**Expected Tool:** `redis_create`  
**Prompt:** Create a Redis cache named <resource_name> with SKU <sku_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.836986 | `redis_create` | ✅ **EXPECTED** |
| 2 | 0.787470 | `storage_account_create` | ❌ |
| 3 | 0.784911 | `redis_list` | ❌ |
| 4 | 0.771132 | `eventhubs_namespace_update` | ❌ |
| 5 | 0.751311 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 342

**Expected Tool:** `redis_create`  
**Prompt:** Create a new Redis cluster with name <resource_name>, SKU <sku_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.834538 | `redis_create` | ✅ **EXPECTED** |
| 2 | 0.782674 | `redis_list` | ❌ |
| 3 | 0.773686 | `eventhubs_namespace_update` | ❌ |
| 4 | 0.769323 | `managedlustre_fs_sku_get` | ❌ |
| 5 | 0.762878 | `storage_account_create` | ❌ |

---

## Test 343

**Expected Tool:** `redis_list`  
**Prompt:** List all Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.919341 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.833124 | `redis_create` | ❌ |
| 3 | 0.819484 | `group_list` | ❌ |
| 4 | 0.817669 | `kusto_cluster_list` | ❌ |
| 5 | 0.813246 | `grafana_list` | ❌ |

---

## Test 344

**Expected Tool:** `redis_list`  
**Prompt:** Show me my Redis resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868860 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.828893 | `redis_create` | ❌ |
| 3 | 0.779446 | `quota_usage_check` | ❌ |
| 4 | 0.766882 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.754078 | `deploy_app_logs_get` | ❌ |

---

## Test 345

**Expected Tool:** `redis_list`  
**Prompt:** Show me the Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.907147 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.838890 | `redis_create` | ❌ |
| 3 | 0.805796 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.805620 | `eventgrid_topic_list` | ❌ |
| 5 | 0.792343 | `group_list` | ❌ |

---

## Test 346

**Expected Tool:** `redis_list`  
**Prompt:** Show me my Redis caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.829540 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.792130 | `redis_create` | ❌ |
| 3 | 0.761341 | `postgres_database_list` | ❌ |
| 4 | 0.749666 | `postgres_table_list` | ❌ |
| 5 | 0.743058 | `quota_usage_check` | ❌ |

---

## Test 347

**Expected Tool:** `redis_list`  
**Prompt:** Get Redis clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789084 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.783134 | `redis_create` | ❌ |
| 3 | 0.782061 | `kusto_cluster_list` | ❌ |
| 4 | 0.768549 | `aks_cluster_get` | ❌ |
| 5 | 0.757925 | `kusto_database_list` | ❌ |

---

## Test 348

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

## Test 349

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

## Test 350

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

## Test 351

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.840651 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.821593 | `resourcehealth_availability-status_list` | ❌ |
| 3 | 0.767782 | `quota_usage_check` | ❌ |
| 4 | 0.756824 | `monitor_metrics_definitions` | ❌ |
| 5 | 0.752841 | `monitor_healthmodels_entity_get` | ❌ |

---

## Test 352

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.844127 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.824312 | `storage_account_get` | ❌ |
| 3 | 0.821574 | `storage_blob_container_get` | ❌ |
| 4 | 0.813201 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.807901 | `quota_usage_check` | ❌ |

---

## Test 353

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.838295 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.832085 | `resourcehealth_availability-status_list` | ❌ |
| 3 | 0.791186 | `quota_usage_check` | ❌ |
| 4 | 0.771561 | `applens_resource_diagnose` | ❌ |
| 5 | 0.770734 | `managedlustre_fs_list` | ❌ |

---

## Test 354

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.909545 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.852783 | `redis_list` | ❌ |
| 3 | 0.838126 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.837929 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.826610 | `search_service_list` | ❌ |

---

## Test 355

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.869075 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.852189 | `quota_usage_check` | ❌ |
| 3 | 0.845670 | `resourcehealth_availability-status_get` | ❌ |
| 4 | 0.822780 | `applens_resource_diagnose` | ❌ |
| 5 | 0.819445 | `loadtesting_testresource_list` | ❌ |

---

## Test 356

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.804274 | `resourcehealth_availability-status_get` | ❌ |
| 2 | 0.802309 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 3 | 0.802099 | `applens_resource_diagnose` | ❌ |
| 4 | 0.785235 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.781158 | `quota_usage_check` | ❌ |

---

## Test 357

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

## Test 358

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.864582 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.844059 | `search_service_list` | ❌ |
| 3 | 0.832410 | `eventgrid_topic_list` | ❌ |
| 4 | 0.825710 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.822477 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 359

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

## Test 360

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

## Test 361

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.835632 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.821663 | `quota_usage_check` | ❌ |
| 3 | 0.817334 | `search_service_list` | ❌ |
| 4 | 0.802165 | `applens_resource_diagnose` | ❌ |
| 5 | 0.800477 | `deploy_app_logs_get` | ❌ |

---

## Test 362

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

## Test 363

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

## Test 364

**Expected Tool:** `servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.840195 | `servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.834069 | `servicebus_topic_details` | ❌ |
| 3 | 0.826504 | `redis_list` | ❌ |
| 4 | 0.823696 | `kusto_cluster_get` | ❌ |
| 5 | 0.815687 | `search_service_list` | ❌ |

---

## Test 365

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

## Test 366

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show me the network information of SignalR runtime <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.839342 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.767355 | `sql_server_show` | ❌ |
| 3 | 0.746593 | `eventhubs_eventhub_get` | ❌ |
| 4 | 0.743524 | `servicebus_topic_details` | ❌ |
| 5 | 0.738830 | `foundry_resource_get` | ❌ |

---

## Test 367

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Describe the SignalR runtime <signalr_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851664 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.771807 | `redis_create` | ❌ |
| 3 | 0.769196 | `foundry_models_deploy` | ❌ |
| 4 | 0.768974 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.766785 | `eventhubs_namespace_get` | ❌ |

---

## Test 368

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Get information about my SignalR runtime <signalr_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.877625 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.780888 | `foundry_resource_get` | ❌ |
| 3 | 0.778225 | `eventhubs_eventhub_get` | ❌ |
| 4 | 0.777934 | `quota_usage_check` | ❌ |
| 5 | 0.776913 | `servicebus_topic_details` | ❌ |

---

## Test 369

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show all the SignalRs information in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.801139 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.788870 | `redis_list` | ❌ |
| 3 | 0.785706 | `group_list` | ❌ |
| 4 | 0.782545 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.773956 | `loadtesting_testresource_list` | ❌ |

---

## Test 370

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

## Test 371

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

## Test 372

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

## Test 373

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

## Test 374

**Expected Tool:** `sql_db_delete`  
**Prompt:** Delete the SQL database <database_name> from server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.826648 | `sql_server_delete` | ❌ |
| 2 | 0.824912 | `sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.767749 | `postgres_database_list` | ❌ |
| 4 | 0.760572 | `mysql_table_list` | ❌ |
| 5 | 0.752778 | `mysql_database_query` | ❌ |

---

## Test 375

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

## Test 376

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

## Test 377

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

## Test 378

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

## Test 379

**Expected Tool:** `sql_db_rename`  
**Prompt:** Rename the SQL database <database_name> on server <server_name> to <new_database_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.820644 | `sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.769441 | `sql_server_delete` | ❌ |
| 3 | 0.766580 | `sql_db_delete` | ❌ |
| 4 | 0.765966 | `mysql_table_list` | ❌ |
| 5 | 0.765664 | `postgres_database_list` | ❌ |

---

## Test 380

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

## Test 381

**Expected Tool:** `sql_db_show`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.841959 | `sql_server_show` | ❌ |
| 2 | 0.838014 | `postgres_server_config_get` | ❌ |
| 3 | 0.833105 | `mysql_server_config_get` | ❌ |
| 4 | 0.800194 | `sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.798840 | `mysql_server_param_get` | ❌ |

---

## Test 382

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

## Test 383

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

## Test 384

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

## Test 385

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

## Test 386

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

## Test 387

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

## Test 388

**Expected Tool:** `sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.864677 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.819256 | `redis_create` | ❌ |
| 3 | 0.811178 | `sql_db_create` | ❌ |
| 4 | 0.809783 | `mysql_server_list` | ❌ |
| 5 | 0.809756 | `storage_account_create` | ❌ |

---

## Test 389

**Expected Tool:** `sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.862810 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.809713 | `sql_server_show` | ❌ |
| 3 | 0.799231 | `sql_db_create` | ❌ |
| 4 | 0.791963 | `storage_account_create` | ❌ |
| 5 | 0.786847 | `sql_server_delete` | ❌ |

---

## Test 390

**Expected Tool:** `sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.833383 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.795420 | `mysql_server_list` | ❌ |
| 3 | 0.791985 | `redis_create` | ❌ |
| 4 | 0.788993 | `sql_server_delete` | ❌ |
| 5 | 0.788158 | `sql_server_show` | ❌ |

---

## Test 391

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

## Test 392

**Expected Tool:** `sql_server_delete`  
**Prompt:** Remove the SQL server <server_name> from my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852916 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.783521 | `postgres_server_list` | ❌ |
| 3 | 0.777659 | `sql_db_delete` | ❌ |
| 4 | 0.752033 | `sql_server_show` | ❌ |
| 5 | 0.747077 | `kusto_cluster_list` | ❌ |

---

## Test 393

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

## Test 394

**Expected Tool:** `sql_server_entra-admin_list`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.898488 | `sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.778346 | `sql_server_show` | ❌ |
| 3 | 0.747307 | `foundry_threads_list` | ❌ |
| 4 | 0.735783 | `mysql_table_list` | ❌ |
| 5 | 0.735595 | `sql_server_list` | ❌ |

---

## Test 395

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

## Test 396

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

## Test 397

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

## Test 398

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.876201 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.802184 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.792933 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.738478 | `mysql_database_query` | ❌ |
| 5 | 0.722482 | `postgres_database_query` | ❌ |

---

## Test 399

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.831918 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.814691 | `sql_server_firewall-rule_delete` | ❌ |
| 3 | 0.806035 | `sql_server_firewall-rule_list` | ❌ |
| 4 | 0.756436 | `mysql_database_query` | ❌ |
| 5 | 0.746850 | `sql_server_create` | ❌ |

---

## Test 400

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

## Test 401

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

## Test 402

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

## Test 403

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

## Test 404

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

## Test 405

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

## Test 406

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

## Test 407

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

## Test 408

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

## Test 409

**Expected Tool:** `sql_server_show`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.864055 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.842651 | `postgres_server_config_get` | ❌ |
| 3 | 0.840563 | `mysql_server_config_get` | ❌ |
| 4 | 0.801763 | `mysql_server_param_get` | ❌ |
| 5 | 0.786885 | `sql_db_show` | ❌ |

---

## Test 410

**Expected Tool:** `sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.849149 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.798681 | `mysql_server_config_get` | ❌ |
| 3 | 0.789909 | `mysql_table_schema_get` | ❌ |
| 4 | 0.783363 | `postgres_database_list` | ❌ |
| 5 | 0.778605 | `mysql_table_list` | ❌ |

---

## Test 411

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.809947 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.803393 | `quota_usage_check` | ❌ |
| 3 | 0.798147 | `storage_blob_container_create` | ❌ |
| 4 | 0.793527 | `storage_account_get` | ❌ |
| 5 | 0.788024 | `redis_create` | ❌ |

---

## Test 412

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.808966 | `redis_create` | ❌ |
| 2 | 0.800424 | `storage_blob_container_create` | ❌ |
| 3 | 0.795156 | `managedlustre_fs_list` | ❌ |
| 4 | 0.794542 | `managedlustre_fs_create` | ❌ |
| 5 | 0.794145 | `storage_account_get` | ❌ |

---

## Test 413

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.792633 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.784383 | `storage_account_get` | ❌ |
| 3 | 0.783429 | `storage_blob_container_create` | ❌ |
| 4 | 0.782223 | `redis_create` | ❌ |
| 5 | 0.771119 | `loadtesting_testresource_create` | ❌ |

---

## Test 414

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

## Test 415

**Expected Tool:** `storage_account_get`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.887388 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.848471 | `storage_blob_container_get` | ❌ |
| 3 | 0.839587 | `storage_account_create` | ❌ |
| 4 | 0.838349 | `storage_blob_get` | ❌ |
| 5 | 0.829422 | `sql_server_show` | ❌ |

---

## Test 416

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

## Test 417

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

## Test 418

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

## Test 419

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.850639 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.834053 | `storage_blob_container_get` | ❌ |
| 3 | 0.802218 | `storage_blob_get` | ❌ |
| 4 | 0.795088 | `storage_account_create` | ❌ |
| 5 | 0.791175 | `cosmos_database_container_list` | ❌ |

---

## Test 420

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

## Test 421

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

## Test 422

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.891728 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.857128 | `storage_blob_get` | ❌ |
| 3 | 0.832367 | `storage_account_get` | ❌ |
| 4 | 0.829749 | `storage_blob_container_create` | ❌ |
| 5 | 0.814633 | `cosmos_database_container_list` | ❌ |

---

## Test 423

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.892410 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.879298 | `storage_blob_get` | ❌ |
| 3 | 0.854342 | `storage_blob_container_create` | ❌ |
| 4 | 0.844717 | `cosmos_database_container_list` | ❌ |
| 5 | 0.829854 | `storage_account_get` | ❌ |

---

## Test 424

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879623 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.847528 | `cosmos_database_container_list` | ❌ |
| 3 | 0.833656 | `storage_blob_get` | ❌ |
| 4 | 0.825182 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.818749 | `storage_account_get` | ❌ |

---

## Test 425

**Expected Tool:** `storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.894876 | `storage_blob_container_get` | ❌ |
| 2 | 0.891593 | `storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.837119 | `storage_blob_container_create` | ❌ |
| 4 | 0.829013 | `storage_account_get` | ❌ |
| 5 | 0.787205 | `cosmos_database_container_list` | ❌ |

---

## Test 426

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

## Test 427

**Expected Tool:** `storage_blob_get`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.893094 | `storage_blob_container_get` | ❌ |
| 2 | 0.890815 | `storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.861403 | `storage_blob_container_create` | ❌ |
| 4 | 0.844959 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.841411 | `cosmos_database_container_list` | ❌ |

---

## Test 428

**Expected Tool:** `storage_blob_get`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.895199 | `storage_blob_container_get` | ❌ |
| 2 | 0.884449 | `storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.847241 | `storage_blob_container_create` | ❌ |
| 4 | 0.827470 | `storage_account_get` | ❌ |
| 5 | 0.819109 | `cosmos_database_container_list` | ❌ |

---

## Test 429

**Expected Tool:** `storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.837264 | `storage_blob_container_create` | ❌ |
| 2 | 0.834825 | `storage_blob_upload` | ✅ **EXPECTED** |
| 3 | 0.822190 | `storage_blob_get` | ❌ |
| 4 | 0.809272 | `storage_blob_container_get` | ❌ |
| 5 | 0.768544 | `storage_account_get` | ❌ |

---

## Test 430

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

## Test 431

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

## Test 432

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

## Test 433

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

## Test 434

**Expected Tool:** `azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879282 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.861340 | `get_bestpractices_get` | ❌ |
| 3 | 0.838960 | `azureaibestpractices_get` | ❌ |
| 4 | 0.814913 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.804409 | `cloudarchitect_design` | ❌ |

---

## Test 435

**Expected Tool:** `azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.821345 | `keyvault_secret_get` | ❌ |
| 2 | 0.812174 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 3 | 0.808736 | `keyvault_secret_create` | ❌ |
| 4 | 0.807316 | `get_bestpractices_get` | ❌ |
| 5 | 0.801636 | `foundry_agents_get-sdk-sample` | ❌ |

---

## Test 436

**Expected Tool:** `virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.875486 | `virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
| 2 | 0.849043 | `virtualdesktop_hostpool_host_list` | ❌ |
| 3 | 0.811700 | `kusto_cluster_list` | ❌ |
| 4 | 0.804164 | `postgres_server_list` | ❌ |
| 5 | 0.802633 | `virtualdesktop_hostpool_host_user-list` | ❌ |

---

## Test 437

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

## Test 438

**Expected Tool:** `virtualdesktop_hostpool_host_user-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.903083 | `virtualdesktop_hostpool_host_user-list` | ✅ **EXPECTED** |
| 2 | 0.852811 | `virtualdesktop_hostpool_host_list` | ❌ |
| 3 | 0.792047 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.754118 | `aks_nodepool_get` | ❌ |
| 5 | 0.742301 | `foundry_threads_list` | ❌ |

---

## Test 439

**Expected Tool:** `workbooks_create`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.814658 | `workbooks_create` | ✅ **EXPECTED** |
| 2 | 0.749806 | `workbooks_update` | ❌ |
| 3 | 0.746902 | `workbooks_delete` | ❌ |
| 4 | 0.741031 | `workbooks_list` | ❌ |
| 5 | 0.734155 | `workbooks_show` | ❌ |

---

## Test 440

**Expected Tool:** `workbooks_delete`  
**Prompt:** Delete the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.870999 | `workbooks_delete` | ✅ **EXPECTED** |
| 2 | 0.800042 | `workbooks_show` | ❌ |
| 3 | 0.775413 | `workbooks_list` | ❌ |
| 4 | 0.767718 | `workbooks_create` | ❌ |
| 5 | 0.760256 | `workbooks_update` | ❌ |

---

## Test 441

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

## Test 442

**Expected Tool:** `workbooks_list`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.831874 | `workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.786614 | `workbooks_create` | ❌ |
| 3 | 0.777631 | `workbooks_delete` | ❌ |
| 4 | 0.770929 | `redis_list` | ❌ |
| 5 | 0.770043 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 443

**Expected Tool:** `workbooks_show`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.885743 | `workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.822054 | `workbooks_create` | ❌ |
| 3 | 0.802642 | `workbooks_list` | ❌ |
| 4 | 0.799931 | `workbooks_delete` | ❌ |
| 5 | 0.792484 | `workbooks_update` | ❌ |

---

## Test 444

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

## Test 445

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

## Test 446

**Expected Tool:** `bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.829298 | `bicepschema_get` | ✅ **EXPECTED** |
| 2 | 0.811738 | `foundry_models_deploy` | ❌ |
| 3 | 0.809275 | `azureaibestpractices_get` | ❌ |
| 4 | 0.805211 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.803405 | `foundry_openai_create-completion` | ❌ |

---

## Test 447

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.839665 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.781938 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.777407 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.768999 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.753698 | `deploy_plan_get` | ❌ |

---

## Test 448

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

## Test 449

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

## Test 450

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.833132 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.796075 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.795825 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.780347 | `speech_stt_recognize` | ❌ |
| 5 | 0.777490 | `quota_usage_check` | ❌ |

---

## Test 451

**Expected Tool:** `foundry_agents_connect`  
**Prompt:** Query an agent in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.867235 | `foundry_agents_connect` | ✅ **EXPECTED** |
| 2 | 0.858626 | `foundry_threads_list` | ❌ |
| 3 | 0.855151 | `foundry_threads_get-messages` | ❌ |
| 4 | 0.848660 | `foundry_agents_list` | ❌ |
| 5 | 0.841828 | `foundry_agents_get-sdk-sample` | ❌ |

---

## Test 452

**Expected Tool:** `foundry_agents_create`  
**Prompt:** Create a new Microsoft Foundry agent using instructions in the active editor  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.877886 | `foundry_agents_get-sdk-sample` | ❌ |
| 2 | 0.875213 | `foundry_agents_create` | ✅ **EXPECTED** |
| 3 | 0.847476 | `foundry_threads_list` | ❌ |
| 4 | 0.837091 | `foundry_threads_create` | ❌ |
| 5 | 0.835023 | `foundry_threads_get-messages` | ❌ |

---

## Test 453

**Expected Tool:** `foundry_agents_evaluate`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.816665 | `foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.805375 | `foundry_agents_connect` | ❌ |
| 3 | 0.774396 | `foundry_agents_evaluate` | ✅ **EXPECTED** |
| 4 | 0.741379 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.731201 | `search_index_query` | ❌ |

---

## Test 454

**Expected Tool:** `foundry_agents_get-sdk-sample`  
**Prompt:** Create a CLI app that can talk to a Microsoft Foundry Agent using Python SDK  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.865434 | `foundry_agents_get-sdk-sample` | ✅ **EXPECTED** |
| 2 | 0.828147 | `foundry_agents_create` | ❌ |
| 3 | 0.810601 | `foundry_threads_get-messages` | ❌ |
| 4 | 0.806691 | `foundry_threads_list` | ❌ |
| 5 | 0.803575 | `foundry_agents_connect` | ❌ |

---

## Test 455

**Expected Tool:** `foundry_agents_list`  
**Prompt:** List all agents in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.874889 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.868465 | `foundry_threads_list` | ❌ |
| 3 | 0.850070 | `foundry_threads_get-messages` | ❌ |
| 4 | 0.831211 | `foundry_openai_models-list` | ❌ |
| 5 | 0.826676 | `foundry_agents_get-sdk-sample` | ❌ |

---

## Test 456

**Expected Tool:** `foundry_agents_list`  
**Prompt:** Show me the available agents in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.868599 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.865799 | `foundry_threads_list` | ❌ |
| 3 | 0.857189 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.848802 | `foundry_threads_get-messages` | ❌ |
| 5 | 0.837519 | `foundry_openai_models-list` | ❌ |

---

## Test 457

**Expected Tool:** `foundry_agents_query-and-evaluate`  
**Prompt:** Query and evaluate an agent in my Microsoft Foundry resource for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.837471 | `foundry_agents_connect` | ❌ |
| 2 | 0.805013 | `foundry_threads_list` | ❌ |
| 3 | 0.802627 | `foundry_agents_create` | ❌ |
| 4 | 0.796484 | `foundry_threads_get-messages` | ❌ |
| 5 | 0.796023 | `foundry_agents_list` | ❌ |

---

## Test 458

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my Microsoft Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.882646 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.825618 | `foundry_agents_list` | ❌ |
| 3 | 0.818500 | `foundry_models_deployments_list` | ❌ |
| 4 | 0.817764 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.817055 | `foundry_resource_get` | ❌ |

---

## Test 459

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** Show me the knowledge indexes in my Microsoft Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.845938 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.838416 | `foundry_agents_get-sdk-sample` | ❌ |
| 3 | 0.815401 | `foundry_knowledge_index_schema` | ❌ |
| 4 | 0.814724 | `foundry_resource_get` | ❌ |
| 5 | 0.812493 | `foundry_models_deployments_list` | ❌ |

---

## Test 460

**Expected Tool:** `foundry_knowledge_index_schema`  
**Prompt:** Show me the schema for knowledge index <index-name> in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.867230 | `foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.823041 | `foundry_resource_get` | ❌ |
| 3 | 0.807527 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.804298 | `kusto_table_schema` | ❌ |
| 5 | 0.800012 | `foundry_agents_get-sdk-sample` | ❌ |

---

## Test 461

**Expected Tool:** `foundry_knowledge_index_schema`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852073 | `foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.784873 | `kusto_table_schema` | ❌ |
| 3 | 0.784505 | `postgres_table_schema_get` | ❌ |
| 4 | 0.778622 | `postgres_server_config_get` | ❌ |
| 5 | 0.757097 | `search_index_get` | ❌ |

---

## Test 462

**Expected Tool:** `foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.845092 | `foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.752056 | `foundry_openai_embeddings-create` | ❌ |
| 3 | 0.749333 | `redis_create` | ❌ |
| 4 | 0.744135 | `loadtesting_testrun_create` | ❌ |
| 5 | 0.744059 | `loadtesting_testresource_create` | ❌ |

---

## Test 463

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** List all Microsoft Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.871219 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.867883 | `foundry_openai_models-list` | ❌ |
| 3 | 0.844224 | `foundry_resource_get` | ❌ |
| 4 | 0.842227 | `foundry_models_deploy` | ❌ |
| 5 | 0.836863 | `foundry_agents_create` | ❌ |

---

## Test 464

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** Show me all Microsoft Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.865766 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.857345 | `foundry_openai_models-list` | ❌ |
| 3 | 0.839798 | `foundry_resource_get` | ❌ |
| 4 | 0.838284 | `foundry_models_deploy` | ❌ |
| 5 | 0.832294 | `foundry_agents_get-sdk-sample` | ❌ |

---

## Test 465

**Expected Tool:** `foundry_models_list`  
**Prompt:** List all Microsoft Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.841117 | `foundry_threads_list` | ❌ |
| 2 | 0.840300 | `foundry_openai_models-list` | ❌ |
| 3 | 0.839827 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.839404 | `foundry_models_list` | ✅ **EXPECTED** |
| 5 | 0.839232 | `foundry_resource_get` | ❌ |

---

## Test 466

**Expected Tool:** `foundry_models_list`  
**Prompt:** Show me the available Microsoft Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.856844 | `foundry_agents_get-sdk-sample` | ❌ |
| 2 | 0.835485 | `foundry_models_list` | ✅ **EXPECTED** |
| 3 | 0.833273 | `foundry_resource_get` | ❌ |
| 4 | 0.829116 | `foundry_threads_list` | ❌ |
| 5 | 0.828666 | `foundry_openai_models-list` | ❌ |

---

## Test 467

**Expected Tool:** `foundry_openai_chat-completions-create`  
**Prompt:** Create a chat completion with the message "Hello, how are you today?" using my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879420 | `foundry_openai_chat-completions-create` | ✅ **EXPECTED** |
| 2 | 0.852761 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.814358 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.796340 | `foundry_threads_get-messages` | ❌ |
| 5 | 0.794456 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 468

**Expected Tool:** `foundry_openai_create-completion`  
**Prompt:** Create a completion with the prompt "What is Azure?" using my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.894548 | `foundry_openai_create-completion` | ✅ **EXPECTED** |
| 2 | 0.857348 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.816217 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.802719 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.788438 | `foundry_resource_get` | ❌ |

---

## Test 469

**Expected Tool:** `foundry_openai_embeddings-create`  
**Prompt:** Generate embeddings for the text "Azure OpenAI Service" using my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.929721 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.868312 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.846850 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.844258 | `foundry_models_deploy` | ❌ |
| 5 | 0.838538 | `foundry_openai_models-list` | ❌ |

---

## Test 470

**Expected Tool:** `foundry_openai_embeddings-create`  
**Prompt:** Create vector embeddings for my text using my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.911299 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.825539 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.808836 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.807193 | `foundry_models_deploy` | ❌ |
| 5 | 0.803516 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 471

**Expected Tool:** `foundry_openai_models-list`  
**Prompt:** List all available OpenAI models in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.910699 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.859678 | `foundry_models_deploy` | ❌ |
| 3 | 0.855422 | `foundry_resource_get` | ❌ |
| 4 | 0.854975 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.854210 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 472

**Expected Tool:** `foundry_openai_models-list`  
**Prompt:** Show me the OpenAI model deployments in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.907173 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.883055 | `foundry_models_deploy` | ❌ |
| 3 | 0.877723 | `foundry_models_deployments_list` | ❌ |
| 4 | 0.872315 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.860925 | `foundry_openai_create-completion` | ❌ |

---

## Test 473

**Expected Tool:** `foundry_resource_get`  
**Prompt:** List all Microsoft Foundry resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.864306 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.842775 | `foundry_openai_models-list` | ❌ |
| 3 | 0.823936 | `redis_list` | ❌ |
| 4 | 0.823880 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.823588 | `foundry_threads_list` | ❌ |

---

## Test 474

**Expected Tool:** `foundry_resource_get`  
**Prompt:** Show me the Microsoft Foundry resources in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.837913 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.826237 | `foundry_openai_models-list` | ❌ |
| 3 | 0.817387 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.815888 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.815298 | `foundry_models_deploy` | ❌ |

---

## Test 475

**Expected Tool:** `foundry_resource_get`  
**Prompt:** Get details for Microsoft Foundry resource <resource_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.857772 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.833423 | `foundry_openai_models-list` | ❌ |
| 3 | 0.801578 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.800082 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.796833 | `foundry_agents_list` | ❌ |

---

## Test 476

**Expected Tool:** `foundry_threads_create`  
**Prompt:** Create a Microsoft Foundry thread to hold the conversation  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.892131 | `foundry_threads_create` | ✅ **EXPECTED** |
| 2 | 0.887435 | `foundry_threads_get-messages` | ❌ |
| 3 | 0.878656 | `foundry_threads_list` | ❌ |
| 4 | 0.822267 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.815628 | `foundry_agents_create` | ❌ |

---

## Test 477

**Expected Tool:** `foundry_threads_get-messages`  
**Prompt:** Show me the messages in the Microsoft Foundry thread with id <thread_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.905501 | `foundry_threads_get-messages` | ✅ **EXPECTED** |
| 2 | 0.867787 | `foundry_threads_list` | ❌ |
| 3 | 0.839016 | `foundry_threads_create` | ❌ |
| 4 | 0.810627 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.796806 | `foundry_agents_connect` | ❌ |

---

## Test 478

**Expected Tool:** `foundry_threads_list`  
**Prompt:** List my Microsoft Foundry threads  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.920093 | `foundry_threads_list` | ✅ **EXPECTED** |
| 2 | 0.888431 | `foundry_threads_get-messages` | ❌ |
| 3 | 0.834885 | `foundry_threads_create` | ❌ |
| 4 | 0.830549 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.795629 | `foundry_agents_connect` | ❌ |

---

## Summary

**Total Prompts Tested:** 478  
**Analysis Execution Time:** 418.3437261s  

### Success Rate Metrics

**Top Choice Success:** 83.5% (399/478 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 92.3% (441/478 tests)  
**🎯 High Confidence (≥0.7):** 99.6% (476/478 tests)  
**✅ Good Confidence (≥0.6):** 99.6% (476/478 tests)  
**👍 Fair Confidence (≥0.5):** 99.6% (476/478 tests)  
**👌 Acceptable Confidence (≥0.4):** 99.6% (476/478 tests)  
**❌ Low Confidence (<0.4):** 0.4% (2/478 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 78.9% (377/478 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 83.5% (399/478 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 83.5% (399/478 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 83.5% (399/478 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 83.5% (399/478 tests)  

### Success Rate Analysis

🟡 **Good** - The tool selection system is performing well.

🎯 **Recommendation:** Tool descriptions are highly optimized for user intent matching.

