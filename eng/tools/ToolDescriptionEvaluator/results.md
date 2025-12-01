# Tool Selection Analysis Setup

**Setup completed:** 2025-11-25 16:22:52  
**Tool count:** 182  
**Database setup time:** 3.6353132s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-11-25 16:22:52  
**Tool count:** 182  

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
- [Test 434: azmcp_tables_list](#test-434)
- [Test 435: azmcp_tables_list](#test-435)
- [Test 436: azureterraformbestpractices_get](#test-436)
- [Test 437: azureterraformbestpractices_get](#test-437)
- [Test 438: virtualdesktop_hostpool_list](#test-438)
- [Test 439: virtualdesktop_hostpool_host_list](#test-439)
- [Test 440: virtualdesktop_hostpool_host_user-list](#test-440)
- [Test 441: workbooks_create](#test-441)
- [Test 442: workbooks_delete](#test-442)
- [Test 443: workbooks_list](#test-443)
- [Test 444: workbooks_list](#test-444)
- [Test 445: workbooks_show](#test-445)
- [Test 446: workbooks_show](#test-446)
- [Test 447: workbooks_update](#test-447)
- [Test 448: bicepschema_get](#test-448)
- [Test 449: cloudarchitect_design](#test-449)
- [Test 450: cloudarchitect_design](#test-450)
- [Test 451: cloudarchitect_design](#test-451)
- [Test 452: cloudarchitect_design](#test-452)
- [Test 453: foundry_agents_connect](#test-453)
- [Test 454: foundry_agents_create](#test-454)
- [Test 455: foundry_agents_evaluate](#test-455)
- [Test 456: foundry_agents_get-sdk-sample](#test-456)
- [Test 457: foundry_agents_list](#test-457)
- [Test 458: foundry_agents_list](#test-458)
- [Test 459: foundry_agents_query-and-evaluate](#test-459)
- [Test 460: foundry_knowledge_index_list](#test-460)
- [Test 461: foundry_knowledge_index_list](#test-461)
- [Test 462: foundry_knowledge_index_schema](#test-462)
- [Test 463: foundry_knowledge_index_schema](#test-463)
- [Test 464: foundry_models_deploy](#test-464)
- [Test 465: foundry_models_deployments_list](#test-465)
- [Test 466: foundry_models_deployments_list](#test-466)
- [Test 467: foundry_models_list](#test-467)
- [Test 468: foundry_models_list](#test-468)
- [Test 469: foundry_openai_chat-completions-create](#test-469)
- [Test 470: foundry_openai_create-completion](#test-470)
- [Test 471: foundry_openai_embeddings-create](#test-471)
- [Test 472: foundry_openai_embeddings-create](#test-472)
- [Test 473: foundry_openai_models-list](#test-473)
- [Test 474: foundry_openai_models-list](#test-474)
- [Test 475: foundry_resource_get](#test-475)
- [Test 476: foundry_resource_get](#test-476)
- [Test 477: foundry_resource_get](#test-477)
- [Test 478: foundry_threads_create](#test-478)
- [Test 479: foundry_threads_get-messages](#test-479)
- [Test 480: foundry_threads_list](#test-480)

---

## Test 1

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Get best practices for building AI applications in Azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663467 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.555579 | `get_bestpractices_get` | ❌ |
| 3 | 0.501210 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.480235 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.477592 | `cloudarchitect_design` | ❌ |

---

## Test 2

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Show me the best practices for Microsoft Foundry agents code generation  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657782 | `foundry_agents_get-sdk-sample` | ❌ |
| 2 | 0.547248 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 3 | 0.507392 | `foundry_agents_create` | ❌ |
| 4 | 0.483323 | `foundry_threads_list` | ❌ |
| 5 | 0.471393 | `foundry_threads_create` | ❌ |

---

## Test 3

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Get guidance for building agents with Microsoft Foundry  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.667712 | `foundry_agents_get-sdk-sample` | ❌ |
| 2 | 0.605102 | `foundry_agents_create` | ❌ |
| 3 | 0.534050 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 4 | 0.513150 | `foundry_threads_create` | ❌ |
| 5 | 0.498635 | `foundry_threads_list` | ❌ |

---

## Test 4

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Create an AI app that helps me to manage travel queries.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.421944 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.314523 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.309426 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.297481 | `applens_resource_diagnose` | ❌ |
| 5 | 0.294646 | `cloudarchitect_design` | ❌ |

---

## Test 5

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Create an AI app that helps me to manage travel queries in Microsoft Foundry  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500471 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.476439 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.473301 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.465270 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.438479 | `foundry_agents_list` | ❌ |

---

## Test 6

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.785967 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.700824 | `search_knowledge_source_get` | ❌ |
| 3 | 0.693471 | `search_service_list` | ❌ |
| 4 | 0.635863 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.586575 | `search_index_get` | ❌ |

---

## Test 7

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.748213 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.668487 | `search_knowledge_source_get` | ❌ |
| 3 | 0.628582 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.624480 | `search_service_list` | ❌ |
| 5 | 0.566618 | `search_index_get` | ❌ |

---

## Test 8

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702942 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.605964 | `search_knowledge_source_get` | ❌ |
| 3 | 0.583234 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.513638 | `search_service_list` | ❌ |
| 5 | 0.471301 | `foundry_knowledge_index_list` | ❌ |

---

## Test 9

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688051 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.599247 | `search_knowledge_source_get` | ❌ |
| 3 | 0.578499 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.457619 | `search_service_list` | ❌ |
| 5 | 0.439996 | `foundry_knowledge_index_list` | ❌ |

---

## Test 10

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Get the details of knowledge base <agent-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769383 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.685640 | `search_knowledge_source_get` | ❌ |
| 3 | 0.636958 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.585949 | `search_index_get` | ❌ |
| 5 | 0.533701 | `search_service_list` | ❌ |

---

## Test 11

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

## Test 12

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in Azure AI Search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.724874 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.650636 | `search_knowledge_base_get` | ❌ |
| 3 | 0.575307 | `search_index_query` | ❌ |
| 4 | 0.567454 | `search_knowledge_source_get` | ❌ |
| 5 | 0.480177 | `search_service_list` | ❌ |

---

## Test 13

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633766 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.589869 | `search_knowledge_base_get` | ❌ |
| 3 | 0.502085 | `search_knowledge_source_get` | ❌ |
| 4 | 0.422547 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.399595 | `search_index_query` | ❌ |

---

## Test 14

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657866 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.557206 | `search_knowledge_base_get` | ❌ |
| 3 | 0.463605 | `search_knowledge_source_get` | ❌ |
| 4 | 0.436627 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.399079 | `foundry_agents_connect` | ❌ |

---

## Test 15

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633766 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.589869 | `search_knowledge_base_get` | ❌ |
| 3 | 0.502085 | `search_knowledge_source_get` | ❌ |
| 4 | 0.422547 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.399595 | `search_index_query` | ❌ |

---

## Test 16

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Query knowledge base <agent-name> in search service <service-name> about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.598868 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.547862 | `search_knowledge_base_get` | ❌ |
| 3 | 0.467822 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.464904 | `search_knowledge_source_get` | ❌ |
| 5 | 0.414323 | `foundry_agents_connect` | ❌ |

---

## Test 17

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Search knowledge base <agent-name> in Azure AI Search service <service-name> for <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649767 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.631435 | `search_knowledge_base_get` | ❌ |
| 3 | 0.581386 | `search_index_query` | ❌ |
| 4 | 0.571156 | `search_knowledge_source_get` | ❌ |
| 5 | 0.544501 | `search_service_list` | ❌ |

---

## Test 18

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** What does knowledge base <agent-name> in search service <service-name> know about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579716 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.560688 | `search_knowledge_base_get` | ❌ |
| 3 | 0.477941 | `search_knowledge_source_get` | ❌ |
| 4 | 0.402510 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.365204 | `foundry_agents_connect` | ❌ |

---

## Test 19

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Find information about <query> using knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582662 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.528610 | `search_knowledge_base_get` | ❌ |
| 3 | 0.449336 | `search_knowledge_source_get` | ❌ |
| 4 | 0.447679 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.401030 | `foundry_agents_connect` | ❌ |

---

## Test 20

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.760416 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.691931 | `search_service_list` | ❌ |
| 3 | 0.665922 | `search_knowledge_base_get` | ❌ |
| 4 | 0.573012 | `search_index_get` | ❌ |
| 5 | 0.560779 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 21

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737860 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.660169 | `search_service_list` | ❌ |
| 3 | 0.652969 | `search_knowledge_base_get` | ❌ |
| 4 | 0.578836 | `search_index_get` | ❌ |
| 5 | 0.560564 | `search_index_query` | ❌ |

---

## Test 22

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657936 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.558516 | `search_knowledge_base_get` | ❌ |
| 3 | 0.511469 | `search_service_list` | ❌ |
| 4 | 0.470560 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.435554 | `foundry_knowledge_index_list` | ❌ |

---

## Test 23

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

## Test 24

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Get the details of knowledge source <source-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.825628 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.693467 | `search_knowledge_base_get` | ❌ |
| 3 | 0.595548 | `search_index_get` | ❌ |
| 4 | 0.540529 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.531166 | `search_service_list` | ❌ |

---

## Test 25

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

## Test 26

**Expected Tool:** `search_index_get`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.681052 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.528153 | `search_knowledge_base_get` | ❌ |
| 3 | 0.521765 | `search_knowledge_source_get` | ❌ |
| 4 | 0.509719 | `foundry_knowledge_index_schema` | ❌ |
| 5 | 0.490625 | `search_service_list` | ❌ |

---

## Test 27

**Expected Tool:** `search_index_get`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640256 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.620140 | `search_service_list` | ❌ |
| 3 | 0.538456 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.511485 | `search_knowledge_base_get` | ❌ |
| 5 | 0.496094 | `search_knowledge_source_get` | ❌ |

---

## Test 28

**Expected Tool:** `search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620759 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.562775 | `search_service_list` | ❌ |
| 3 | 0.543811 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.500365 | `search_knowledge_base_get` | ❌ |
| 5 | 0.490025 | `search_knowledge_source_get` | ❌ |

---

## Test 29

**Expected Tool:** `search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522826 | `search_index_get` | ❌ |
| 2 | 0.515870 | `search_index_query` | ✅ **EXPECTED** |
| 3 | 0.497467 | `search_service_list` | ❌ |
| 4 | 0.447977 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.437715 | `postgres_database_query` | ❌ |

---

## Test 30

**Expected Tool:** `search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793651 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.552853 | `kusto_cluster_list` | ❌ |
| 3 | 0.509542 | `subscription_list` | ❌ |
| 4 | 0.505971 | `search_index_get` | ❌ |
| 5 | 0.504693 | `marketplace_product_list` | ❌ |

---

## Test 31

**Expected Tool:** `search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686140 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.484092 | `marketplace_product_list` | ❌ |
| 3 | 0.479898 | `search_index_get` | ❌ |
| 4 | 0.462337 | `search_knowledge_base_get` | ❌ |
| 5 | 0.461637 | `kusto_cluster_list` | ❌ |

---

## Test 32

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

## Test 33

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert this audio file to text using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682065 | `speech_tts_synthesize` | ❌ |
| 2 | 0.666038 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 3 | 0.381443 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.351127 | `deploy_plan_get` | ❌ |
| 5 | 0.340581 | `azureaibestpractices_get` | ❌ |

---

## Test 34

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Recognize speech from my audio file with language detection  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.511324 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.344404 | `speech_tts_synthesize` | ❌ |
| 3 | 0.200748 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.181312 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.175779 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 35

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe speech from audio file <file_path> with profanity filtering  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.486489 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.335116 | `speech_tts_synthesize` | ❌ |
| 3 | 0.167783 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.164583 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.156850 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 36

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text from audio file <file_path> using endpoint <endpoint>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611992 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.573185 | `speech_tts_synthesize` | ❌ |
| 3 | 0.319169 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.251320 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.246500 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 37

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe the audio file <file_path> in Spanish language  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.410533 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.353783 | `speech_tts_synthesize` | ❌ |
| 3 | 0.159181 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.158411 | `foundry_models_deploy` | ❌ |
| 5 | 0.151632 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 38

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text with detailed output format from audio file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546259 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.480203 | `speech_tts_synthesize` | ❌ |
| 3 | 0.216567 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.200334 | `foundry_resource_get` | ❌ |
| 5 | 0.183420 | `extension_azqr` | ❌ |

---

## Test 39

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Recognize speech from <file_path> with phrase hints for better accuracy  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.539963 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.367401 | `speech_tts_synthesize` | ❌ |
| 3 | 0.240278 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.209541 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.205136 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 40

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe audio using multiple phrase hints: "Azure", "cognitive services", "machine learning"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549151 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.468161 | `speech_tts_synthesize` | ❌ |
| 3 | 0.393308 | `azureaibestpractices_get` | ❌ |
| 4 | 0.342537 | `extension_cli_generate` | ❌ |
| 5 | 0.337387 | `cloudarchitect_design` | ❌ |

---

## Test 41

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text with comma-separated phrase hints: "Azure, cognitive services, API"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532536 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.515532 | `speech_tts_synthesize` | ❌ |
| 3 | 0.349355 | `azureaibestpractices_get` | ❌ |
| 4 | 0.340575 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.335038 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 42

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe audio with raw profanity output from file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.453396 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.322710 | `speech_tts_synthesize` | ❌ |
| 3 | 0.173895 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.173205 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.168275 | `foundry_openai_create-completion` | ❌ |

---

## Test 43

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Convert text to speech and save to output.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521797 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.422457 | `speech_stt_recognize` | ❌ |
| 3 | 0.208124 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.194603 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.181208 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 44

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

## Test 45

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Generate speech audio from text "Hello world" using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592156 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.534001 | `speech_stt_recognize` | ❌ |
| 3 | 0.341214 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.337528 | `azureaibestpractices_get` | ❌ |
| 5 | 0.326670 | `foundry_openai_create-completion` | ❌ |

---

## Test 46

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Convert text to speech with Spanish language and save to spanish-audio.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.501096 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.452648 | `speech_stt_recognize` | ❌ |
| 3 | 0.214674 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.193771 | `foundry_models_deploy` | ❌ |
| 5 | 0.192473 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 47

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Synthesize speech with voice en-US-JennyNeural from text "Azure AI Services"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604878 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.496715 | `speech_stt_recognize` | ❌ |
| 3 | 0.407368 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.383405 | `azureaibestpractices_get` | ❌ |
| 5 | 0.369146 | `foundry_openai_create-completion` | ❌ |

---

## Test 48

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Create MP3 audio file from text "Welcome to Azure" with high quality format  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.561284 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.510909 | `speech_stt_recognize` | ❌ |
| 3 | 0.352091 | `azureaibestpractices_get` | ❌ |
| 4 | 0.351339 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.347597 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 49

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Generate speech with custom voice model using endpoint ID <endpoint-id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527294 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.455734 | `speech_stt_recognize` | ❌ |
| 3 | 0.348553 | `foundry_models_deploy` | ❌ |
| 4 | 0.339510 | `foundry_resource_get` | ❌ |
| 5 | 0.335994 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 50

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Convert text to OGG/Opus format audio file  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.432836 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.410086 | `speech_stt_recognize` | ❌ |
| 3 | 0.251789 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.196153 | `extension_cli_generate` | ❌ |
| 5 | 0.183982 | `foundry_openai_create-completion` | ❌ |

---

## Test 51

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Synthesize long text content to audio file with streaming  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.428079 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.369045 | `speech_stt_recognize` | ❌ |
| 3 | 0.235175 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.220111 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.217152 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 52

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Create audio file from text in French language with appropriate voice  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.444444 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.385267 | `speech_stt_recognize` | ❌ |
| 3 | 0.236455 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.232592 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.219091 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 53

**Expected Tool:** `appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786351 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.530613 | `appconfig_kv_get` | ❌ |
| 3 | 0.491380 | `postgres_server_list` | ❌ |
| 4 | 0.481113 | `kusto_cluster_list` | ❌ |
| 5 | 0.480061 | `subscription_list` | ❌ |

---

## Test 54

**Expected Tool:** `appconfig_account_list`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634962 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.464865 | `appconfig_kv_get` | ❌ |
| 3 | 0.398530 | `subscription_list` | ❌ |
| 4 | 0.391262 | `redis_list` | ❌ |
| 5 | 0.372456 | `postgres_server_list` | ❌ |

---

## Test 55

**Expected Tool:** `appconfig_account_list`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565393 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.465344 | `appconfig_kv_get` | ❌ |
| 3 | 0.355916 | `postgres_server_config_get` | ❌ |
| 4 | 0.348954 | `appconfig_kv_delete` | ❌ |
| 5 | 0.327234 | `appconfig_kv_set` | ❌ |

---

## Test 56

**Expected Tool:** `appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618285 | `appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.464358 | `appconfig_kv_get` | ❌ |
| 3 | 0.424344 | `appconfig_kv_set` | ❌ |
| 4 | 0.422700 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.391989 | `appconfig_account_list` | ❌ |

---

## Test 57

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.632687 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.557806 | `appconfig_account_list` | ❌ |
| 3 | 0.530884 | `appconfig_kv_set` | ❌ |
| 4 | 0.464873 | `appconfig_kv_delete` | ❌ |
| 5 | 0.439089 | `appconfig_kv_lock_set` | ❌ |

---

## Test 58

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612555 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.522428 | `appconfig_account_list` | ❌ |
| 3 | 0.512945 | `appconfig_kv_set` | ❌ |
| 4 | 0.468775 | `appconfig_kv_delete` | ❌ |
| 5 | 0.457866 | `appconfig_kv_lock_set` | ❌ |

---

## Test 59

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** List all key-value settings with key name starting with 'prod-' in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512883 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.449896 | `appconfig_account_list` | ❌ |
| 3 | 0.398684 | `appconfig_kv_set` | ❌ |
| 4 | 0.380781 | `appconfig_kv_delete` | ❌ |
| 5 | 0.346166 | `appconfig_kv_lock_set` | ❌ |

---

## Test 60

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552300 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.448912 | `appconfig_kv_set` | ❌ |
| 3 | 0.441804 | `appconfig_kv_delete` | ❌ |
| 4 | 0.437406 | `appconfig_account_list` | ❌ |
| 5 | 0.416264 | `appconfig_kv_lock_set` | ❌ |

---

## Test 61

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591237 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.487174 | `appconfig_kv_get` | ❌ |
| 3 | 0.445551 | `appconfig_kv_set` | ❌ |
| 4 | 0.431655 | `appconfig_kv_delete` | ❌ |
| 5 | 0.373658 | `appconfig_account_list` | ❌ |

---

## Test 62

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555699 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.505681 | `appconfig_kv_get` | ❌ |
| 3 | 0.476645 | `appconfig_kv_delete` | ❌ |
| 4 | 0.425488 | `appconfig_kv_set` | ❌ |
| 5 | 0.409411 | `appconfig_account_list` | ❌ |

---

## Test 63

**Expected Tool:** `appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609635 | `appconfig_kv_set` | ✅ **EXPECTED** |
| 2 | 0.536497 | `appconfig_kv_lock_set` | ❌ |
| 3 | 0.512707 | `appconfig_kv_get` | ❌ |
| 4 | 0.505552 | `appconfig_kv_delete` | ❌ |
| 5 | 0.377882 | `appconfig_account_list` | ❌ |

---

## Test 64

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

## Test 65

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

## Test 66

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492820 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.256325 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.242574 | `cloudarchitect_design` | ❌ |
| 4 | 0.225608 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.211565 | `deploy_app_logs_get` | ❌ |

---

## Test 67

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection <connection_string> to my app service <app_name> for database <database_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.717887 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.401337 | `sql_db_rename` | ❌ |
| 3 | 0.399834 | `sql_db_create` | ❌ |
| 4 | 0.362889 | `sql_db_show` | ❌ |
| 5 | 0.357819 | `sql_db_list` | ❌ |

---

## Test 68

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure SQL Server database <database_name> for app service <app_name> with connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688360 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.498156 | `sql_db_rename` | ❌ |
| 3 | 0.496984 | `sql_db_create` | ❌ |
| 4 | 0.469511 | `sql_db_show` | ❌ |
| 5 | 0.453110 | `sql_db_list` | ❌ |

---

## Test 69

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add MySQL database <database_name> to app service <app_name> using connection <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674541 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.464349 | `sql_db_create` | ❌ |
| 3 | 0.452110 | `sql_db_rename` | ❌ |
| 4 | 0.433229 | `mysql_server_list` | ❌ |
| 5 | 0.409832 | `sql_db_show` | ❌ |

---

## Test 70

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add PostgreSQL database <database_name> to app service <app_name> using connection <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627734 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.444219 | `sql_db_create` | ❌ |
| 3 | 0.404766 | `postgres_database_query` | ❌ |
| 4 | 0.401197 | `postgres_database_list` | ❌ |
| 5 | 0.400681 | `sql_db_rename` | ❌ |

---

## Test 71

**Expected Tool:** `appservice_database_add`  
**Prompt:** Connect CosmosDB database <database_name> using connection string <connection_string> to app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662858 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.446542 | `cosmos_database_list` | ❌ |
| 3 | 0.442030 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.427438 | `cosmos_database_container_list` | ❌ |
| 5 | 0.420468 | `sql_db_rename` | ❌ |

---

## Test 72

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection <connection_string> for database <database_name> on server <database_server> to app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.733775 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.454087 | `sql_db_create` | ❌ |
| 3 | 0.415274 | `sql_db_rename` | ❌ |
| 4 | 0.414045 | `sql_server_create` | ❌ |
| 5 | 0.410247 | `sql_db_list` | ❌ |

---

## Test 73

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection string for <database_name> to app service <app_name> using connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.746370 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.441616 | `sql_db_rename` | ❌ |
| 3 | 0.433997 | `sql_db_create` | ❌ |
| 4 | 0.391335 | `sql_db_list` | ❌ |
| 5 | 0.390203 | `sql_db_show` | ❌ |

---

## Test 74

**Expected Tool:** `appservice_database_add`  
**Prompt:** Connect database <database_name> to my app service <app_name> using connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680400 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.429317 | `sql_db_rename` | ❌ |
| 3 | 0.406240 | `sql_db_create` | ❌ |
| 4 | 0.396522 | `sql_db_show` | ❌ |
| 5 | 0.391447 | `sql_db_list` | ❌ |

---

## Test 75

**Expected Tool:** `appservice_database_add`  
**Prompt:** Set up database <database_name> for app service <app_name> with connection string <connection_string> under resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640547 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.456353 | `sql_db_create` | ❌ |
| 3 | 0.402743 | `sql_db_rename` | ❌ |
| 4 | 0.402138 | `sql_db_show` | ❌ |
| 5 | 0.394242 | `sql_db_list` | ❌ |

---

## Test 76

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure database <database_name> for app service <app_name> with the connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688347 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.449170 | `sql_db_rename` | ❌ |
| 3 | 0.448119 | `sql_db_create` | ❌ |
| 4 | 0.414329 | `sql_db_show` | ❌ |
| 5 | 0.411805 | `sql_db_list` | ❌ |

---

## Test 77

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572473 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.451489 | `azureaibestpractices_get` | ❌ |
| 3 | 0.445157 | `get_bestpractices_get` | ❌ |
| 4 | 0.390478 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.383948 | `applens_resource_diagnose` | ❌ |

---

## Test 78

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** Show me code optimization recommendations for all Application Insights resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696531 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.508180 | `azureaibestpractices_get` | ❌ |
| 3 | 0.468384 | `get_bestpractices_get` | ❌ |
| 4 | 0.452231 | `applens_resource_diagnose` | ❌ |
| 5 | 0.435241 | `azureterraformbestpractices_get` | ❌ |

---

## Test 79

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626722 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.488002 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.479392 | `mysql_server_list` | ❌ |
| 4 | 0.477396 | `applens_resource_diagnose` | ❌ |
| 5 | 0.468847 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 80

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509502 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.434147 | `azureaibestpractices_get` | ❌ |
| 3 | 0.419670 | `applens_resource_diagnose` | ❌ |
| 4 | 0.383767 | `get_bestpractices_get` | ❌ |
| 5 | 0.367278 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 81

**Expected Tool:** `extension_cli_generate`  
**Prompt:** Create a Storage account with name <storage_account_name> using Azure CLI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593241 | `storage_account_create` | ❌ |
| 2 | 0.564940 | `storage_blob_container_create` | ❌ |
| 3 | 0.493684 | `storage_account_get` | ❌ |
| 4 | 0.474398 | `storage_blob_container_get` | ❌ |
| 5 | 0.470505 | `redis_create` | ❌ |

---

## Test 82

**Expected Tool:** `extension_cli_generate`  
**Prompt:** List all virtual machines in my subscription using Azure CLI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593467 | `search_service_list` | ❌ |
| 2 | 0.575167 | `kusto_cluster_list` | ❌ |
| 3 | 0.549802 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.544412 | `monitor_workspace_list` | ❌ |
| 5 | 0.536283 | `subscription_list` | ❌ |

---

## Test 83

**Expected Tool:** `extension_cli_generate`  
**Prompt:** Show me the details of the storage account <account_name> with Azure CLI commands  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710307 | `storage_account_get` | ❌ |
| 2 | 0.602173 | `storage_blob_container_get` | ❌ |
| 3 | 0.543268 | `storage_blob_get` | ❌ |
| 4 | 0.524045 | `tables_list` | ❌ |
| 5 | 0.519788 | `storage_account_create` | ❌ |

---

## Test 84

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

## Test 85

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

## Test 86

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

## Test 87

**Expected Tool:** `acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743568 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.711580 | `acr_registry_repository_list` | ❌ |
| 3 | 0.585599 | `kusto_cluster_list` | ❌ |
| 4 | 0.541506 | `search_service_list` | ❌ |
| 5 | 0.531536 | `tables_list` | ❌ |

---

## Test 88

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586014 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563636 | `acr_registry_repository_list` | ❌ |
| 3 | 0.460544 | `storage_blob_container_get` | ❌ |
| 4 | 0.415552 | `cosmos_database_container_list` | ❌ |
| 5 | 0.402253 | `redis_list` | ❌ |

---

## Test 89

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.637099 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563460 | `acr_registry_repository_list` | ❌ |
| 3 | 0.516606 | `kusto_cluster_list` | ❌ |
| 4 | 0.515343 | `storage_blob_container_get` | ❌ |
| 5 | 0.480256 | `redis_list` | ❌ |

---

## Test 90

**Expected Tool:** `acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654318 | `acr_registry_repository_list` | ❌ |
| 2 | 0.633938 | `acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.476015 | `mysql_server_list` | ❌ |
| 4 | 0.454929 | `group_list` | ❌ |
| 5 | 0.454003 | `datadog_monitoredresources_list` | ❌ |

---

## Test 91

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639391 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.637972 | `acr_registry_repository_list` | ❌ |
| 3 | 0.468028 | `mysql_server_list` | ❌ |
| 4 | 0.449649 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.445741 | `group_list` | ❌ |

---

## Test 92

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626482 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.617504 | `acr_registry_list` | ❌ |
| 3 | 0.544093 | `kusto_cluster_list` | ❌ |
| 4 | 0.508483 | `storage_blob_container_get` | ❌ |
| 5 | 0.495567 | `postgres_server_list` | ❌ |

---

## Test 93

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546334 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.469295 | `acr_registry_list` | ❌ |
| 3 | 0.451083 | `storage_blob_container_get` | ❌ |
| 4 | 0.407973 | `cosmos_database_container_list` | ❌ |
| 5 | 0.373464 | `storage_blob_get` | ❌ |

---

## Test 94

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674296 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.541779 | `acr_registry_list` | ❌ |
| 3 | 0.437510 | `storage_blob_container_get` | ❌ |
| 4 | 0.433927 | `cosmos_database_container_list` | ❌ |
| 5 | 0.383183 | `kusto_database_list` | ❌ |

---

## Test 95

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600780 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.501842 | `acr_registry_list` | ❌ |
| 3 | 0.430880 | `storage_blob_container_get` | ❌ |
| 4 | 0.418623 | `cosmos_database_container_list` | ❌ |
| 5 | 0.378199 | `redis_list` | ❌ |

---

## Test 96

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498026 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.229119 | `communication_sms_send` | ❌ |
| 3 | 0.188975 | `eventgrid_events_publish` | ❌ |
| 4 | 0.149092 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.145951 | `servicebus_topic_details` | ❌ |

---

## Test 97

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email from my communication service to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498730 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.314397 | `communication_sms_send` | ❌ |
| 3 | 0.238985 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.215316 | `speech_tts_synthesize` | ❌ |
| 5 | 0.211083 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 98

**Expected Tool:** `communication_email_send`  
**Prompt:** Send HTML-formatted email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.519870 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.207690 | `communication_sms_send` | ❌ |
| 3 | 0.152418 | `eventgrid_events_publish` | ❌ |
| 4 | 0.152013 | `servicebus_topic_details` | ❌ |
| 5 | 0.147540 | `foundry_agents_connect` | ❌ |

---

## Test 99

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with CC to <email-address-1> and <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533530 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.219600 | `communication_sms_send` | ❌ |
| 3 | 0.106189 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.106090 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.084905 | `cosmos_account_list` | ❌ |

---

## Test 100

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email to multiple recipients: <email-address-1>, <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.541104 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.244590 | `communication_sms_send` | ❌ |
| 3 | 0.138567 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.114325 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.087063 | `postgres_server_param_set` | ❌ |

---

## Test 101

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with reply-to address set to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512117 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.200218 | `communication_sms_send` | ❌ |
| 3 | 0.164115 | `mysql_server_param_set` | ❌ |
| 4 | 0.158759 | `postgres_server_param_set` | ❌ |
| 5 | 0.143574 | `appconfig_kv_set` | ❌ |

---

## Test 102

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with custom sender name <sender-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.472806 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.255232 | `communication_sms_send` | ❌ |
| 3 | 0.168394 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.166531 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.156838 | `cosmos_database_container_item_query` | ❌ |

---

## Test 103

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email with BCC recipients  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.528818 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.241121 | `communication_sms_send` | ❌ |
| 3 | 0.137538 | `confidentialledger_entries_append` | ❌ |
| 4 | 0.108748 | `confidentialledger_entries_get` | ❌ |
| 5 | 0.105033 | `storage_blob_upload` | ❌ |

---

## Test 104

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send an SMS message to <phone-number> saying "Hello"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533869 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.251284 | `communication_email_send` | ❌ |
| 3 | 0.221055 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.166041 | `speech_tts_synthesize` | ❌ |
| 5 | 0.154678 | `foundry_threads_create` | ❌ |

---

## Test 105

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS to <phone-number-2> from <phone-number-1> with message "Test message"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545968 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.294662 | `communication_email_send` | ❌ |
| 3 | 0.204569 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.200505 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.139280 | `foundry_agents_create` | ❌ |

---

## Test 106

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS to multiple recipients: <phone-number-1>, <phone-number-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545777 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.422345 | `communication_email_send` | ❌ |
| 3 | 0.189219 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.141957 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.106261 | `foundry_threads_get-messages` | ❌ |

---

## Test 107

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS with delivery reporting enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.554709 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.268959 | `communication_email_send` | ❌ |
| 3 | 0.191848 | `extension_azqr` | ❌ |
| 4 | 0.186765 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.170669 | `foundry_agents_query-and-evaluate` | ❌ |

---

## Test 108

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS message with custom tracking tag "campaign1"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.538777 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.269958 | `communication_email_send` | ❌ |
| 3 | 0.192503 | `foundry_agents_create` | ❌ |
| 4 | 0.188153 | `loadtesting_testrun_create` | ❌ |
| 5 | 0.185542 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 109

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send broadcast SMS to <phone-number-1> and <phone-number-2> saying "Urgent notification"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.474761 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.286432 | `communication_email_send` | ❌ |
| 3 | 0.164225 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.150225 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.128704 | `cosmos_account_list` | ❌ |

---

## Test 110

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS from my communication service to <phone-number-1>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564087 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.302699 | `communication_email_send` | ❌ |
| 3 | 0.240744 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.183651 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.173726 | `foundry_openai_create-completion` | ❌ |

---

## Test 111

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send an SMS with delivery receipt tracking  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.598157 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.314385 | `communication_email_send` | ❌ |
| 3 | 0.206946 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.201433 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.187824 | `confidentialledger_entries_append` | ❌ |

---

## Test 112

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Append an entry to my ledger <ledger_name> with data {"key": "value"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510464 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.294748 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.291613 | `appconfig_kv_set` | ❌ |
| 4 | 0.258871 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.249681 | `keyvault_certificate_import` | ❌ |

---

## Test 113

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Write a tamper-proof entry to ledger <ledger_name> containing {"transaction": "data"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602247 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.357646 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.211990 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.195427 | `keyvault_secret_create` | ❌ |
| 5 | 0.184077 | `keyvault_certificate_import` | ❌ |

---

## Test 114

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Append {"hello": "from mcp"} to my confidential ledger <ledger_name> in collection <collection_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546641 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.451888 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.225106 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.215941 | `appconfig_kv_set` | ❌ |
| 5 | 0.203344 | `keyvault_certificate_import` | ❌ |

---

## Test 115

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Create an immutable ledger entry in <ledger_name> with content {"audit": "log"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496023 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.340187 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.218473 | `monitor_activitylog_list` | ❌ |
| 4 | 0.215229 | `storage_blob_container_create` | ❌ |
| 5 | 0.204925 | `monitor_resource_log_query` | ❌ |

---

## Test 116

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Write an entry to confidential ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622138 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.524777 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.252508 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.240285 | `keyvault_secret_create` | ❌ |
| 5 | 0.186890 | `appconfig_kv_set` | ❌ |

---

## Test 117

**Expected Tool:** `confidentialledger_entries_get`  
**Prompt:** Get entry from Confidential Ledger for transaction <transaction_id> on ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.707252 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.551953 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.245025 | `keyvault_secret_get` | ❌ |
| 4 | 0.229943 | `keyvault_key_get` | ❌ |
| 5 | 0.212658 | `loadtesting_testrun_get` | ❌ |

---

## Test 118

**Expected Tool:** `confidentialledger_entries_get`  
**Prompt:** Get transaction <transaction_id> from ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509714 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.416580 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.224523 | `loadtesting_testrun_get` | ❌ |
| 4 | 0.218412 | `monitor_resource_log_query` | ❌ |
| 5 | 0.217671 | `loadtesting_testrun_list` | ❌ |

---

## Test 119

**Expected Tool:** `cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818357 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.668480 | `cosmos_database_list` | ❌ |
| 3 | 0.636050 | `subscription_list` | ❌ |
| 4 | 0.615268 | `cosmos_database_container_list` | ❌ |
| 5 | 0.601335 | `kusto_cluster_list` | ❌ |

---

## Test 120

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665401 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605336 | `cosmos_database_list` | ❌ |
| 3 | 0.571592 | `cosmos_database_container_list` | ❌ |
| 4 | 0.549515 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.503727 | `storage_account_get` | ❌ |

---

## Test 121

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752494 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.607228 | `subscription_list` | ❌ |
| 3 | 0.605125 | `cosmos_database_list` | ❌ |
| 4 | 0.566249 | `cosmos_database_container_list` | ❌ |
| 5 | 0.564010 | `cosmos_database_container_item_query` | ❌ |

---

## Test 122

**Expected Tool:** `cosmos_database_container_item_query`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658778 | `cosmos_database_container_item_query` | ✅ **EXPECTED** |
| 2 | 0.605253 | `cosmos_database_container_list` | ❌ |
| 3 | 0.487612 | `storage_blob_container_get` | ❌ |
| 4 | 0.477874 | `cosmos_database_list` | ❌ |
| 5 | 0.447757 | `cosmos_account_list` | ❌ |

---

## Test 123

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852832 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.681044 | `cosmos_database_list` | ❌ |
| 3 | 0.680865 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.632335 | `storage_blob_container_get` | ❌ |
| 5 | 0.630659 | `cosmos_account_list` | ❌ |

---

## Test 124

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789395 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.648249 | `cosmos_database_container_item_query` | ❌ |
| 3 | 0.614220 | `cosmos_database_list` | ❌ |
| 4 | 0.591361 | `storage_blob_container_get` | ❌ |
| 5 | 0.562062 | `cosmos_account_list` | ❌ |

---

## Test 125

**Expected Tool:** `cosmos_database_list`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815683 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.668515 | `cosmos_account_list` | ❌ |
| 3 | 0.665298 | `cosmos_database_container_list` | ❌ |
| 4 | 0.606516 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.583535 | `kusto_database_list` | ❌ |

---

## Test 126

**Expected Tool:** `cosmos_database_list`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.749370 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.624759 | `cosmos_database_container_list` | ❌ |
| 3 | 0.614572 | `cosmos_account_list` | ❌ |
| 4 | 0.580039 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.538467 | `mysql_database_list` | ❌ |

---

## Test 127

**Expected Tool:** `kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590264 | `kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.463898 | `kusto_cluster_list` | ❌ |
| 3 | 0.428159 | `kusto_query` | ❌ |
| 4 | 0.425669 | `kusto_database_list` | ❌ |
| 5 | 0.422577 | `kusto_table_schema` | ❌ |

---

## Test 128

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793760 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.630507 | `kusto_database_list` | ❌ |
| 3 | 0.573395 | `kusto_cluster_get` | ❌ |
| 4 | 0.525025 | `aks_cluster_get` | ❌ |
| 5 | 0.509397 | `grafana_list` | ❌ |

---

## Test 129

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531409 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.465277 | `kusto_cluster_get` | ❌ |
| 3 | 0.432288 | `kusto_database_list` | ❌ |
| 4 | 0.369596 | `aks_cluster_get` | ❌ |
| 5 | 0.363119 | `kusto_table_schema` | ❌ |

---

## Test 130

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701496 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.571191 | `kusto_cluster_get` | ❌ |
| 3 | 0.548685 | `kusto_database_list` | ❌ |
| 4 | 0.498909 | `aks_cluster_get` | ❌ |
| 5 | 0.474200 | `redis_list` | ❌ |

---

## Test 131

**Expected Tool:** `kusto_database_list`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.677059 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.560646 | `kusto_cluster_list` | ❌ |
| 3 | 0.556795 | `kusto_table_list` | ❌ |
| 4 | 0.553218 | `postgres_database_list` | ❌ |
| 5 | 0.549673 | `cosmos_database_list` | ❌ |

---

## Test 132

**Expected Tool:** `kusto_database_list`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623523 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.509992 | `kusto_cluster_list` | ❌ |
| 3 | 0.507073 | `kusto_table_list` | ❌ |
| 4 | 0.497144 | `cosmos_database_list` | ❌ |
| 5 | 0.491412 | `mysql_database_list` | ❌ |

---

## Test 133

**Expected Tool:** `kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.423660 | `kusto_query` | ✅ **EXPECTED** |
| 2 | 0.409558 | `postgres_database_query` | ❌ |
| 3 | 0.408178 | `kusto_table_schema` | ❌ |
| 4 | 0.407740 | `kusto_sample` | ❌ |
| 5 | 0.403981 | `kusto_cluster_list` | ❌ |

---

## Test 134

**Expected Tool:** `kusto_sample`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595554 | `kusto_sample` | ✅ **EXPECTED** |
| 2 | 0.510233 | `kusto_table_schema` | ❌ |
| 3 | 0.424212 | `kusto_table_list` | ❌ |
| 4 | 0.400951 | `kusto_cluster_list` | ❌ |
| 5 | 0.399525 | `kusto_cluster_get` | ❌ |

---

## Test 135

**Expected Tool:** `kusto_table_list`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.679642 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.585237 | `postgres_table_list` | ❌ |
| 3 | 0.581207 | `kusto_database_list` | ❌ |
| 4 | 0.556724 | `mysql_table_list` | ❌ |
| 5 | 0.549794 | `monitor_table_list` | ❌ |

---

## Test 136

**Expected Tool:** `kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619252 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.554332 | `kusto_table_schema` | ❌ |
| 3 | 0.527625 | `kusto_database_list` | ❌ |
| 4 | 0.524691 | `mysql_table_list` | ❌ |
| 5 | 0.523432 | `postgres_table_list` | ❌ |

---

## Test 137

**Expected Tool:** `kusto_table_schema`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.667052 | `kusto_table_schema` | ✅ **EXPECTED** |
| 2 | 0.564311 | `postgres_table_schema_get` | ❌ |
| 3 | 0.527917 | `mysql_table_schema_get` | ❌ |
| 4 | 0.490904 | `kusto_sample` | ❌ |
| 5 | 0.489680 | `kusto_table_list` | ❌ |

---

## Test 138

**Expected Tool:** `mysql_database_list`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634056 | `postgres_database_list` | ❌ |
| 2 | 0.623328 | `mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.534457 | `mysql_table_list` | ❌ |
| 4 | 0.498918 | `mysql_server_list` | ❌ |
| 5 | 0.490175 | `sql_db_list` | ❌ |

---

## Test 139

**Expected Tool:** `mysql_database_list`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588026 | `mysql_database_list` | ✅ **EXPECTED** |
| 2 | 0.574089 | `postgres_database_list` | ❌ |
| 3 | 0.483855 | `mysql_table_list` | ❌ |
| 4 | 0.463244 | `mysql_server_list` | ❌ |
| 5 | 0.444583 | `sql_db_list` | ❌ |

---

## Test 140

**Expected Tool:** `mysql_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476423 | `mysql_table_list` | ❌ |
| 2 | 0.455614 | `mysql_database_list` | ❌ |
| 3 | 0.433392 | `mysql_database_query` | ✅ **EXPECTED** |
| 4 | 0.419859 | `mysql_server_list` | ❌ |
| 5 | 0.409445 | `mysql_table_schema_get` | ❌ |

---

## Test 141

**Expected Tool:** `mysql_server_config_get`  
**Prompt:** Show me the configuration of MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531887 | `postgres_server_config_get` | ❌ |
| 2 | 0.516893 | `mysql_server_param_set` | ❌ |
| 3 | 0.489816 | `mysql_server_config_get` | ✅ **EXPECTED** |
| 4 | 0.476863 | `mysql_server_param_get` | ❌ |
| 5 | 0.426507 | `mysql_table_schema_get` | ❌ |

---

## Test 142

**Expected Tool:** `mysql_server_list`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678473 | `postgres_server_list` | ❌ |
| 2 | 0.558087 | `mysql_database_list` | ❌ |
| 3 | 0.554818 | `mysql_server_list` | ✅ **EXPECTED** |
| 4 | 0.513576 | `kusto_cluster_list` | ❌ |
| 5 | 0.501199 | `mysql_table_list` | ❌ |

---

## Test 143

**Expected Tool:** `mysql_server_list`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478411 | `mysql_database_list` | ❌ |
| 2 | 0.474586 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.435642 | `postgres_server_list` | ❌ |
| 4 | 0.412380 | `mysql_table_list` | ❌ |
| 5 | 0.389993 | `postgres_database_list` | ❌ |

---

## Test 144

**Expected Tool:** `mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636435 | `postgres_server_list` | ❌ |
| 2 | 0.534266 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.530137 | `mysql_database_list` | ❌ |
| 4 | 0.474918 | `kusto_cluster_list` | ❌ |
| 5 | 0.470458 | `redis_list` | ❌ |

---

## Test 145

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

## Test 146

**Expected Tool:** `mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.449503 | `mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.381185 | `mysql_server_param_get` | ❌ |
| 3 | 0.303552 | `postgres_server_param_set` | ❌ |
| 4 | 0.298912 | `mysql_database_query` | ❌ |
| 5 | 0.254231 | `mysql_server_list` | ❌ |

---

## Test 147

**Expected Tool:** `mysql_table_list`  
**Prompt:** List all tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633448 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.573844 | `postgres_table_list` | ❌ |
| 3 | 0.550898 | `postgres_database_list` | ❌ |
| 4 | 0.546825 | `mysql_database_list` | ❌ |
| 5 | 0.511847 | `kusto_table_list` | ❌ |

---

## Test 148

**Expected Tool:** `mysql_table_list`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609131 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.526236 | `postgres_table_list` | ❌ |
| 3 | 0.525575 | `mysql_database_list` | ❌ |
| 4 | 0.507258 | `mysql_table_schema_get` | ❌ |
| 5 | 0.498050 | `postgres_database_list` | ❌ |

---

## Test 149

**Expected Tool:** `mysql_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630820 | `mysql_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.558512 | `postgres_table_schema_get` | ❌ |
| 3 | 0.545483 | `mysql_table_list` | ❌ |
| 4 | 0.517793 | `kusto_table_schema` | ❌ |
| 5 | 0.457795 | `mysql_database_list` | ❌ |

---

## Test 150

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

## Test 151

**Expected Tool:** `postgres_database_list`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.760033 | `postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.589784 | `postgres_server_list` | ❌ |
| 3 | 0.585891 | `postgres_table_list` | ❌ |
| 4 | 0.552660 | `postgres_server_config_get` | ❌ |
| 5 | 0.495629 | `postgres_server_param_get` | ❌ |

---

## Test 152

**Expected Tool:** `postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546211 | `postgres_database_list` | ❌ |
| 2 | 0.523142 | `postgres_database_query` | ✅ **EXPECTED** |
| 3 | 0.503267 | `postgres_table_list` | ❌ |
| 4 | 0.466599 | `postgres_server_list` | ❌ |
| 5 | 0.403969 | `postgres_server_param_get` | ❌ |

---

## Test 153

**Expected Tool:** `postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756593 | `postgres_server_config_get` | ✅ **EXPECTED** |
| 2 | 0.615429 | `postgres_server_param_set` | ❌ |
| 3 | 0.599471 | `postgres_server_param_get` | ❌ |
| 4 | 0.535050 | `postgres_database_list` | ❌ |
| 5 | 0.518574 | `postgres_server_list` | ❌ |

---

## Test 154

**Expected Tool:** `postgres_server_list`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.900023 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.640733 | `postgres_database_list` | ❌ |
| 3 | 0.565914 | `postgres_table_list` | ❌ |
| 4 | 0.538997 | `postgres_server_config_get` | ❌ |
| 5 | 0.534115 | `kusto_cluster_list` | ❌ |

---

## Test 155

**Expected Tool:** `postgres_server_list`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674327 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.607062 | `postgres_database_list` | ❌ |
| 3 | 0.576348 | `postgres_server_config_get` | ❌ |
| 4 | 0.522995 | `postgres_table_list` | ❌ |
| 5 | 0.506171 | `postgres_server_param_get` | ❌ |

---

## Test 156

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

## Test 157

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

## Test 158

**Expected Tool:** `postgres_server_param_set`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579909 | `postgres_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.488496 | `postgres_server_config_get` | ❌ |
| 3 | 0.469810 | `postgres_server_list` | ❌ |
| 4 | 0.447036 | `postgres_server_param_get` | ❌ |
| 5 | 0.440716 | `postgres_database_list` | ❌ |

---

## Test 159

**Expected Tool:** `postgres_table_list`  
**Prompt:** List all tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789883 | `postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.750580 | `postgres_database_list` | ❌ |
| 3 | 0.574931 | `postgres_server_list` | ❌ |
| 4 | 0.519820 | `postgres_table_schema_get` | ❌ |
| 5 | 0.501400 | `postgres_server_config_get` | ❌ |

---

## Test 160

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

## Test 161

**Expected Tool:** `postgres_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.714877 | `postgres_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.597846 | `postgres_table_list` | ❌ |
| 3 | 0.574230 | `postgres_database_list` | ❌ |
| 4 | 0.508082 | `postgres_server_config_get` | ❌ |
| 5 | 0.502626 | `kusto_table_schema` | ❌ |

---

## Test 162

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

## Test 163

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

## Test 164

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

## Test 165

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

## Test 166

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

## Test 167

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Publish an event to Event Grid topic <topic_name> using <event_schema> with the following data <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755266 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.482336 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.465317 | `eventgrid_topic_list` | ❌ |
| 4 | 0.360316 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.354649 | `servicebus_topic_details` | ❌ |

---

## Test 168

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Publish event to my Event Grid topic <topic_name> with the following events <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654284 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.524422 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.509724 | `eventgrid_topic_list` | ❌ |
| 4 | 0.373785 | `servicebus_topic_details` | ❌ |
| 5 | 0.359229 | `eventhubs_eventhub_update` | ❌ |

---

## Test 169

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Send an event to Event Grid topic <topic_name> in resource group <resource_group_name> with <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600474 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.520852 | `eventgrid_topic_list` | ❌ |
| 3 | 0.504734 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.411158 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.389371 | `eventhubs_eventhub_consumergroup_get` | ❌ |

---

## Test 170

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.770128 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.745471 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.561803 | `kusto_cluster_list` | ❌ |
| 4 | 0.545540 | `search_service_list` | ❌ |
| 5 | 0.526173 | `subscription_list` | ❌ |

---

## Test 171

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738252 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.737486 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.492545 | `kusto_cluster_list` | ❌ |
| 4 | 0.480334 | `subscription_list` | ❌ |
| 5 | 0.475119 | `search_service_list` | ❌ |

---

## Test 172

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.770174 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.721362 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.535269 | `kusto_cluster_list` | ❌ |
| 4 | 0.514248 | `search_service_list` | ❌ |
| 5 | 0.495996 | `subscription_list` | ❌ |

---

## Test 173

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.758632 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.704462 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.609175 | `group_list` | ❌ |
| 4 | 0.544809 | `monitor_webtests_list` | ❌ |
| 5 | 0.524209 | `eventhubs_namespace_get` | ❌ |

---

## Test 174

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769097 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.720767 | `eventgrid_topic_list` | ❌ |
| 3 | 0.498615 | `servicebus_topic_details` | ❌ |
| 4 | 0.486216 | `servicebus_topic_subscription_details` | ❌ |
| 5 | 0.486162 | `eventgrid_events_publish` | ❌ |

---

## Test 175

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.718109 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.710016 | `eventgrid_topic_list` | ❌ |
| 3 | 0.539977 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.529286 | `servicebus_topic_details` | ❌ |
| 5 | 0.477876 | `eventgrid_events_publish` | ❌ |

---

## Test 176

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.746815 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.746225 | `eventgrid_topic_list` | ❌ |
| 3 | 0.535569 | `monitor_webtests_list` | ❌ |
| 4 | 0.524919 | `group_list` | ❌ |
| 5 | 0.503158 | `servicebus_topic_details` | ❌ |

---

## Test 177

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736436 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.659742 | `eventgrid_topic_list` | ❌ |
| 3 | 0.569315 | `subscription_list` | ❌ |
| 4 | 0.537839 | `kusto_cluster_list` | ❌ |
| 5 | 0.518858 | `search_service_list` | ❌ |

---

## Test 178

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684543 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.656398 | `eventgrid_topic_list` | ❌ |
| 3 | 0.542379 | `subscription_list` | ❌ |
| 4 | 0.520955 | `kusto_cluster_list` | ❌ |
| 5 | 0.510115 | `group_list` | ❌ |

---

## Test 179

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696101 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.691638 | `eventgrid_topic_list` | ❌ |
| 3 | 0.557573 | `group_list` | ❌ |
| 4 | 0.510684 | `monitor_webtests_list` | ❌ |
| 5 | 0.504984 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 180

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.709801 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.642227 | `eventgrid_topic_list` | ❌ |
| 3 | 0.506716 | `subscription_list` | ❌ |
| 4 | 0.476763 | `search_service_list` | ❌ |
| 5 | 0.475715 | `kusto_cluster_list` | ❌ |

---

## Test 181

**Expected Tool:** `eventhubs_eventhub_consumergroup_delete`  
**Prompt:** Delete my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.767340 | `eventhubs_eventhub_consumergroup_delete` | ✅ **EXPECTED** |
| 2 | 0.675955 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.641141 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.633936 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.605618 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 182

**Expected Tool:** `eventhubs_eventhub_consumergroup_get`  
**Prompt:** List all consumer groups in my event hub <event_hub_name> in namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738475 | `eventhubs_eventhub_consumergroup_get` | ✅ **EXPECTED** |
| 2 | 0.634517 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.625517 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.606619 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.593098 | `eventhubs_eventhub_get` | ❌ |

---

## Test 183

**Expected Tool:** `eventhubs_eventhub_consumergroup_get`  
**Prompt:** Get the details of my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712176 | `eventhubs_eventhub_consumergroup_get` | ✅ **EXPECTED** |
| 2 | 0.636523 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.624374 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.576225 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.529437 | `eventhubs_eventhub_get` | ❌ |

---

## Test 184

**Expected Tool:** `eventhubs_eventhub_consumergroup_update`  
**Prompt:** Create a new consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756909 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
| 2 | 0.688312 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 3 | 0.668256 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.553819 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.544525 | `eventhubs_namespace_get` | ❌ |

---

## Test 185

**Expected Tool:** `eventhubs_eventhub_consumergroup_update`  
**Prompt:** Update my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738818 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
| 2 | 0.654923 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 3 | 0.642219 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.552234 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.524019 | `eventhubs_namespace_delete` | ❌ |

---

## Test 186

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

## Test 187

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

## Test 188

**Expected Tool:** `eventhubs_eventhub_get`  
**Prompt:** Get the details of my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638452 | `eventhubs_namespace_get` | ❌ |
| 2 | 0.628145 | `eventhubs_eventhub_get` | ✅ **EXPECTED** |
| 3 | 0.571191 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.527941 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.522301 | `eventhubs_namespace_delete` | ❌ |

---

## Test 189

**Expected Tool:** `eventhubs_eventhub_update`  
**Prompt:** Create a new event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645976 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.605856 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.574389 | `eventhubs_eventhub_get` | ❌ |
| 4 | 0.571676 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.557550 | `eventhubs_namespace_delete` | ❌ |

---

## Test 190

**Expected Tool:** `eventhubs_eventhub_update`  
**Prompt:** Update my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655269 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.571610 | `eventhubs_eventhub_delete` | ❌ |
| 3 | 0.568535 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 4 | 0.568338 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.565942 | `eventhubs_namespace_delete` | ❌ |

---

## Test 191

**Expected Tool:** `eventhubs_namespace_delete`  
**Prompt:** Delete my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623995 | `eventhubs_namespace_delete` | ✅ **EXPECTED** |
| 2 | 0.525446 | `eventhubs_namespace_update` | ❌ |
| 3 | 0.506829 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.449841 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.435037 | `workbooks_delete` | ❌ |

---

## Test 192

**Expected Tool:** `eventhubs_namespace_get`  
**Prompt:** List all Event Hubs namespaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659838 | `eventhubs_eventhub_get` | ❌ |
| 2 | 0.658827 | `eventhubs_namespace_get` | ✅ **EXPECTED** |
| 3 | 0.607310 | `kusto_cluster_list` | ❌ |
| 4 | 0.557195 | `eventgrid_topic_list` | ❌ |
| 5 | 0.556126 | `eventgrid_subscription_list` | ❌ |

---

## Test 193

**Expected Tool:** `eventhubs_namespace_get`  
**Prompt:** Get the details of my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509749 | `eventhubs_namespace_get` | ✅ **EXPECTED** |
| 2 | 0.509432 | `monitor_webtests_get` | ❌ |
| 3 | 0.497399 | `servicebus_queue_details` | ❌ |
| 4 | 0.490055 | `eventhubs_namespace_update` | ❌ |
| 5 | 0.472974 | `foundry_resource_get` | ❌ |

---

## Test 194

**Expected Tool:** `eventhubs_namespace_update`  
**Prompt:** Create an new namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610456 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.466721 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.458458 | `eventhubs_namespace_delete` | ❌ |
| 4 | 0.456301 | `redis_create` | ❌ |
| 5 | 0.449724 | `workbooks_create` | ❌ |

---

## Test 195

**Expected Tool:** `eventhubs_namespace_update`  
**Prompt:** Update my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622338 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.474098 | `eventhubs_namespace_delete` | ❌ |
| 3 | 0.448723 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.436549 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.372632 | `sql_db_rename` | ❌ |

---

## Test 196

**Expected Tool:** `functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660116 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.451613 | `deploy_app_logs_get` | ❌ |
| 3 | 0.450457 | `applens_resource_diagnose` | ❌ |
| 4 | 0.390048 | `mysql_server_list` | ❌ |
| 5 | 0.380314 | `get_bestpractices_get` | ❌ |

---

## Test 197

**Expected Tool:** `functionapp_get`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607276 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.447400 | `mysql_server_config_get` | ❌ |
| 3 | 0.424606 | `appconfig_account_list` | ❌ |
| 4 | 0.411267 | `appconfig_kv_get` | ❌ |
| 5 | 0.400402 | `deploy_app_logs_get` | ❌ |

---

## Test 198

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

## Test 199

**Expected Tool:** `functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690933 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.440301 | `foundry_resource_get` | ❌ |
| 3 | 0.432317 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.431821 | `applens_resource_diagnose` | ❌ |
| 5 | 0.429077 | `storage_account_get` | ❌ |

---

## Test 200

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

## Test 201

**Expected Tool:** `functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687356 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.449589 | `deploy_app_logs_get` | ❌ |
| 3 | 0.428689 | `applens_resource_diagnose` | ❌ |
| 4 | 0.421759 | `foundry_resource_get` | ❌ |
| 5 | 0.391781 | `monitor_webtests_get` | ❌ |

---

## Test 202

**Expected Tool:** `functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644882 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.430189 | `deploy_app_logs_get` | ❌ |
| 3 | 0.421082 | `storage_account_get` | ❌ |
| 4 | 0.403311 | `signalr_runtime_get` | ❌ |
| 5 | 0.384393 | `foundry_resource_get` | ❌ |

---

## Test 203

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

## Test 204

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

## Test 205

**Expected Tool:** `functionapp_get`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646561 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.559382 | `search_service_list` | ❌ |
| 3 | 0.534985 | `subscription_list` | ❌ |
| 4 | 0.528908 | `kusto_cluster_list` | ❌ |
| 5 | 0.516618 | `cosmos_account_list` | ❌ |

---

## Test 206

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

## Test 207

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

## Test 208

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** Get the account settings for my key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604780 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.532196 | `storage_account_get` | ❌ |
| 3 | 0.496629 | `keyvault_key_get` | ❌ |
| 4 | 0.452367 | `appconfig_kv_set` | ❌ |
| 5 | 0.447197 | `keyvault_secret_get` | ❌ |

---

## Test 209

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** Show me the account settings for managed HSM keyvault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671370 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.455561 | `storage_account_get` | ❌ |
| 3 | 0.441224 | `keyvault_key_get` | ❌ |
| 4 | 0.404666 | `appconfig_kv_set` | ❌ |
| 5 | 0.394600 | `keyvault_secret_get` | ❌ |

---

## Test 210

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** What's the value of the <setting_name> setting in my key vault with name <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.505750 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.496540 | `appconfig_kv_set` | ❌ |
| 3 | 0.420145 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.419126 | `keyvault_key_get` | ❌ |
| 5 | 0.409771 | `keyvault_secret_get` | ❌ |

---

## Test 211

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627692 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.570319 | `keyvault_certificate_import` | ❌ |
| 3 | 0.540199 | `keyvault_key_create` | ❌ |
| 4 | 0.519218 | `keyvault_certificate_get` | ❌ |
| 5 | 0.500027 | `keyvault_certificate_list` | ❌ |

---

## Test 212

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Generate a certificate named <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599851 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.561445 | `keyvault_certificate_import` | ❌ |
| 3 | 0.522706 | `keyvault_certificate_get` | ❌ |
| 4 | 0.502128 | `keyvault_key_create` | ❌ |
| 5 | 0.497145 | `keyvault_certificate_list` | ❌ |

---

## Test 213

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Request creation of certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573986 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.527759 | `keyvault_certificate_import` | ❌ |
| 3 | 0.498278 | `keyvault_certificate_get` | ❌ |
| 4 | 0.481548 | `keyvault_key_create` | ❌ |
| 5 | 0.469601 | `keyvault_certificate_list` | ❌ |

---

## Test 214

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591849 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.562265 | `keyvault_certificate_import` | ❌ |
| 3 | 0.522147 | `keyvault_certificate_get` | ❌ |
| 4 | 0.502529 | `keyvault_key_create` | ❌ |
| 5 | 0.479992 | `keyvault_certificate_list` | ❌ |

---

## Test 215

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Issue a certificate <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622825 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.558532 | `keyvault_certificate_import` | ❌ |
| 3 | 0.534503 | `keyvault_certificate_get` | ❌ |
| 4 | 0.521316 | `keyvault_certificate_list` | ❌ |
| 5 | 0.465056 | `keyvault_key_create` | ❌ |

---

## Test 216

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600625 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.528405 | `keyvault_certificate_list` | ❌ |
| 3 | 0.519037 | `keyvault_certificate_import` | ❌ |
| 4 | 0.499345 | `keyvault_certificate_create` | ❌ |
| 5 | 0.486608 | `keyvault_key_get` | ❌ |

---

## Test 217

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646098 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.562988 | `keyvault_key_get` | ❌ |
| 3 | 0.513954 | `keyvault_secret_get` | ❌ |
| 4 | 0.509446 | `keyvault_certificate_list` | ❌ |
| 5 | 0.507738 | `keyvault_certificate_import` | ❌ |

---

## Test 218

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Get the certificate <certificate_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609523 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.515570 | `keyvault_certificate_list` | ❌ |
| 3 | 0.511216 | `keyvault_certificate_create` | ❌ |
| 4 | 0.507768 | `keyvault_certificate_import` | ❌ |
| 5 | 0.474394 | `keyvault_key_get` | ❌ |

---

## Test 219

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Display the certificate details for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.647669 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527400 | `keyvault_key_get` | ❌ |
| 3 | 0.521556 | `keyvault_certificate_list` | ❌ |
| 4 | 0.509796 | `keyvault_certificate_import` | ❌ |
| 5 | 0.501806 | `keyvault_secret_get` | ❌ |

---

## Test 220

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595959 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527404 | `keyvault_certificate_list` | ❌ |
| 3 | 0.519059 | `keyvault_certificate_import` | ❌ |
| 4 | 0.501315 | `keyvault_certificate_create` | ❌ |
| 5 | 0.465174 | `keyvault_key_get` | ❌ |

---

## Test 221

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585481 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.420747 | `keyvault_certificate_get` | ❌ |
| 3 | 0.402635 | `keyvault_certificate_create` | ❌ |
| 4 | 0.399342 | `keyvault_certificate_list` | ❌ |
| 5 | 0.352905 | `keyvault_key_create` | ❌ |

---

## Test 222

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622125 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.504314 | `keyvault_certificate_get` | ❌ |
| 3 | 0.498849 | `keyvault_certificate_create` | ❌ |
| 4 | 0.448105 | `keyvault_certificate_list` | ❌ |
| 5 | 0.419811 | `keyvault_key_create` | ❌ |

---

## Test 223

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Upload certificate file <file_path> to key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595707 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.453868 | `keyvault_certificate_create` | ❌ |
| 3 | 0.452551 | `keyvault_certificate_get` | ❌ |
| 4 | 0.418203 | `keyvault_certificate_list` | ❌ |
| 5 | 0.413377 | `keyvault_key_create` | ❌ |

---

## Test 224

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619480 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.517804 | `keyvault_certificate_get` | ❌ |
| 3 | 0.480742 | `keyvault_certificate_create` | ❌ |
| 4 | 0.444386 | `keyvault_certificate_list` | ❌ |
| 5 | 0.381873 | `keyvault_key_create` | ❌ |

---

## Test 225

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595634 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.452488 | `keyvault_certificate_create` | ❌ |
| 3 | 0.441647 | `keyvault_certificate_get` | ❌ |
| 4 | 0.407883 | `keyvault_key_create` | ❌ |
| 5 | 0.392175 | `keyvault_secret_create` | ❌ |

---

## Test 226

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726124 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.582980 | `keyvault_key_list` | ❌ |
| 3 | 0.531988 | `keyvault_secret_list` | ❌ |
| 4 | 0.515236 | `keyvault_certificate_get` | ❌ |
| 5 | 0.485886 | `keyvault_certificate_create` | ❌ |

---

## Test 227

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615541 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.522453 | `keyvault_certificate_get` | ❌ |
| 3 | 0.475027 | `keyvault_key_list` | ❌ |
| 4 | 0.461072 | `keyvault_certificate_create` | ❌ |
| 5 | 0.448139 | `keyvault_key_get` | ❌ |

---

## Test 228

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** What certificates are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624710 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.519739 | `keyvault_certificate_get` | ❌ |
| 3 | 0.510089 | `keyvault_certificate_create` | ❌ |
| 4 | 0.505534 | `keyvault_certificate_import` | ❌ |
| 5 | 0.497249 | `keyvault_key_list` | ❌ |

---

## Test 229

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** List certificate names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672622 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.553863 | `keyvault_key_list` | ❌ |
| 3 | 0.511905 | `keyvault_secret_list` | ❌ |
| 4 | 0.507062 | `keyvault_certificate_get` | ❌ |
| 5 | 0.492354 | `keyvault_certificate_create` | ❌ |

---

## Test 230

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Enumerate certificates in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747408 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.594099 | `keyvault_key_list` | ❌ |
| 3 | 0.558771 | `keyvault_secret_list` | ❌ |
| 4 | 0.515568 | `keyvault_certificate_get` | ❌ |
| 5 | 0.491036 | `keyvault_certificate_create` | ❌ |

---

## Test 231

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show certificate names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639790 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.512468 | `keyvault_certificate_get` | ❌ |
| 3 | 0.507532 | `keyvault_key_list` | ❌ |
| 4 | 0.482686 | `keyvault_certificate_create` | ❌ |
| 5 | 0.464842 | `keyvault_secret_list` | ❌ |

---

## Test 232

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661707 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.456829 | `keyvault_secret_create` | ❌ |
| 3 | 0.452064 | `keyvault_certificate_create` | ❌ |
| 4 | 0.430000 | `keyvault_certificate_import` | ❌ |
| 5 | 0.399470 | `keyvault_key_get` | ❌ |

---

## Test 233

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Generate a key <key_name> with type <key_type> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641159 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.428546 | `keyvault_key_get` | ❌ |
| 3 | 0.422631 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420190 | `keyvault_secret_create` | ❌ |
| 5 | 0.405688 | `appconfig_kv_set` | ❌ |

---

## Test 234

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an oct key in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547395 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.463526 | `keyvault_secret_create` | ❌ |
| 3 | 0.447163 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420308 | `keyvault_key_get` | ❌ |
| 5 | 0.404242 | `keyvault_certificate_import` | ❌ |

---

## Test 235

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an RSA key in the vault <key_vault_account_name> with name <key_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641369 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.501654 | `keyvault_secret_create` | ❌ |
| 3 | 0.491503 | `keyvault_certificate_create` | ❌ |
| 4 | 0.464557 | `keyvault_certificate_import` | ❌ |
| 5 | 0.451016 | `keyvault_key_get` | ❌ |

---

## Test 236

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an EC key with name <key_name> in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571718 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.443260 | `keyvault_certificate_create` | ❌ |
| 3 | 0.434687 | `keyvault_secret_create` | ❌ |
| 4 | 0.421721 | `keyvault_key_get` | ❌ |
| 5 | 0.400533 | `keyvault_certificate_import` | ❌ |

---

## Test 237

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549488 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.467394 | `keyvault_secret_get` | ❌ |
| 3 | 0.452816 | `keyvault_key_create` | ❌ |
| 4 | 0.439805 | `keyvault_key_list` | ❌ |
| 5 | 0.426545 | `keyvault_certificate_get` | ❌ |

---

## Test 238

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629552 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.531614 | `keyvault_secret_get` | ❌ |
| 3 | 0.512278 | `storage_account_get` | ❌ |
| 4 | 0.495957 | `keyvault_certificate_get` | ❌ |
| 5 | 0.456992 | `keyvault_key_create` | ❌ |

---

## Test 239

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Get the key <key_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.484645 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.443182 | `keyvault_key_create` | ❌ |
| 3 | 0.408497 | `keyvault_secret_get` | ❌ |
| 4 | 0.395491 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.383519 | `appconfig_kv_lock_set` | ❌ |

---

## Test 240

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Display the key details for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590260 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.487276 | `keyvault_secret_get` | ❌ |
| 3 | 0.476421 | `storage_account_get` | ❌ |
| 4 | 0.460716 | `keyvault_certificate_get` | ❌ |
| 5 | 0.436464 | `keyvault_admin_settings_get` | ❌ |

---

## Test 241

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Retrieve key metadata for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518886 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.432950 | `storage_account_get` | ❌ |
| 3 | 0.432742 | `keyvault_admin_settings_get` | ❌ |
| 4 | 0.429131 | `keyvault_key_create` | ❌ |
| 5 | 0.421798 | `keyvault_secret_get` | ❌ |

---

## Test 242

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701301 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.601513 | `keyvault_certificate_list` | ❌ |
| 3 | 0.587427 | `keyvault_secret_list` | ❌ |
| 4 | 0.505496 | `tables_list` | ❌ |
| 5 | 0.498767 | `cosmos_account_list` | ❌ |

---

## Test 243

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549324 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.506815 | `keyvault_key_get` | ❌ |
| 3 | 0.475507 | `keyvault_certificate_list` | ❌ |
| 4 | 0.472465 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.455276 | `keyvault_secret_get` | ❌ |

---

## Test 244

**Expected Tool:** `keyvault_key_list`  
**Prompt:** What keys are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581859 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.502245 | `keyvault_admin_settings_get` | ❌ |
| 3 | 0.501481 | `keyvault_certificate_list` | ❌ |
| 4 | 0.476470 | `keyvault_key_get` | ❌ |
| 5 | 0.472414 | `keyvault_secret_list` | ❌ |

---

## Test 245

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List key names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641133 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.559550 | `keyvault_certificate_list` | ❌ |
| 3 | 0.553553 | `keyvault_secret_list` | ❌ |
| 4 | 0.491260 | `tables_list` | ❌ |
| 5 | 0.486377 | `keyvault_admin_settings_get` | ❌ |

---

## Test 246

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Enumerate keys in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.722999 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.610928 | `keyvault_certificate_list` | ❌ |
| 3 | 0.610904 | `keyvault_secret_list` | ❌ |
| 4 | 0.473643 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.442736 | `keyvault_key_get` | ❌ |

---

## Test 247

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Show key names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570274 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.501073 | `keyvault_key_get` | ❌ |
| 3 | 0.500103 | `keyvault_certificate_list` | ❌ |
| 4 | 0.496817 | `storage_account_get` | ❌ |
| 5 | 0.490367 | `keyvault_secret_list` | ❌ |

---

## Test 248

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678497 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.553018 | `keyvault_key_create` | ❌ |
| 3 | 0.512214 | `keyvault_secret_get` | ❌ |
| 4 | 0.475005 | `keyvault_certificate_create` | ❌ |
| 5 | 0.461437 | `appconfig_kv_set` | ❌ |

---

## Test 249

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663051 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.518660 | `keyvault_secret_get` | ❌ |
| 3 | 0.512233 | `appconfig_kv_set` | ❌ |
| 4 | 0.458502 | `keyvault_key_create` | ❌ |
| 5 | 0.429785 | `appconfig_kv_lock_set` | ❌ |

---

## Test 250

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639861 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.508961 | `keyvault_secret_get` | ❌ |
| 3 | 0.485203 | `appconfig_kv_set` | ❌ |
| 4 | 0.484680 | `keyvault_key_create` | ❌ |
| 5 | 0.448995 | `appconfig_kv_lock_set` | ❌ |

---

## Test 251

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.675049 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.498464 | `keyvault_secret_get` | ❌ |
| 3 | 0.498228 | `keyvault_key_create` | ❌ |
| 4 | 0.479174 | `keyvault_certificate_import` | ❌ |
| 5 | 0.458574 | `appconfig_kv_set` | ❌ |

---

## Test 252

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Update secret <secret_name> to value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571731 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.512742 | `keyvault_secret_get` | ❌ |
| 3 | 0.441106 | `appconfig_kv_set` | ❌ |
| 4 | 0.417944 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.408306 | `keyvault_key_get` | ❌ |

---

## Test 253

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.601896 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.504212 | `keyvault_key_get` | ❌ |
| 3 | 0.501354 | `keyvault_secret_create` | ❌ |
| 4 | 0.478769 | `keyvault_secret_list` | ❌ |
| 5 | 0.439521 | `keyvault_certificate_get` | ❌ |

---

## Test 254

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.652724 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.566786 | `keyvault_key_get` | ❌ |
| 3 | 0.517547 | `storage_account_get` | ❌ |
| 4 | 0.496050 | `keyvault_certificate_get` | ❌ |
| 5 | 0.485249 | `keyvault_secret_list` | ❌ |

---

## Test 255

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Get the secret <secret_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578652 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.491111 | `keyvault_secret_create` | ❌ |
| 3 | 0.489792 | `keyvault_key_get` | ❌ |
| 4 | 0.444496 | `keyvault_secret_list` | ❌ |
| 5 | 0.422362 | `keyvault_admin_settings_get` | ❌ |

---

## Test 256

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Display the secret details for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.648319 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.546992 | `keyvault_key_get` | ❌ |
| 3 | 0.497402 | `storage_account_get` | ❌ |
| 4 | 0.492583 | `keyvault_certificate_get` | ❌ |
| 5 | 0.491597 | `keyvault_secret_list` | ❌ |

---

## Test 257

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Retrieve secret metadata for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576314 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.475443 | `keyvault_key_get` | ❌ |
| 3 | 0.466823 | `keyvault_secret_create` | ❌ |
| 4 | 0.447602 | `keyvault_secret_list` | ❌ |
| 5 | 0.439583 | `storage_account_get` | ❌ |

---

## Test 258

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701227 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.563605 | `keyvault_key_list` | ❌ |
| 3 | 0.538337 | `keyvault_certificate_list` | ❌ |
| 4 | 0.499162 | `keyvault_secret_get` | ❌ |
| 5 | 0.455500 | `cosmos_account_list` | ❌ |

---

## Test 259

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555681 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.543203 | `keyvault_secret_get` | ❌ |
| 3 | 0.497525 | `keyvault_key_get` | ❌ |
| 4 | 0.464537 | `keyvault_key_list` | ❌ |
| 5 | 0.453130 | `keyvault_admin_settings_get` | ❌ |

---

## Test 260

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** What secrets are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572540 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.528946 | `keyvault_secret_get` | ❌ |
| 3 | 0.493620 | `keyvault_key_list` | ❌ |
| 4 | 0.487620 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.475273 | `keyvault_key_get` | ❌ |

---

## Test 261

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** List secrets names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624290 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.559523 | `keyvault_key_list` | ❌ |
| 3 | 0.517516 | `keyvault_certificate_list` | ❌ |
| 4 | 0.479120 | `keyvault_secret_get` | ❌ |
| 5 | 0.454288 | `storage_blob_container_get` | ❌ |

---

## Test 262

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Enumerate secrets in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.742358 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.601048 | `keyvault_key_list` | ❌ |
| 3 | 0.567827 | `keyvault_certificate_list` | ❌ |
| 4 | 0.495823 | `keyvault_secret_get` | ❌ |
| 5 | 0.437560 | `keyvault_admin_settings_get` | ❌ |

---

## Test 263

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Show secrets names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567110 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.522011 | `keyvault_secret_get` | ❌ |
| 3 | 0.476172 | `keyvault_key_list` | ❌ |
| 4 | 0.462651 | `keyvault_secret_create` | ❌ |
| 5 | 0.461326 | `keyvault_key_get` | ❌ |

---

## Test 264

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

## Test 265

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621759 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.575626 | `aks_nodepool_get` | ❌ |
| 3 | 0.567870 | `kusto_cluster_get` | ❌ |
| 4 | 0.461466 | `sql_db_show` | ❌ |
| 5 | 0.444327 | `monitor_webtests_get` | ❌ |

---

## Test 266

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522525 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.483220 | `aks_nodepool_get` | ❌ |
| 3 | 0.434684 | `kusto_cluster_get` | ❌ |
| 4 | 0.380301 | `mysql_server_config_get` | ❌ |
| 5 | 0.366718 | `kusto_cluster_list` | ❌ |

---

## Test 267

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588634 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.550555 | `aks_nodepool_get` | ❌ |
| 3 | 0.527511 | `kusto_cluster_get` | ❌ |
| 4 | 0.445722 | `storage_account_get` | ❌ |
| 5 | 0.434587 | `functionapp_get` | ❌ |

---

## Test 268

**Expected Tool:** `aks_cluster_get`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756471 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.749353 | `kusto_cluster_list` | ❌ |
| 3 | 0.590166 | `aks_nodepool_get` | ❌ |
| 4 | 0.568403 | `kusto_database_list` | ❌ |
| 5 | 0.562043 | `search_service_list` | ❌ |

---

## Test 269

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612123 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.586693 | `kusto_cluster_list` | ❌ |
| 3 | 0.507757 | `aks_nodepool_get` | ❌ |
| 4 | 0.489724 | `kusto_cluster_get` | ❌ |
| 5 | 0.462874 | `kusto_database_list` | ❌ |

---

## Test 270

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628429 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.563189 | `aks_nodepool_get` | ❌ |
| 3 | 0.526760 | `kusto_cluster_list` | ❌ |
| 4 | 0.426157 | `kusto_cluster_get` | ❌ |
| 5 | 0.409103 | `kusto_database_list` | ❌ |

---

## Test 271

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728937 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.517021 | `kusto_cluster_get` | ❌ |
| 3 | 0.509820 | `aks_cluster_get` | ❌ |
| 4 | 0.468435 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.463192 | `sql_elastic-pool_list` | ❌ |

---

## Test 272

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654106 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.458596 | `sql_elastic-pool_list` | ❌ |
| 3 | 0.446035 | `aks_cluster_get` | ❌ |
| 4 | 0.440240 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.413758 | `kusto_cluster_get` | ❌ |

---

## Test 273

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592806 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.402556 | `aks_cluster_get` | ❌ |
| 3 | 0.385198 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.383045 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.355090 | `kusto_cluster_get` | ❌ |

---

## Test 274

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692231 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.519037 | `aks_cluster_get` | ❌ |
| 3 | 0.506677 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.500758 | `kusto_cluster_list` | ❌ |
| 5 | 0.487707 | `sql_elastic-pool_list` | ❌ |

---

## Test 275

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732132 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.561829 | `aks_cluster_get` | ❌ |
| 3 | 0.510269 | `sql_elastic-pool_list` | ❌ |
| 4 | 0.509812 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.486685 | `kusto_cluster_list` | ❌ |

---

## Test 276

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629358 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.456911 | `aks_cluster_get` | ❌ |
| 3 | 0.443897 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.433015 | `kusto_cluster_list` | ❌ |
| 5 | 0.425448 | `sql_elastic-pool_list` | ❌ |

---

## Test 277

**Expected Tool:** `loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577811 | `loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.519418 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.512099 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.472887 | `monitor_webtests_create` | ❌ |
| 5 | 0.460717 | `loadtesting_testresource_list` | ❌ |

---

## Test 278

**Expected Tool:** `loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626226 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.619944 | `loadtesting_test_get` | ✅ **EXPECTED** |
| 3 | 0.594666 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.590698 | `monitor_webtests_get` | ❌ |
| 5 | 0.536024 | `monitor_webtests_list` | ❌ |

---

## Test 279

**Expected Tool:** `loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645537 | `loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.618773 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.541746 | `loadtesting_test_create` | ❌ |
| 4 | 0.539771 | `loadtesting_testrun_create` | ❌ |
| 5 | 0.526684 | `monitor_webtests_list` | ❌ |

---

## Test 280

**Expected Tool:** `loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.794326 | `loadtesting_testresource_list` | ✅ **EXPECTED** |
| 2 | 0.653165 | `monitor_webtests_list` | ❌ |
| 3 | 0.577408 | `group_list` | ❌ |
| 4 | 0.575172 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.565565 | `datadog_monitoredresources_list` | ❌ |

---

## Test 281

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

## Test 282

**Expected Tool:** `loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619155 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.601969 | `loadtesting_test_get` | ❌ |
| 3 | 0.597467 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.577510 | `monitor_webtests_get` | ❌ |
| 5 | 0.566089 | `loadtesting_testrun_list` | ❌ |

---

## Test 283

**Expected Tool:** `loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669180 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.640360 | `loadtesting_testrun_list` | ✅ **EXPECTED** |
| 3 | 0.601075 | `loadtesting_test_get` | ❌ |
| 4 | 0.577461 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.569424 | `monitor_webtests_get` | ❌ |

---

## Test 284

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

## Test 285

**Expected Tool:** `grafana_list`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599338 | `kusto_cluster_list` | ❌ |
| 2 | 0.578892 | `grafana_list` | ✅ **EXPECTED** |
| 3 | 0.551851 | `search_service_list` | ❌ |
| 4 | 0.550373 | `subscription_list` | ❌ |
| 5 | 0.531197 | `redis_list` | ❌ |

---

## Test 286

**Expected Tool:** `managedlustre_fs_create`  
**Prompt:** Create an Azure Managed Lustre filesystem with name <filesystem_name>, size <filesystem_size>, SKU <sku>, and subnet <subnet_id> for availability zone <zone> in location <location>. Maintenance should occur on <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728113 | `managedlustre_fs_create` | ✅ **EXPECTED** |
| 2 | 0.616164 | `managedlustre_fs_list` | ❌ |
| 3 | 0.605731 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.598255 | `managedlustre_fs_update` | ❌ |
| 5 | 0.557729 | `managedlustre_fs_subnetsize_validate` | ❌ |

---

## Test 287

**Expected Tool:** `managedlustre_fs_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.750675 | `managedlustre_fs_list` | ✅ **EXPECTED** |
| 2 | 0.631746 | `managedlustre_fs_sku_get` | ❌ |
| 3 | 0.582660 | `managedlustre_fs_create` | ❌ |
| 4 | 0.562256 | `kusto_cluster_list` | ❌ |
| 5 | 0.513156 | `search_service_list` | ❌ |

---

## Test 288

**Expected Tool:** `managedlustre_fs_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743903 | `managedlustre_fs_list` | ✅ **EXPECTED** |
| 2 | 0.613185 | `managedlustre_fs_sku_get` | ❌ |
| 3 | 0.565856 | `managedlustre_fs_create` | ❌ |
| 4 | 0.519986 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.515433 | `loadtesting_testresource_list` | ❌ |

---

## Test 289

**Expected Tool:** `managedlustre_fs_sku_get`  
**Prompt:** List the Azure Managed Lustre SKUs available in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.827346 | `managedlustre_fs_sku_get` | ✅ **EXPECTED** |
| 2 | 0.613674 | `managedlustre_fs_list` | ❌ |
| 3 | 0.513242 | `managedlustre_fs_create` | ❌ |
| 4 | 0.496240 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 5 | 0.470199 | `kusto_cluster_list` | ❌ |

---

## Test 290

**Expected Tool:** `managedlustre_fs_subnetsize_ask`  
**Prompt:** Tell me how many IP addresses I need for an Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739766 | `managedlustre_fs_subnetsize_ask` | ✅ **EXPECTED** |
| 2 | 0.651605 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 3 | 0.594558 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.559498 | `managedlustre_fs_list` | ❌ |
| 5 | 0.533684 | `managedlustre_fs_create` | ❌ |

---

## Test 291

**Expected Tool:** `managedlustre_fs_subnetsize_validate`  
**Prompt:** Validate if the network <subnet_id> can host Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879543 | `managedlustre_fs_subnetsize_validate` | ✅ **EXPECTED** |
| 2 | 0.623762 | `managedlustre_fs_subnetsize_ask` | ❌ |
| 3 | 0.543471 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.515285 | `managedlustre_fs_create` | ❌ |
| 5 | 0.479348 | `managedlustre_fs_list` | ❌ |

---

## Test 292

**Expected Tool:** `managedlustre_fs_update`  
**Prompt:** Update the maintenance window of the Azure Managed Lustre filesystem <filesystem_name> to <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739000 | `managedlustre_fs_update` | ✅ **EXPECTED** |
| 2 | 0.527525 | `managedlustre_fs_create` | ❌ |
| 3 | 0.487193 | `managedlustre_fs_list` | ❌ |
| 4 | 0.385317 | `managedlustre_fs_sku_get` | ❌ |
| 5 | 0.344880 | `managedlustre_fs_subnetsize_validate` | ❌ |

---

## Test 293

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

## Test 294

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607917 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.443133 | `marketplace_product_get` | ❌ |
| 3 | 0.380880 | `foundry_models_list` | ❌ |
| 4 | 0.343549 | `search_service_list` | ❌ |
| 5 | 0.338190 | `foundry_threads_list` | ❌ |

---

## Test 295

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537726 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.385167 | `marketplace_product_get` | ❌ |
| 3 | 0.341241 | `foundry_models_list` | ❌ |
| 4 | 0.288002 | `redis_list` | ❌ |
| 5 | 0.260340 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 296

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.647926 | `azureaibestpractices_get` | ❌ |
| 2 | 0.646844 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 3 | 0.635406 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.586907 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.531728 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 297

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

## Test 298

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625259 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.594323 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.536176 | `azureaibestpractices_get` | ❌ |
| 4 | 0.518643 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.465573 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 299

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624273 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.584766 | `azureaibestpractices_get` | ❌ |
| 3 | 0.570488 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.522998 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.493998 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 300

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

## Test 301

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610986 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.532790 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.518431 | `azureaibestpractices_get` | ❌ |
| 4 | 0.487322 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.458060 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 302

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557862 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.513262 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.509176 | `azureaibestpractices_get` | ❌ |
| 4 | 0.505123 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.483705 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 303

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582541 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.500368 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.481262 | `azureaibestpractices_get` | ❌ |
| 4 | 0.472112 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.433134 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 304

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** configure azure mcp in coding agent for my repo  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488855 | `deploy_plan_get` | ❌ |
| 2 | 0.460956 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.411825 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.390270 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.370391 | `azureaibestpractices_get` | ❌ |

---

## Test 305

**Expected Tool:** `monitor_activitylog_list`  
**Prompt:** List the activity logs of the last month for <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537859 | `monitor_activitylog_list` | ✅ **EXPECTED** |
| 2 | 0.506227 | `monitor_resource_log_query` | ❌ |
| 3 | 0.371703 | `monitor_workspace_log_query` | ❌ |
| 4 | 0.363699 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.344560 | `datadog_monitoredresources_list` | ❌ |

---

## Test 306

**Expected Tool:** `monitor_healthmodels_entity_get`  
**Prompt:** Show me the health status of entity <entity_id> using the health model <health_model_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660947 | `monitor_healthmodels_entity_get` | ✅ **EXPECTED** |
| 2 | 0.609276 | `resourcehealth_availability-status_get` | ❌ |
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
| 1 | 0.592625 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424079 | `monitor_metrics_query` | ❌ |
| 3 | 0.368313 | `bicepschema_get` | ❌ |
| 4 | 0.333091 | `foundry_resource_get` | ❌ |
| 5 | 0.332354 | `monitor_table_type_list` | ❌ |

---

## Test 308

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607600 | `storage_account_get` | ❌ |
| 2 | 0.587736 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.546698 | `tables_list` | ❌ |
| 4 | 0.544781 | `storage_blob_container_get` | ❌ |
| 5 | 0.495829 | `storage_blob_get` | ❌ |

---

## Test 309

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633173 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
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
| 1 | 0.555382 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.527498 | `monitor_resource_log_query` | ❌ |
| 3 | 0.464786 | `applens_resource_diagnose` | ❌ |
| 4 | 0.420480 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.413315 | `applicationinsights_recommendation_list` | ❌ |

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
| 5 | 0.438233 | `monitor_metrics_definitions` | ❌ |

---

## Test 312

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
| 3 | 0.384482 | `monitor_metrics_definitions` | ❌ |
| 4 | 0.347723 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.330713 | `resourcehealth_availability-status_get` | ❌ |

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
| 1 | 0.687856 | `monitor_resource_log_query` | ✅ **EXPECTED** |
| 2 | 0.621915 | `monitor_workspace_log_query` | ❌ |
| 3 | 0.598380 | `monitor_activitylog_list` | ❌ |
| 4 | 0.485627 | `deploy_app_logs_get` | ❌ |
| 5 | 0.469697 | `monitor_metrics_query` | ❌ |

---

## Test 317

**Expected Tool:** `monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.850569 | `monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725738 | `monitor_table_type_list` | ❌ |
| 3 | 0.645028 | `tables_list` | ❌ |
| 4 | 0.620445 | `monitor_workspace_list` | ❌ |
| 5 | 0.541928 | `kusto_table_list` | ❌ |

---

## Test 318

**Expected Tool:** `monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.797978 | `monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.701122 | `monitor_table_type_list` | ❌ |
| 3 | 0.599916 | `monitor_workspace_list` | ❌ |
| 4 | 0.571611 | `tables_list` | ❌ |
| 5 | 0.542820 | `monitor_workspace_log_query` | ❌ |

---

## Test 319

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881524 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.765377 | `monitor_table_list` | ❌ |
| 3 | 0.569921 | `monitor_workspace_list` | ❌ |
| 4 | 0.562068 | `tables_list` | ❌ |
| 5 | 0.504683 | `mysql_table_list` | ❌ |

---

## Test 320

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843139 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.736565 | `monitor_table_list` | ❌ |
| 3 | 0.576731 | `monitor_workspace_list` | ❌ |
| 4 | 0.516593 | `tables_list` | ❌ |
| 5 | 0.509598 | `monitor_workspace_log_query` | ❌ |

---

## Test 321

**Expected Tool:** `monitor_webtests_create`  
**Prompt:** Create a new Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650719 | `monitor_webtests_create` | ✅ **EXPECTED** |
| 2 | 0.570207 | `monitor_webtests_list` | ❌ |
| 3 | 0.550072 | `monitor_webtests_update` | ❌ |
| 4 | 0.533352 | `monitor_webtests_get` | ❌ |
| 5 | 0.482145 | `loadtesting_testresource_create` | ❌ |

---

## Test 322

**Expected Tool:** `monitor_webtests_get`  
**Prompt:** Get Web Test details for <webtest_resource_name> in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759015 | `monitor_webtests_get` | ✅ **EXPECTED** |
| 2 | 0.725442 | `monitor_webtests_list` | ❌ |
| 3 | 0.583815 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.562797 | `monitor_webtests_update` | ❌ |
| 5 | 0.530190 | `monitor_webtests_create` | ❌ |

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
| 4 | 0.520875 | `redis_list` | ❌ |
| 5 | 0.496166 | `monitor_webtests_get` | ❌ |

---

## Test 324

**Expected Tool:** `monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793807 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.675965 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.584429 | `monitor_webtests_get` | ❌ |
| 4 | 0.573602 | `group_list` | ❌ |
| 5 | 0.546088 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 325

**Expected Tool:** `monitor_webtests_update`  
**Prompt:** Update an existing Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686449 | `monitor_webtests_update` | ✅ **EXPECTED** |
| 2 | 0.559296 | `monitor_webtests_get` | ❌ |
| 3 | 0.558211 | `monitor_webtests_create` | ❌ |
| 4 | 0.553726 | `monitor_webtests_list` | ❌ |
| 5 | 0.508736 | `loadtesting_testrun_update` | ❌ |

---

## Test 326

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813868 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.680111 | `grafana_list` | ❌ |
| 3 | 0.659478 | `monitor_table_list` | ❌ |
| 4 | 0.610512 | `kusto_cluster_list` | ❌ |
| 5 | 0.600816 | `search_service_list` | ❌ |

---

## Test 327

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656194 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.584631 | `monitor_table_list` | ❌ |
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
| 3 | 0.579574 | `monitor_table_list` | ❌ |
| 4 | 0.523782 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.522656 | `kusto_cluster_list` | ❌ |

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
| 5 | 0.485401 | `monitor_table_list` | ❌ |

---

## Test 330

**Expected Tool:** `datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669239 | `datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.454659 | `redis_list` | ❌ |
| 3 | 0.413617 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.412945 | `monitor_metrics_query` | ❌ |
| 5 | 0.401795 | `grafana_list` | ❌ |

---

## Test 331

**Expected Tool:** `datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624066 | `datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.443481 | `monitor_metrics_query` | ❌ |
| 3 | 0.440092 | `redis_list` | ❌ |
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
| 2 | 0.481143 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.476826 | `extension_azqr` | ✅ **EXPECTED** |
| 4 | 0.471551 | `subscription_list` | ❌ |
| 5 | 0.468404 | `applens_resource_diagnose` | ❌ |

---

## Test 333

**Expected Tool:** `extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532792 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.492863 | `get_bestpractices_get` | ❌ |
| 3 | 0.476164 | `applicationinsights_recommendation_list` | ❌ |
| 4 | 0.473474 | `azureaibestpractices_get` | ❌ |
| 5 | 0.473365 | `deploy_iac_rules_get` | ❌ |

---

## Test 334

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

## Test 335

**Expected Tool:** `quota_region_availability_list`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590878 | `quota_region_availability_list` | ✅ **EXPECTED** |
| 2 | 0.413274 | `quota_usage_check` | ❌ |
| 3 | 0.391387 | `redis_list` | ❌ |
| 4 | 0.372940 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.369828 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 336

**Expected Tool:** `quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609432 | `quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.491285 | `quota_region_availability_list` | ❌ |
| 3 | 0.386918 | `foundry_resource_get` | ❌ |
| 4 | 0.384067 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.376125 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 337

**Expected Tool:** `role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645258 | `role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.539806 | `subscription_list` | ❌ |
| 3 | 0.483988 | `group_list` | ❌ |
| 4 | 0.478700 | `grafana_list` | ❌ |
| 5 | 0.471364 | `cosmos_account_list` | ❌ |

---

## Test 338

**Expected Tool:** `role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609704 | `role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.514754 | `subscription_list` | ❌ |
| 3 | 0.456956 | `grafana_list` | ❌ |
| 4 | 0.449210 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.445141 | `redis_list` | ❌ |

---

## Test 339

**Expected Tool:** `redis_create`  
**Prompt:** Create a new Redis resource named <resource_name> with SKU <sku_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684958 | `redis_create` | ✅ **EXPECTED** |
| 2 | 0.491981 | `redis_list` | ❌ |
| 3 | 0.489599 | `storage_account_create` | ❌ |
| 4 | 0.457201 | `workbooks_create` | ❌ |
| 5 | 0.440892 | `eventhubs_namespace_update` | ❌ |

---

## Test 340

**Expected Tool:** `redis_create`  
**Prompt:** Create a new Redis resource for me  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638960 | `redis_create` | ✅ **EXPECTED** |
| 2 | 0.479259 | `redis_list` | ❌ |
| 3 | 0.374539 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.318545 | `loadtesting_testrun_create` | ❌ |
| 5 | 0.287562 | `workbooks_create` | ❌ |

---

## Test 341

**Expected Tool:** `redis_create`  
**Prompt:** Create a Redis cache named <resource_name> with SKU <sku_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622176 | `redis_create` | ✅ **EXPECTED** |
| 2 | 0.475871 | `storage_account_create` | ❌ |
| 3 | 0.464121 | `redis_list` | ❌ |
| 4 | 0.419044 | `eventhubs_namespace_update` | ❌ |
| 5 | 0.408168 | `workbooks_create` | ❌ |

---

## Test 342

**Expected Tool:** `redis_create`  
**Prompt:** Create a new Redis cluster with name <resource_name>, SKU <sku_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595030 | `redis_create` | ✅ **EXPECTED** |
| 2 | 0.425935 | `redis_list` | ❌ |
| 3 | 0.403540 | `kusto_cluster_get` | ❌ |
| 4 | 0.377343 | `eventhubs_namespace_update` | ❌ |
| 5 | 0.361824 | `storage_account_create` | ❌ |

---

## Test 343

**Expected Tool:** `redis_list`  
**Prompt:** List all Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.810555 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.587836 | `grafana_list` | ❌ |
| 3 | 0.512850 | `kusto_cluster_list` | ❌ |
| 4 | 0.508532 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.501218 | `postgres_server_list` | ❌ |

---

## Test 344

**Expected Tool:** `redis_list`  
**Prompt:** Show me my Redis resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685231 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.470132 | `redis_create` | ❌ |
| 3 | 0.374327 | `grafana_list` | ❌ |
| 4 | 0.364197 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.359659 | `mysql_server_list` | ❌ |

---

## Test 345

**Expected Tool:** `redis_list`  
**Prompt:** Show me the Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.781282 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.539177 | `grafana_list` | ❌ |
| 3 | 0.519600 | `redis_create` | ❌ |
| 4 | 0.449276 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.449014 | `postgres_server_list` | ❌ |

---

## Test 346

**Expected Tool:** `redis_list`  
**Prompt:** Show me my Redis caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572792 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.379419 | `redis_create` | ❌ |
| 3 | 0.316626 | `mysql_database_list` | ❌ |
| 4 | 0.301786 | `postgres_database_list` | ❌ |
| 5 | 0.286513 | `mysql_server_list` | ❌ |

---

## Test 347

**Expected Tool:** `redis_list`  
**Prompt:** Get Redis clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478125 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.456400 | `kusto_cluster_list` | ❌ |
| 3 | 0.425798 | `redis_create` | ❌ |
| 4 | 0.384630 | `kusto_cluster_get` | ❌ |
| 5 | 0.359434 | `kusto_database_list` | ❌ |

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
| 4 | 0.552633 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.549477 | `monitor_webtests_list` | ❌ |

---

## Test 349

**Expected Tool:** `group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529504 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.464762 | `redis_list` | ❌ |
| 3 | 0.463685 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.462391 | `mysql_server_list` | ❌ |
| 5 | 0.460280 | `loadtesting_testresource_list` | ❌ |

---

## Test 350

**Expected Tool:** `group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665772 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.532656 | `datadog_monitoredresources_list` | ❌ |
| 3 | 0.532547 | `redis_list` | ❌ |
| 4 | 0.531934 | `eventgrid_topic_list` | ❌ |
| 5 | 0.531920 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 351

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.556629 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
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
| 2 | 0.564128 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.556167 | `storage_blob_container_get` | ❌ |
| 4 | 0.509114 | `tables_list` | ❌ |
| 5 | 0.487207 | `storage_blob_get` | ❌ |

---

## Test 353

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577398 | `resourcehealth_availability-status_list` | ❌ |
| 2 | 0.501568 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.424939 | `mysql_server_list` | ❌ |
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
| 2 | 0.585499 | `redis_list` | ❌ |
| 3 | 0.549914 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.548549 | `grafana_list` | ❌ |
| 5 | 0.544551 | `subscription_list` | ❌ |

---

## Test 355

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644982 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.545208 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.509740 | `resourcehealth_health-events_list` | ❌ |
| 4 | 0.508252 | `quota_usage_check` | ❌ |
| 5 | 0.505780 | `redis_list` | ❌ |

---

## Test 356

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

## Test 357

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690720 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.554895 | `search_service_list` | ❌ |
| 3 | 0.534389 | `eventgrid_topic_list` | ❌ |
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
| 3 | 0.513815 | `search_service_list` | ❌ |
| 4 | 0.513473 | `eventgrid_topic_list` | ❌ |
| 5 | 0.501150 | `subscription_list` | ❌ |

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
| 5 | 0.211842 | `search_service_list` | ❌ |

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
| 4 | 0.520197 | `search_service_list` | ❌ |
| 5 | 0.502064 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 361

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

## Test 362

**Expected Tool:** `servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642876 | `servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.460932 | `servicebus_topic_subscription_details` | ❌ |
| 3 | 0.436980 | `servicebus_topic_details` | ❌ |
| 4 | 0.385812 | `search_knowledge_base_get` | ❌ |
| 5 | 0.384139 | `storage_account_get` | ❌ |

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
| 4 | 0.483187 | `eventgrid_topic_list` | ❌ |
| 5 | 0.458712 | `eventgrid_subscription_list` | ❌ |

---

## Test 364

**Expected Tool:** `servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633250 | `servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.517658 | `servicebus_topic_details` | ❌ |
| 3 | 0.494536 | `servicebus_queue_details` | ❌ |
| 4 | 0.494125 | `eventgrid_topic_list` | ❌ |
| 5 | 0.472124 | `eventgrid_subscription_list` | ❌ |

---

## Test 365

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show me the details of SignalR <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532544 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.355050 | `redis_list` | ❌ |
| 3 | 0.336067 | `foundry_resource_get` | ❌ |
| 4 | 0.319981 | `sql_server_show` | ❌ |
| 5 | 0.304420 | `servicebus_queue_details` | ❌ |

---

## Test 366

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show me the network information of SignalR runtime <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573446 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.337342 | `sql_server_show` | ❌ |
| 3 | 0.322982 | `foundry_resource_get` | ❌ |
| 4 | 0.305048 | `redis_list` | ❌ |
| 5 | 0.300956 | `servicebus_topic_details` | ❌ |

---

## Test 367

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Describe the SignalR runtime <signalr_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710353 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.419280 | `foundry_resource_get` | ❌ |
| 3 | 0.411396 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.399412 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.382028 | `sql_server_list` | ❌ |

---

## Test 368

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Get information about my SignalR runtime <signalr_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.715974 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.469364 | `foundry_resource_get` | ❌ |
| 3 | 0.430829 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.430765 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.417032 | `functionapp_get` | ❌ |

---

## Test 369

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show all the SignalRs information in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564071 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.501162 | `redis_list` | ❌ |
| 3 | 0.494478 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.481428 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.462090 | `mysql_server_list` | ❌ |

---

## Test 370

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** List all SignalRs in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530646 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.507654 | `postgres_server_list` | ❌ |
| 3 | 0.495145 | `redis_list` | ❌ |
| 4 | 0.494394 | `kusto_cluster_list` | ❌ |
| 5 | 0.487924 | `subscription_list` | ❌ |

---

## Test 371

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a new SQL database named <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.514735 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.470892 | `sql_server_create` | ❌ |
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
| 1 | 0.570117 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.459672 | `sql_server_create` | ❌ |
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
| 1 | 0.603599 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.545955 | `sql_server_create` | ❌ |
| 3 | 0.504037 | `sql_db_rename` | ❌ |
| 4 | 0.494440 | `sql_db_show` | ❌ |
| 5 | 0.474050 | `sql_db_list` | ❌ |

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
| 5 | 0.364776 | `sql_db_show` | ❌ |

---

## Test 375

**Expected Tool:** `sql_db_delete`  
**Prompt:** Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567513 | `sql_server_delete` | ❌ |
| 2 | 0.543440 | `sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.500756 | `sql_db_show` | ❌ |
| 4 | 0.481083 | `sql_db_rename` | ❌ |
| 5 | 0.478779 | `sql_db_list` | ❌ |

---

## Test 376

**Expected Tool:** `sql_db_delete`  
**Prompt:** Delete the database called <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509925 | `sql_db_delete` | ✅ **EXPECTED** |
| 2 | 0.490935 | `sql_server_delete` | ❌ |
| 3 | 0.364195 | `postgres_database_list` | ❌ |
| 4 | 0.355173 | `mysql_database_list` | ❌ |
| 5 | 0.347754 | `sql_db_rename` | ❌ |

---

## Test 377

**Expected Tool:** `sql_db_list`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643200 | `sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.639426 | `mysql_database_list` | ❌ |
| 3 | 0.609079 | `postgres_database_list` | ❌ |
| 4 | 0.602854 | `cosmos_database_list` | ❌ |
| 5 | 0.570083 | `kusto_database_list` | ❌ |

---

## Test 378

**Expected Tool:** `sql_db_list`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.617746 | `sql_server_show` | ❌ |
| 2 | 0.609389 | `sql_db_list` | ✅ **EXPECTED** |
| 3 | 0.557226 | `mysql_database_list` | ❌ |
| 4 | 0.553488 | `mysql_server_config_get` | ❌ |
| 5 | 0.524274 | `sql_db_show` | ❌ |

---

## Test 379

**Expected Tool:** `sql_db_rename`  
**Prompt:** Rename the SQL database <database_name> on server <server_name> to <new_database_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593619 | `sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.425346 | `sql_server_delete` | ❌ |
| 3 | 0.415956 | `sql_db_delete` | ❌ |
| 4 | 0.395420 | `sql_db_create` | ❌ |
| 5 | 0.345773 | `sql_db_show` | ❌ |

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
| 4 | 0.500380 | `sql_db_create` | ❌ |
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
| 3 | 0.530422 | `mysql_server_config_get` | ❌ |
| 4 | 0.528136 | `sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.465747 | `sql_db_list` | ❌ |

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
| 5 | 0.432746 | `mysql_database_list` | ❌ |

---

## Test 383

**Expected Tool:** `sql_db_update`  
**Prompt:** Update the performance tier of SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603366 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.466242 | `sql_db_create` | ❌ |
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
| 1 | 0.550556 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.418358 | `sql_server_delete` | ❌ |
| 3 | 0.401771 | `sql_db_list` | ❌ |
| 4 | 0.395518 | `sql_db_rename` | ❌ |
| 5 | 0.394770 | `sql_db_show` | ❌ |

---

## Test 385

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678124 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502389 | `sql_db_list` | ❌ |
| 3 | 0.498247 | `mysql_database_list` | ❌ |
| 4 | 0.485249 | `aks_nodepool_get` | ❌ |
| 5 | 0.479044 | `sql_server_show` | ❌ |

---

## Test 386

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606425 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502877 | `sql_server_show` | ❌ |
| 3 | 0.457193 | `sql_db_list` | ❌ |
| 4 | 0.450743 | `aks_nodepool_get` | ❌ |
| 5 | 0.432715 | `mysql_database_list` | ❌ |

---

## Test 387

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592709 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.420248 | `mysql_database_list` | ❌ |
| 3 | 0.407169 | `aks_nodepool_get` | ❌ |
| 4 | 0.402616 | `mysql_server_list` | ❌ |
| 5 | 0.397657 | `sql_db_list` | ❌ |

---

## Test 388

**Expected Tool:** `sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682605 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.562642 | `sql_db_create` | ❌ |
| 3 | 0.529198 | `sql_server_list` | ❌ |
| 4 | 0.482102 | `storage_account_create` | ❌ |
| 5 | 0.476194 | `redis_create` | ❌ |

---

## Test 389

**Expected Tool:** `sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618309 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.509377 | `sql_db_create` | ❌ |
| 3 | 0.472463 | `sql_server_show` | ❌ |
| 4 | 0.441174 | `sql_server_delete` | ❌ |
| 5 | 0.417796 | `redis_create` | ❌ |

---

## Test 390

**Expected Tool:** `sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589818 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.499976 | `sql_db_create` | ❌ |
| 3 | 0.497890 | `sql_server_list` | ❌ |
| 4 | 0.461181 | `sql_db_rename` | ❌ |
| 5 | 0.442934 | `mysql_server_list` | ❌ |

---

## Test 391

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

## Test 392

**Expected Tool:** `sql_server_delete`  
**Prompt:** Remove the SQL server <server_name> from my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615073 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.393885 | `postgres_server_list` | ❌ |
| 3 | 0.379760 | `sql_db_delete` | ❌ |
| 4 | 0.376660 | `sql_server_show` | ❌ |
| 5 | 0.350103 | `sql_server_list` | ❌ |

---

## Test 393

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
| 5 | 0.376108 | `sql_db_list` | ❌ |

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
| 4 | 0.316036 | `sql_db_list` | ❌ |
| 5 | 0.311085 | `postgres_server_list` | ❌ |

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
| 4 | 0.307823 | `sql_server_create` | ❌ |
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
| 4 | 0.448822 | `sql_server_create` | ❌ |
| 5 | 0.440845 | `sql_server_delete` | ❌ |

---

## Test 398

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669404 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.532665 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.502728 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.316586 | `sql_server_list` | ❌ |
| 5 | 0.302488 | `sql_server_delete` | ❌ |

---

## Test 399

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685107 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.574336 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539577 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.428920 | `sql_server_create` | ❌ |
| 5 | 0.393420 | `sql_db_create` | ❌ |

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
| 1 | 0.630740 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.524126 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.476770 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.410704 | `sql_server_show` | ❌ |
| 5 | 0.348145 | `sql_server_list` | ❌ |

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
| 2 | 0.596686 | `mysql_server_list` | ❌ |
| 3 | 0.578311 | `sql_db_list` | ❌ |
| 4 | 0.515851 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.509789 | `sql_db_show` | ❌ |

---

## Test 407

**Expected Tool:** `sql_server_list`  
**Prompt:** Show me every SQL server available in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618218 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.593837 | `mysql_server_list` | ❌ |
| 3 | 0.542445 | `sql_db_list` | ❌ |
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
| 4 | 0.559893 | `mysql_server_list` | ❌ |
| 5 | 0.540243 | `sql_db_list` | ❌ |

---

## Test 409

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

## Test 410

**Expected Tool:** `sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563103 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.392452 | `postgres_server_config_get` | ❌ |
| 3 | 0.379946 | `postgres_server_param_get` | ❌ |
| 4 | 0.372126 | `sql_server_firewall-rule_list` | ❌ |
| 5 | 0.370508 | `sql_db_show` | ❌ |

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
| 5 | 0.370957 | `managedlustre_fs_create` | ❌ |

---

## Test 412

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500638 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.483202 | `managedlustre_fs_create` | ❌ |
| 3 | 0.407222 | `storage_account_get` | ❌ |
| 4 | 0.406804 | `storage_blob_container_create` | ❌ |
| 5 | 0.400179 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 413

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589095 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.535482 | `managedlustre_fs_create` | ❌ |
| 3 | 0.509795 | `storage_blob_container_create` | ❌ |
| 4 | 0.462558 | `storage_account_get` | ❌ |
| 5 | 0.447623 | `sql_db_create` | ❌ |

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
| 4 | 0.494746 | `tables_list` | ❌ |
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
| 5 | 0.462024 | `tables_list` | ❌ |

---

## Test 416

**Expected Tool:** `storage_account_get`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649215 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.563812 | `tables_list` | ❌ |
| 3 | 0.557074 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.550163 | `storage_blob_container_get` | ❌ |
| 5 | 0.547563 | `subscription_list` | ❌ |

---

## Test 417

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.556860 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.482418 | `storage_blob_container_get` | ❌ |
| 3 | 0.478618 | `tables_list` | ❌ |
| 4 | 0.461284 | `managedlustre_fs_list` | ❌ |
| 5 | 0.421642 | `cosmos_account_list` | ❌ |

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
| 4 | 0.516371 | `tables_list` | ❌ |
| 5 | 0.473598 | `cosmos_account_list` | ❌ |

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
| 5 | 0.447784 | `cosmos_database_container_list` | ❌ |

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
| 5 | 0.435099 | `cosmos_database_container_list` | ❌ |

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
| 5 | 0.523289 | `cosmos_database_container_list` | ❌ |

---

## Test 423

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712037 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.680802 | `storage_blob_get` | ❌ |
| 3 | 0.613933 | `cosmos_database_container_list` | ❌ |
| 4 | 0.598086 | `tables_list` | ❌ |
| 5 | 0.556319 | `storage_blob_container_create` | ❌ |

---

## Test 424

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713527 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.592373 | `cosmos_database_container_list` | ❌ |
| 3 | 0.586169 | `storage_blob_get` | ❌ |
| 4 | 0.580835 | `tables_list` | ❌ |
| 5 | 0.523322 | `storage_account_get` | ❌ |

---

## Test 425

**Expected Tool:** `storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.700955 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.646922 | `storage_blob_container_get` | ❌ |
| 3 | 0.540969 | `storage_blob_container_create` | ❌ |
| 4 | 0.527342 | `storage_account_get` | ❌ |
| 5 | 0.477915 | `cosmos_database_container_list` | ❌ |

---

## Test 426

**Expected Tool:** `storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694997 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.631161 | `storage_blob_container_get` | ❌ |
| 3 | 0.589151 | `storage_blob_container_create` | ❌ |
| 4 | 0.580226 | `storage_account_get` | ❌ |
| 5 | 0.457038 | `storage_account_create` | ❌ |

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
| 4 | 0.579070 | `cosmos_database_container_list` | ❌ |
| 5 | 0.561320 | `tables_list` | ❌ |

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
| 4 | 0.533515 | `cosmos_database_container_list` | ❌ |
| 5 | 0.492652 | `tables_list` | ❌ |

---

## Test 429

**Expected Tool:** `storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566299 | `storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.525640 | `storage_blob_container_create` | ❌ |
| 3 | 0.517578 | `storage_blob_get` | ❌ |
| 4 | 0.473597 | `storage_blob_container_get` | ❌ |
| 5 | 0.382116 | `storage_account_create` | ❌ |

---

## Test 430

**Expected Tool:** `subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654119 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512964 | `cosmos_account_list` | ❌ |
| 3 | 0.471653 | `postgres_server_list` | ❌ |
| 4 | 0.468890 | `kusto_cluster_list` | ❌ |
| 5 | 0.461043 | `redis_list` | ❌ |

---

## Test 431

**Expected Tool:** `subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.458843 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.407471 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.393870 | `eventgrid_topic_list` | ❌ |
| 4 | 0.391493 | `redis_list` | ❌ |
| 5 | 0.381238 | `postgres_server_list` | ❌ |

---

## Test 432

**Expected Tool:** `subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433144 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.319579 | `marketplace_product_list` | ❌ |
| 3 | 0.315547 | `marketplace_product_get` | ❌ |
| 4 | 0.293772 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.289525 | `eventgrid_topic_list` | ❌ |

---

## Test 433

**Expected Tool:** `subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.477587 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.357625 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.354286 | `marketplace_product_list` | ❌ |
| 4 | 0.344502 | `redis_list` | ❌ |
| 5 | 0.341002 | `eventgrid_topic_list` | ❌ |

---

## Test 434

**Expected Tool:** `azmcp_tables_list`  
**Prompt:** List all tables in the storage account <storage-account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.859210 | `tables_list` | ❌ |
| 2 | 0.574655 | `storage_blob_container_get` | ❌ |
| 3 | 0.570754 | `monitor_table_list` | ❌ |
| 4 | 0.548671 | `mysql_table_list` | ❌ |
| 5 | 0.530735 | `kusto_table_list` | ❌ |

---

## Test 435

**Expected Tool:** `azmcp_tables_list`  
**Prompt:** Show me the tables in the storage account <storage-account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.801059 | `tables_list` | ❌ |
| 2 | 0.577541 | `storage_blob_container_get` | ❌ |
| 3 | 0.526684 | `storage_account_get` | ❌ |
| 4 | 0.526279 | `monitor_table_list` | ❌ |
| 5 | 0.521996 | `mysql_table_list` | ❌ |

---

## Test 436

**Expected Tool:** `azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686886 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.625270 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.605048 | `get_bestpractices_get` | ❌ |
| 4 | 0.482936 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.480899 | `azureaibestpractices_get` | ❌ |

---

## Test 437

**Expected Tool:** `azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581316 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.512141 | `get_bestpractices_get` | ❌ |
| 3 | 0.510005 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.473362 | `keyvault_secret_get` | ❌ |
| 5 | 0.456553 | `azureaibestpractices_get` | ❌ |

---

## Test 438

**Expected Tool:** `virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711933 | `virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
| 2 | 0.659763 | `virtualdesktop_hostpool_host_list` | ❌ |
| 3 | 0.620537 | `kusto_cluster_list` | ❌ |
| 4 | 0.548888 | `search_service_list` | ❌ |
| 5 | 0.535739 | `virtualdesktop_hostpool_host_user-list` | ❌ |

---

## Test 439

**Expected Tool:** `virtualdesktop_hostpool_host_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.727054 | `virtualdesktop_hostpool_host_list` | ✅ **EXPECTED** |
| 2 | 0.714468 | `virtualdesktop_hostpool_host_user-list` | ❌ |
| 3 | 0.573364 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.438659 | `aks_nodepool_get` | ❌ |
| 5 | 0.393721 | `sql_elastic-pool_list` | ❌ |

---

## Test 440

**Expected Tool:** `virtualdesktop_hostpool_host_user-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812659 | `virtualdesktop_hostpool_host_user-list` | ✅ **EXPECTED** |
| 2 | 0.659213 | `virtualdesktop_hostpool_host_list` | ❌ |
| 3 | 0.501136 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.357561 | `aks_nodepool_get` | ❌ |
| 5 | 0.336385 | `monitor_workspace_list` | ❌ |

---

## Test 441

**Expected Tool:** `workbooks_create`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552211 | `workbooks_create` | ✅ **EXPECTED** |
| 2 | 0.417965 | `workbooks_update` | ❌ |
| 3 | 0.361395 | `workbooks_delete` | ❌ |
| 4 | 0.329117 | `workbooks_show` | ❌ |
| 5 | 0.328106 | `workbooks_list` | ❌ |

---

## Test 442

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

## Test 443

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

## Test 444

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

## Test 445

**Expected Tool:** `workbooks_show`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686087 | `workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.498390 | `workbooks_create` | ❌ |
| 3 | 0.494708 | `workbooks_list` | ❌ |
| 4 | 0.463156 | `workbooks_update` | ❌ |
| 5 | 0.452348 | `workbooks_delete` | ❌ |

---

## Test 446

**Expected Tool:** `workbooks_show`  
**Prompt:** Show me the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581508 | `workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.500470 | `workbooks_list` | ❌ |
| 3 | 0.468970 | `workbooks_create` | ❌ |
| 4 | 0.466297 | `workbooks_update` | ❌ |
| 5 | 0.455344 | `workbooks_delete` | ❌ |

---

## Test 447

**Expected Tool:** `workbooks_update`  
**Prompt:** Update the workbook <workbook_resource_id> with a new text step  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586347 | `workbooks_update` | ✅ **EXPECTED** |
| 2 | 0.382651 | `workbooks_create` | ❌ |
| 3 | 0.349689 | `workbooks_delete` | ❌ |
| 4 | 0.347944 | `workbooks_show` | ❌ |
| 5 | 0.292904 | `loadtesting_testrun_update` | ❌ |

---

## Test 448

**Expected Tool:** `bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543154 | `bicepschema_get` | ✅ **EXPECTED** |
| 2 | 0.485889 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.478288 | `foundry_models_deploy` | ❌ |
| 4 | 0.478098 | `azureaibestpractices_get` | ❌ |
| 5 | 0.448373 | `get_bestpractices_get` | ❌ |

---

## Test 449

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502125 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.290902 | `storage_blob_upload` | ❌ |
| 3 | 0.259162 | `managedlustre_fs_create` | ❌ |
| 4 | 0.254991 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.245066 | `managedlustre_fs_subnetsize_validate` | ❌ |

---

## Test 450

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Help me design an Azure cloud service that will serve as an ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.508153 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.377941 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.346883 | `azureaibestpractices_get` | ❌ |
| 4 | 0.341462 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.328747 | `get_bestpractices_get` | ❌ |

---

## Test 451

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.423577 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.271943 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.265972 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.251606 | `azureaibestpractices_get` | ❌ |
| 5 | 0.242581 | `deploy_plan_get` | ❌ |

---

## Test 452

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.534690 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.369969 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.356331 | `managedlustre_fs_create` | ❌ |
| 4 | 0.352797 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.331957 | `azureaibestpractices_get` | ❌ |

---

## Test 453

**Expected Tool:** `foundry_agents_connect`  
**Prompt:** Query an agent in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653761 | `foundry_agents_connect` | ✅ **EXPECTED** |
| 2 | 0.593439 | `foundry_agents_get-sdk-sample` | ❌ |
| 3 | 0.587483 | `foundry_threads_list` | ❌ |
| 4 | 0.560051 | `foundry_agents_list` | ❌ |
| 5 | 0.553874 | `foundry_threads_get-messages` | ❌ |

---

## Test 454

**Expected Tool:** `foundry_agents_create`  
**Prompt:** Create a new Microsoft Foundry agent using instructions in the active editor  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.651806 | `foundry_agents_create` | ✅ **EXPECTED** |
| 2 | 0.605298 | `foundry_threads_create` | ❌ |
| 3 | 0.592591 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.507379 | `foundry_threads_list` | ❌ |
| 5 | 0.450138 | `foundry_threads_get-messages` | ❌ |

---

## Test 455

**Expected Tool:** `foundry_agents_evaluate`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.544124 | `foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.497672 | `foundry_agents_connect` | ❌ |
| 3 | 0.469428 | `foundry_agents_evaluate` | ✅ **EXPECTED** |
| 4 | 0.283016 | `foundry_agents_list` | ❌ |
| 5 | 0.268553 | `foundry_threads_get-messages` | ❌ |

---

## Test 456

**Expected Tool:** `foundry_agents_get-sdk-sample`  
**Prompt:** Create a CLI app that can talk to a Microsoft Foundry Agent using Python SDK  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640177 | `foundry_agents_get-sdk-sample` | ✅ **EXPECTED** |
| 2 | 0.542864 | `foundry_threads_create` | ❌ |
| 3 | 0.542346 | `foundry_agents_create` | ❌ |
| 4 | 0.473155 | `foundry_agents_connect` | ❌ |
| 5 | 0.464149 | `foundry_threads_list` | ❌ |

---

## Test 457

**Expected Tool:** `foundry_agents_list`  
**Prompt:** List all agents in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.681576 | `foundry_threads_list` | ❌ |
| 2 | 0.680850 | `foundry_agents_list` | ✅ **EXPECTED** |
| 3 | 0.574383 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.565382 | `foundry_resource_get` | ❌ |
| 5 | 0.553438 | `foundry_threads_get-messages` | ❌ |

---

## Test 458

**Expected Tool:** `foundry_agents_list`  
**Prompt:** Show me the available agents in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656082 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.639619 | `foundry_threads_list` | ❌ |
| 3 | 0.613719 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.546827 | `foundry_resource_get` | ❌ |
| 5 | 0.538991 | `foundry_threads_get-messages` | ❌ |

---

## Test 459

**Expected Tool:** `foundry_agents_query-and-evaluate`  
**Prompt:** Query and evaluate an agent in my Microsoft Foundry resource for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623476 | `foundry_agents_connect` | ❌ |
| 2 | 0.585015 | `foundry_agents_query-and-evaluate` | ✅ **EXPECTED** |
| 3 | 0.508685 | `foundry_agents_evaluate` | ❌ |
| 4 | 0.499770 | `foundry_agents_list` | ❌ |
| 5 | 0.468929 | `foundry_agents_get-sdk-sample` | ❌ |

---

## Test 460

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my Microsoft Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.709443 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.549219 | `foundry_knowledge_index_schema` | ❌ |
| 3 | 0.498884 | `foundry_agents_list` | ❌ |
| 4 | 0.462128 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.449142 | `foundry_threads_list` | ❌ |

---

## Test 461

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** Show me the knowledge indexes in my Microsoft Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.597932 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.499788 | `foundry_knowledge_index_schema` | ❌ |
| 3 | 0.425795 | `foundry_agents_get-sdk-sample` | ❌ |
| 4 | 0.416167 | `foundry_agents_list` | ❌ |
| 5 | 0.411250 | `foundry_resource_get` | ❌ |

---

## Test 462

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

## Test 463

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

## Test 464

**Expected Tool:** `foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565477 | `foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.310121 | `foundry_openai_models-list` | ❌ |
| 3 | 0.298490 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.293050 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.282464 | `mysql_server_list` | ❌ |

---

## Test 465

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** List all Microsoft Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672200 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.646986 | `foundry_openai_models-list` | ❌ |
| 3 | 0.589990 | `foundry_resource_get` | ❌ |
| 4 | 0.579329 | `foundry_threads_list` | ❌ |
| 5 | 0.565886 | `foundry_models_list` | ❌ |

---

## Test 466

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

## Test 467

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

## Test 468

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

## Test 469

**Expected Tool:** `foundry_openai_chat-completions-create`  
**Prompt:** Create a chat completion with the message "Hello, how are you today?" using my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571040 | `foundry_openai_chat-completions-create` | ✅ **EXPECTED** |
| 2 | 0.471693 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.405305 | `foundry_threads_create` | ❌ |
| 4 | 0.349571 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.346970 | `foundry_agents_create` | ❌ |

---

## Test 470

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

## Test 471

**Expected Tool:** `foundry_openai_embeddings-create`  
**Prompt:** Generate embeddings for the text "Azure OpenAI Service" using my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.751801 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.532600 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.532551 | `foundry_models_deploy` | ❌ |
| 4 | 0.521119 | `foundry_openai_models-list` | ❌ |
| 5 | 0.494236 | `foundry_resource_get` | ❌ |

---

## Test 472

**Expected Tool:** `foundry_openai_embeddings-create`  
**Prompt:** Create vector embeddings for my text using my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650333 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.425667 | `foundry_resource_get` | ❌ |
| 3 | 0.413294 | `foundry_models_deploy` | ❌ |
| 4 | 0.411815 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.405917 | `foundry_agents_create` | ❌ |

---

## Test 473

**Expected Tool:** `foundry_openai_models-list`  
**Prompt:** List all available OpenAI models in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.783808 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.642953 | `foundry_models_deployments_list` | ❌ |
| 3 | 0.634098 | `foundry_resource_get` | ❌ |
| 4 | 0.631001 | `foundry_agents_list` | ❌ |
| 5 | 0.622537 | `foundry_models_list` | ❌ |

---

## Test 474

**Expected Tool:** `foundry_openai_models-list`  
**Prompt:** Show me the OpenAI model deployments in my Microsoft Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729835 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.655589 | `foundry_models_deploy` | ❌ |
| 3 | 0.639541 | `foundry_models_deployments_list` | ❌ |
| 4 | 0.617675 | `foundry_resource_get` | ❌ |
| 5 | 0.605893 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 475

**Expected Tool:** `foundry_resource_get`  
**Prompt:** List all Microsoft Foundry resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630611 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.596863 | `foundry_threads_list` | ❌ |
| 3 | 0.558609 | `foundry_openai_models-list` | ❌ |
| 4 | 0.542898 | `redis_list` | ❌ |
| 5 | 0.526428 | `foundry_agents_list` | ❌ |

---

## Test 476

**Expected Tool:** `foundry_resource_get`  
**Prompt:** Show me the Microsoft Foundry resources in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634839 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.527671 | `foundry_openai_models-list` | ❌ |
| 3 | 0.524778 | `foundry_threads_list` | ❌ |
| 4 | 0.488532 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.480089 | `foundry_agents_list` | ❌ |

---

## Test 477

**Expected Tool:** `foundry_resource_get`  
**Prompt:** Get details for Microsoft Foundry resource <resource_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728275 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.544746 | `foundry_openai_models-list` | ❌ |
| 3 | 0.506476 | `monitor_webtests_get` | ❌ |
| 4 | 0.481577 | `functionapp_get` | ❌ |
| 5 | 0.480676 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 478

**Expected Tool:** `foundry_threads_create`  
**Prompt:** Create a Microsoft Foundry thread to hold the conversation  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671344 | `foundry_threads_create` | ✅ **EXPECTED** |
| 2 | 0.551672 | `foundry_threads_get-messages` | ❌ |
| 3 | 0.545637 | `foundry_threads_list` | ❌ |
| 4 | 0.493505 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.487203 | `foundry_agents_create` | ❌ |

---

## Test 479

**Expected Tool:** `foundry_threads_get-messages`  
**Prompt:** Show me the messages in the Microsoft Foundry thread with id <thread_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662733 | `foundry_threads_get-messages` | ✅ **EXPECTED** |
| 2 | 0.553252 | `foundry_threads_create` | ❌ |
| 3 | 0.538595 | `foundry_threads_list` | ❌ |
| 4 | 0.419581 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.404274 | `foundry_agents_create` | ❌ |

---

## Test 480

**Expected Tool:** `foundry_threads_list`  
**Prompt:** List my Microsoft Foundry threads  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.720787 | `foundry_threads_list` | ✅ **EXPECTED** |
| 2 | 0.598835 | `foundry_threads_get-messages` | ❌ |
| 3 | 0.572404 | `foundry_threads_create` | ❌ |
| 4 | 0.479893 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.396690 | `foundry_resource_get` | ❌ |

---

## Summary

**Total Prompts Tested:** 480  
**Analysis Execution Time:** 126.6265823s  

### Success Rate Metrics

**Top Choice Success:** 91.7% (440/480 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 3.1% (15/480 tests)  
**🎯 High Confidence (≥0.7):** 21.5% (103/480 tests)  
**✅ Good Confidence (≥0.6):** 60.8% (292/480 tests)  
**👍 Fair Confidence (≥0.5):** 91.5% (439/480 tests)  
**👌 Acceptable Confidence (≥0.4):** 99.2% (476/480 tests)  
**❌ Low Confidence (<0.4):** 0.8% (4/480 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 3.1% (15/480 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 21.5% (103/480 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 58.5% (281/480 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 85.8% (412/480 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 91.7% (440/480 tests)  

### Success Rate Analysis

🟢 **Excellent** - The tool selection system is performing exceptionally well.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

