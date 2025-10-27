# Tool Selection Analysis Setup

**Setup completed:** 2025-10-20 20:26:41  
**Tool count:** 173  
**Database setup time:** 9.5647803s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-10-20 20:26:41  
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
- [Test 84: applicationinsights_recommendation_list](#test-84)
- [Test 85: applicationinsights_recommendation_list](#test-85)
- [Test 86: applicationinsights_recommendation_list](#test-86)
- [Test 87: applicationinsights_recommendation_list](#test-87)
- [Test 88: extension_cli_generate](#test-88)
- [Test 89: extension_cli_generate](#test-89)
- [Test 90: extension_cli_generate](#test-90)
- [Test 91: extension_cli_install](#test-91)
- [Test 92: extension_cli_install](#test-92)
- [Test 93: extension_cli_install](#test-93)
- [Test 94: acr_registry_list](#test-94)
- [Test 95: acr_registry_list](#test-95)
- [Test 96: acr_registry_list](#test-96)
- [Test 97: acr_registry_list](#test-97)
- [Test 98: acr_registry_list](#test-98)
- [Test 99: acr_registry_repository_list](#test-99)
- [Test 100: acr_registry_repository_list](#test-100)
- [Test 101: acr_registry_repository_list](#test-101)
- [Test 102: acr_registry_repository_list](#test-102)
- [Test 103: communication_email_send](#test-103)
- [Test 104: communication_email_send](#test-104)
- [Test 105: communication_email_send](#test-105)
- [Test 106: communication_email_send](#test-106)
- [Test 107: communication_email_send](#test-107)
- [Test 108: communication_email_send](#test-108)
- [Test 109: communication_email_send](#test-109)
- [Test 110: communication_email_send](#test-110)
- [Test 111: communication_sms_send](#test-111)
- [Test 112: communication_sms_send](#test-112)
- [Test 113: communication_sms_send](#test-113)
- [Test 114: communication_sms_send](#test-114)
- [Test 115: communication_sms_send](#test-115)
- [Test 116: communication_sms_send](#test-116)
- [Test 117: communication_sms_send](#test-117)
- [Test 118: communication_sms_send](#test-118)
- [Test 119: confidentialledger_entries_append](#test-119)
- [Test 120: confidentialledger_entries_append](#test-120)
- [Test 121: confidentialledger_entries_append](#test-121)
- [Test 122: confidentialledger_entries_append](#test-122)
- [Test 123: confidentialledger_entries_append](#test-123)
- [Test 124: confidentialledger_entries_get](#test-124)
- [Test 125: confidentialledger_entries_get](#test-125)
- [Test 126: cosmos_account_list](#test-126)
- [Test 127: cosmos_account_list](#test-127)
- [Test 128: cosmos_account_list](#test-128)
- [Test 129: cosmos_database_container_item_query](#test-129)
- [Test 130: cosmos_database_container_list](#test-130)
- [Test 131: cosmos_database_container_list](#test-131)
- [Test 132: cosmos_database_list](#test-132)
- [Test 133: cosmos_database_list](#test-133)
- [Test 134: kusto_cluster_get](#test-134)
- [Test 135: kusto_cluster_list](#test-135)
- [Test 136: kusto_cluster_list](#test-136)
- [Test 137: kusto_cluster_list](#test-137)
- [Test 138: kusto_database_list](#test-138)
- [Test 139: kusto_database_list](#test-139)
- [Test 140: kusto_query](#test-140)
- [Test 141: kusto_sample](#test-141)
- [Test 142: kusto_table_list](#test-142)
- [Test 143: kusto_table_list](#test-143)
- [Test 144: kusto_table_schema](#test-144)
- [Test 145: mysql_database_list](#test-145)
- [Test 146: mysql_database_list](#test-146)
- [Test 147: mysql_database_query](#test-147)
- [Test 148: mysql_server_config_get](#test-148)
- [Test 149: mysql_server_list](#test-149)
- [Test 150: mysql_server_list](#test-150)
- [Test 151: mysql_server_list](#test-151)
- [Test 152: mysql_server_param_get](#test-152)
- [Test 153: mysql_server_param_set](#test-153)
- [Test 154: mysql_table_list](#test-154)
- [Test 155: mysql_table_list](#test-155)
- [Test 156: mysql_table_schema_get](#test-156)
- [Test 157: postgres_database_list](#test-157)
- [Test 158: postgres_database_list](#test-158)
- [Test 159: postgres_database_query](#test-159)
- [Test 160: postgres_server_config_get](#test-160)
- [Test 161: postgres_server_list](#test-161)
- [Test 162: postgres_server_list](#test-162)
- [Test 163: postgres_server_list](#test-163)
- [Test 164: postgres_server_param_get](#test-164)
- [Test 165: postgres_server_param_set](#test-165)
- [Test 166: postgres_table_list](#test-166)
- [Test 167: postgres_table_list](#test-167)
- [Test 168: postgres_table_schema_get](#test-168)
- [Test 169: deploy_app_logs_get](#test-169)
- [Test 170: deploy_architecture_diagram_generate](#test-170)
- [Test 171: deploy_iac_rules_get](#test-171)
- [Test 172: deploy_pipeline_guidance_get](#test-172)
- [Test 173: deploy_plan_get](#test-173)
- [Test 174: eventgrid_events_publish](#test-174)
- [Test 175: eventgrid_events_publish](#test-175)
- [Test 176: eventgrid_events_publish](#test-176)
- [Test 177: eventgrid_topic_list](#test-177)
- [Test 178: eventgrid_topic_list](#test-178)
- [Test 179: eventgrid_topic_list](#test-179)
- [Test 180: eventgrid_topic_list](#test-180)
- [Test 181: eventgrid_subscription_list](#test-181)
- [Test 182: eventgrid_subscription_list](#test-182)
- [Test 183: eventgrid_subscription_list](#test-183)
- [Test 184: eventgrid_subscription_list](#test-184)
- [Test 185: eventgrid_subscription_list](#test-185)
- [Test 186: eventgrid_subscription_list](#test-186)
- [Test 187: eventgrid_subscription_list](#test-187)
- [Test 188: eventhubs_consumergroup_delete](#test-188)
- [Test 189: eventhubs_consumergroup_get](#test-189)
- [Test 190: eventhubs_consumergroup_get](#test-190)
- [Test 191: eventhubs_consumergroup_update](#test-191)
- [Test 192: eventhubs_consumergroup_update](#test-192)
- [Test 193: eventhubs_eventhub_delete](#test-193)
- [Test 194: eventhubs_eventhub_get](#test-194)
- [Test 195: eventhubs_eventhub_get](#test-195)
- [Test 196: eventhubs_eventhub_update](#test-196)
- [Test 197: eventhubs_eventhub_update](#test-197)
- [Test 198: eventhubs_namespace_delete](#test-198)
- [Test 199: eventhubs_namespace_get](#test-199)
- [Test 200: eventhubs_namespace_get](#test-200)
- [Test 201: eventhubs_namespace_update](#test-201)
- [Test 202: eventhubs_namespace_update](#test-202)
- [Test 203: functionapp_get](#test-203)
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
- [Test 215: keyvault_admin_settings_get](#test-215)
- [Test 216: keyvault_admin_settings_get](#test-216)
- [Test 217: keyvault_admin_settings_get](#test-217)
- [Test 218: keyvault_certificate_create](#test-218)
- [Test 219: keyvault_certificate_create](#test-219)
- [Test 220: keyvault_certificate_create](#test-220)
- [Test 221: keyvault_certificate_create](#test-221)
- [Test 222: keyvault_certificate_create](#test-222)
- [Test 223: keyvault_certificate_get](#test-223)
- [Test 224: keyvault_certificate_get](#test-224)
- [Test 225: keyvault_certificate_get](#test-225)
- [Test 226: keyvault_certificate_get](#test-226)
- [Test 227: keyvault_certificate_get](#test-227)
- [Test 228: keyvault_certificate_import](#test-228)
- [Test 229: keyvault_certificate_import](#test-229)
- [Test 230: keyvault_certificate_import](#test-230)
- [Test 231: keyvault_certificate_import](#test-231)
- [Test 232: keyvault_certificate_import](#test-232)
- [Test 233: keyvault_certificate_list](#test-233)
- [Test 234: keyvault_certificate_list](#test-234)
- [Test 235: keyvault_certificate_list](#test-235)
- [Test 236: keyvault_certificate_list](#test-236)
- [Test 237: keyvault_certificate_list](#test-237)
- [Test 238: keyvault_certificate_list](#test-238)
- [Test 239: keyvault_key_create](#test-239)
- [Test 240: keyvault_key_create](#test-240)
- [Test 241: keyvault_key_create](#test-241)
- [Test 242: keyvault_key_create](#test-242)
- [Test 243: keyvault_key_create](#test-243)
- [Test 244: keyvault_key_get](#test-244)
- [Test 245: keyvault_key_get](#test-245)
- [Test 246: keyvault_key_get](#test-246)
- [Test 247: keyvault_key_get](#test-247)
- [Test 248: keyvault_key_get](#test-248)
- [Test 249: keyvault_key_list](#test-249)
- [Test 250: keyvault_key_list](#test-250)
- [Test 251: keyvault_key_list](#test-251)
- [Test 252: keyvault_key_list](#test-252)
- [Test 253: keyvault_key_list](#test-253)
- [Test 254: keyvault_key_list](#test-254)
- [Test 255: keyvault_secret_create](#test-255)
- [Test 256: keyvault_secret_create](#test-256)
- [Test 257: keyvault_secret_create](#test-257)
- [Test 258: keyvault_secret_create](#test-258)
- [Test 259: keyvault_secret_create](#test-259)
- [Test 260: keyvault_secret_get](#test-260)
- [Test 261: keyvault_secret_get](#test-261)
- [Test 262: keyvault_secret_get](#test-262)
- [Test 263: keyvault_secret_get](#test-263)
- [Test 264: keyvault_secret_get](#test-264)
- [Test 265: keyvault_secret_list](#test-265)
- [Test 266: keyvault_secret_list](#test-266)
- [Test 267: keyvault_secret_list](#test-267)
- [Test 268: keyvault_secret_list](#test-268)
- [Test 269: keyvault_secret_list](#test-269)
- [Test 270: keyvault_secret_list](#test-270)
- [Test 271: aks_cluster_get](#test-271)
- [Test 272: aks_cluster_get](#test-272)
- [Test 273: aks_cluster_get](#test-273)
- [Test 274: aks_cluster_get](#test-274)
- [Test 275: aks_cluster_get](#test-275)
- [Test 276: aks_cluster_get](#test-276)
- [Test 277: aks_cluster_get](#test-277)
- [Test 278: aks_nodepool_get](#test-278)
- [Test 279: aks_nodepool_get](#test-279)
- [Test 280: aks_nodepool_get](#test-280)
- [Test 281: aks_nodepool_get](#test-281)
- [Test 282: aks_nodepool_get](#test-282)
- [Test 283: aks_nodepool_get](#test-283)
- [Test 284: loadtesting_test_create](#test-284)
- [Test 285: loadtesting_test_get](#test-285)
- [Test 286: loadtesting_testresource_create](#test-286)
- [Test 287: loadtesting_testresource_list](#test-287)
- [Test 288: loadtesting_testrun_create](#test-288)
- [Test 289: loadtesting_testrun_get](#test-289)
- [Test 290: loadtesting_testrun_list](#test-290)
- [Test 291: loadtesting_testrun_update](#test-291)
- [Test 292: grafana_list](#test-292)
- [Test 293: managedlustre_filesystem_create](#test-293)
- [Test 294: managedlustre_filesystem_list](#test-294)
- [Test 295: managedlustre_filesystem_list](#test-295)
- [Test 296: managedlustre_filesystem_sku_get](#test-296)
- [Test 297: managedlustre_filesystem_subnetsize_ask](#test-297)
- [Test 298: managedlustre_filesystem_subnetsize_validate](#test-298)
- [Test 299: managedlustre_filesystem_update](#test-299)
- [Test 300: marketplace_product_get](#test-300)
- [Test 301: marketplace_product_list](#test-301)
- [Test 302: marketplace_product_list](#test-302)
- [Test 303: get_bestpractices_get](#test-303)
- [Test 304: get_bestpractices_get](#test-304)
- [Test 305: get_bestpractices_get](#test-305)
- [Test 306: get_bestpractices_get](#test-306)
- [Test 307: get_bestpractices_get](#test-307)
- [Test 308: get_bestpractices_get](#test-308)
- [Test 309: get_bestpractices_get](#test-309)
- [Test 310: get_bestpractices_get](#test-310)
- [Test 311: monitor_activitylog_list](#test-311)
- [Test 312: monitor_healthmodels_entity_gethealth](#test-312)
- [Test 313: monitor_metrics_definitions](#test-313)
- [Test 314: monitor_metrics_definitions](#test-314)
- [Test 315: monitor_metrics_definitions](#test-315)
- [Test 316: monitor_metrics_query](#test-316)
- [Test 317: monitor_metrics_query](#test-317)
- [Test 318: monitor_metrics_query](#test-318)
- [Test 319: monitor_metrics_query](#test-319)
- [Test 320: monitor_metrics_query](#test-320)
- [Test 321: monitor_metrics_query](#test-321)
- [Test 322: monitor_resource_log_query](#test-322)
- [Test 323: monitor_table_list](#test-323)
- [Test 324: monitor_table_list](#test-324)
- [Test 325: monitor_table_type_list](#test-325)
- [Test 326: monitor_table_type_list](#test-326)
- [Test 327: monitor_webtests_create](#test-327)
- [Test 328: monitor_webtests_get](#test-328)
- [Test 329: monitor_webtests_list](#test-329)
- [Test 330: monitor_webtests_list](#test-330)
- [Test 331: monitor_webtests_update](#test-331)
- [Test 332: monitor_workspace_list](#test-332)
- [Test 333: monitor_workspace_list](#test-333)
- [Test 334: monitor_workspace_list](#test-334)
- [Test 335: monitor_workspace_log_query](#test-335)
- [Test 336: datadog_monitoredresources_list](#test-336)
- [Test 337: datadog_monitoredresources_list](#test-337)
- [Test 338: extension_azqr](#test-338)
- [Test 339: extension_azqr](#test-339)
- [Test 340: extension_azqr](#test-340)
- [Test 341: quota_region_availability_list](#test-341)
- [Test 342: quota_usage_check](#test-342)
- [Test 343: role_assignment_list](#test-343)
- [Test 344: role_assignment_list](#test-344)
- [Test 345: redis_list](#test-345)
- [Test 346: redis_list](#test-346)
- [Test 347: redis_list](#test-347)
- [Test 348: redis_list](#test-348)
- [Test 349: redis_list](#test-349)
- [Test 350: group_list](#test-350)
- [Test 351: group_list](#test-351)
- [Test 352: group_list](#test-352)
- [Test 353: resourcehealth_availability-status_get](#test-353)
- [Test 354: resourcehealth_availability-status_get](#test-354)
- [Test 355: resourcehealth_availability-status_get](#test-355)
- [Test 356: resourcehealth_availability-status_list](#test-356)
- [Test 357: resourcehealth_availability-status_list](#test-357)
- [Test 358: resourcehealth_availability-status_list](#test-358)
- [Test 359: resourcehealth_service-health-events_list](#test-359)
- [Test 360: resourcehealth_service-health-events_list](#test-360)
- [Test 361: resourcehealth_service-health-events_list](#test-361)
- [Test 362: resourcehealth_service-health-events_list](#test-362)
- [Test 363: resourcehealth_service-health-events_list](#test-363)
- [Test 364: servicebus_queue_details](#test-364)
- [Test 365: servicebus_topic_details](#test-365)
- [Test 366: servicebus_topic_subscription_details](#test-366)
- [Test 367: signalr_runtime_get](#test-367)
- [Test 368: signalr_runtime_get](#test-368)
- [Test 369: signalr_runtime_get](#test-369)
- [Test 370: signalr_runtime_get](#test-370)
- [Test 371: signalr_runtime_get](#test-371)
- [Test 372: signalr_runtime_get](#test-372)
- [Test 373: sql_db_create](#test-373)
- [Test 374: sql_db_create](#test-374)
- [Test 375: sql_db_create](#test-375)
- [Test 376: sql_db_delete](#test-376)
- [Test 377: sql_db_delete](#test-377)
- [Test 378: sql_db_delete](#test-378)
- [Test 379: sql_db_list](#test-379)
- [Test 380: sql_db_list](#test-380)
- [Test 381: sql_db_rename](#test-381)
- [Test 382: sql_db_rename](#test-382)
- [Test 383: sql_db_show](#test-383)
- [Test 384: sql_db_show](#test-384)
- [Test 385: sql_db_update](#test-385)
- [Test 386: sql_db_update](#test-386)
- [Test 387: sql_elastic-pool_list](#test-387)
- [Test 388: sql_elastic-pool_list](#test-388)
- [Test 389: sql_elastic-pool_list](#test-389)
- [Test 390: sql_server_create](#test-390)
- [Test 391: sql_server_create](#test-391)
- [Test 392: sql_server_create](#test-392)
- [Test 393: sql_server_delete](#test-393)
- [Test 394: sql_server_delete](#test-394)
- [Test 395: sql_server_delete](#test-395)
- [Test 396: sql_server_entra-admin_list](#test-396)
- [Test 397: sql_server_entra-admin_list](#test-397)
- [Test 398: sql_server_entra-admin_list](#test-398)
- [Test 399: sql_server_firewall-rule_create](#test-399)
- [Test 400: sql_server_firewall-rule_create](#test-400)
- [Test 401: sql_server_firewall-rule_create](#test-401)
- [Test 402: sql_server_firewall-rule_delete](#test-402)
- [Test 403: sql_server_firewall-rule_delete](#test-403)
- [Test 404: sql_server_firewall-rule_delete](#test-404)
- [Test 405: sql_server_firewall-rule_list](#test-405)
- [Test 406: sql_server_firewall-rule_list](#test-406)
- [Test 407: sql_server_firewall-rule_list](#test-407)
- [Test 408: sql_server_list](#test-408)
- [Test 409: sql_server_list](#test-409)
- [Test 410: sql_server_show](#test-410)
- [Test 411: sql_server_show](#test-411)
- [Test 412: sql_server_show](#test-412)
- [Test 413: storage_account_create](#test-413)
- [Test 414: storage_account_create](#test-414)
- [Test 415: storage_account_create](#test-415)
- [Test 416: storage_account_get](#test-416)
- [Test 417: storage_account_get](#test-417)
- [Test 418: storage_account_get](#test-418)
- [Test 419: storage_account_get](#test-419)
- [Test 420: storage_account_get](#test-420)
- [Test 421: storage_blob_container_create](#test-421)
- [Test 422: storage_blob_container_create](#test-422)
- [Test 423: storage_blob_container_create](#test-423)
- [Test 424: storage_blob_container_get](#test-424)
- [Test 425: storage_blob_container_get](#test-425)
- [Test 426: storage_blob_container_get](#test-426)
- [Test 427: storage_blob_get](#test-427)
- [Test 428: storage_blob_get](#test-428)
- [Test 429: storage_blob_get](#test-429)
- [Test 430: storage_blob_get](#test-430)
- [Test 431: storage_blob_upload](#test-431)
- [Test 432: subscription_list](#test-432)
- [Test 433: subscription_list](#test-433)
- [Test 434: subscription_list](#test-434)
- [Test 435: subscription_list](#test-435)
- [Test 436: azureterraformbestpractices_get](#test-436)
- [Test 437: azureterraformbestpractices_get](#test-437)
- [Test 438: virtualdesktop_hostpool_list](#test-438)
- [Test 439: virtualdesktop_hostpool_sessionhost_list](#test-439)
- [Test 440: virtualdesktop_hostpool_sessionhost_usersession-list](#test-440)
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

---

## Test 1

**Expected Tool:** `foundry_agents_connect`  
**Prompt:** Query an agent in my AI foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622854 | `foundry_agents_connect` | ✅ **EXPECTED** |
| 2 | 0.603176 | `foundry_agents_query-and-evaluate` | ❌ |
| 3 | 0.494462 | `foundry_agents_list` | ❌ |
| 4 | 0.443011 | `foundry_agents_evaluate` | ❌ |
| 5 | 0.392604 | `foundry_resource_get` | ❌ |

---

## Test 2

**Expected Tool:** `foundry_agents_evaluate`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543957 | `foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.469428 | `foundry_agents_evaluate` | ✅ **EXPECTED** |
| 3 | 0.445964 | `foundry_agents_connect` | ❌ |
| 4 | 0.250023 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.235412 | `foundry_agents_list` | ❌ |

---

## Test 3

**Expected Tool:** `foundry_agents_list`  
**Prompt:** List all agents in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.725613 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.468941 | `foundry_agents_connect` | ❌ |
| 3 | 0.457994 | `foundry_resource_get` | ❌ |
| 4 | 0.454081 | `foundry_models_list` | ❌ |
| 5 | 0.432449 | `foundry_models_deployments_list` | ❌ |

---

## Test 4

**Expected Tool:** `foundry_agents_list`  
**Prompt:** Show me the available agents in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.714029 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.479155 | `foundry_agents_connect` | ❌ |
| 3 | 0.469283 | `foundry_resource_get` | ❌ |
| 4 | 0.466215 | `foundry_models_list` | ❌ |
| 5 | 0.429842 | `foundry_knowledge_index_list` | ❌ |

---

## Test 5

**Expected Tool:** `foundry_agents_query-and-evaluate`  
**Prompt:** Query and evaluate an agent in my AI Foundry project for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580498 | `foundry_agents_query-and-evaluate` | ✅ **EXPECTED** |
| 2 | 0.568662 | `foundry_agents_connect` | ❌ |
| 3 | 0.518655 | `foundry_agents_evaluate` | ❌ |
| 4 | 0.381887 | `foundry_agents_list` | ❌ |
| 5 | 0.326026 | `foundry_models_deploy` | ❌ |

---

## Test 6

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.695201 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.532985 | `foundry_agents_list` | ❌ |
| 3 | 0.526528 | `foundry_knowledge_index_schema` | ❌ |
| 4 | 0.441964 | `foundry_resource_get` | ❌ |
| 5 | 0.435696 | `search_knowledge_base_get` | ❌ |

---

## Test 7

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** Show me the knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603396 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.489311 | `foundry_knowledge_index_schema` | ❌ |
| 3 | 0.473949 | `foundry_agents_list` | ❌ |
| 4 | 0.441521 | `foundry_resource_get` | ❌ |
| 5 | 0.401099 | `search_knowledge_base_get` | ❌ |

---

## Test 8

**Expected Tool:** `foundry_knowledge_index_schema`  
**Prompt:** Show me the schema for knowledge index <index-name> in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672577 | `foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.564860 | `foundry_knowledge_index_list` | ❌ |
| 3 | 0.431309 | `search_knowledge_base_get` | ❌ |
| 4 | 0.424581 | `search_index_get` | ❌ |
| 5 | 0.417942 | `search_knowledge_source_get` | ❌ |

---

## Test 9

**Expected Tool:** `foundry_knowledge_index_schema`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650269 | `foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.432759 | `postgres_table_schema_get` | ❌ |
| 3 | 0.417421 | `kusto_table_schema` | ❌ |
| 4 | 0.415963 | `foundry_knowledge_index_list` | ❌ |
| 5 | 0.398186 | `mysql_table_schema_get` | ❌ |

---

## Test 10

**Expected Tool:** `foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.562920 | `foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.335757 | `foundry_openai_models-list` | ❌ |
| 3 | 0.298404 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.293050 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.282483 | `mysql_server_list` | ❌ |

---

## Test 11

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663532 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.582886 | `foundry_openai_models-list` | ❌ |
| 3 | 0.566272 | `foundry_resource_get` | ❌ |
| 4 | 0.549636 | `foundry_models_list` | ❌ |
| 5 | 0.539695 | `foundry_agents_list` | ❌ |

---

## Test 12

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** Show me all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606692 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.543400 | `foundry_resource_get` | ❌ |
| 3 | 0.521537 | `foundry_models_deploy` | ❌ |
| 4 | 0.518278 | `foundry_models_list` | ❌ |
| 5 | 0.506744 | `foundry_openai_models-list` | ❌ |

---

## Test 13

**Expected Tool:** `foundry_models_list`  
**Prompt:** List all AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560022 | `foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.514253 | `foundry_resource_get` | ❌ |
| 3 | 0.506770 | `foundry_models_deployments_list` | ❌ |
| 4 | 0.491952 | `foundry_agents_list` | ❌ |
| 5 | 0.474683 | `foundry_openai_models-list` | ❌ |

---

## Test 14

**Expected Tool:** `foundry_models_list`  
**Prompt:** Show me the available AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.574818 | `foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.525312 | `foundry_resource_get` | ❌ |
| 3 | 0.497284 | `foundry_models_deployments_list` | ❌ |
| 4 | 0.475139 | `foundry_agents_list` | ❌ |
| 5 | 0.467671 | `foundry_models_deploy` | ❌ |

---

## Test 15

**Expected Tool:** `foundry_openai_chat-completions-create`  
**Prompt:** Create a chat completion with the message "Hello, how are you today?"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.417723 | `foundry_openai_chat-completions-create` | ✅ **EXPECTED** |
| 2 | 0.332543 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.211879 | `foundry_agents_connect` | ❌ |
| 4 | 0.203439 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.168300 | `communication_sms_send` | ❌ |

---

## Test 16

**Expected Tool:** `foundry_openai_create-completion`  
**Prompt:** Create a completion with the prompt "What is Azure?"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553675 | `foundry_openai_create-completion` | ✅ **EXPECTED** |
| 2 | 0.447828 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.434373 | `extension_cli_generate` | ❌ |
| 4 | 0.403431 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.394144 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 17

**Expected Tool:** `foundry_openai_embeddings-create`  
**Prompt:** Generate embeddings for the text "Azure OpenAI Service"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656299 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.443469 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.408298 | `foundry_openai_models-list` | ❌ |
| 4 | 0.403956 | `foundry_models_deploy` | ❌ |
| 5 | 0.399853 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 18

**Expected Tool:** `foundry_openai_embeddings-create`  
**Prompt:** Create vector embeddings for my text using Azure OpenAI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702777 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.460359 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.426022 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.409975 | `foundry_models_deploy` | ❌ |
| 5 | 0.407541 | `foundry_openai_models-list` | ❌ |

---

## Test 19

**Expected Tool:** `foundry_openai_models-list`  
**Prompt:** List all available OpenAI models in my Azure resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.787677 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.655391 | `foundry_agents_list` | ❌ |
| 3 | 0.586908 | `foundry_models_list` | ❌ |
| 4 | 0.565893 | `search_service_list` | ❌ |
| 5 | 0.562009 | `foundry_resource_get` | ❌ |

---

## Test 20

**Expected Tool:** `foundry_openai_models-list`  
**Prompt:** Show me the OpenAI model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.574738 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.512409 | `foundry_models_deploy` | ❌ |
| 3 | 0.503634 | `foundry_models_deployments_list` | ❌ |
| 4 | 0.412858 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.405167 | `foundry_agents_list` | ❌ |

---

## Test 21

**Expected Tool:** `foundry_resource_get`  
**Prompt:** List all AI Foundry resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594096 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.558290 | `search_service_list` | ❌ |
| 3 | 0.524657 | `foundry_agents_list` | ❌ |
| 4 | 0.524645 | `grafana_list` | ❌ |
| 5 | 0.521417 | `redis_list` | ❌ |

---

## Test 22

**Expected Tool:** `foundry_resource_get`  
**Prompt:** Show me the AI Foundry resources in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665311 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.492911 | `foundry_models_deploy` | ❌ |
| 3 | 0.474905 | `foundry_agents_list` | ❌ |
| 4 | 0.467197 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.453679 | `foundry_openai_models-list` | ❌ |

---

## Test 23

**Expected Tool:** `foundry_resource_get`  
**Prompt:** Get details for AI Foundry resource <resource_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.735316 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.509368 | `monitor_webtests_get` | ❌ |
| 3 | 0.455585 | `foundry_openai_models-list` | ❌ |
| 4 | 0.452340 | `foundry_models_deploy` | ❌ |
| 5 | 0.444394 | `loadtesting_testresource_list` | ❌ |

---

## Test 24

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.785967 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.700824 | `search_knowledge_source_get` | ❌ |
| 3 | 0.693471 | `search_service_list` | ❌ |
| 4 | 0.635863 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.603324 | `foundry_knowledge_index_list` | ❌ |

---

## Test 25

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

## Test 26

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702942 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.605964 | `search_knowledge_source_get` | ❌ |
| 3 | 0.583234 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.513638 | `search_service_list` | ❌ |
| 5 | 0.494288 | `foundry_knowledge_index_list` | ❌ |

---

## Test 27

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688051 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.599247 | `search_knowledge_source_get` | ❌ |
| 3 | 0.578499 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.457619 | `search_service_list` | ❌ |
| 5 | 0.451801 | `foundry_knowledge_index_list` | ❌ |

---

## Test 28

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

## Test 29

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595585 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.551922 | `search_knowledge_base_retrieve` | ❌ |
| 3 | 0.515480 | `search_knowledge_source_get` | ❌ |
| 4 | 0.376599 | `foundry_knowledge_index_list` | ❌ |
| 5 | 0.366893 | `search_service_list` | ❌ |

---

## Test 30

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in Azure AI Search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.724892 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.650665 | `search_knowledge_base_get` | ❌ |
| 3 | 0.575384 | `search_index_query` | ❌ |
| 4 | 0.567426 | `search_knowledge_source_get` | ❌ |
| 5 | 0.520320 | `foundry_agents_connect` | ❌ |

---

## Test 31

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633766 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.589869 | `search_knowledge_base_get` | ❌ |
| 3 | 0.502085 | `search_knowledge_source_get` | ❌ |
| 4 | 0.422786 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.399595 | `search_index_query` | ❌ |

---

## Test 32

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657865 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.557206 | `search_knowledge_base_get` | ❌ |
| 3 | 0.463605 | `search_knowledge_source_get` | ❌ |
| 4 | 0.436781 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.422173 | `foundry_agents_connect` | ❌ |

---

## Test 33

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633766 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.589869 | `search_knowledge_base_get` | ❌ |
| 3 | 0.502085 | `search_knowledge_source_get` | ❌ |
| 4 | 0.422786 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.399595 | `search_index_query` | ❌ |

---

## Test 34

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Query knowledge base <agent-name> in search service <service-name> about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.598868 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.547862 | `search_knowledge_base_get` | ❌ |
| 3 | 0.467968 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.464904 | `search_knowledge_source_get` | ❌ |
| 5 | 0.412481 | `foundry_agents_connect` | ❌ |

---

## Test 35

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Search knowledge base <agent-name> in Azure AI Search service <service-name> for <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649767 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.631435 | `search_knowledge_base_get` | ❌ |
| 3 | 0.581387 | `search_index_query` | ❌ |
| 4 | 0.571156 | `search_knowledge_source_get` | ❌ |
| 5 | 0.544501 | `search_service_list` | ❌ |

---

## Test 36

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** What does knowledge base <agent-name> in search service <service-name> know about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579731 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.560726 | `search_knowledge_base_get` | ❌ |
| 3 | 0.477970 | `search_knowledge_source_get` | ❌ |
| 4 | 0.402690 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.371126 | `foundry_knowledge_index_list` | ❌ |

---

## Test 37

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Find information about <query> using knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582662 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.528610 | `search_knowledge_base_get` | ❌ |
| 3 | 0.449336 | `search_knowledge_source_get` | ❌ |
| 4 | 0.447877 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.397187 | `foundry_agents_connect` | ❌ |

---

## Test 38

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.760416 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.691931 | `search_service_list` | ❌ |
| 3 | 0.665923 | `search_knowledge_base_get` | ❌ |
| 4 | 0.579006 | `foundry_knowledge_index_list` | ❌ |
| 5 | 0.573012 | `search_index_get` | ❌ |

---

## Test 39

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

## Test 40

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657935 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.558516 | `search_knowledge_base_get` | ❌ |
| 3 | 0.511469 | `search_service_list` | ❌ |
| 4 | 0.470560 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.448614 | `foundry_knowledge_index_list` | ❌ |

---

## Test 41

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

## Test 42

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

## Test 43

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

## Test 44

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

## Test 45

**Expected Tool:** `search_index_get`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640256 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.620140 | `search_service_list` | ❌ |
| 3 | 0.561856 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.511485 | `search_knowledge_base_get` | ❌ |
| 5 | 0.496094 | `search_knowledge_source_get` | ❌ |

---

## Test 46

**Expected Tool:** `search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620759 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.562775 | `search_service_list` | ❌ |
| 3 | 0.561154 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.500365 | `search_knowledge_base_get` | ❌ |
| 5 | 0.490025 | `search_knowledge_source_get` | ❌ |

---

## Test 47

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

## Test 48

**Expected Tool:** `search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793651 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.552956 | `kusto_cluster_list` | ❌ |
| 3 | 0.520340 | `foundry_agents_list` | ❌ |
| 4 | 0.509461 | `subscription_list` | ❌ |
| 5 | 0.505971 | `search_index_get` | ❌ |

---

## Test 49

**Expected Tool:** `search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686140 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.484092 | `marketplace_product_list` | ❌ |
| 3 | 0.479898 | `search_index_get` | ❌ |
| 4 | 0.467337 | `foundry_agents_list` | ❌ |
| 5 | 0.462336 | `search_knowledge_base_get` | ❌ |

---

## Test 50

**Expected Tool:** `search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553006 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.436178 | `search_index_get` | ❌ |
| 3 | 0.417057 | `foundry_agents_list` | ❌ |
| 4 | 0.415217 | `search_knowledge_base_get` | ❌ |
| 5 | 0.410412 | `search_knowledge_source_get` | ❌ |

---

## Test 51

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert this audio file to text using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.666038 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.415224 | `foundry_openai_embeddings-create` | ❌ |
| 3 | 0.365228 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.351127 | `deploy_plan_get` | ❌ |
| 5 | 0.342808 | `foundry_openai_create-completion` | ❌ |

---

## Test 52

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Recognize speech from my audio file with language detection  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.511324 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.202056 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.190197 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.184542 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.159108 | `foundry_agents_connect` | ❌ |

---

## Test 53

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe speech from audio file <file_path> with profanity filtering  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485889 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.179808 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.178052 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.159239 | `foundry_agents_connect` | ❌ |
| 5 | 0.155094 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 54

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text from audio file <file_path> using endpoint <endpoint>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611992 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.322301 | `foundry_openai_embeddings-create` | ❌ |
| 3 | 0.263196 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.251200 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.244218 | `foundry_resource_get` | ❌ |

---

## Test 55

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe the audio file <file_path> in Spanish language  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.410533 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.159775 | `foundry_openai_embeddings-create` | ❌ |
| 3 | 0.158032 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.152137 | `foundry_models_deploy` | ❌ |
| 5 | 0.151632 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 56

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text with detailed output format from audio file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546259 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.225372 | `foundry_openai_embeddings-create` | ❌ |
| 3 | 0.218092 | `foundry_resource_get` | ❌ |
| 4 | 0.200865 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.196743 | `foundry_openai_create-completion` | ❌ |

---

## Test 57

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Recognize speech from <file_path> with phrase hints for better accuracy  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.539963 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.246979 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.238192 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.203413 | `foundry_agents_connect` | ❌ |
| 5 | 0.195889 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 58

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe audio using multiple phrase hints: "Azure", "cognitive services", "machine learning"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549254 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.357842 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.345680 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.342454 | `extension_cli_generate` | ❌ |
| 5 | 0.337314 | `cloudarchitect_design` | ❌ |

---

## Test 59

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text with comma-separated phrase hints: "Azure, cognitive services, API"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532536 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.385033 | `foundry_openai_embeddings-create` | ❌ |
| 3 | 0.381487 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.378382 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.326712 | `get_bestpractices_get` | ❌ |

---

## Test 60

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe audio with raw profanity output from file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.453396 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.181994 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.174375 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.173205 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.160483 | `foundry_agents_connect` | ❌ |

---

## Test 61

**Expected Tool:** `appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786360 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.530613 | `appconfig_kv_get` | ❌ |
| 3 | 0.491380 | `postgres_server_list` | ❌ |
| 4 | 0.481218 | `kusto_cluster_list` | ❌ |
| 5 | 0.479988 | `subscription_list` | ❌ |

---

## Test 62

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

## Test 63

**Expected Tool:** `appconfig_account_list`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565435 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.465344 | `appconfig_kv_get` | ❌ |
| 3 | 0.355916 | `postgres_server_config_get` | ❌ |
| 4 | 0.348661 | `appconfig_kv_delete` | ❌ |
| 5 | 0.327179 | `appconfig_kv_set` | ❌ |

---

## Test 64

**Expected Tool:** `appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618277 | `appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.464358 | `appconfig_kv_get` | ❌ |
| 3 | 0.424406 | `appconfig_kv_set` | ❌ |
| 4 | 0.422700 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.392016 | `appconfig_account_list` | ❌ |

---

## Test 65

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.632687 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.557810 | `appconfig_account_list` | ❌ |
| 3 | 0.530784 | `appconfig_kv_set` | ❌ |
| 4 | 0.464635 | `appconfig_kv_delete` | ❌ |
| 5 | 0.439089 | `appconfig_kv_lock_set` | ❌ |

---

## Test 66

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612555 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.522426 | `appconfig_account_list` | ❌ |
| 3 | 0.512839 | `appconfig_kv_set` | ❌ |
| 4 | 0.468503 | `appconfig_kv_delete` | ❌ |
| 5 | 0.457866 | `appconfig_kv_lock_set` | ❌ |

---

## Test 67

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** List all key-value settings with key name starting with 'prod-' in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512883 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.449905 | `appconfig_account_list` | ❌ |
| 3 | 0.398663 | `appconfig_kv_set` | ❌ |
| 4 | 0.380614 | `appconfig_kv_delete` | ❌ |
| 5 | 0.346166 | `appconfig_kv_lock_set` | ❌ |

---

## Test 68

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552300 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.448904 | `appconfig_kv_set` | ❌ |
| 3 | 0.441713 | `appconfig_kv_delete` | ❌ |
| 4 | 0.437432 | `appconfig_account_list` | ❌ |
| 5 | 0.416264 | `appconfig_kv_lock_set` | ❌ |

---

## Test 69

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591237 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.487174 | `appconfig_kv_get` | ❌ |
| 3 | 0.445534 | `appconfig_kv_set` | ❌ |
| 4 | 0.431516 | `appconfig_kv_delete` | ❌ |
| 5 | 0.373656 | `appconfig_account_list` | ❌ |

---

## Test 70

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555699 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.505681 | `appconfig_kv_get` | ❌ |
| 3 | 0.476496 | `appconfig_kv_delete` | ❌ |
| 4 | 0.425497 | `appconfig_kv_set` | ❌ |
| 5 | 0.409406 | `appconfig_account_list` | ❌ |

---

## Test 71

**Expected Tool:** `appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609540 | `appconfig_kv_set` | ✅ **EXPECTED** |
| 2 | 0.536497 | `appconfig_kv_lock_set` | ❌ |
| 3 | 0.512707 | `appconfig_kv_get` | ❌ |
| 4 | 0.505571 | `appconfig_kv_delete` | ❌ |
| 5 | 0.377919 | `appconfig_account_list` | ❌ |

---

## Test 72

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595632 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.336090 | `deploy_app_logs_get` | ❌ |
| 3 | 0.300786 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.273082 | `cloudarchitect_design` | ❌ |
| 5 | 0.254473 | `monitor_resource_log_query` | ❌ |

---

## Test 73

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** Use app lens to check why my app is slow?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502361 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.316297 | `deploy_app_logs_get` | ❌ |
| 3 | 0.255570 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.249583 | `monitor_resource_log_query` | ❌ |
| 5 | 0.226018 | `quota_usage_check` | ❌ |

---

## Test 74

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492820 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.256325 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.242574 | `cloudarchitect_design` | ❌ |
| 4 | 0.225608 | `resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.211564 | `deploy_app_logs_get` | ❌ |

---

## Test 75

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add a database connection to my app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728759 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.398199 | `sql_db_create` | ❌ |
| 3 | 0.379422 | `sql_db_rename` | ❌ |
| 4 | 0.367930 | `sql_db_list` | ❌ |
| 5 | 0.364428 | `mysql_server_list` | ❌ |

---

## Test 76

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure a SQL Server database for app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612164 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.473224 | `sql_db_update` | ❌ |
| 3 | 0.471103 | `sql_db_create` | ❌ |
| 4 | 0.454417 | `sql_db_rename` | ❌ |
| 5 | 0.412229 | `sql_server_delete` | ❌ |

---

## Test 77

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add a MySQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.648524 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.418934 | `sql_db_create` | ❌ |
| 3 | 0.409437 | `mysql_database_list` | ❌ |
| 4 | 0.397904 | `sql_db_rename` | ❌ |
| 5 | 0.382932 | `mysql_server_list` | ❌ |

---

## Test 78

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add a PostgreSQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579502 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.449085 | `postgres_database_list` | ❌ |
| 3 | 0.416337 | `postgres_server_param_set` | ❌ |
| 4 | 0.409515 | `postgres_table_list` | ❌ |
| 5 | 0.405431 | `postgres_server_list` | ❌ |

---

## Test 79

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add a CosmosDB database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643046 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.477031 | `cosmos_database_list` | ❌ |
| 3 | 0.465637 | `sql_db_create` | ❌ |
| 4 | 0.431581 | `sql_db_rename` | ❌ |
| 5 | 0.428101 | `cosmos_database_container_item_query` | ❌ |

---

## Test 80

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database <database_name> on server <database_server> to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645533 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.489228 | `sql_db_create` | ❌ |
| 3 | 0.440007 | `sql_db_rename` | ❌ |
| 4 | 0.431453 | `sql_db_delete` | ❌ |
| 5 | 0.426090 | `sql_server_delete` | ❌ |

---

## Test 81

**Expected Tool:** `appservice_database_add`  
**Prompt:** Set connection string for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665216 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.401714 | `sql_db_rename` | ❌ |
| 3 | 0.369071 | `sql_db_create` | ❌ |
| 4 | 0.332062 | `appconfig_kv_set` | ❌ |
| 5 | 0.328637 | `sql_db_update` | ❌ |

---

## Test 82

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure tenant <tenant> for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536761 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.408796 | `sql_db_rename` | ❌ |
| 3 | 0.394572 | `sql_db_create` | ❌ |
| 4 | 0.355309 | `sql_db_update` | ❌ |
| 5 | 0.329110 | `keyvault_secret_create` | ❌ |

---

## Test 83

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database <database_name> with retry policy to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560268 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.426753 | `sql_db_create` | ❌ |
| 3 | 0.392376 | `sql_db_rename` | ❌ |
| 4 | 0.371892 | `sql_db_delete` | ❌ |
| 5 | 0.361028 | `cosmos_database_list` | ❌ |

---

## Test 84

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

## Test 85

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

## Test 86

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626722 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.487989 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.479168 | `mysql_server_list` | ❌ |
| 4 | 0.477396 | `applens_resource_diagnose` | ❌ |
| 5 | 0.468847 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 87

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509502 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.419670 | `applens_resource_diagnose` | ❌ |
| 3 | 0.383767 | `get_bestpractices_get` | ❌ |
| 4 | 0.367278 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.343931 | `cloudarchitect_design` | ❌ |

---

## Test 88

**Expected Tool:** `extension_cli_generate`  
**Prompt:** Create a Storage account with name <storage_account_name> using Azure CLI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593242 | `storage_account_create` | ❌ |
| 2 | 0.564940 | `storage_blob_container_create` | ❌ |
| 3 | 0.493684 | `storage_account_get` | ❌ |
| 4 | 0.474399 | `storage_blob_container_get` | ❌ |
| 5 | 0.454194 | `managedlustre_filesystem_create` | ❌ |

---

## Test 89

**Expected Tool:** `extension_cli_generate`  
**Prompt:** List all virtual machines in my subscription using Azure CLI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593467 | `search_service_list` | ❌ |
| 2 | 0.575196 | `kusto_cluster_list` | ❌ |
| 3 | 0.549966 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.544688 | `monitor_workspace_list` | ❌ |
| 5 | 0.536252 | `subscription_list` | ❌ |

---

## Test 90

**Expected Tool:** `extension_cli_generate`  
**Prompt:** Show me the details of the storage account <account_name> with Azure CLI commands  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710307 | `storage_account_get` | ❌ |
| 2 | 0.602173 | `storage_blob_container_get` | ❌ |
| 3 | 0.543547 | `storage_blob_get` | ❌ |
| 4 | 0.519788 | `storage_account_create` | ❌ |
| 5 | 0.493145 | `cosmos_account_list` | ❌ |

---

## Test 91

**Expected Tool:** `extension_cli_install`  
**Prompt:** <uninstall az cli on your machine and run test prompts for extension_cli_generate>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.497833 | `extension_cli_generate` | ❌ |
| 2 | 0.497483 | `extension_cli_install` | ✅ **EXPECTED** |
| 3 | 0.401540 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.383664 | `deploy_plan_get` | ❌ |
| 5 | 0.382602 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 92

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

## Test 93

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

## Test 94

**Expected Tool:** `acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743568 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.711580 | `acr_registry_repository_list` | ❌ |
| 3 | 0.585653 | `kusto_cluster_list` | ❌ |
| 4 | 0.541506 | `search_service_list` | ❌ |
| 5 | 0.514293 | `cosmos_account_list` | ❌ |

---

## Test 95

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

## Test 96

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.637130 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563476 | `acr_registry_repository_list` | ❌ |
| 3 | 0.516749 | `kusto_cluster_list` | ❌ |
| 4 | 0.515378 | `storage_blob_container_get` | ❌ |
| 5 | 0.480352 | `redis_list` | ❌ |

---

## Test 97

**Expected Tool:** `acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654318 | `acr_registry_repository_list` | ❌ |
| 2 | 0.633938 | `acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.476046 | `mysql_server_list` | ❌ |
| 4 | 0.454929 | `group_list` | ❌ |
| 5 | 0.454003 | `datadog_monitoredresources_list` | ❌ |

---

## Test 98

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639391 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.637972 | `acr_registry_repository_list` | ❌ |
| 3 | 0.468083 | `mysql_server_list` | ❌ |
| 4 | 0.449649 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.445741 | `group_list` | ❌ |

---

## Test 99

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626482 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.617504 | `acr_registry_list` | ❌ |
| 3 | 0.544137 | `kusto_cluster_list` | ❌ |
| 4 | 0.508483 | `storage_blob_container_get` | ❌ |
| 5 | 0.495567 | `postgres_server_list` | ❌ |

---

## Test 100

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546333 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.469295 | `acr_registry_list` | ❌ |
| 3 | 0.451083 | `storage_blob_container_get` | ❌ |
| 4 | 0.407973 | `cosmos_database_container_list` | ❌ |
| 5 | 0.373475 | `storage_blob_get` | ❌ |

---

## Test 101

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674296 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.541779 | `acr_registry_list` | ❌ |
| 3 | 0.437509 | `storage_blob_container_get` | ❌ |
| 4 | 0.433927 | `cosmos_database_container_list` | ❌ |
| 5 | 0.383157 | `kusto_database_list` | ❌ |

---

## Test 102

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

## Test 103

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498396 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.229081 | `communication_sms_send` | ❌ |
| 3 | 0.189025 | `eventgrid_events_publish` | ❌ |
| 4 | 0.145951 | `servicebus_topic_details` | ❌ |
| 5 | 0.136621 | `speech_stt_recognize` | ❌ |

---

## Test 104

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email from my communication service to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498505 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.314462 | `communication_sms_send` | ❌ |
| 3 | 0.218524 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.211154 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.196532 | `appservice_database_add` | ❌ |

---

## Test 105

**Expected Tool:** `communication_email_send`  
**Prompt:** Send HTML-formatted email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521087 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.207658 | `communication_sms_send` | ❌ |
| 3 | 0.152396 | `eventgrid_events_publish` | ❌ |
| 4 | 0.152013 | `servicebus_topic_details` | ❌ |
| 5 | 0.143660 | `foundry_agents_evaluate` | ❌ |

---

## Test 106

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with CC to <email-address-1> and <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533533 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.219584 | `communication_sms_send` | ❌ |
| 3 | 0.106026 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.087784 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.084905 | `cosmos_account_list` | ❌ |

---

## Test 107

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email to multiple recipients: <email-address-1>, <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.540864 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.244521 | `communication_sms_send` | ❌ |
| 3 | 0.114290 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.098798 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.087063 | `postgres_server_param_set` | ❌ |

---

## Test 108

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with reply-to address set to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512730 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.200154 | `communication_sms_send` | ❌ |
| 3 | 0.164178 | `mysql_server_param_set` | ❌ |
| 4 | 0.158744 | `postgres_server_param_set` | ❌ |
| 5 | 0.143243 | `appconfig_kv_set` | ❌ |

---

## Test 109

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with custom sender name <sender-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.473192 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.255169 | `communication_sms_send` | ❌ |
| 3 | 0.156621 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.143626 | `sql_db_rename` | ❌ |
| 5 | 0.139388 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 110

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email with BCC recipients  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.528899 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.241114 | `communication_sms_send` | ❌ |
| 3 | 0.137538 | `confidentialledger_entries_append` | ❌ |
| 4 | 0.108748 | `confidentialledger_entries_get` | ❌ |
| 5 | 0.105018 | `storage_blob_upload` | ❌ |

---

## Test 111

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send an SMS message to <phone-number> saying "Hello"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533868 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.251480 | `communication_email_send` | ❌ |
| 3 | 0.178085 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.148584 | `foundry_agents_connect` | ❌ |
| 5 | 0.141194 | `foundry_openai_create-completion` | ❌ |

---

## Test 112

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS to <phone-number-2> from <phone-number-1> with message "Test message"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545986 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.294922 | `communication_email_send` | ❌ |
| 3 | 0.204477 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.155976 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.136833 | `loadtesting_testrun_update` | ❌ |

---

## Test 113

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS to multiple recipients: <phone-number-1>, <phone-number-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545755 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.422029 | `communication_email_send` | ❌ |
| 3 | 0.142602 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.142008 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.104124 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 114

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS with delivery reporting enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.554908 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.269203 | `communication_email_send` | ❌ |
| 3 | 0.191848 | `extension_azqr` | ❌ |
| 4 | 0.170662 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.166385 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 115

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS message with custom tracking tag "campaign1"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.538827 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.269915 | `communication_email_send` | ❌ |
| 3 | 0.187936 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.159028 | `appconfig_kv_set` | ❌ |
| 5 | 0.158295 | `loadtesting_test_create` | ❌ |

---

## Test 116

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send broadcast SMS to <phone-number-1> and <phone-number-2> saying "Urgent notification"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.474786 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.286381 | `communication_email_send` | ❌ |
| 3 | 0.164274 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.129965 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.128704 | `cosmos_account_list` | ❌ |

---

## Test 117

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS from my communication service to <phone-number-1>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564114 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.302379 | `communication_email_send` | ❌ |
| 3 | 0.213669 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.183651 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.177315 | `appservice_database_add` | ❌ |

---

## Test 118

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send an SMS with delivery receipt tracking  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.598328 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.314282 | `communication_email_send` | ❌ |
| 3 | 0.206929 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.187845 | `confidentialledger_entries_append` | ❌ |
| 5 | 0.181885 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 119

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Append an entry to my ledger <ledger_name> with data {"key": "value"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510651 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.294885 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.291746 | `appconfig_kv_set` | ❌ |
| 4 | 0.258967 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.249908 | `keyvault_certificate_import` | ❌ |

---

## Test 120

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Write a tamper-proof entry to ledger <ledger_name> containing {"transaction": "data"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602247 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.357646 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.211990 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.195471 | `keyvault_secret_create` | ❌ |
| 5 | 0.184077 | `keyvault_certificate_import` | ❌ |

---

## Test 121

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Append {"hello": "from mcp"} to my confidential ledger <ledger_name> in collection <collection_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546560 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.451657 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.225072 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.215805 | `appconfig_kv_set` | ❌ |
| 5 | 0.211858 | `appservice_database_add` | ❌ |

---

## Test 122

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Create an immutable ledger entry in <ledger_name> with content {"audit": "log"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496023 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.340187 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.218755 | `monitor_activitylog_list` | ❌ |
| 4 | 0.215229 | `storage_blob_container_create` | ❌ |
| 5 | 0.204925 | `monitor_resource_log_query` | ❌ |

---

## Test 123

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Write an entry to confidential ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622138 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.524777 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.252508 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.240252 | `keyvault_secret_create` | ❌ |
| 5 | 0.186662 | `appconfig_kv_set` | ❌ |

---

## Test 124

**Expected Tool:** `confidentialledger_entries_get`  
**Prompt:** Get entry from Confidential Ledger for transaction <transaction_id> on ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.707003 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.551775 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.245683 | `keyvault_secret_get` | ❌ |
| 4 | 0.230073 | `keyvault_key_get` | ❌ |
| 5 | 0.211815 | `loadtesting_testrun_get` | ❌ |

---

## Test 125

**Expected Tool:** `confidentialledger_entries_get`  
**Prompt:** Get transaction <transaction_id> from ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509714 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.416580 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.223959 | `loadtesting_testrun_get` | ❌ |
| 4 | 0.218412 | `monitor_resource_log_query` | ❌ |
| 5 | 0.217671 | `loadtesting_testrun_list` | ❌ |

---

## Test 126

**Expected Tool:** `cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818357 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.668480 | `cosmos_database_list` | ❌ |
| 3 | 0.636036 | `subscription_list` | ❌ |
| 4 | 0.615268 | `cosmos_database_container_list` | ❌ |
| 5 | 0.601432 | `kusto_cluster_list` | ❌ |

---

## Test 127

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665447 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605357 | `cosmos_database_list` | ❌ |
| 3 | 0.571613 | `cosmos_database_container_list` | ❌ |
| 4 | 0.549222 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.503830 | `storage_account_get` | ❌ |

---

## Test 128

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752494 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.607201 | `subscription_list` | ❌ |
| 3 | 0.605125 | `cosmos_database_list` | ❌ |
| 4 | 0.566249 | `cosmos_database_container_list` | ❌ |
| 5 | 0.563682 | `cosmos_database_container_item_query` | ❌ |

---

## Test 129

**Expected Tool:** `cosmos_database_container_item_query`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658899 | `cosmos_database_container_item_query` | ✅ **EXPECTED** |
| 2 | 0.605253 | `cosmos_database_container_list` | ❌ |
| 3 | 0.487612 | `storage_blob_container_get` | ❌ |
| 4 | 0.477874 | `cosmos_database_list` | ❌ |
| 5 | 0.447757 | `cosmos_account_list` | ❌ |

---

## Test 130

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852832 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.681044 | `cosmos_database_list` | ❌ |
| 3 | 0.680443 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.632335 | `storage_blob_container_get` | ❌ |
| 5 | 0.630659 | `cosmos_account_list` | ❌ |

---

## Test 131

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789371 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.647797 | `cosmos_database_container_item_query` | ❌ |
| 3 | 0.614282 | `cosmos_database_list` | ❌ |
| 4 | 0.591222 | `storage_blob_container_get` | ❌ |
| 5 | 0.562006 | `cosmos_account_list` | ❌ |

---

## Test 132

**Expected Tool:** `cosmos_database_list`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815683 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.668515 | `cosmos_account_list` | ❌ |
| 3 | 0.665298 | `cosmos_database_container_list` | ❌ |
| 4 | 0.605971 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.583479 | `kusto_database_list` | ❌ |

---

## Test 133

**Expected Tool:** `cosmos_database_list`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.749370 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.624759 | `cosmos_database_container_list` | ❌ |
| 3 | 0.614572 | `cosmos_account_list` | ❌ |
| 4 | 0.579582 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.537665 | `mysql_database_list` | ❌ |

---

## Test 134

**Expected Tool:** `kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590264 | `kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.463885 | `kusto_cluster_list` | ❌ |
| 3 | 0.428159 | `kusto_query` | ❌ |
| 4 | 0.425553 | `kusto_database_list` | ❌ |
| 5 | 0.422577 | `kusto_table_schema` | ❌ |

---

## Test 135

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793716 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.630457 | `kusto_database_list` | ❌ |
| 3 | 0.573395 | `kusto_cluster_get` | ❌ |
| 4 | 0.525025 | `aks_cluster_get` | ❌ |
| 5 | 0.509396 | `grafana_list` | ❌ |

---

## Test 136

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531393 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.465277 | `kusto_cluster_get` | ❌ |
| 3 | 0.432189 | `kusto_database_list` | ❌ |
| 4 | 0.369596 | `aks_cluster_get` | ❌ |
| 5 | 0.363119 | `kusto_table_schema` | ❌ |

---

## Test 137

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701501 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.571243 | `kusto_cluster_get` | ❌ |
| 3 | 0.548694 | `kusto_database_list` | ❌ |
| 4 | 0.498948 | `aks_cluster_get` | ❌ |
| 5 | 0.474213 | `redis_list` | ❌ |

---

## Test 138

**Expected Tool:** `kusto_database_list`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676895 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.560647 | `kusto_cluster_list` | ❌ |
| 3 | 0.556795 | `kusto_table_list` | ❌ |
| 4 | 0.553218 | `postgres_database_list` | ❌ |
| 5 | 0.549673 | `cosmos_database_list` | ❌ |

---

## Test 139

**Expected Tool:** `kusto_database_list`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623350 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.510018 | `kusto_cluster_list` | ❌ |
| 3 | 0.507073 | `kusto_table_list` | ❌ |
| 4 | 0.497144 | `cosmos_database_list` | ❌ |
| 5 | 0.491170 | `mysql_database_list` | ❌ |

---

## Test 140

**Expected Tool:** `kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.423660 | `kusto_query` | ✅ **EXPECTED** |
| 2 | 0.409558 | `postgres_database_query` | ❌ |
| 3 | 0.408178 | `kusto_table_schema` | ❌ |
| 4 | 0.407748 | `kusto_sample` | ❌ |
| 5 | 0.403998 | `kusto_cluster_list` | ❌ |

---

## Test 141

**Expected Tool:** `kusto_sample`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595558 | `kusto_sample` | ✅ **EXPECTED** |
| 2 | 0.510233 | `kusto_table_schema` | ❌ |
| 3 | 0.424212 | `kusto_table_list` | ❌ |
| 4 | 0.400953 | `kusto_cluster_list` | ❌ |
| 5 | 0.399525 | `kusto_cluster_get` | ❌ |

---

## Test 142

**Expected Tool:** `kusto_table_list`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.679682 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.585520 | `postgres_table_list` | ❌ |
| 3 | 0.580901 | `kusto_database_list` | ❌ |
| 4 | 0.556877 | `mysql_table_list` | ❌ |
| 5 | 0.550112 | `monitor_table_list` | ❌ |

---

## Test 143

**Expected Tool:** `kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619252 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.554333 | `kusto_table_schema` | ❌ |
| 3 | 0.527472 | `kusto_database_list` | ❌ |
| 4 | 0.524691 | `mysql_table_list` | ❌ |
| 5 | 0.523432 | `postgres_table_list` | ❌ |

---

## Test 144

**Expected Tool:** `kusto_table_schema`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.667052 | `kusto_table_schema` | ✅ **EXPECTED** |
| 2 | 0.564311 | `postgres_table_schema_get` | ❌ |
| 3 | 0.527917 | `mysql_table_schema_get` | ❌ |
| 4 | 0.490898 | `kusto_sample` | ❌ |
| 5 | 0.489680 | `kusto_table_list` | ❌ |

---

## Test 145

**Expected Tool:** `mysql_database_list`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634056 | `postgres_database_list` | ❌ |
| 2 | 0.622477 | `mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.534457 | `mysql_table_list` | ❌ |
| 4 | 0.498586 | `mysql_server_list` | ❌ |
| 5 | 0.490148 | `sql_db_list` | ❌ |

---

## Test 146

**Expected Tool:** `mysql_database_list`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587376 | `mysql_database_list` | ✅ **EXPECTED** |
| 2 | 0.574089 | `postgres_database_list` | ❌ |
| 3 | 0.483855 | `mysql_table_list` | ❌ |
| 4 | 0.462929 | `mysql_server_list` | ❌ |
| 5 | 0.444547 | `sql_db_list` | ❌ |

---

## Test 147

**Expected Tool:** `mysql_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476475 | `mysql_table_list` | ❌ |
| 2 | 0.454879 | `mysql_database_list` | ❌ |
| 3 | 0.433426 | `mysql_database_query` | ✅ **EXPECTED** |
| 4 | 0.419785 | `mysql_server_list` | ❌ |
| 5 | 0.409475 | `mysql_table_schema_get` | ❌ |

---

## Test 148

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

## Test 149

**Expected Tool:** `mysql_server_list`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678472 | `postgres_server_list` | ❌ |
| 2 | 0.556901 | `mysql_database_list` | ❌ |
| 3 | 0.554589 | `mysql_server_list` | ✅ **EXPECTED** |
| 4 | 0.513688 | `kusto_cluster_list` | ❌ |
| 5 | 0.501199 | `mysql_table_list` | ❌ |

---

## Test 150

**Expected Tool:** `mysql_server_list`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476775 | `mysql_database_list` | ❌ |
| 2 | 0.474383 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.435642 | `postgres_server_list` | ❌ |
| 4 | 0.412380 | `mysql_table_list` | ❌ |
| 5 | 0.389993 | `postgres_database_list` | ❌ |

---

## Test 151

**Expected Tool:** `mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636435 | `postgres_server_list` | ❌ |
| 2 | 0.534079 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.529813 | `mysql_database_list` | ❌ |
| 4 | 0.475060 | `kusto_cluster_list` | ❌ |
| 5 | 0.470468 | `redis_list` | ❌ |

---

## Test 152

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

## Test 153

**Expected Tool:** `mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.449419 | `mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.381144 | `mysql_server_param_get` | ❌ |
| 3 | 0.303499 | `postgres_server_param_set` | ❌ |
| 4 | 0.298911 | `mysql_database_query` | ❌ |
| 5 | 0.277569 | `appservice_database_add` | ❌ |

---

## Test 154

**Expected Tool:** `mysql_table_list`  
**Prompt:** List all tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633448 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.573844 | `postgres_table_list` | ❌ |
| 3 | 0.550898 | `postgres_database_list` | ❌ |
| 4 | 0.545618 | `mysql_database_list` | ❌ |
| 5 | 0.511847 | `kusto_table_list` | ❌ |

---

## Test 155

**Expected Tool:** `mysql_table_list`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609131 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.526236 | `postgres_table_list` | ❌ |
| 3 | 0.524374 | `mysql_database_list` | ❌ |
| 4 | 0.507258 | `mysql_table_schema_get` | ❌ |
| 5 | 0.498050 | `postgres_database_list` | ❌ |

---

## Test 156

**Expected Tool:** `mysql_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630623 | `mysql_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.558306 | `postgres_table_schema_get` | ❌ |
| 3 | 0.545025 | `mysql_table_list` | ❌ |
| 4 | 0.517419 | `kusto_table_schema` | ❌ |
| 5 | 0.455877 | `mysql_database_list` | ❌ |

---

## Test 157

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

## Test 158

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

## Test 159

**Expected Tool:** `postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546249 | `postgres_database_list` | ❌ |
| 2 | 0.523222 | `postgres_database_query` | ✅ **EXPECTED** |
| 3 | 0.503363 | `postgres_table_list` | ❌ |
| 4 | 0.466655 | `postgres_server_list` | ❌ |
| 5 | 0.404017 | `postgres_server_param_get` | ❌ |

---

## Test 160

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

## Test 161

**Expected Tool:** `postgres_server_list`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.900017 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.640727 | `postgres_database_list` | ❌ |
| 3 | 0.565902 | `postgres_table_list` | ❌ |
| 4 | 0.538996 | `postgres_server_config_get` | ❌ |
| 5 | 0.534302 | `kusto_cluster_list` | ❌ |

---

## Test 162

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

## Test 163

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

## Test 164

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

## Test 165

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

## Test 166

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

## Test 167

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

## Test 168

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

## Test 169

**Expected Tool:** `deploy_app_logs_get`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711770 | `deploy_app_logs_get` | ✅ **EXPECTED** |
| 2 | 0.471692 | `deploy_plan_get` | ❌ |
| 3 | 0.451688 | `monitor_activitylog_list` | ❌ |
| 4 | 0.404890 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.401388 | `monitor_resource_log_query` | ❌ |

---

## Test 170

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

## Test 171

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

## Test 172

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

## Test 173

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

## Test 174

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Publish an event to Event Grid topic <topic_name> using <event_schema> with the following data <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755328 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.483021 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.466031 | `eventgrid_topic_list` | ❌ |
| 4 | 0.360676 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.355599 | `servicebus_topic_details` | ❌ |

---

## Test 175

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Publish event to my Event Grid topic <topic_name> with the following events <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654542 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.524503 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.510039 | `eventgrid_topic_list` | ❌ |
| 4 | 0.373718 | `servicebus_topic_details` | ❌ |
| 5 | 0.359908 | `eventhubs_eventhub_update` | ❌ |

---

## Test 176

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Send an event to Event Grid topic <topic_name> in resource group <resource_group_name> with <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600224 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.521138 | `eventgrid_topic_list` | ❌ |
| 3 | 0.504714 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.411400 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.389406 | `eventhubs_eventhub_consumergroup_get` | ❌ |

---

## Test 177

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.770140 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.745470 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.561748 | `kusto_cluster_list` | ❌ |
| 4 | 0.545540 | `search_service_list` | ❌ |
| 5 | 0.526138 | `subscription_list` | ❌ |

---

## Test 178

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738258 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.737486 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.492522 | `kusto_cluster_list` | ❌ |
| 4 | 0.480287 | `subscription_list` | ❌ |
| 5 | 0.475119 | `search_service_list` | ❌ |

---

## Test 179

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.770140 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.721362 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.535177 | `kusto_cluster_list` | ❌ |
| 4 | 0.514248 | `search_service_list` | ❌ |
| 5 | 0.495987 | `subscription_list` | ❌ |

---

## Test 180

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.758816 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.704462 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.609175 | `group_list` | ❌ |
| 4 | 0.544809 | `monitor_webtests_list` | ❌ |
| 5 | 0.524209 | `eventhubs_namespace_get` | ❌ |

---

## Test 181

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769097 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.720606 | `eventgrid_topic_list` | ❌ |
| 3 | 0.498615 | `servicebus_topic_details` | ❌ |
| 4 | 0.486216 | `servicebus_topic_subscription_details` | ❌ |
| 5 | 0.486069 | `eventgrid_events_publish` | ❌ |

---

## Test 182

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.718109 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.709805 | `eventgrid_topic_list` | ❌ |
| 3 | 0.539977 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.529286 | `servicebus_topic_details` | ❌ |
| 5 | 0.477789 | `eventgrid_events_publish` | ❌ |

---

## Test 183

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.746815 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.746174 | `eventgrid_topic_list` | ❌ |
| 3 | 0.535569 | `monitor_webtests_list` | ❌ |
| 4 | 0.524919 | `group_list` | ❌ |
| 5 | 0.503158 | `servicebus_topic_details` | ❌ |

---

## Test 184

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736460 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.659772 | `eventgrid_topic_list` | ❌ |
| 3 | 0.569370 | `subscription_list` | ❌ |
| 4 | 0.537891 | `kusto_cluster_list` | ❌ |
| 5 | 0.518970 | `search_service_list` | ❌ |

---

## Test 185

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684543 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.656277 | `eventgrid_topic_list` | ❌ |
| 3 | 0.542388 | `subscription_list` | ❌ |
| 4 | 0.520891 | `kusto_cluster_list` | ❌ |
| 5 | 0.510115 | `group_list` | ❌ |

---

## Test 186

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696101 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.691739 | `eventgrid_topic_list` | ❌ |
| 3 | 0.557573 | `group_list` | ❌ |
| 4 | 0.510684 | `monitor_webtests_list` | ❌ |
| 5 | 0.504984 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 187

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.709801 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.642095 | `eventgrid_topic_list` | ❌ |
| 3 | 0.506697 | `subscription_list` | ❌ |
| 4 | 0.476763 | `search_service_list` | ❌ |
| 5 | 0.475704 | `kusto_cluster_list` | ❌ |

---

## Test 188

**Expected Tool:** `eventhubs_consumergroup_delete`  
**Prompt:** Delete my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.767386 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 2 | 0.675145 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.641111 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.633787 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.605436 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 189

**Expected Tool:** `eventhubs_consumergroup_get`  
**Prompt:** List all consumer groups in my event hub <event_hub_name> in namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738475 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 2 | 0.634345 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.625470 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.606619 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.593098 | `eventhubs_eventhub_get` | ❌ |

---

## Test 190

**Expected Tool:** `eventhubs_consumergroup_get`  
**Prompt:** Get the details of my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712861 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 2 | 0.637418 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.625090 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.576800 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.529940 | `eventhubs_eventhub_get` | ❌ |

---

## Test 191

**Expected Tool:** `eventhubs_consumergroup_update`  
**Prompt:** Create a new consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.757520 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 2 | 0.688923 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 3 | 0.668790 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.554314 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.545003 | `eventhubs_namespace_get` | ❌ |

---

## Test 192

**Expected Tool:** `eventhubs_consumergroup_update`  
**Prompt:** Update my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739339 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 2 | 0.654965 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 3 | 0.642271 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.552274 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.524061 | `eventhubs_namespace_delete` | ❌ |

---

## Test 193

**Expected Tool:** `eventhubs_eventhub_delete`  
**Prompt:** Delete my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.698863 | `eventhubs_namespace_delete` | ❌ |
| 2 | 0.688422 | `eventhubs_eventhub_delete` | ✅ **EXPECTED** |
| 3 | 0.629110 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.578797 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.552790 | `eventhubs_eventhub_get` | ❌ |

---

## Test 194

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

## Test 195

**Expected Tool:** `eventhubs_eventhub_get`  
**Prompt:** Get the details of my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638146 | `eventhubs_namespace_get` | ❌ |
| 2 | 0.627697 | `eventhubs_eventhub_get` | ✅ **EXPECTED** |
| 3 | 0.570985 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.527705 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.521990 | `eventhubs_namespace_delete` | ❌ |

---

## Test 196

**Expected Tool:** `eventhubs_eventhub_update`  
**Prompt:** Create a new event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645897 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.605523 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.574128 | `eventhubs_eventhub_get` | ❌ |
| 4 | 0.571760 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.557340 | `eventhubs_namespace_delete` | ❌ |

---

## Test 197

**Expected Tool:** `eventhubs_eventhub_update`  
**Prompt:** Update my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655283 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.571733 | `eventhubs_eventhub_delete` | ❌ |
| 3 | 0.569263 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 4 | 0.568396 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.565977 | `eventhubs_namespace_delete` | ❌ |

---

## Test 198

**Expected Tool:** `eventhubs_namespace_delete`  
**Prompt:** Delete my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623995 | `eventhubs_namespace_delete` | ✅ **EXPECTED** |
| 2 | 0.525446 | `eventhubs_namespace_update` | ❌ |
| 3 | 0.506773 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.449841 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.435037 | `workbooks_delete` | ❌ |

---

## Test 199

**Expected Tool:** `eventhubs_namespace_get`  
**Prompt:** List all Event Hubs namespaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659851 | `eventhubs_eventhub_get` | ❌ |
| 2 | 0.658798 | `eventhubs_namespace_get` | ✅ **EXPECTED** |
| 3 | 0.607238 | `kusto_cluster_list` | ❌ |
| 4 | 0.557101 | `eventgrid_topic_list` | ❌ |
| 5 | 0.556032 | `eventgrid_subscription_list` | ❌ |

---

## Test 200

**Expected Tool:** `eventhubs_namespace_get`  
**Prompt:** Get the details of my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509749 | `eventhubs_namespace_get` | ✅ **EXPECTED** |
| 2 | 0.509218 | `monitor_webtests_get` | ❌ |
| 3 | 0.497399 | `servicebus_queue_details` | ❌ |
| 4 | 0.490055 | `eventhubs_namespace_update` | ❌ |
| 5 | 0.470455 | `functionapp_get` | ❌ |

---

## Test 201

**Expected Tool:** `eventhubs_namespace_update`  
**Prompt:** Create an new namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610456 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.466721 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.458458 | `eventhubs_namespace_delete` | ❌ |
| 4 | 0.449724 | `workbooks_create` | ❌ |
| 5 | 0.438886 | `eventhubs_eventhub_consumergroup_update` | ❌ |

---

## Test 202

**Expected Tool:** `eventhubs_namespace_update`  
**Prompt:** Update my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622338 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.474099 | `eventhubs_namespace_delete` | ❌ |
| 3 | 0.448723 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.437139 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.372632 | `sql_db_rename` | ❌ |

---

## Test 203

**Expected Tool:** `functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660116 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.451613 | `deploy_app_logs_get` | ❌ |
| 3 | 0.450457 | `applens_resource_diagnose` | ❌ |
| 4 | 0.390100 | `mysql_server_list` | ❌ |
| 5 | 0.380314 | `get_bestpractices_get` | ❌ |

---

## Test 204

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

## Test 205

**Expected Tool:** `functionapp_get`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622384 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.411650 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.390708 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.383533 | `deploy_app_logs_get` | ❌ |
| 5 | 0.360665 | `storage_account_get` | ❌ |

---

## Test 206

**Expected Tool:** `functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690837 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.441937 | `foundry_resource_get` | ❌ |
| 3 | 0.432269 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.431739 | `applens_resource_diagnose` | ❌ |
| 5 | 0.429060 | `storage_account_get` | ❌ |

---

## Test 207

**Expected Tool:** `functionapp_get`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592791 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.417641 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.409712 | `deploy_app_logs_get` | ❌ |
| 4 | 0.399953 | `storage_account_get` | ❌ |
| 5 | 0.392237 | `applens_resource_diagnose` | ❌ |

---

## Test 208

**Expected Tool:** `functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687356 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.449588 | `deploy_app_logs_get` | ❌ |
| 3 | 0.428689 | `applens_resource_diagnose` | ❌ |
| 4 | 0.424686 | `foundry_resource_get` | ❌ |
| 5 | 0.391645 | `monitor_webtests_get` | ❌ |

---

## Test 209

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

## Test 210

**Expected Tool:** `functionapp_get`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.554980 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.426965 | `quota_usage_check` | ❌ |
| 3 | 0.424610 | `deploy_app_logs_get` | ❌ |
| 4 | 0.408011 | `deploy_plan_get` | ❌ |
| 5 | 0.381629 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 211

**Expected Tool:** `functionapp_get`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565797 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.403665 | `deploy_app_logs_get` | ❌ |
| 3 | 0.384159 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.369868 | `applens_resource_diagnose` | ❌ |
| 5 | 0.353063 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 212

**Expected Tool:** `functionapp_get`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646561 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.559382 | `search_service_list` | ❌ |
| 3 | 0.534930 | `subscription_list` | ❌ |
| 4 | 0.528973 | `kusto_cluster_list` | ❌ |
| 5 | 0.516618 | `cosmos_account_list` | ❌ |

---

## Test 213

**Expected Tool:** `functionapp_get`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560249 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.464985 | `deploy_app_logs_get` | ❌ |
| 3 | 0.436167 | `foundry_agents_list` | ❌ |
| 4 | 0.412646 | `search_service_list` | ❌ |
| 5 | 0.411323 | `get_bestpractices_get` | ❌ |

---

## Test 214

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

## Test 215

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** Get the account settings for my key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604780 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.532196 | `storage_account_get` | ❌ |
| 3 | 0.496629 | `keyvault_key_get` | ❌ |
| 4 | 0.452182 | `appconfig_kv_set` | ❌ |
| 5 | 0.448039 | `keyvault_secret_get` | ❌ |

---

## Test 216

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** Show me the account settings for managed HSM keyvault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671370 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.455561 | `storage_account_get` | ❌ |
| 3 | 0.441225 | `keyvault_key_get` | ❌ |
| 4 | 0.404514 | `appconfig_kv_set` | ❌ |
| 5 | 0.395274 | `keyvault_secret_get` | ❌ |

---

## Test 217

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** What's the value of the <setting_name> setting in my key vault with name <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.505732 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.496337 | `appconfig_kv_set` | ❌ |
| 3 | 0.420075 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.419061 | `keyvault_key_get` | ❌ |
| 5 | 0.410231 | `keyvault_secret_get` | ❌ |

---

## Test 218

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627727 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.570318 | `keyvault_certificate_import` | ❌ |
| 3 | 0.539871 | `keyvault_key_create` | ❌ |
| 4 | 0.519218 | `keyvault_certificate_get` | ❌ |
| 5 | 0.500027 | `keyvault_certificate_list` | ❌ |

---

## Test 219

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Generate a certificate named <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599990 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.561445 | `keyvault_certificate_import` | ❌ |
| 3 | 0.522706 | `keyvault_certificate_get` | ❌ |
| 4 | 0.501747 | `keyvault_key_create` | ❌ |
| 5 | 0.497145 | `keyvault_certificate_list` | ❌ |

---

## Test 220

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Request creation of certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573998 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.527759 | `keyvault_certificate_import` | ❌ |
| 3 | 0.498278 | `keyvault_certificate_get` | ❌ |
| 4 | 0.481062 | `keyvault_key_create` | ❌ |
| 5 | 0.469601 | `keyvault_certificate_list` | ❌ |

---

## Test 221

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591697 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.562265 | `keyvault_certificate_import` | ❌ |
| 3 | 0.522147 | `keyvault_certificate_get` | ❌ |
| 4 | 0.502132 | `keyvault_key_create` | ❌ |
| 5 | 0.479992 | `keyvault_certificate_list` | ❌ |

---

## Test 222

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Issue a certificate <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622788 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.558532 | `keyvault_certificate_import` | ❌ |
| 3 | 0.534503 | `keyvault_certificate_get` | ❌ |
| 4 | 0.521316 | `keyvault_certificate_list` | ❌ |
| 5 | 0.464692 | `keyvault_key_create` | ❌ |

---

## Test 223

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600397 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.528337 | `keyvault_certificate_list` | ❌ |
| 3 | 0.518919 | `keyvault_certificate_import` | ❌ |
| 4 | 0.499515 | `keyvault_certificate_create` | ❌ |
| 5 | 0.486232 | `keyvault_key_get` | ❌ |

---

## Test 224

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646098 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.562988 | `keyvault_key_get` | ❌ |
| 3 | 0.514170 | `keyvault_secret_get` | ❌ |
| 4 | 0.509446 | `keyvault_certificate_list` | ❌ |
| 5 | 0.507737 | `keyvault_certificate_import` | ❌ |

---

## Test 225

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

## Test 226

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Display the certificate details for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.647940 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527530 | `keyvault_key_get` | ❌ |
| 3 | 0.521242 | `keyvault_certificate_list` | ❌ |
| 4 | 0.509718 | `keyvault_certificate_import` | ❌ |
| 5 | 0.501775 | `keyvault_secret_get` | ❌ |

---

## Test 227

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

## Test 228

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585481 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.420747 | `keyvault_certificate_get` | ❌ |
| 3 | 0.402595 | `keyvault_certificate_create` | ❌ |
| 4 | 0.399342 | `keyvault_certificate_list` | ❌ |
| 5 | 0.352483 | `keyvault_key_create` | ❌ |

---

## Test 229

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622125 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.504314 | `keyvault_certificate_get` | ❌ |
| 3 | 0.498847 | `keyvault_certificate_create` | ❌ |
| 4 | 0.448105 | `keyvault_certificate_list` | ❌ |
| 5 | 0.419419 | `keyvault_key_create` | ❌ |

---

## Test 230

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Upload certificate file <file_path> to key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595707 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.453929 | `keyvault_certificate_create` | ❌ |
| 3 | 0.452551 | `keyvault_certificate_get` | ❌ |
| 4 | 0.418203 | `keyvault_certificate_list` | ❌ |
| 5 | 0.413076 | `keyvault_key_create` | ❌ |

---

## Test 231

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619480 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.517804 | `keyvault_certificate_get` | ❌ |
| 3 | 0.480815 | `keyvault_certificate_create` | ❌ |
| 4 | 0.444386 | `keyvault_certificate_list` | ❌ |
| 5 | 0.381454 | `keyvault_key_create` | ❌ |

---

## Test 232

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595417 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.452489 | `keyvault_certificate_create` | ❌ |
| 3 | 0.441616 | `keyvault_certificate_get` | ❌ |
| 4 | 0.407786 | `keyvault_key_create` | ❌ |
| 5 | 0.392244 | `keyvault_secret_create` | ❌ |

---

## Test 233

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726124 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.583110 | `keyvault_key_list` | ❌ |
| 3 | 0.532223 | `keyvault_secret_list` | ❌ |
| 4 | 0.515236 | `keyvault_certificate_get` | ❌ |
| 5 | 0.485792 | `keyvault_certificate_create` | ❌ |

---

## Test 234

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615541 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.522453 | `keyvault_certificate_get` | ❌ |
| 3 | 0.475156 | `keyvault_key_list` | ❌ |
| 4 | 0.460973 | `keyvault_certificate_create` | ❌ |
| 5 | 0.448139 | `keyvault_key_get` | ❌ |

---

## Test 235

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** What certificates are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624711 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.519739 | `keyvault_certificate_get` | ❌ |
| 3 | 0.510048 | `keyvault_certificate_create` | ❌ |
| 4 | 0.505534 | `keyvault_certificate_import` | ❌ |
| 5 | 0.497356 | `keyvault_key_list` | ❌ |

---

## Test 236

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** List certificate names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672622 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.553990 | `keyvault_key_list` | ❌ |
| 3 | 0.512111 | `keyvault_secret_list` | ❌ |
| 4 | 0.507062 | `keyvault_certificate_get` | ❌ |
| 5 | 0.492357 | `keyvault_certificate_create` | ❌ |

---

## Test 237

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Enumerate certificates in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.746603 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.593637 | `keyvault_key_list` | ❌ |
| 3 | 0.558168 | `keyvault_secret_list` | ❌ |
| 4 | 0.516284 | `keyvault_certificate_get` | ❌ |
| 5 | 0.491198 | `keyvault_certificate_create` | ❌ |

---

## Test 238

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show certificate names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639711 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.512475 | `keyvault_certificate_get` | ❌ |
| 3 | 0.507572 | `keyvault_key_list` | ❌ |
| 4 | 0.482583 | `keyvault_certificate_create` | ❌ |
| 5 | 0.464906 | `keyvault_secret_list` | ❌ |

---

## Test 239

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661186 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.456580 | `keyvault_secret_create` | ❌ |
| 3 | 0.451790 | `keyvault_certificate_create` | ❌ |
| 4 | 0.429614 | `keyvault_certificate_import` | ❌ |
| 5 | 0.399326 | `keyvault_key_get` | ❌ |

---

## Test 240

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Generate a key <key_name> with type <key_type> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640735 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.428502 | `keyvault_key_get` | ❌ |
| 3 | 0.422763 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420045 | `keyvault_secret_create` | ❌ |
| 5 | 0.405436 | `appconfig_kv_set` | ❌ |

---

## Test 241

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an oct key in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547023 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.463557 | `keyvault_secret_create` | ❌ |
| 3 | 0.447410 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420366 | `keyvault_key_get` | ❌ |
| 5 | 0.404350 | `keyvault_certificate_import` | ❌ |

---

## Test 242

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an RSA key in the vault <key_vault_account_name> with name <key_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641093 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.501636 | `keyvault_secret_create` | ❌ |
| 3 | 0.491735 | `keyvault_certificate_create` | ❌ |
| 4 | 0.464557 | `keyvault_certificate_import` | ❌ |
| 5 | 0.451016 | `keyvault_key_get` | ❌ |

---

## Test 243

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an EC key with name <key_name> in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571759 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.443534 | `keyvault_certificate_create` | ❌ |
| 3 | 0.434855 | `keyvault_secret_create` | ❌ |
| 4 | 0.421849 | `keyvault_key_get` | ❌ |
| 5 | 0.400694 | `keyvault_certificate_import` | ❌ |

---

## Test 244

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549488 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.468165 | `keyvault_secret_get` | ❌ |
| 3 | 0.452565 | `keyvault_key_create` | ❌ |
| 4 | 0.439969 | `keyvault_key_list` | ❌ |
| 5 | 0.426545 | `keyvault_certificate_get` | ❌ |

---

## Test 245

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629552 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.532651 | `keyvault_secret_get` | ❌ |
| 3 | 0.512278 | `storage_account_get` | ❌ |
| 4 | 0.495957 | `keyvault_certificate_get` | ❌ |
| 5 | 0.456703 | `keyvault_key_create` | ❌ |

---

## Test 246

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Get the key <key_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.484645 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.442960 | `keyvault_key_create` | ❌ |
| 3 | 0.409388 | `keyvault_secret_get` | ❌ |
| 4 | 0.395491 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.383519 | `appconfig_kv_lock_set` | ❌ |

---

## Test 247

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Display the key details for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590303 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.488213 | `keyvault_secret_get` | ❌ |
| 3 | 0.476498 | `storage_account_get` | ❌ |
| 4 | 0.460796 | `keyvault_certificate_get` | ❌ |
| 5 | 0.436511 | `keyvault_admin_settings_get` | ❌ |

---

## Test 248

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Retrieve key metadata for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518886 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.432950 | `storage_account_get` | ❌ |
| 3 | 0.432742 | `keyvault_admin_settings_get` | ❌ |
| 4 | 0.428957 | `keyvault_key_create` | ❌ |
| 5 | 0.422536 | `keyvault_secret_get` | ❌ |

---

## Test 249

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701448 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.601513 | `keyvault_certificate_list` | ❌ |
| 3 | 0.587431 | `keyvault_secret_list` | ❌ |
| 4 | 0.498767 | `cosmos_account_list` | ❌ |
| 5 | 0.480129 | `keyvault_admin_settings_get` | ❌ |

---

## Test 250

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550733 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.507685 | `keyvault_key_get` | ❌ |
| 3 | 0.476341 | `keyvault_certificate_list` | ❌ |
| 4 | 0.472335 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.456264 | `keyvault_secret_get` | ❌ |

---

## Test 251

**Expected Tool:** `keyvault_key_list`  
**Prompt:** What keys are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581970 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.502245 | `keyvault_admin_settings_get` | ❌ |
| 3 | 0.501481 | `keyvault_certificate_list` | ❌ |
| 4 | 0.476470 | `keyvault_key_get` | ❌ |
| 5 | 0.472075 | `keyvault_secret_list` | ❌ |

---

## Test 252

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List key names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641314 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.559550 | `keyvault_certificate_list` | ❌ |
| 3 | 0.553593 | `keyvault_secret_list` | ❌ |
| 4 | 0.486377 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.475992 | `cosmos_account_list` | ❌ |

---

## Test 253

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Enumerate keys in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.723266 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.611366 | `keyvault_certificate_list` | ❌ |
| 3 | 0.610992 | `keyvault_secret_list` | ❌ |
| 4 | 0.473886 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.441881 | `keyvault_key_get` | ❌ |

---

## Test 254

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Show key names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570444 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.501073 | `keyvault_key_get` | ❌ |
| 3 | 0.500103 | `keyvault_certificate_list` | ❌ |
| 4 | 0.496817 | `storage_account_get` | ❌ |
| 5 | 0.490558 | `keyvault_secret_list` | ❌ |

---

## Test 255

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678542 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.552649 | `keyvault_key_create` | ❌ |
| 3 | 0.512877 | `keyvault_secret_get` | ❌ |
| 4 | 0.474844 | `keyvault_certificate_create` | ❌ |
| 5 | 0.461148 | `appconfig_kv_set` | ❌ |

---

## Test 256

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663123 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.519616 | `keyvault_secret_get` | ❌ |
| 3 | 0.511850 | `appconfig_kv_set` | ❌ |
| 4 | 0.458214 | `keyvault_key_create` | ❌ |
| 5 | 0.429633 | `appconfig_kv_lock_set` | ❌ |

---

## Test 257

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639893 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.509547 | `keyvault_secret_get` | ❌ |
| 3 | 0.485163 | `appconfig_kv_set` | ❌ |
| 4 | 0.484233 | `keyvault_key_create` | ❌ |
| 5 | 0.449076 | `appconfig_kv_lock_set` | ❌ |

---

## Test 258

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.675099 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.499598 | `keyvault_secret_get` | ❌ |
| 3 | 0.498035 | `keyvault_key_create` | ❌ |
| 4 | 0.479176 | `keyvault_certificate_import` | ❌ |
| 5 | 0.458387 | `appconfig_kv_set` | ❌ |

---

## Test 259

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Update secret <secret_name> to value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571642 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.513954 | `keyvault_secret_get` | ❌ |
| 3 | 0.440994 | `appconfig_kv_set` | ❌ |
| 4 | 0.417852 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.408461 | `keyvault_key_get` | ❌ |

---

## Test 260

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602769 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.504212 | `keyvault_key_get` | ❌ |
| 3 | 0.501397 | `keyvault_secret_create` | ❌ |
| 4 | 0.478876 | `keyvault_secret_list` | ❌ |
| 5 | 0.439521 | `keyvault_certificate_get` | ❌ |

---

## Test 261

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653871 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.566786 | `keyvault_key_get` | ❌ |
| 3 | 0.517547 | `storage_account_get` | ❌ |
| 4 | 0.496050 | `keyvault_certificate_get` | ❌ |
| 5 | 0.485375 | `keyvault_secret_list` | ❌ |

---

## Test 262

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Get the secret <secret_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578479 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.492213 | `keyvault_key_get` | ❌ |
| 3 | 0.488705 | `keyvault_secret_create` | ❌ |
| 4 | 0.443676 | `keyvault_secret_list` | ❌ |
| 5 | 0.424167 | `keyvault_admin_settings_get` | ❌ |

---

## Test 263

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Display the secret details for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649534 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.547178 | `keyvault_key_get` | ❌ |
| 3 | 0.497288 | `storage_account_get` | ❌ |
| 4 | 0.492692 | `keyvault_certificate_get` | ❌ |
| 5 | 0.491551 | `keyvault_secret_list` | ❌ |

---

## Test 264

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Retrieve secret metadata for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577477 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.475443 | `keyvault_key_get` | ❌ |
| 3 | 0.466890 | `keyvault_secret_create` | ❌ |
| 4 | 0.447412 | `keyvault_secret_list` | ❌ |
| 5 | 0.439583 | `storage_account_get` | ❌ |

---

## Test 265

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701368 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.563736 | `keyvault_key_list` | ❌ |
| 3 | 0.538337 | `keyvault_certificate_list` | ❌ |
| 4 | 0.499642 | `keyvault_secret_get` | ❌ |
| 5 | 0.455500 | `cosmos_account_list` | ❌ |

---

## Test 266

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555574 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.543861 | `keyvault_secret_get` | ❌ |
| 3 | 0.497525 | `keyvault_key_get` | ❌ |
| 4 | 0.464661 | `keyvault_key_list` | ❌ |
| 5 | 0.453130 | `keyvault_admin_settings_get` | ❌ |

---

## Test 267

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** What secrets are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572075 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.529258 | `keyvault_secret_get` | ❌ |
| 3 | 0.493761 | `keyvault_key_list` | ❌ |
| 4 | 0.487620 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.475273 | `keyvault_key_get` | ❌ |

---

## Test 268

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** List secrets names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624334 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.559681 | `keyvault_key_list` | ❌ |
| 3 | 0.517516 | `keyvault_certificate_list` | ❌ |
| 4 | 0.479547 | `keyvault_secret_get` | ❌ |
| 5 | 0.454288 | `storage_blob_container_get` | ❌ |

---

## Test 269

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Enumerate secrets in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.742222 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.601183 | `keyvault_key_list` | ❌ |
| 3 | 0.567827 | `keyvault_certificate_list` | ❌ |
| 4 | 0.496127 | `keyvault_secret_get` | ❌ |
| 5 | 0.437560 | `keyvault_admin_settings_get` | ❌ |

---

## Test 270

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Show secrets names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567193 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.522398 | `keyvault_secret_get` | ❌ |
| 3 | 0.476309 | `keyvault_key_list` | ❌ |
| 4 | 0.462676 | `keyvault_secret_create` | ❌ |
| 5 | 0.461326 | `keyvault_key_get` | ❌ |

---

## Test 271

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588300 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.544302 | `aks_nodepool_get` | ❌ |
| 3 | 0.517279 | `kusto_cluster_get` | ❌ |
| 4 | 0.481416 | `mysql_server_config_get` | ❌ |
| 5 | 0.430975 | `postgres_server_config_get` | ❌ |

---

## Test 272

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621759 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.575625 | `aks_nodepool_get` | ❌ |
| 3 | 0.567870 | `kusto_cluster_get` | ❌ |
| 4 | 0.461414 | `sql_db_show` | ❌ |
| 5 | 0.443899 | `monitor_webtests_get` | ❌ |

---

## Test 273

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522525 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.483220 | `aks_nodepool_get` | ❌ |
| 3 | 0.434684 | `kusto_cluster_get` | ❌ |
| 4 | 0.380301 | `mysql_server_config_get` | ❌ |
| 5 | 0.366784 | `kusto_cluster_list` | ❌ |

---

## Test 274

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588634 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.550555 | `aks_nodepool_get` | ❌ |
| 3 | 0.527511 | `kusto_cluster_get` | ❌ |
| 4 | 0.445722 | `storage_account_get` | ❌ |
| 5 | 0.435597 | `foundry_resource_get` | ❌ |

---

## Test 275

**Expected Tool:** `aks_cluster_get`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756471 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.749395 | `kusto_cluster_list` | ❌ |
| 3 | 0.590166 | `aks_nodepool_get` | ❌ |
| 4 | 0.568463 | `kusto_database_list` | ❌ |
| 5 | 0.562043 | `search_service_list` | ❌ |

---

## Test 276

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612123 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.586776 | `kusto_cluster_list` | ❌ |
| 3 | 0.507757 | `aks_nodepool_get` | ❌ |
| 4 | 0.489724 | `kusto_cluster_get` | ❌ |
| 5 | 0.462891 | `kusto_database_list` | ❌ |

---

## Test 277

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628429 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.563189 | `aks_nodepool_get` | ❌ |
| 3 | 0.526823 | `kusto_cluster_list` | ❌ |
| 4 | 0.426157 | `kusto_cluster_get` | ❌ |
| 5 | 0.409115 | `kusto_database_list` | ❌ |

---

## Test 278

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728569 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.516573 | `kusto_cluster_get` | ❌ |
| 3 | 0.509315 | `aks_cluster_get` | ❌ |
| 4 | 0.468426 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.463208 | `sql_elastic-pool_list` | ❌ |

---

## Test 279

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654106 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.458678 | `sql_elastic-pool_list` | ❌ |
| 3 | 0.446035 | `aks_cluster_get` | ❌ |
| 4 | 0.440182 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.413758 | `kusto_cluster_get` | ❌ |

---

## Test 280

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592689 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.402355 | `aks_cluster_get` | ❌ |
| 3 | 0.384707 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.382894 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.354681 | `kusto_cluster_get` | ❌ |

---

## Test 281

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692231 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.519037 | `aks_cluster_get` | ❌ |
| 3 | 0.506624 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.500858 | `kusto_cluster_list` | ❌ |
| 5 | 0.487680 | `sql_elastic-pool_list` | ❌ |

---

## Test 282

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732131 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.561829 | `aks_cluster_get` | ❌ |
| 3 | 0.510307 | `sql_elastic-pool_list` | ❌ |
| 4 | 0.509732 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.486792 | `kusto_cluster_list` | ❌ |

---

## Test 283

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629359 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.456911 | `aks_cluster_get` | ❌ |
| 3 | 0.443902 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.433120 | `kusto_cluster_list` | ❌ |
| 5 | 0.425452 | `sql_elastic-pool_list` | ❌ |

---

## Test 284

**Expected Tool:** `loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579047 | `loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.520402 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.513284 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.473835 | `monitor_webtests_create` | ❌ |
| 5 | 0.461930 | `loadtesting_testresource_list` | ❌ |

---

## Test 285

**Expected Tool:** `loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626137 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.620023 | `loadtesting_test_get` | ✅ **EXPECTED** |
| 3 | 0.594743 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.590519 | `monitor_webtests_get` | ❌ |
| 5 | 0.535905 | `monitor_webtests_list` | ❌ |

---

## Test 286

**Expected Tool:** `loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645537 | `loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.618698 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.541746 | `loadtesting_test_create` | ❌ |
| 4 | 0.540047 | `loadtesting_testrun_create` | ❌ |
| 5 | 0.526684 | `monitor_webtests_list` | ❌ |

---

## Test 287

**Expected Tool:** `loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.794216 | `loadtesting_testresource_list` | ✅ **EXPECTED** |
| 2 | 0.653165 | `monitor_webtests_list` | ❌ |
| 3 | 0.577408 | `group_list` | ❌ |
| 4 | 0.575172 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.565565 | `datadog_monitoredresources_list` | ❌ |

---

## Test 288

**Expected Tool:** `loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688907 | `loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.594879 | `loadtesting_testrun_update` | ❌ |
| 3 | 0.558636 | `loadtesting_test_create` | ❌ |
| 4 | 0.547102 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.496209 | `loadtesting_testresource_list` | ❌ |

---

## Test 289

**Expected Tool:** `loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619101 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.601927 | `loadtesting_test_get` | ❌ |
| 3 | 0.597430 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.577608 | `monitor_webtests_get` | ❌ |
| 5 | 0.565996 | `loadtesting_testrun_list` | ❌ |

---

## Test 290

**Expected Tool:** `loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669143 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.640360 | `loadtesting_testrun_list` | ✅ **EXPECTED** |
| 3 | 0.601075 | `loadtesting_test_get` | ❌ |
| 4 | 0.577460 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.569589 | `monitor_webtests_get` | ❌ |

---

## Test 291

**Expected Tool:** `loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.706747 | `loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.514580 | `loadtesting_testrun_create` | ❌ |
| 3 | 0.486983 | `monitor_webtests_update` | ❌ |
| 4 | 0.470360 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.468258 | `monitor_webtests_get` | ❌ |

---

## Test 292

**Expected Tool:** `grafana_list`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599384 | `kusto_cluster_list` | ❌ |
| 2 | 0.578892 | `grafana_list` | ✅ **EXPECTED** |
| 3 | 0.551851 | `search_service_list` | ❌ |
| 4 | 0.550372 | `subscription_list` | ❌ |
| 5 | 0.531259 | `redis_list` | ❌ |

---

## Test 293

**Expected Tool:** `managedlustre_filesystem_create`  
**Prompt:** Create an Azure Managed Lustre filesystem with name <filesystem_name>, size <filesystem_size>, SKU <sku>, and subnet <subnet_id> for availability zone <zone> in location <location>. Maintenance should occur on <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728113 | `managedlustre_filesystem_create` | ✅ **EXPECTED** |
| 2 | 0.616164 | `managedlustre_filesystem_list` | ❌ |
| 3 | 0.605775 | `managedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.598255 | `managedlustre_filesystem_update` | ❌ |
| 5 | 0.557720 | `managedlustre_filesystem_subnetsize_validate` | ❌ |

---

## Test 294

**Expected Tool:** `managedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.750675 | `managedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.631770 | `managedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.582660 | `managedlustre_filesystem_create` | ❌ |
| 4 | 0.562353 | `kusto_cluster_list` | ❌ |
| 5 | 0.513156 | `search_service_list` | ❌ |

---

## Test 295

**Expected Tool:** `managedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743903 | `managedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.613217 | `managedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.565856 | `managedlustre_filesystem_create` | ❌ |
| 4 | 0.519986 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.515370 | `loadtesting_testresource_list` | ❌ |

---

## Test 296

**Expected Tool:** `managedlustre_filesystem_sku_get`  
**Prompt:** List the Azure Managed Lustre SKUs available in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.827381 | `managedlustre_filesystem_sku_get` | ✅ **EXPECTED** |
| 2 | 0.613674 | `managedlustre_filesystem_list` | ❌ |
| 3 | 0.513242 | `managedlustre_filesystem_create` | ❌ |
| 4 | 0.496242 | `managedlustre_filesystem_subnetsize_validate` | ❌ |
| 5 | 0.470260 | `kusto_cluster_list` | ❌ |

---

## Test 297

**Expected Tool:** `managedlustre_filesystem_subnetsize_ask`  
**Prompt:** Tell me how many IP addresses I need for an Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739766 | `managedlustre_filesystem_subnetsize_ask` | ✅ **EXPECTED** |
| 2 | 0.651598 | `managedlustre_filesystem_subnetsize_validate` | ❌ |
| 3 | 0.594585 | `managedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.559498 | `managedlustre_filesystem_list` | ❌ |
| 5 | 0.533684 | `managedlustre_filesystem_create` | ❌ |

---

## Test 298

**Expected Tool:** `managedlustre_filesystem_subnetsize_validate`  
**Prompt:** Validate if the network <subnet_id> can host Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879389 | `managedlustre_filesystem_subnetsize_validate` | ✅ **EXPECTED** |
| 2 | 0.622463 | `managedlustre_filesystem_subnetsize_ask` | ❌ |
| 3 | 0.542808 | `managedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.515936 | `managedlustre_filesystem_create` | ❌ |
| 5 | 0.480855 | `managedlustre_filesystem_list` | ❌ |

---

## Test 299

**Expected Tool:** `managedlustre_filesystem_update`  
**Prompt:** Update the maintenance window of the Azure Managed Lustre filesystem <filesystem_name> to <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739000 | `managedlustre_filesystem_update` | ✅ **EXPECTED** |
| 2 | 0.527525 | `managedlustre_filesystem_create` | ❌ |
| 3 | 0.487193 | `managedlustre_filesystem_list` | ❌ |
| 4 | 0.385349 | `managedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.344891 | `managedlustre_filesystem_subnetsize_validate` | ❌ |

---

## Test 300

**Expected Tool:** `marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570145 | `marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.499184 | `marketplace_product_list` | ❌ |
| 3 | 0.353256 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.333160 | `servicebus_topic_details` | ❌ |
| 5 | 0.330935 | `servicebus_queue_details` | ❌ |

---

## Test 301

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607916 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.443133 | `marketplace_product_get` | ❌ |
| 3 | 0.343549 | `search_service_list` | ❌ |
| 4 | 0.330500 | `foundry_models_list` | ❌ |
| 5 | 0.328676 | `managedlustre_filesystem_sku_get` | ❌ |

---

## Test 302

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537726 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.385167 | `marketplace_product_get` | ❌ |
| 3 | 0.308769 | `foundry_models_list` | ❌ |
| 4 | 0.288006 | `redis_list` | ❌ |
| 5 | 0.260387 | `managedlustre_filesystem_sku_get` | ❌ |

---

## Test 303

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

## Test 304

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

## Test 305

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625259 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.594323 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.518643 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.465572 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.450629 | `cloudarchitect_design` | ❌ |

---

## Test 306

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

## Test 307

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

## Test 308

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

## Test 309

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557862 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.513262 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.505123 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.483705 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.421581 | `cloudarchitect_design` | ❌ |

---

## Test 310

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582541 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.500368 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.472112 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.433134 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.432087 | `cloudarchitect_design` | ❌ |

---

## Test 311

**Expected Tool:** `monitor_activitylog_list`  
**Prompt:** List the activity logs of the last month for <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537891 | `monitor_activitylog_list` | ✅ **EXPECTED** |
| 2 | 0.506212 | `monitor_resource_log_query` | ❌ |
| 3 | 0.371727 | `monitor_workspace_log_query` | ❌ |
| 4 | 0.363798 | `resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.344629 | `datadog_monitoredresources_list` | ❌ |

---

## Test 312

**Expected Tool:** `monitor_healthmodels_entity_gethealth`  
**Prompt:** Show me the health status of entity <entity_id> using the health model <health_model_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660947 | `monitor_healthmodels_entity_gethealth` | ✅ **EXPECTED** |
| 2 | 0.603767 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.355315 | `foundry_openai_models-list` | ❌ |
| 4 | 0.351697 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.328321 | `resourcehealth_service-health-events_list` | ❌ |

---

## Test 313

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592664 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424141 | `monitor_metrics_query` | ❌ |
| 3 | 0.368319 | `bicepschema_get` | ❌ |
| 4 | 0.332369 | `monitor_table_type_list` | ❌ |
| 5 | 0.322486 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 314

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607600 | `storage_account_get` | ❌ |
| 2 | 0.587677 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.544781 | `storage_blob_container_get` | ❌ |
| 4 | 0.495760 | `storage_blob_get` | ❌ |
| 5 | 0.473421 | `managedlustre_filesystem_list` | ❌ |

---

## Test 315

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633138 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495513 | `monitor_metrics_query` | ❌ |
| 3 | 0.433945 | `monitor_resource_log_query` | ❌ |
| 4 | 0.392917 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.388750 | `bicepschema_get` | ❌ |

---

## Test 316

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555377 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.527530 | `monitor_resource_log_query` | ❌ |
| 3 | 0.464743 | `applens_resource_diagnose` | ❌ |
| 4 | 0.420462 | `resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.413282 | `applicationinsights_recommendation_list` | ❌ |

---

## Test 317

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557830 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.476671 | `monitor_resource_log_query` | ❌ |
| 3 | 0.460611 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.456308 | `quota_usage_check` | ❌ |
| 5 | 0.438216 | `monitor_metrics_definitions` | ❌ |

---

## Test 318

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461249 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.390037 | `monitor_metrics_definitions` | ❌ |
| 3 | 0.338557 | `monitor_resource_log_query` | ❌ |
| 4 | 0.329996 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.306338 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 319

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496894 | `monitor_resource_log_query` | ❌ |
| 2 | 0.492132 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 3 | 0.448153 | `applens_resource_diagnose` | ❌ |
| 4 | 0.412248 | `resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.397884 | `quota_usage_check` | ❌ |

---

## Test 320

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525585 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.406185 | `monitor_resource_log_query` | ❌ |
| 3 | 0.384514 | `monitor_metrics_definitions` | ❌ |
| 4 | 0.347723 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.325685 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 321

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480140 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.444779 | `monitor_resource_log_query` | ❌ |
| 3 | 0.388382 | `applens_resource_diagnose` | ❌ |
| 4 | 0.363639 | `quota_usage_check` | ❌ |
| 5 | 0.350076 | `resourcehealth_service-health-events_list` | ❌ |

---

## Test 322

**Expected Tool:** `monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687852 | `monitor_resource_log_query` | ✅ **EXPECTED** |
| 2 | 0.621919 | `monitor_workspace_log_query` | ❌ |
| 3 | 0.598253 | `monitor_activitylog_list` | ❌ |
| 4 | 0.485633 | `deploy_app_logs_get` | ❌ |
| 5 | 0.469703 | `monitor_metrics_query` | ❌ |

---

## Test 323

**Expected Tool:** `monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851149 | `monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725693 | `monitor_table_type_list` | ❌ |
| 3 | 0.620451 | `monitor_workspace_list` | ❌ |
| 4 | 0.541928 | `kusto_table_list` | ❌ |
| 5 | 0.539481 | `monitor_workspace_log_query` | ❌ |

---

## Test 324

**Expected Tool:** `monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798533 | `monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.701092 | `monitor_table_type_list` | ❌ |
| 3 | 0.600003 | `monitor_workspace_list` | ❌ |
| 4 | 0.542821 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.502882 | `monitor_resource_log_query` | ❌ |

---

## Test 325

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881468 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.765732 | `monitor_table_list` | ❌ |
| 3 | 0.570092 | `monitor_workspace_list` | ❌ |
| 4 | 0.504683 | `mysql_table_list` | ❌ |
| 5 | 0.497622 | `monitor_workspace_log_query` | ❌ |

---

## Test 326

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843110 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.736855 | `monitor_table_list` | ❌ |
| 3 | 0.576934 | `monitor_workspace_list` | ❌ |
| 4 | 0.509598 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.481189 | `mysql_table_list` | ❌ |

---

## Test 327

**Expected Tool:** `monitor_webtests_create`  
**Prompt:** Create a new Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650530 | `monitor_webtests_create` | ✅ **EXPECTED** |
| 2 | 0.569712 | `monitor_webtests_list` | ❌ |
| 3 | 0.549740 | `monitor_webtests_update` | ❌ |
| 4 | 0.532978 | `monitor_webtests_get` | ❌ |
| 5 | 0.482211 | `loadtesting_testresource_create` | ❌ |

---

## Test 328

**Expected Tool:** `monitor_webtests_get`  
**Prompt:** Get Web Test details for <webtest_resource_name> in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.758851 | `monitor_webtests_get` | ✅ **EXPECTED** |
| 2 | 0.725436 | `monitor_webtests_list` | ❌ |
| 3 | 0.583920 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.562799 | `monitor_webtests_update` | ❌ |
| 5 | 0.530659 | `monitor_webtests_create` | ❌ |

---

## Test 329

**Expected Tool:** `monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.730616 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.610064 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.547708 | `grafana_list` | ❌ |
| 4 | 0.520829 | `redis_list` | ❌ |
| 5 | 0.495747 | `monitor_webtests_get` | ❌ |

---

## Test 330

**Expected Tool:** `monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793807 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.675892 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.584089 | `monitor_webtests_get` | ❌ |
| 4 | 0.573602 | `group_list` | ❌ |
| 5 | 0.546088 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 331

**Expected Tool:** `monitor_webtests_update`  
**Prompt:** Update an existing Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686485 | `monitor_webtests_update` | ✅ **EXPECTED** |
| 2 | 0.558955 | `monitor_webtests_get` | ❌ |
| 3 | 0.558239 | `monitor_webtests_create` | ❌ |
| 4 | 0.553726 | `monitor_webtests_list` | ❌ |
| 5 | 0.508736 | `loadtesting_testrun_update` | ❌ |

---

## Test 332

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813871 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.680201 | `grafana_list` | ❌ |
| 3 | 0.660214 | `monitor_table_list` | ❌ |
| 4 | 0.610574 | `kusto_cluster_list` | ❌ |
| 5 | 0.600802 | `search_service_list` | ❌ |

---

## Test 333

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656200 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.585449 | `monitor_table_list` | ❌ |
| 3 | 0.531093 | `monitor_table_type_list` | ❌ |
| 4 | 0.518254 | `grafana_list` | ❌ |
| 5 | 0.506772 | `monitor_workspace_log_query` | ❌ |

---

## Test 334

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732964 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.601481 | `grafana_list` | ❌ |
| 3 | 0.580298 | `monitor_table_list` | ❌ |
| 4 | 0.523782 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.522738 | `kusto_cluster_list` | ❌ |

---

## Test 335

**Expected Tool:** `monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610115 | `monitor_workspace_log_query` | ✅ **EXPECTED** |
| 2 | 0.587614 | `monitor_resource_log_query` | ❌ |
| 3 | 0.527577 | `monitor_activitylog_list` | ❌ |
| 4 | 0.498269 | `deploy_app_logs_get` | ❌ |
| 5 | 0.486025 | `monitor_table_list` | ❌ |

---

## Test 336

**Expected Tool:** `datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.668827 | `datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.454270 | `redis_list` | ❌ |
| 3 | 0.413583 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.413173 | `monitor_metrics_query` | ❌ |
| 5 | 0.401731 | `grafana_list` | ❌ |

---

## Test 337

**Expected Tool:** `datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624066 | `datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.443481 | `monitor_metrics_query` | ❌ |
| 3 | 0.440052 | `redis_list` | ❌ |
| 4 | 0.424391 | `monitor_resource_log_query` | ❌ |
| 5 | 0.385075 | `loadtesting_testresource_list` | ❌ |

---

## Test 338

**Expected Tool:** `extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533386 | `quota_usage_check` | ❌ |
| 2 | 0.481143 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.476826 | `extension_azqr` | ✅ **EXPECTED** |
| 4 | 0.471499 | `subscription_list` | ❌ |
| 5 | 0.468404 | `applens_resource_diagnose` | ❌ |

---

## Test 339

**Expected Tool:** `extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532792 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.492863 | `get_bestpractices_get` | ❌ |
| 3 | 0.476164 | `applicationinsights_recommendation_list` | ❌ |
| 4 | 0.473365 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.464604 | `cloudarchitect_design` | ❌ |

---

## Test 340

**Expected Tool:** `extension_azqr`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536934 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.516925 | `extension_azqr` | ✅ **EXPECTED** |
| 3 | 0.514978 | `applicationinsights_recommendation_list` | ❌ |
| 4 | 0.504902 | `quota_usage_check` | ❌ |
| 5 | 0.494872 | `deploy_plan_get` | ❌ |

---

## Test 341

**Expected Tool:** `quota_region_availability_list`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590878 | `quota_region_availability_list` | ✅ **EXPECTED** |
| 2 | 0.413587 | `quota_usage_check` | ❌ |
| 3 | 0.391332 | `redis_list` | ❌ |
| 4 | 0.372940 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.369855 | `managedlustre_filesystem_sku_get` | ❌ |

---

## Test 342

**Expected Tool:** `quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609679 | `quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.491058 | `quota_region_availability_list` | ❌ |
| 3 | 0.384350 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.373815 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.371407 | `redis_list` | ❌ |

---

## Test 343

**Expected Tool:** `role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645259 | `role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.539761 | `subscription_list` | ❌ |
| 3 | 0.483988 | `group_list` | ❌ |
| 4 | 0.478700 | `grafana_list` | ❌ |
| 5 | 0.471364 | `cosmos_account_list` | ❌ |

---

## Test 344

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

## Test 345

**Expected Tool:** `redis_list`  
**Prompt:** List all Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.810504 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.587836 | `grafana_list` | ❌ |
| 3 | 0.512891 | `kusto_cluster_list` | ❌ |
| 4 | 0.508531 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.501218 | `postgres_server_list` | ❌ |

---

## Test 346

**Expected Tool:** `redis_list`  
**Prompt:** Show me my Redis resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685128 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.374328 | `grafana_list` | ❌ |
| 3 | 0.364197 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.359512 | `mysql_server_list` | ❌ |
| 5 | 0.331227 | `mysql_database_list` | ❌ |

---

## Test 347

**Expected Tool:** `redis_list`  
**Prompt:** Show me the Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.781228 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.539177 | `grafana_list` | ❌ |
| 3 | 0.449276 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.449014 | `postgres_server_list` | ❌ |
| 5 | 0.442824 | `kusto_cluster_list` | ❌ |

---

## Test 348

**Expected Tool:** `redis_list`  
**Prompt:** Show me my Redis caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572767 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.316591 | `mysql_database_list` | ❌ |
| 3 | 0.301786 | `postgres_database_list` | ❌ |
| 4 | 0.286470 | `mysql_server_list` | ❌ |
| 5 | 0.273064 | `kusto_cluster_list` | ❌ |

---

## Test 349

**Expected Tool:** `redis_list`  
**Prompt:** Get Redis clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478070 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.456461 | `kusto_cluster_list` | ❌ |
| 3 | 0.384630 | `kusto_cluster_get` | ❌ |
| 4 | 0.359373 | `kusto_database_list` | ❌ |
| 5 | 0.343305 | `aks_cluster_get` | ❌ |

---

## Test 350

**Expected Tool:** `group_list`  
**Prompt:** List all resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755935 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.566552 | `workbooks_list` | ❌ |
| 3 | 0.564538 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.552633 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.549477 | `monitor_webtests_list` | ❌ |

---

## Test 351

**Expected Tool:** `group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529504 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.464690 | `redis_list` | ❌ |
| 3 | 0.463685 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.462294 | `mysql_server_list` | ❌ |
| 5 | 0.460277 | `loadtesting_testresource_list` | ❌ |

---

## Test 352

**Expected Tool:** `group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665771 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.532656 | `datadog_monitoredresources_list` | ❌ |
| 3 | 0.532505 | `redis_list` | ❌ |
| 4 | 0.532054 | `eventgrid_topic_list` | ❌ |
| 5 | 0.531920 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 353

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555166 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.538273 | `resourcehealth_availability-status_list` | ❌ |
| 3 | 0.405307 | `foundry_openai_models-list` | ❌ |
| 4 | 0.377957 | `quota_usage_check` | ❌ |
| 5 | 0.373112 | `monitor_healthmodels_entity_gethealth` | ❌ |

---

## Test 354

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576591 | `storage_account_get` | ❌ |
| 2 | 0.565992 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.556167 | `storage_blob_container_get` | ❌ |
| 4 | 0.487467 | `storage_blob_get` | ❌ |
| 5 | 0.466885 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 355

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577398 | `resourcehealth_availability-status_list` | ❌ |
| 2 | 0.501221 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.424655 | `mysql_server_list` | ❌ |
| 4 | 0.413772 | `foundry_openai_models-list` | ❌ |
| 5 | 0.412023 | `loadtesting_testresource_list` | ❌ |

---

## Test 356

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737219 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.585501 | `redis_list` | ❌ |
| 3 | 0.549845 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.548549 | `grafana_list` | ❌ |
| 5 | 0.544505 | `subscription_list` | ❌ |

---

## Test 357

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644982 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.546808 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.509740 | `resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.508683 | `quota_usage_check` | ❌ |
| 5 | 0.505776 | `redis_list` | ❌ |

---

## Test 358

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.596890 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.550357 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.496640 | `resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.441921 | `applens_resource_diagnose` | ❌ |
| 5 | 0.433550 | `loadtesting_testresource_list` | ❌ |

---

## Test 359

**Expected Tool:** `resourcehealth_service-health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690719 | `resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.554895 | `search_service_list` | ❌ |
| 3 | 0.534250 | `eventgrid_topic_list` | ❌ |
| 4 | 0.529761 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.518372 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 360

**Expected Tool:** `resourcehealth_service-health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686448 | `resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.534556 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.513815 | `search_service_list` | ❌ |
| 4 | 0.513259 | `eventgrid_topic_list` | ❌ |
| 5 | 0.501135 | `subscription_list` | ❌ |

---

## Test 361

**Expected Tool:** `resourcehealth_service-health-events_list`  
**Prompt:** What service issues have occurred in the last 30 days?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.450841 | `resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.267663 | `applens_resource_diagnose` | ❌ |
| 3 | 0.245720 | `cloudarchitect_design` | ❌ |
| 4 | 0.216847 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.211842 | `search_service_list` | ❌ |

---

## Test 362

**Expected Tool:** `resourcehealth_service-health-events_list`  
**Prompt:** List active service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685391 | `resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.527905 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.524063 | `eventgrid_topic_list` | ❌ |
| 4 | 0.520197 | `search_service_list` | ❌ |
| 5 | 0.502064 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 363

**Expected Tool:** `resourcehealth_service-health-events_list`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565851 | `resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.437868 | `search_service_list` | ❌ |
| 3 | 0.403665 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.402493 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.402232 | `foundry_agents_list` | ❌ |

---

## Test 364

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

## Test 365

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

## Test 366

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

## Test 367

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

## Test 368

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

## Test 369

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Describe the SignalR runtime <signalr_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710353 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.411414 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.410606 | `foundry_resource_get` | ❌ |
| 4 | 0.399412 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.382028 | `sql_server_list` | ❌ |

---

## Test 370

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Get information about my SignalR runtime <signalr_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.715974 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.459045 | `foundry_resource_get` | ❌ |
| 3 | 0.430829 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.430780 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.417032 | `functionapp_get` | ❌ |

---

## Test 371

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show all the SignalRs information in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564072 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.501077 | `redis_list` | ❌ |
| 3 | 0.494478 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.481386 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.461911 | `mysql_server_list` | ❌ |

---

## Test 372

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** List all SignalRs in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530646 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.507653 | `postgres_server_list` | ❌ |
| 3 | 0.495157 | `redis_list` | ❌ |
| 4 | 0.494342 | `kusto_cluster_list` | ❌ |
| 5 | 0.487856 | `subscription_list` | ❌ |

---

## Test 373

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

## Test 374

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a SQL database <database_name> with Basic tier in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571760 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.459672 | `sql_server_create` | ❌ |
| 3 | 0.437526 | `sql_server_delete` | ❌ |
| 4 | 0.424021 | `appservice_database_add` | ❌ |
| 5 | 0.420666 | `sql_db_show` | ❌ |

---

## Test 375

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604472 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.545906 | `sql_server_create` | ❌ |
| 3 | 0.504013 | `sql_db_rename` | ❌ |
| 4 | 0.494081 | `sql_db_show` | ❌ |
| 5 | 0.473975 | `sql_db_list` | ❌ |

---

## Test 376

**Expected Tool:** `sql_db_delete`  
**Prompt:** Delete the SQL database <database_name> from server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.568196 | `sql_db_delete` | ✅ **EXPECTED** |
| 2 | 0.567412 | `sql_server_delete` | ❌ |
| 3 | 0.391509 | `sql_db_rename` | ❌ |
| 4 | 0.386564 | `sql_server_firewall-rule_delete` | ❌ |
| 5 | 0.364491 | `sql_db_show` | ❌ |

---

## Test 377

**Expected Tool:** `sql_db_delete`  
**Prompt:** Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567513 | `sql_server_delete` | ❌ |
| 2 | 0.543440 | `sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.500447 | `sql_db_show` | ❌ |
| 4 | 0.481083 | `sql_db_rename` | ❌ |
| 5 | 0.478729 | `sql_db_list` | ❌ |

---

## Test 378

**Expected Tool:** `sql_db_delete`  
**Prompt:** Delete the database called <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509916 | `sql_db_delete` | ✅ **EXPECTED** |
| 2 | 0.490892 | `sql_server_delete` | ❌ |
| 3 | 0.364494 | `postgres_database_list` | ❌ |
| 4 | 0.355483 | `mysql_database_list` | ❌ |
| 5 | 0.347837 | `sql_db_rename` | ❌ |

---

## Test 379

**Expected Tool:** `sql_db_list`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643186 | `sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.640909 | `mysql_database_list` | ❌ |
| 3 | 0.609178 | `postgres_database_list` | ❌ |
| 4 | 0.602890 | `cosmos_database_list` | ❌ |
| 5 | 0.570054 | `kusto_database_list` | ❌ |

---

## Test 380

**Expected Tool:** `sql_db_list`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.617746 | `sql_server_show` | ❌ |
| 2 | 0.609322 | `sql_db_list` | ✅ **EXPECTED** |
| 3 | 0.558748 | `mysql_database_list` | ❌ |
| 4 | 0.553488 | `mysql_server_config_get` | ❌ |
| 5 | 0.524095 | `sql_db_show` | ❌ |

---

## Test 381

**Expected Tool:** `sql_db_rename`  
**Prompt:** Rename the SQL database <database_name> on server <server_name> to <new_database_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593348 | `sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.425282 | `sql_server_delete` | ❌ |
| 3 | 0.416207 | `sql_db_delete` | ❌ |
| 4 | 0.396947 | `sql_db_create` | ❌ |
| 5 | 0.345764 | `sql_db_show` | ❌ |

---

## Test 382

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

## Test 383

**Expected Tool:** `sql_db_show`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610995 | `sql_server_show` | ❌ |
| 2 | 0.593096 | `postgres_server_config_get` | ❌ |
| 3 | 0.530393 | `mysql_server_config_get` | ❌ |
| 4 | 0.528037 | `sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.465742 | `sql_db_list` | ❌ |

---

## Test 384

**Expected Tool:** `sql_db_show`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529945 | `sql_db_show` | ✅ **EXPECTED** |
| 2 | 0.503681 | `sql_server_show` | ❌ |
| 3 | 0.440073 | `sql_db_list` | ❌ |
| 4 | 0.438622 | `mysql_table_schema_get` | ❌ |
| 5 | 0.434145 | `mysql_database_list` | ❌ |

---

## Test 385

**Expected Tool:** `sql_db_update`  
**Prompt:** Update the performance tier of SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603366 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.467571 | `sql_db_create` | ❌ |
| 3 | 0.440493 | `sql_db_rename` | ❌ |
| 4 | 0.427585 | `sql_db_show` | ❌ |
| 5 | 0.413941 | `sql_server_delete` | ❌ |

---

## Test 386

**Expected Tool:** `sql_db_update`  
**Prompt:** Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550556 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.418358 | `sql_server_delete` | ❌ |
| 3 | 0.401817 | `sql_db_list` | ❌ |
| 4 | 0.395518 | `sql_db_rename` | ❌ |
| 5 | 0.394767 | `sql_db_show` | ❌ |

---

## Test 387

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678126 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502376 | `sql_db_list` | ❌ |
| 3 | 0.499400 | `mysql_database_list` | ❌ |
| 4 | 0.485249 | `aks_nodepool_get` | ❌ |
| 5 | 0.479044 | `sql_server_show` | ❌ |

---

## Test 388

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606450 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502877 | `sql_server_show` | ❌ |
| 3 | 0.457163 | `sql_db_list` | ❌ |
| 4 | 0.450743 | `aks_nodepool_get` | ❌ |
| 5 | 0.434328 | `mysql_database_list` | ❌ |

---

## Test 389

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592743 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.421920 | `mysql_database_list` | ❌ |
| 3 | 0.407169 | `aks_nodepool_get` | ❌ |
| 4 | 0.402402 | `mysql_server_list` | ❌ |
| 5 | 0.397670 | `sql_db_list` | ❌ |

---

## Test 390

**Expected Tool:** `sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682407 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.563482 | `sql_db_create` | ❌ |
| 3 | 0.529212 | `sql_server_list` | ❌ |
| 4 | 0.481779 | `storage_account_create` | ❌ |
| 5 | 0.474208 | `sql_db_rename` | ❌ |

---

## Test 391

**Expected Tool:** `sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618290 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.510201 | `sql_db_create` | ❌ |
| 3 | 0.472343 | `sql_server_show` | ❌ |
| 4 | 0.441090 | `sql_server_delete` | ❌ |
| 5 | 0.400935 | `sql_db_rename` | ❌ |

---

## Test 392

**Expected Tool:** `sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589596 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.500967 | `sql_db_create` | ❌ |
| 3 | 0.497732 | `sql_server_list` | ❌ |
| 4 | 0.460954 | `sql_db_rename` | ❌ |
| 5 | 0.442837 | `mysql_server_list` | ❌ |

---

## Test 393

**Expected Tool:** `sql_server_delete`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656593 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.548064 | `sql_db_delete` | ❌ |
| 3 | 0.518036 | `sql_server_list` | ❌ |
| 4 | 0.495550 | `sql_server_create` | ❌ |
| 5 | 0.483132 | `workbooks_delete` | ❌ |

---

## Test 394

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

## Test 395

**Expected Tool:** `sql_server_delete`  
**Prompt:** Delete SQL server <server_name> permanently  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624310 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.454892 | `sql_db_delete` | ❌ |
| 3 | 0.362389 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.341503 | `sql_server_show` | ❌ |
| 5 | 0.318821 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 396

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

## Test 397

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

## Test 398

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

## Test 399

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.635491 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.532752 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.522209 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.448879 | `sql_server_create` | ❌ |
| 5 | 0.440953 | `sql_server_delete` | ❌ |

---

## Test 400

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670189 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533562 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503648 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.316619 | `sql_server_list` | ❌ |
| 5 | 0.302362 | `sql_server_delete` | ❌ |

---

## Test 401

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685112 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.574424 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539632 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.428934 | `sql_server_create` | ❌ |
| 5 | 0.395197 | `sql_db_create` | ❌ |

---

## Test 402

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

## Test 403

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

## Test 404

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

## Test 405

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

## Test 406

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

## Test 407

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

## Test 408

**Expected Tool:** `sql_server_list`  
**Prompt:** List all Azure SQL servers in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694350 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.596464 | `mysql_server_list` | ❌ |
| 3 | 0.578164 | `sql_db_list` | ❌ |
| 4 | 0.515621 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.509488 | `sql_db_show` | ❌ |

---

## Test 409

**Expected Tool:** `sql_server_list`  
**Prompt:** Show me every SQL server available in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618218 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.593658 | `mysql_server_list` | ❌ |
| 3 | 0.542398 | `sql_db_list` | ❌ |
| 4 | 0.507404 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.496200 | `group_list` | ❌ |

---

## Test 410

**Expected Tool:** `sql_server_show`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629551 | `sql_db_show` | ❌ |
| 2 | 0.595184 | `sql_server_show` | ✅ **EXPECTED** |
| 3 | 0.587728 | `sql_server_list` | ❌ |
| 4 | 0.559597 | `mysql_server_list` | ❌ |
| 5 | 0.540218 | `sql_db_list` | ❌ |

---

## Test 411

**Expected Tool:** `sql_server_show`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658817 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.610507 | `postgres_server_config_get` | ❌ |
| 3 | 0.538034 | `mysql_server_config_get` | ❌ |
| 4 | 0.471436 | `sql_db_show` | ❌ |
| 5 | 0.445430 | `postgres_server_param_get` | ❌ |

---

## Test 412

**Expected Tool:** `sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563143 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.392532 | `postgres_server_config_get` | ❌ |
| 3 | 0.380021 | `postgres_server_param_get` | ❌ |
| 4 | 0.372194 | `sql_server_firewall-rule_list` | ❌ |
| 5 | 0.370282 | `sql_db_show` | ❌ |

---

## Test 413

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533552 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.438046 | `storage_blob_container_create` | ❌ |
| 3 | 0.418191 | `storage_account_get` | ❌ |
| 4 | 0.414518 | `storage_blob_container_get` | ❌ |
| 5 | 0.370957 | `managedlustre_filesystem_create` | ❌ |

---

## Test 414

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500638 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.483202 | `managedlustre_filesystem_create` | ❌ |
| 3 | 0.407222 | `storage_account_get` | ❌ |
| 4 | 0.406804 | `storage_blob_container_create` | ❌ |
| 5 | 0.400151 | `managedlustre_filesystem_sku_get` | ❌ |

---

## Test 415

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589003 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.535501 | `managedlustre_filesystem_create` | ❌ |
| 3 | 0.509731 | `storage_blob_container_create` | ❌ |
| 4 | 0.462519 | `storage_account_get` | ❌ |
| 5 | 0.447156 | `sql_db_create` | ❌ |

---

## Test 416

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.673749 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.608256 | `storage_blob_container_get` | ❌ |
| 3 | 0.556487 | `storage_blob_get` | ❌ |
| 4 | 0.483435 | `storage_account_create` | ❌ |
| 5 | 0.439236 | `cosmos_account_list` | ❌ |

---

## Test 417

**Expected Tool:** `storage_account_get`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692687 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.577547 | `storage_blob_container_get` | ❌ |
| 3 | 0.529275 | `storage_blob_get` | ❌ |
| 4 | 0.518215 | `storage_account_create` | ❌ |
| 5 | 0.448507 | `storage_blob_container_create` | ❌ |

---

## Test 418

**Expected Tool:** `storage_account_get`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649215 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.557016 | `managedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.550148 | `storage_blob_container_get` | ❌ |
| 4 | 0.547647 | `subscription_list` | ❌ |
| 5 | 0.536909 | `cosmos_account_list` | ❌ |

---

## Test 419

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.556860 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.482418 | `storage_blob_container_get` | ❌ |
| 3 | 0.461284 | `managedlustre_filesystem_list` | ❌ |
| 4 | 0.421642 | `cosmos_account_list` | ❌ |
| 5 | 0.410888 | `storage_blob_get` | ❌ |

---

## Test 420

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619462 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.556436 | `storage_blob_container_get` | ❌ |
| 3 | 0.518458 | `storage_blob_get` | ❌ |
| 4 | 0.473598 | `cosmos_account_list` | ❌ |
| 5 | 0.465571 | `subscription_list` | ❌ |

---

## Test 421

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649793 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.583896 | `storage_blob_container_get` | ❌ |
| 3 | 0.524779 | `storage_account_create` | ❌ |
| 4 | 0.496592 | `storage_blob_get` | ❌ |
| 5 | 0.447784 | `cosmos_database_container_list` | ❌ |

---

## Test 422

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682161 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.590160 | `storage_blob_container_get` | ❌ |
| 3 | 0.559039 | `storage_blob_get` | ❌ |
| 4 | 0.500624 | `storage_account_create` | ❌ |
| 5 | 0.420514 | `storage_account_get` | ❌ |

---

## Test 423

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625397 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.543503 | `storage_blob_container_get` | ❌ |
| 3 | 0.497529 | `storage_blob_get` | ❌ |
| 4 | 0.463198 | `storage_account_create` | ❌ |
| 5 | 0.435099 | `cosmos_database_container_list` | ❌ |

---

## Test 424

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701642 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.623683 | `storage_blob_get` | ❌ |
| 3 | 0.577921 | `storage_account_get` | ❌ |
| 4 | 0.549803 | `storage_blob_container_create` | ❌ |
| 5 | 0.523288 | `cosmos_database_container_list` | ❌ |

---

## Test 425

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712037 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.680346 | `storage_blob_get` | ❌ |
| 3 | 0.613933 | `cosmos_database_container_list` | ❌ |
| 4 | 0.556319 | `storage_blob_container_create` | ❌ |
| 5 | 0.518266 | `storage_account_get` | ❌ |

---

## Test 426

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713527 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.592373 | `cosmos_database_container_list` | ❌ |
| 3 | 0.586127 | `storage_blob_get` | ❌ |
| 4 | 0.523322 | `storage_account_get` | ❌ |
| 5 | 0.487521 | `storage_blob_container_create` | ❌ |

---

## Test 427

**Expected Tool:** `storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.700921 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.646973 | `storage_blob_container_get` | ❌ |
| 3 | 0.541019 | `storage_blob_container_create` | ❌ |
| 4 | 0.527427 | `storage_account_get` | ❌ |
| 5 | 0.477946 | `cosmos_database_container_list` | ❌ |

---

## Test 428

**Expected Tool:** `storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694910 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.631161 | `storage_blob_container_get` | ❌ |
| 3 | 0.589152 | `storage_blob_container_create` | ❌ |
| 4 | 0.580226 | `storage_account_get` | ❌ |
| 5 | 0.457038 | `storage_account_create` | ❌ |

---

## Test 429

**Expected Tool:** `storage_blob_get`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.733262 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.700890 | `storage_blob_container_get` | ❌ |
| 3 | 0.606054 | `storage_blob_container_create` | ❌ |
| 4 | 0.579069 | `cosmos_database_container_list` | ❌ |
| 5 | 0.506636 | `cosmos_database_container_item_query` | ❌ |

---

## Test 430

**Expected Tool:** `storage_blob_get`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.704288 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.664940 | `storage_blob_container_get` | ❌ |
| 3 | 0.561557 | `storage_blob_container_create` | ❌ |
| 4 | 0.533515 | `cosmos_database_container_list` | ❌ |
| 5 | 0.484018 | `storage_account_get` | ❌ |

---

## Test 431

**Expected Tool:** `storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565408 | `storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.525402 | `storage_blob_container_create` | ❌ |
| 3 | 0.517699 | `storage_blob_get` | ❌ |
| 4 | 0.473657 | `storage_blob_container_get` | ❌ |
| 5 | 0.381596 | `storage_account_create` | ❌ |

---

## Test 432

**Expected Tool:** `subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654071 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512964 | `cosmos_account_list` | ❌ |
| 3 | 0.471653 | `postgres_server_list` | ❌ |
| 4 | 0.468865 | `kusto_cluster_list` | ❌ |
| 5 | 0.461078 | `redis_list` | ❌ |

---

## Test 433

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

## Test 434

**Expected Tool:** `subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433196 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.319579 | `marketplace_product_list` | ❌ |
| 3 | 0.315547 | `marketplace_product_get` | ❌ |
| 4 | 0.293772 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.289334 | `eventgrid_topic_list` | ❌ |

---

## Test 435

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

## Test 436

**Expected Tool:** `azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686886 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.625270 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.605047 | `get_bestpractices_get` | ❌ |
| 4 | 0.482936 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.466199 | `deploy_plan_get` | ❌ |

---

## Test 437

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

## Test 438

**Expected Tool:** `virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711969 | `virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
| 2 | 0.659763 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.620629 | `kusto_cluster_list` | ❌ |
| 4 | 0.548888 | `search_service_list` | ❌ |
| 5 | 0.535739 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 439

**Expected Tool:** `virtualdesktop_hostpool_sessionhost_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.727054 | `virtualdesktop_hostpool_sessionhost_list` | ✅ **EXPECTED** |
| 2 | 0.714469 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 3 | 0.573352 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.438659 | `aks_nodepool_get` | ❌ |
| 5 | 0.393757 | `sql_elastic-pool_list` | ❌ |

---

## Test 440

**Expected Tool:** `virtualdesktop_hostpool_sessionhost_usersession-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812659 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ✅ **EXPECTED** |
| 2 | 0.659212 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.501167 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.357561 | `aks_nodepool_get` | ❌ |
| 5 | 0.336576 | `monitor_workspace_list` | ❌ |

---

## Test 441

**Expected Tool:** `workbooks_create`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552212 | `workbooks_create` | ✅ **EXPECTED** |
| 2 | 0.417970 | `workbooks_update` | ❌ |
| 3 | 0.361364 | `workbooks_delete` | ❌ |
| 4 | 0.329118 | `workbooks_show` | ❌ |
| 5 | 0.328113 | `workbooks_list` | ❌ |

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
| 5 | 0.421821 | `workbooks_update` | ❌ |

---

## Test 443

**Expected Tool:** `workbooks_list`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.772431 | `workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.562485 | `workbooks_create` | ❌ |
| 3 | 0.516739 | `grafana_list` | ❌ |
| 4 | 0.494073 | `workbooks_show` | ❌ |
| 5 | 0.488600 | `group_list` | ❌ |

---

## Test 444

**Expected Tool:** `workbooks_list`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.708612 | `workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.570259 | `workbooks_create` | ❌ |
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
| 4 | 0.462988 | `workbooks_update` | ❌ |
| 5 | 0.452348 | `workbooks_delete` | ❌ |

---

## Test 446

**Expected Tool:** `workbooks_show`  
**Prompt:** Show me the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581501 | `workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.500475 | `workbooks_list` | ❌ |
| 3 | 0.468996 | `workbooks_create` | ❌ |
| 4 | 0.466137 | `workbooks_update` | ❌ |
| 5 | 0.455311 | `workbooks_delete` | ❌ |

---

## Test 447

**Expected Tool:** `workbooks_update`  
**Prompt:** Update the workbook <workbook_resource_id> with a new text step  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586432 | `workbooks_update` | ✅ **EXPECTED** |
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
| 2 | 0.485970 | `foundry_models_deploy` | ❌ |
| 3 | 0.485889 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.462146 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.448373 | `get_bestpractices_get` | ❌ |

---

## Test 449

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502217 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.290814 | `storage_blob_upload` | ❌ |
| 3 | 0.259235 | `managedlustre_filesystem_create` | ❌ |
| 4 | 0.255202 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.245148 | `managedlustre_filesystem_subnetsize_validate` | ❌ |

---

## Test 450

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Help me create a cloud service that will serve as ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.405148 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.267683 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.258160 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.242606 | `extension_cli_generate` | ❌ |
| 5 | 0.225870 | `foundry_models_deploy` | ❌ |

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
| 4 | 0.242581 | `deploy_plan_get` | ❌ |
| 5 | 0.229074 | `extension_cli_generate` | ❌ |

---

## Test 452

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.534690 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.369969 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.356331 | `managedlustre_filesystem_create` | ❌ |
| 4 | 0.352797 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.323857 | `storage_blob_upload` | ❌ |

---

## Summary

**Total Prompts Tested:** 452  
**Analysis Execution Time:** 163.9199660s  

### Success Rate Metrics

**Top Choice Success:** 91.8% (415/452 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 3.3% (15/452 tests)  
**🎯 High Confidence (≥0.7):** 20.8% (94/452 tests)  
**✅ Good Confidence (≥0.6):** 60.0% (271/452 tests)  
**👍 Fair Confidence (≥0.5):** 90.9% (411/452 tests)  
**👌 Acceptable Confidence (≥0.4):** 98.7% (446/452 tests)  
**❌ Low Confidence (<0.4):** 1.3% (6/452 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 3.3% (15/452 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 20.8% (94/452 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 58.2% (263/452 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 86.3% (390/452 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 91.8% (415/452 tests)  

### Success Rate Analysis

🟢 **Excellent** - The tool selection system is performing exceptionally well.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

