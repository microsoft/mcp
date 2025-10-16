# Tool Selection Analysis Setup

**Setup completed:** 2025-10-16 16:24:16  
**Tool count:** 60  
**Database setup time:** 1.2856550s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-10-16 16:24:16  
**Tool count:** 60  

## Table of Contents

- [Test 1: get_azure_subscriptions_and_resource_groups](#test-1)
- [Test 2: get_azure_subscriptions_and_resource_groups](#test-2)
- [Test 3: get_azure_subscriptions_and_resource_groups](#test-3)
- [Test 4: get_azure_subscriptions_and_resource_groups](#test-4)
- [Test 5: get_azure_subscriptions_and_resource_groups](#test-5)
- [Test 6: get_azure_subscriptions_and_resource_groups](#test-6)
- [Test 7: get_azure_subscriptions_and_resource_groups](#test-7)
- [Test 8: get_azure_app_resource_details](#test-8)
- [Test 9: get_azure_app_resource_details](#test-9)
- [Test 10: get_azure_app_resource_details](#test-10)
- [Test 11: get_azure_app_resource_details](#test-11)
- [Test 12: get_azure_app_resource_details](#test-12)
- [Test 13: get_azure_app_resource_details](#test-13)
- [Test 14: get_azure_app_resource_details](#test-14)
- [Test 15: get_azure_app_resource_details](#test-15)
- [Test 16: get_azure_app_resource_details](#test-16)
- [Test 17: get_azure_app_resource_details](#test-17)
- [Test 18: get_azure_app_resource_details](#test-18)
- [Test 19: get_azure_app_resource_details](#test-19)
- [Test 20: add_azure_app_service_database](#test-20)
- [Test 21: add_azure_app_service_database](#test-21)
- [Test 22: add_azure_app_service_database](#test-22)
- [Test 23: add_azure_app_service_database](#test-23)
- [Test 24: add_azure_app_service_database](#test-24)
- [Test 25: add_azure_app_service_database](#test-25)
- [Test 26: add_azure_app_service_database](#test-26)
- [Test 27: add_azure_app_service_database](#test-27)
- [Test 28: add_azure_app_service_database](#test-28)
- [Test 29: get_azure_databases_details](#test-29)
- [Test 30: get_azure_databases_details](#test-30)
- [Test 31: get_azure_databases_details](#test-31)
- [Test 32: get_azure_databases_details](#test-32)
- [Test 33: get_azure_databases_details](#test-33)
- [Test 34: get_azure_databases_details](#test-34)
- [Test 35: get_azure_databases_details](#test-35)
- [Test 36: get_azure_databases_details](#test-36)
- [Test 37: get_azure_databases_details](#test-37)
- [Test 38: get_azure_databases_details](#test-38)
- [Test 39: get_azure_databases_details](#test-39)
- [Test 40: get_azure_databases_details](#test-40)
- [Test 41: get_azure_databases_details](#test-41)
- [Test 42: get_azure_databases_details](#test-42)
- [Test 43: get_azure_databases_details](#test-43)
- [Test 44: get_azure_databases_details](#test-44)
- [Test 45: get_azure_databases_details](#test-45)
- [Test 46: get_azure_databases_details](#test-46)
- [Test 47: get_azure_databases_details](#test-47)
- [Test 48: get_azure_databases_details](#test-48)
- [Test 49: get_azure_databases_details](#test-49)
- [Test 50: get_azure_databases_details](#test-50)
- [Test 51: get_azure_databases_details](#test-51)
- [Test 52: get_azure_databases_details](#test-52)
- [Test 53: get_azure_databases_details](#test-53)
- [Test 54: get_azure_databases_details](#test-54)
- [Test 55: get_azure_databases_details](#test-55)
- [Test 56: get_azure_databases_details](#test-56)
- [Test 57: get_azure_databases_details](#test-57)
- [Test 58: get_azure_databases_details](#test-58)
- [Test 59: get_azure_databases_details](#test-59)
- [Test 60: get_azure_databases_details](#test-60)
- [Test 61: get_azure_databases_details](#test-61)
- [Test 62: get_azure_databases_details](#test-62)
- [Test 63: get_azure_databases_details](#test-63)
- [Test 64: get_azure_databases_details](#test-64)
- [Test 65: get_azure_databases_details](#test-65)
- [Test 66: get_azure_databases_details](#test-66)
- [Test 67: get_azure_databases_details](#test-67)
- [Test 68: create_azure_sql_databases_and_servers](#test-68)
- [Test 69: create_azure_sql_databases_and_servers](#test-69)
- [Test 70: create_azure_sql_databases_and_servers](#test-70)
- [Test 71: create_azure_sql_databases_and_servers](#test-71)
- [Test 72: create_azure_sql_databases_and_servers](#test-72)
- [Test 73: create_azure_sql_databases_and_servers](#test-73)
- [Test 74: rename_azure_sql_databases](#test-74)
- [Test 75: rename_azure_sql_databases](#test-75)
- [Test 76: edit_azure_sql_databases_and_servers](#test-76)
- [Test 77: edit_azure_sql_databases_and_servers](#test-77)
- [Test 78: edit_azure_sql_databases_and_servers](#test-78)
- [Test 79: edit_azure_sql_databases_and_servers](#test-79)
- [Test 80: edit_azure_sql_databases_and_servers](#test-80)
- [Test 81: edit_azure_sql_databases_and_servers](#test-81)
- [Test 82: edit_azure_sql_databases_and_servers](#test-82)
- [Test 83: edit_azure_sql_databases_and_servers](#test-83)
- [Test 84: edit_azure_databases](#test-84)
- [Test 85: edit_azure_databases](#test-85)
- [Test 86: get_azure_resource_and_app_health_status](#test-86)
- [Test 87: get_azure_resource_and_app_health_status](#test-87)
- [Test 88: get_azure_resource_and_app_health_status](#test-88)
- [Test 89: get_azure_resource_and_app_health_status](#test-89)
- [Test 90: get_azure_resource_and_app_health_status](#test-90)
- [Test 91: get_azure_resource_and_app_health_status](#test-91)
- [Test 92: get_azure_resource_and_app_health_status](#test-92)
- [Test 93: get_azure_resource_and_app_health_status](#test-93)
- [Test 94: get_azure_resource_and_app_health_status](#test-94)
- [Test 95: get_azure_resource_and_app_health_status](#test-95)
- [Test 96: get_azure_resource_and_app_health_status](#test-96)
- [Test 97: get_azure_resource_and_app_health_status](#test-97)
- [Test 98: get_azure_resource_and_app_health_status](#test-98)
- [Test 99: get_azure_resource_and_app_health_status](#test-99)
- [Test 100: get_azure_resource_and_app_health_status](#test-100)
- [Test 101: get_azure_resource_and_app_health_status](#test-101)
- [Test 102: get_azure_resource_and_app_health_status](#test-102)
- [Test 103: get_azure_resource_and_app_health_status](#test-103)
- [Test 104: get_azure_resource_and_app_health_status](#test-104)
- [Test 105: get_azure_resource_and_app_health_status](#test-105)
- [Test 106: get_azure_resource_and_app_health_status](#test-106)
- [Test 107: get_azure_resource_and_app_health_status](#test-107)
- [Test 108: get_azure_resource_and_app_health_status](#test-108)
- [Test 109: get_azure_resource_and_app_health_status](#test-109)
- [Test 110: get_azure_resource_and_app_health_status](#test-110)
- [Test 111: get_azure_resource_and_app_health_status](#test-111)
- [Test 112: get_azure_resource_and_app_health_status](#test-112)
- [Test 113: get_azure_resource_and_app_health_status](#test-113)
- [Test 114: get_azure_resource_and_app_health_status](#test-114)
- [Test 115: get_azure_resource_and_app_health_status](#test-115)
- [Test 116: get_azure_resource_and_app_health_status](#test-116)
- [Test 117: get_azure_resource_and_app_health_status](#test-117)
- [Test 118: get_azure_resource_and_app_health_status](#test-118)
- [Test 119: get_azure_resource_and_app_health_status](#test-119)
- [Test 120: get_azure_resource_and_app_health_status](#test-120)
- [Test 121: get_azure_resource_and_app_health_status](#test-121)
- [Test 122: get_azure_resource_and_app_health_status](#test-122)
- [Test 123: get_azure_resource_and_app_health_status](#test-123)
- [Test 124: get_azure_resource_and_app_health_status](#test-124)
- [Test 125: get_azure_resource_and_app_health_status](#test-125)
- [Test 126: get_azure_resource_and_app_health_status](#test-126)
- [Test 127: deploy_azure_resources_and_applications](#test-127)
- [Test 128: deploy_azure_resources_and_applications](#test-128)
- [Test 129: deploy_azure_resources_and_applications](#test-129)
- [Test 130: deploy_azure_resources_and_applications](#test-130)
- [Test 131: deploy_azure_ai_models](#test-131)
- [Test 132: get_azure_app_config_settings](#test-132)
- [Test 133: get_azure_app_config_settings](#test-133)
- [Test 134: get_azure_app_config_settings](#test-134)
- [Test 135: get_azure_app_config_settings](#test-135)
- [Test 136: get_azure_app_config_settings](#test-136)
- [Test 137: get_azure_app_config_settings](#test-137)
- [Test 138: get_azure_app_config_settings](#test-138)
- [Test 139: edit_azure_app_config_settings](#test-139)
- [Test 140: edit_azure_app_config_settings](#test-140)
- [Test 141: lock_unlock_azure_app_config_settings](#test-141)
- [Test 142: lock_unlock_azure_app_config_settings](#test-142)
- [Test 143: edit_azure_workbooks](#test-143)
- [Test 144: edit_azure_workbooks](#test-144)
- [Test 145: create_azure_workbooks](#test-145)
- [Test 146: get_azure_workbooks_details](#test-146)
- [Test 147: get_azure_workbooks_details](#test-147)
- [Test 148: get_azure_workbooks_details](#test-148)
- [Test 149: get_azure_workbooks_details](#test-149)
- [Test 150: audit_azure_resources_compliance](#test-150)
- [Test 151: audit_azure_resources_compliance](#test-151)
- [Test 152: audit_azure_resources_compliance](#test-152)
- [Test 153: generate_azure_cli_commands](#test-153)
- [Test 154: generate_azure_cli_commands](#test-154)
- [Test 155: generate_azure_cli_commands](#test-155)
- [Test 156: get_azure_security_configurations](#test-156)
- [Test 157: get_azure_security_configurations](#test-157)
- [Test 158: get_azure_key_vault_items](#test-158)
- [Test 159: get_azure_key_vault_items](#test-159)
- [Test 160: get_azure_key_vault_items](#test-160)
- [Test 161: get_azure_key_vault_items](#test-161)
- [Test 162: get_azure_key_vault_items](#test-162)
- [Test 163: get_azure_key_vault_items](#test-163)
- [Test 164: get_azure_key_vault_items](#test-164)
- [Test 165: get_azure_key_vault_items](#test-165)
- [Test 166: get_azure_key_vault_items](#test-166)
- [Test 167: get_azure_key_vault_items](#test-167)
- [Test 168: get_azure_key_vault_items](#test-168)
- [Test 169: get_azure_key_vault_items](#test-169)
- [Test 170: get_azure_key_vault_items](#test-170)
- [Test 171: get_azure_key_vault_items](#test-171)
- [Test 172: get_azure_key_vault_items](#test-172)
- [Test 173: get_azure_key_vault_items](#test-173)
- [Test 174: get_azure_key_vault_items](#test-174)
- [Test 175: get_azure_key_vault_items](#test-175)
- [Test 176: get_azure_key_vault_items](#test-176)
- [Test 177: get_azure_key_vault_items](#test-177)
- [Test 178: get_azure_key_vault_items](#test-178)
- [Test 179: get_azure_key_vault_items](#test-179)
- [Test 180: get_azure_key_vault_items](#test-180)
- [Test 181: get_azure_key_vault_items](#test-181)
- [Test 182: get_azure_key_vault_items](#test-182)
- [Test 183: get_azure_key_vault_items](#test-183)
- [Test 184: get_azure_key_vault_items](#test-184)
- [Test 185: get_azure_key_vault_items](#test-185)
- [Test 186: get_azure_key_vault_items](#test-186)
- [Test 187: get_azure_key_vault_items](#test-187)
- [Test 188: get_azure_key_vault_items](#test-188)
- [Test 189: get_azure_key_vault_secret_values](#test-189)
- [Test 190: get_azure_key_vault_secret_values](#test-190)
- [Test 191: get_azure_key_vault_secret_values](#test-191)
- [Test 192: get_azure_key_vault_secret_values](#test-192)
- [Test 193: get_azure_key_vault_secret_values](#test-193)
- [Test 194: create_azure_key_vault_items](#test-194)
- [Test 195: create_azure_key_vault_items](#test-195)
- [Test 196: create_azure_key_vault_items](#test-196)
- [Test 197: create_azure_key_vault_items](#test-197)
- [Test 198: create_azure_key_vault_items](#test-198)
- [Test 199: create_azure_key_vault_items](#test-199)
- [Test 200: create_azure_key_vault_items](#test-200)
- [Test 201: create_azure_key_vault_items](#test-201)
- [Test 202: create_azure_key_vault_items](#test-202)
- [Test 203: create_azure_key_vault_items](#test-203)
- [Test 204: create_azure_key_vault_secrets](#test-204)
- [Test 205: create_azure_key_vault_secrets](#test-205)
- [Test 206: create_azure_key_vault_secrets](#test-206)
- [Test 207: create_azure_key_vault_secrets](#test-207)
- [Test 208: create_azure_key_vault_secrets](#test-208)
- [Test 209: import_azure_key_vault_certificates](#test-209)
- [Test 210: import_azure_key_vault_certificates](#test-210)
- [Test 211: import_azure_key_vault_certificates](#test-211)
- [Test 212: import_azure_key_vault_certificates](#test-212)
- [Test 213: import_azure_key_vault_certificates](#test-213)
- [Test 214: get_azure_best_practices](#test-214)
- [Test 215: get_azure_best_practices](#test-215)
- [Test 216: get_azure_best_practices](#test-216)
- [Test 217: get_azure_best_practices](#test-217)
- [Test 218: get_azure_best_practices](#test-218)
- [Test 219: get_azure_best_practices](#test-219)
- [Test 220: get_azure_best_practices](#test-220)
- [Test 221: get_azure_best_practices](#test-221)
- [Test 222: get_azure_best_practices](#test-222)
- [Test 223: get_azure_best_practices](#test-223)
- [Test 224: get_azure_best_practices](#test-224)
- [Test 225: design_azure_architecture](#test-225)
- [Test 226: design_azure_architecture](#test-226)
- [Test 227: design_azure_architecture](#test-227)
- [Test 228: design_azure_architecture](#test-228)
- [Test 229: design_azure_architecture](#test-229)
- [Test 230: get_azure_load_testing_details](#test-230)
- [Test 231: get_azure_load_testing_details](#test-231)
- [Test 232: get_azure_load_testing_details](#test-232)
- [Test 233: get_azure_load_testing_details](#test-233)
- [Test 234: create_azure_load_testing](#test-234)
- [Test 235: create_azure_load_testing](#test-235)
- [Test 236: create_azure_load_testing](#test-236)
- [Test 237: update_azure_load_testing_configurations](#test-237)
- [Test 238: get_azure_ai_resources_details](#test-238)
- [Test 239: get_azure_ai_resources_details](#test-239)
- [Test 240: get_azure_ai_resources_details](#test-240)
- [Test 241: get_azure_ai_resources_details](#test-241)
- [Test 242: get_azure_ai_resources_details](#test-242)
- [Test 243: get_azure_ai_resources_details](#test-243)
- [Test 244: get_azure_ai_resources_details](#test-244)
- [Test 245: get_azure_ai_resources_details](#test-245)
- [Test 246: get_azure_ai_resources_details](#test-246)
- [Test 247: get_azure_ai_resources_details](#test-247)
- [Test 248: get_azure_ai_resources_details](#test-248)
- [Test 249: get_azure_ai_resources_details](#test-249)
- [Test 250: get_azure_ai_resources_details](#test-250)
- [Test 251: get_azure_ai_resources_details](#test-251)
- [Test 252: get_azure_ai_resources_details](#test-252)
- [Test 253: get_azure_ai_resources_details](#test-253)
- [Test 254: get_azure_ai_resources_details](#test-254)
- [Test 255: get_azure_ai_resources_details](#test-255)
- [Test 256: get_azure_ai_resources_details](#test-256)
- [Test 257: get_azure_ai_resources_details](#test-257)
- [Test 258: get_azure_ai_resources_details](#test-258)
- [Test 259: get_azure_ai_resources_details](#test-259)
- [Test 260: get_azure_ai_resources_details](#test-260)
- [Test 261: get_azure_ai_resources_details](#test-261)
- [Test 262: get_azure_ai_resources_details](#test-262)
- [Test 263: get_azure_ai_resources_details](#test-263)
- [Test 264: get_azure_ai_resources_details](#test-264)
- [Test 265: get_azure_ai_resources_details](#test-265)
- [Test 266: get_azure_ai_resources_details](#test-266)
- [Test 267: get_azure_ai_resources_details](#test-267)
- [Test 268: get_azure_ai_resources_details](#test-268)
- [Test 269: get_azure_ai_resources_details](#test-269)
- [Test 270: get_azure_ai_resources_details](#test-270)
- [Test 271: get_azure_ai_resources_details](#test-271)
- [Test 272: retrieve_azure_ai_knowledge_base_content](#test-272)
- [Test 273: retrieve_azure_ai_knowledge_base_content](#test-273)
- [Test 274: retrieve_azure_ai_knowledge_base_content](#test-274)
- [Test 275: retrieve_azure_ai_knowledge_base_content](#test-275)
- [Test 276: retrieve_azure_ai_knowledge_base_content](#test-276)
- [Test 277: retrieve_azure_ai_knowledge_base_content](#test-277)
- [Test 278: retrieve_azure_ai_knowledge_base_content](#test-278)
- [Test 279: use_azure_openai_models](#test-279)
- [Test 280: use_azure_openai_models](#test-280)
- [Test 281: use_azure_openai_models](#test-281)
- [Test 282: use_azure_openai_models](#test-282)
- [Test 283: connect_azure_ai_foundry_agents](#test-283)
- [Test 284: query_and_evaluate_azure_ai_foundry_agents](#test-284)
- [Test 285: evaluate_azure_ai_foundry_agents](#test-285)
- [Test 286: get_azure_storage_details](#test-286)
- [Test 287: get_azure_storage_details](#test-287)
- [Test 288: get_azure_storage_details](#test-288)
- [Test 289: get_azure_storage_details](#test-289)
- [Test 290: get_azure_storage_details](#test-290)
- [Test 291: get_azure_storage_details](#test-291)
- [Test 292: get_azure_storage_details](#test-292)
- [Test 293: get_azure_storage_details](#test-293)
- [Test 294: get_azure_storage_details](#test-294)
- [Test 295: get_azure_storage_details](#test-295)
- [Test 296: get_azure_storage_details](#test-296)
- [Test 297: get_azure_storage_details](#test-297)
- [Test 298: get_azure_storage_details](#test-298)
- [Test 299: get_azure_storage_details](#test-299)
- [Test 300: get_azure_storage_details](#test-300)
- [Test 301: get_azure_storage_details](#test-301)
- [Test 302: get_azure_storage_details](#test-302)
- [Test 303: create_azure_storage](#test-303)
- [Test 304: create_azure_storage](#test-304)
- [Test 305: create_azure_storage](#test-305)
- [Test 306: create_azure_storage](#test-306)
- [Test 307: create_azure_storage](#test-307)
- [Test 308: create_azure_storage](#test-308)
- [Test 309: create_azure_storage](#test-309)
- [Test 310: update_azure_managed_lustre_filesystems](#test-310)
- [Test 311: upload_azure_storage_blobs](#test-311)
- [Test 312: browse_azure_marketplace_products](#test-312)
- [Test 313: browse_azure_marketplace_products](#test-313)
- [Test 314: browse_azure_marketplace_products](#test-314)
- [Test 315: get_azure_capacity](#test-315)
- [Test 316: get_azure_capacity](#test-316)
- [Test 317: get_azure_messaging_service_details](#test-317)
- [Test 318: get_azure_messaging_service_details](#test-318)
- [Test 319: get_azure_messaging_service_details](#test-319)
- [Test 320: get_azure_messaging_service_details](#test-320)
- [Test 321: get_azure_messaging_service_details](#test-321)
- [Test 322: get_azure_messaging_service_details](#test-322)
- [Test 323: get_azure_messaging_service_details](#test-323)
- [Test 324: get_azure_messaging_service_details](#test-324)
- [Test 325: get_azure_messaging_service_details](#test-325)
- [Test 326: get_azure_messaging_service_details](#test-326)
- [Test 327: get_azure_messaging_service_details](#test-327)
- [Test 328: get_azure_messaging_service_details](#test-328)
- [Test 329: get_azure_messaging_service_details](#test-329)
- [Test 330: get_azure_messaging_service_details](#test-330)
- [Test 331: get_azure_messaging_service_details](#test-331)
- [Test 332: get_azure_messaging_service_details](#test-332)
- [Test 333: get_azure_messaging_service_details](#test-333)
- [Test 334: get_azure_messaging_service_details](#test-334)
- [Test 335: get_azure_messaging_service_details](#test-335)
- [Test 336: get_azure_messaging_service_details](#test-336)
- [Test 337: edit_azure_data_analytics_resources](#test-337)
- [Test 338: edit_azure_data_analytics_resources](#test-338)
- [Test 339: edit_azure_data_analytics_resources](#test-339)
- [Test 340: edit_azure_data_analytics_resources](#test-340)
- [Test 341: edit_azure_data_analytics_resources](#test-341)
- [Test 342: edit_azure_data_analytics_resources](#test-342)
- [Test 343: edit_azure_data_analytics_resources](#test-343)
- [Test 344: edit_azure_data_analytics_resources](#test-344)
- [Test 345: edit_azure_data_analytics_resources](#test-345)
- [Test 346: publish_azure_eventgrid_events](#test-346)
- [Test 347: publish_azure_eventgrid_events](#test-347)
- [Test 348: publish_azure_eventgrid_events](#test-348)
- [Test 349: get_azure_data_explorer_kusto_details](#test-349)
- [Test 350: get_azure_data_explorer_kusto_details](#test-350)
- [Test 351: get_azure_data_explorer_kusto_details](#test-351)
- [Test 352: get_azure_data_explorer_kusto_details](#test-352)
- [Test 353: get_azure_data_explorer_kusto_details](#test-353)
- [Test 354: get_azure_data_explorer_kusto_details](#test-354)
- [Test 355: get_azure_data_explorer_kusto_details](#test-355)
- [Test 356: get_azure_data_explorer_kusto_details](#test-356)
- [Test 357: get_azure_data_explorer_kusto_details](#test-357)
- [Test 358: get_azure_data_explorer_kusto_details](#test-358)
- [Test 359: get_azure_data_explorer_kusto_details](#test-359)
- [Test 360: create_azure_database_admin_configurations](#test-360)
- [Test 361: create_azure_database_admin_configurations](#test-361)
- [Test 362: create_azure_database_admin_configurations](#test-362)
- [Test 363: delete_azure_database_admin_configurations](#test-363)
- [Test 364: delete_azure_database_admin_configurations](#test-364)
- [Test 365: delete_azure_database_admin_configurations](#test-365)
- [Test 366: get_azure_database_admin_configuration_details](#test-366)
- [Test 367: get_azure_database_admin_configuration_details](#test-367)
- [Test 368: get_azure_database_admin_configuration_details](#test-368)
- [Test 369: get_azure_database_admin_configuration_details](#test-369)
- [Test 370: get_azure_database_admin_configuration_details](#test-370)
- [Test 371: get_azure_database_admin_configuration_details](#test-371)
- [Test 372: get_azure_database_admin_configuration_details](#test-372)
- [Test 373: get_azure_database_admin_configuration_details](#test-373)
- [Test 374: get_azure_database_admin_configuration_details](#test-374)
- [Test 375: get_azure_container_details](#test-375)
- [Test 376: get_azure_container_details](#test-376)
- [Test 377: get_azure_container_details](#test-377)
- [Test 378: get_azure_container_details](#test-378)
- [Test 379: get_azure_container_details](#test-379)
- [Test 380: get_azure_container_details](#test-380)
- [Test 381: get_azure_container_details](#test-381)
- [Test 382: get_azure_container_details](#test-382)
- [Test 383: get_azure_container_details](#test-383)
- [Test 384: get_azure_container_details](#test-384)
- [Test 385: get_azure_container_details](#test-385)
- [Test 386: get_azure_container_details](#test-386)
- [Test 387: get_azure_container_details](#test-387)
- [Test 388: get_azure_container_details](#test-388)
- [Test 389: get_azure_container_details](#test-389)
- [Test 390: get_azure_container_details](#test-390)
- [Test 391: get_azure_container_details](#test-391)
- [Test 392: get_azure_container_details](#test-392)
- [Test 393: get_azure_container_details](#test-393)
- [Test 394: get_azure_container_details](#test-394)
- [Test 395: get_azure_container_details](#test-395)
- [Test 396: get_azure_container_details](#test-396)
- [Test 397: get_azure_virtual_desktop_details](#test-397)
- [Test 398: get_azure_virtual_desktop_details](#test-398)
- [Test 399: get_azure_virtual_desktop_details](#test-399)
- [Test 400: get_azure_signalr_details](#test-400)
- [Test 401: get_azure_signalr_details](#test-401)
- [Test 402: get_azure_signalr_details](#test-402)
- [Test 403: get_azure_signalr_details](#test-403)
- [Test 404: get_azure_signalr_details](#test-404)
- [Test 405: get_azure_signalr_details](#test-405)
- [Test 406: get_azure_confidential_ledger_entries](#test-406)
- [Test 407: get_azure_confidential_ledger_entries](#test-407)
- [Test 408: append_azure_confidential_ledger_entries](#test-408)
- [Test 409: append_azure_confidential_ledger_entries](#test-409)
- [Test 410: append_azure_confidential_ledger_entries](#test-410)
- [Test 411: append_azure_confidential_ledger_entries](#test-411)
- [Test 412: append_azure_confidential_ledger_entries](#test-412)
- [Test 413: send_azure_communication_messages](#test-413)
- [Test 414: send_azure_communication_messages](#test-414)
- [Test 415: send_azure_communication_messages](#test-415)
- [Test 416: send_azure_communication_messages](#test-416)
- [Test 417: send_azure_communication_messages](#test-417)
- [Test 418: send_azure_communication_messages](#test-418)
- [Test 419: send_azure_communication_messages](#test-419)
- [Test 420: send_azure_communication_messages](#test-420)
- [Test 421: send_azure_communication_messages](#test-421)
- [Test 422: send_azure_communication_messages](#test-422)
- [Test 423: send_azure_communication_messages](#test-423)
- [Test 424: send_azure_communication_messages](#test-424)
- [Test 425: send_azure_communication_messages](#test-425)
- [Test 426: send_azure_communication_messages](#test-426)
- [Test 427: send_azure_communication_messages](#test-427)
- [Test 428: send_azure_communication_messages](#test-428)
- [Test 429: recognize_speech_from_audio](#test-429)
- [Test 430: recognize_speech_from_audio](#test-430)
- [Test 431: recognize_speech_from_audio](#test-431)
- [Test 432: recognize_speech_from_audio](#test-432)
- [Test 433: recognize_speech_from_audio](#test-433)
- [Test 434: recognize_speech_from_audio](#test-434)
- [Test 435: recognize_speech_from_audio](#test-435)
- [Test 436: recognize_speech_from_audio](#test-436)
- [Test 437: recognize_speech_from_audio](#test-437)
- [Test 438: recognize_speech_from_audio](#test-438)

---

## Test 1

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** List all resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638889 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.420089 | `get_azure_security_configurations` | ❌ |
| 3 | 0.401415 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.384567 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.375987 | `get_azure_capacity` | ❌ |

---

## Test 2

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.415793 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.328705 | `get_azure_security_configurations` | ❌ |
| 3 | 0.317407 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.313066 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.285674 | `get_azure_storage_details` | ❌ |

---

## Test 3

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549609 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.418812 | `get_azure_security_configurations` | ❌ |
| 3 | 0.370452 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.366696 | `get_azure_capacity` | ❌ |
| 5 | 0.364712 | `get_azure_load_testing_details` | ❌ |

---

## Test 4

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.347561 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.278264 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.242552 | `get_azure_security_configurations` | ❌ |
| 4 | 0.238929 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.189155 | `get_azure_container_details` | ❌ |

---

## Test 5

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671045 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.437174 | `get_azure_security_configurations` | ❌ |
| 3 | 0.423039 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.407266 | `get_azure_capacity` | ❌ |
| 5 | 0.381286 | `get_azure_load_testing_details` | ❌ |

---

## Test 6

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.322378 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.246134 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.234029 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.214599 | `get_azure_capacity` | ❌ |
| 5 | 0.210046 | `get_azure_signalr_details` | ❌ |

---

## Test 7

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.380158 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.303497 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.274746 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.248135 | `get_azure_capacity` | ❌ |
| 5 | 0.239796 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 8

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567577 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.464896 | `deploy_azure_resources_and_applications` | ❌ |
| 3 | 0.438587 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.414459 | `get_azure_resource_and_app_health_status` | ❌ |
| 5 | 0.391945 | `generate_azure_cli_commands` | ❌ |

---

## Test 9

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622593 | `get_azure_app_config_settings` | ❌ |
| 2 | 0.565172 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 3 | 0.480175 | `lock_unlock_azure_app_config_settings` | ❌ |
| 4 | 0.434907 | `get_azure_best_practices` | ❌ |
| 5 | 0.415185 | `edit_azure_app_config_settings` | ❌ |

---

## Test 10

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551165 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.444696 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.412107 | `get_azure_resource_and_app_health_status` | ❌ |
| 4 | 0.345823 | `get_azure_signalr_details` | ❌ |
| 5 | 0.345466 | `deploy_azure_resources_and_applications` | ❌ |

---

## Test 11

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606770 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.516861 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.498775 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.468858 | `get_azure_signalr_details` | ❌ |
| 5 | 0.429110 | `get_azure_messaging_service_details` | ❌ |

---

## Test 12

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.558485 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.427410 | `get_azure_security_configurations` | ❌ |
| 3 | 0.421965 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.420740 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.409842 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 13

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545132 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.462941 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.432003 | `get_azure_resource_and_app_health_status` | ❌ |
| 4 | 0.396163 | `get_azure_signalr_details` | ❌ |
| 5 | 0.383867 | `deploy_azure_resources_and_applications` | ❌ |

---

## Test 14

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630201 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.514721 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.434995 | `get_azure_signalr_details` | ❌ |
| 4 | 0.430445 | `deploy_azure_resources_and_applications` | ❌ |
| 5 | 0.417338 | `get_azure_messaging_service_details` | ❌ |

---

## Test 15

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560507 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.469871 | `deploy_azure_resources_and_applications` | ❌ |
| 3 | 0.462610 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.444987 | `get_azure_security_configurations` | ❌ |
| 5 | 0.437162 | `get_azure_app_config_settings` | ❌ |

---

## Test 16

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650735 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.570557 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.467885 | `get_azure_signalr_details` | ❌ |
| 4 | 0.427326 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.394452 | `deploy_azure_resources_and_applications` | ❌ |

---

## Test 17

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.534810 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.433102 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.428962 | `deploy_azure_resources_and_applications` | ❌ |
| 4 | 0.415746 | `get_azure_capacity` | ❌ |
| 5 | 0.390919 | `get_azure_best_practices` | ❌ |

---

## Test 18

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.416096 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.320318 | `get_azure_resource_and_app_health_status` | ❌ |
| 3 | 0.305243 | `deploy_azure_resources_and_applications` | ❌ |
| 4 | 0.269663 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.259482 | `generate_azure_cli_commands` | ❌ |

---

## Test 19

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547132 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.435720 | `get_azure_resource_and_app_health_status` | ❌ |
| 3 | 0.419467 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.388259 | `deploy_azure_resources_and_applications` | ❌ |
| 5 | 0.385260 | `get_azure_signalr_details` | ❌ |

---

## Test 20

**Expected Tool:** `add_azure_app_service_database`  
**Prompt:** Add a CosmosDB database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645526 | `add_azure_app_service_database` | ✅ **EXPECTED** |
| 2 | 0.460200 | `get_azure_databases_details` | ❌ |
| 3 | 0.441681 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.441557 | `edit_azure_databases` | ❌ |
| 5 | 0.429157 | `create_azure_sql_databases_and_servers` | ❌ |

---

## Test 21

**Expected Tool:** `add_azure_app_service_database`  
**Prompt:** Add a database connection to my app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604807 | `add_azure_app_service_database` | ✅ **EXPECTED** |
| 2 | 0.399204 | `edit_azure_databases` | ❌ |
| 3 | 0.374759 | `create_azure_sql_databases_and_servers` | ❌ |
| 4 | 0.371454 | `rename_azure_sql_databases` | ❌ |
| 5 | 0.339497 | `get_azure_databases_details` | ❌ |

---

## Test 22

**Expected Tool:** `add_azure_app_service_database`  
**Prompt:** Add a MySQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662946 | `add_azure_app_service_database` | ✅ **EXPECTED** |
| 2 | 0.510959 | `edit_azure_databases` | ❌ |
| 3 | 0.461314 | `get_azure_databases_details` | ❌ |
| 4 | 0.409706 | `rename_azure_sql_databases` | ❌ |
| 5 | 0.404462 | `create_azure_sql_databases_and_servers` | ❌ |

---

## Test 23

**Expected Tool:** `add_azure_app_service_database`  
**Prompt:** Add a PostgreSQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607029 | `add_azure_app_service_database` | ✅ **EXPECTED** |
| 2 | 0.488067 | `edit_azure_databases` | ❌ |
| 3 | 0.423578 | `get_azure_databases_details` | ❌ |
| 4 | 0.352923 | `create_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.345103 | `rename_azure_sql_databases` | ❌ |

---

## Test 24

**Expected Tool:** `add_azure_app_service_database`  
**Prompt:** Add database <database_name> on server <database_server> to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612095 | `add_azure_app_service_database` | ✅ **EXPECTED** |
| 2 | 0.488180 | `rename_azure_sql_databases` | ❌ |
| 3 | 0.455573 | `create_azure_sql_databases_and_servers` | ❌ |
| 4 | 0.445556 | `edit_azure_databases` | ❌ |
| 5 | 0.436063 | `get_azure_databases_details` | ❌ |

---

## Test 25

**Expected Tool:** `add_azure_app_service_database`  
**Prompt:** Add database <database_name> with retry policy to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550372 | `add_azure_app_service_database` | ✅ **EXPECTED** |
| 2 | 0.418395 | `rename_azure_sql_databases` | ❌ |
| 3 | 0.398384 | `create_azure_sql_databases_and_servers` | ❌ |
| 4 | 0.380948 | `edit_azure_databases` | ❌ |
| 5 | 0.359762 | `get_azure_databases_details` | ❌ |

---

## Test 26

**Expected Tool:** `add_azure_app_service_database`  
**Prompt:** Configure a SQL Server database for app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.637234 | `add_azure_app_service_database` | ✅ **EXPECTED** |
| 2 | 0.518406 | `create_azure_sql_databases_and_servers` | ❌ |
| 3 | 0.512464 | `edit_azure_databases` | ❌ |
| 4 | 0.476589 | `rename_azure_sql_databases` | ❌ |
| 5 | 0.468705 | `edit_azure_sql_databases_and_servers` | ❌ |

---

## Test 27

**Expected Tool:** `add_azure_app_service_database`  
**Prompt:** Configure tenant <tenant> for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.539469 | `add_azure_app_service_database` | ✅ **EXPECTED** |
| 2 | 0.446249 | `rename_azure_sql_databases` | ❌ |
| 3 | 0.427201 | `edit_azure_databases` | ❌ |
| 4 | 0.397353 | `lock_unlock_azure_app_config_settings` | ❌ |
| 5 | 0.395684 | `create_azure_sql_databases_and_servers` | ❌ |

---

## Test 28

**Expected Tool:** `add_azure_app_service_database`  
**Prompt:** Set connection string for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543922 | `add_azure_app_service_database` | ✅ **EXPECTED** |
| 2 | 0.468293 | `edit_azure_databases` | ❌ |
| 3 | 0.428196 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.395735 | `lock_unlock_azure_app_config_settings` | ❌ |
| 5 | 0.366333 | `get_azure_databases_details` | ❌ |

---

## Test 29

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.394601 | `get_azure_database_admin_configuration_details` | ❌ |
| 2 | 0.378321 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 3 | 0.346100 | `edit_azure_databases` | ❌ |
| 4 | 0.321543 | `create_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.313666 | `edit_azure_sql_databases_and_servers` | ❌ |

---

## Test 30

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.515990 | `get_azure_database_admin_configuration_details` | ❌ |
| 2 | 0.515480 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.369218 | `edit_azure_databases` | ❌ |
| 4 | 0.349838 | `edit_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.348994 | `create_azure_sql_databases_and_servers` | ❌ |

---

## Test 31

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478394 | `get_azure_database_admin_configuration_details` | ❌ |
| 2 | 0.449240 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.378667 | `edit_azure_sql_databases_and_servers` | ❌ |
| 4 | 0.375171 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 5 | 0.373528 | `edit_azure_databases` | ❌ |

---

## Test 32

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all Azure SQL servers in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.437870 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.435676 | `create_azure_sql_databases_and_servers` | ❌ |
| 3 | 0.424056 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.423838 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 5 | 0.422579 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 33

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.483707 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.470240 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.450588 | `get_azure_storage_details` | ❌ |
| 4 | 0.439142 | `get_azure_security_configurations` | ❌ |
| 5 | 0.421076 | `get_azure_messaging_service_details` | ❌ |

---

## Test 34

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550050 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.538253 | `rename_azure_sql_databases` | ❌ |
| 3 | 0.449947 | `create_azure_sql_databases_and_servers` | ❌ |
| 4 | 0.447448 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.446772 | `edit_azure_sql_databases_and_servers` | ❌ |

---

## Test 35

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.459813 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.353887 | `edit_azure_databases` | ❌ |
| 3 | 0.334742 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.272054 | `edit_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.255147 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 36

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496395 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.418769 | `edit_azure_databases` | ❌ |
| 3 | 0.335259 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.322093 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.316396 | `edit_azure_sql_databases_and_servers` | ❌ |

---

## Test 37

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.411217 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.321958 | `edit_azure_databases` | ❌ |
| 3 | 0.293602 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.243209 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.234938 | `edit_azure_sql_databases_and_servers` | ❌ |

---

## Test 38

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.455154 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.397754 | `edit_azure_databases` | ❌ |
| 3 | 0.341439 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.319877 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.298417 | `create_azure_sql_databases_and_servers` | ❌ |

---

## Test 39

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.416288 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.319283 | `edit_azure_databases` | ❌ |
| 3 | 0.284157 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.253452 | `edit_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.224061 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 40

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.375280 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.295866 | `edit_azure_databases` | ❌ |
| 3 | 0.247094 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.217705 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.215628 | `edit_azure_sql_databases_and_servers` | ❌ |

---

## Test 41

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485364 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.410568 | `get_azure_storage_details` | ❌ |
| 3 | 0.393277 | `get_azure_container_details` | ❌ |
| 4 | 0.379221 | `create_azure_storage` | ❌ |
| 5 | 0.331050 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 42

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521537 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.403937 | `rename_azure_sql_databases` | ❌ |
| 3 | 0.372579 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.353114 | `get_azure_storage_details` | ❌ |
| 5 | 0.340541 | `get_azure_security_configurations` | ❌ |

---

## Test 43

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.400716 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.313467 | `edit_azure_databases` | ❌ |
| 3 | 0.301000 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 4 | 0.252886 | `get_azure_ai_resources_details` | ❌ |
| 5 | 0.246393 | `edit_azure_sql_databases_and_servers` | ❌ |

---

## Test 44

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.357501 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.282346 | `edit_azure_databases` | ❌ |
| 3 | 0.276540 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 4 | 0.230739 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.228187 | `get_azure_ai_resources_details` | ❌ |

---

## Test 45

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.558366 | `get_azure_database_admin_configuration_details` | ❌ |
| 2 | 0.530329 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 3 | 0.486536 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.468156 | `edit_azure_databases` | ❌ |
| 5 | 0.463670 | `create_azure_sql_databases_and_servers` | ❌ |

---

## Test 46

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me every SQL server available in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.459302 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.424000 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.422684 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 4 | 0.409588 | `create_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.396496 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 47

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.318903 | `edit_azure_databases` | ❌ |
| 2 | 0.251389 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 3 | 0.215877 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.168314 | `get_azure_signalr_details` | ❌ |
| 5 | 0.165104 | `create_azure_database_admin_configurations` | ❌ |

---

## Test 48

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.494153 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.423273 | `get_azure_storage_details` | ❌ |
| 3 | 0.421777 | `get_azure_security_configurations` | ❌ |
| 4 | 0.389268 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.388560 | `get_azure_container_details` | ❌ |

---

## Test 49

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.404968 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.357185 | `edit_azure_databases` | ❌ |
| 3 | 0.274336 | `create_azure_sql_databases_and_servers` | ❌ |
| 4 | 0.270884 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.264802 | `edit_azure_sql_databases_and_servers` | ❌ |

---

## Test 50

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.380591 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.354214 | `edit_azure_databases` | ❌ |
| 3 | 0.274990 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.253387 | `create_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.240477 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 51

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the configuration of MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.424259 | `edit_azure_databases` | ❌ |
| 2 | 0.345881 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 3 | 0.318976 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.282061 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.252978 | `edit_azure_sql_databases_and_servers` | ❌ |

---

## Test 52

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.396395 | `edit_azure_databases` | ❌ |
| 2 | 0.305000 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.302014 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 4 | 0.250062 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.214600 | `update_azure_managed_lustre_filesystems` | ❌ |

---

## Test 53

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.467713 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.380264 | `get_azure_storage_details` | ❌ |
| 3 | 0.377338 | `get_azure_container_details` | ❌ |
| 4 | 0.351073 | `create_azure_storage` | ❌ |
| 5 | 0.325368 | `rename_azure_sql_databases` | ❌ |

---

## Test 54

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488113 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.481986 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 3 | 0.458807 | `get_azure_storage_details` | ❌ |
| 4 | 0.444953 | `get_azure_security_configurations` | ❌ |
| 5 | 0.431831 | `get_azure_messaging_service_details` | ❌ |

---

## Test 55

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496823 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.380341 | `rename_azure_sql_databases` | ❌ |
| 3 | 0.368375 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.340381 | `get_azure_storage_details` | ❌ |
| 5 | 0.336643 | `add_azure_app_service_database` | ❌ |

---

## Test 56

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549599 | `get_azure_database_admin_configuration_details` | ❌ |
| 2 | 0.500933 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.493713 | `get_azure_signalr_details` | ❌ |
| 4 | 0.490565 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.489035 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 57

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.430729 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.415495 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.354815 | `edit_azure_databases` | ❌ |
| 4 | 0.343500 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.336660 | `rename_azure_sql_databases` | ❌ |

---

## Test 58

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.425531 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.418012 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 3 | 0.380456 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.349347 | `get_azure_storage_details` | ❌ |
| 5 | 0.346397 | `get_azure_container_details` | ❌ |

---

## Test 59

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.443295 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.358413 | `edit_azure_databases` | ❌ |
| 3 | 0.322639 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.287308 | `edit_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.277441 | `create_azure_sql_databases_and_servers` | ❌ |

---

## Test 60

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.511745 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.461477 | `edit_azure_databases` | ❌ |
| 3 | 0.365358 | `create_azure_sql_databases_and_servers` | ❌ |
| 4 | 0.364272 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.353379 | `browse_azure_marketplace_products` | ❌ |

---

## Test 61

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.406462 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.332482 | `edit_azure_databases` | ❌ |
| 3 | 0.277752 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.247838 | `edit_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.245490 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 62

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469057 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.433648 | `edit_azure_databases` | ❌ |
| 3 | 0.360364 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.343503 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.339133 | `browse_azure_marketplace_products` | ❌ |

---

## Test 63

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.383189 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.314466 | `edit_azure_databases` | ❌ |
| 3 | 0.225610 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.224830 | `rename_azure_sql_databases` | ❌ |
| 5 | 0.221419 | `get_azure_best_practices` | ❌ |

---

## Test 64

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.344385 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.284782 | `edit_azure_databases` | ❌ |
| 3 | 0.214431 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.201470 | `rename_azure_sql_databases` | ❌ |
| 5 | 0.200118 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 65

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.421279 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.335190 | `edit_azure_databases` | ❌ |
| 3 | 0.262481 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.256053 | `edit_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.248928 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 66

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.375595 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.310370 | `edit_azure_databases` | ❌ |
| 3 | 0.230608 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.225953 | `rename_azure_sql_databases` | ❌ |
| 5 | 0.223967 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 67

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.364740 | `edit_azure_databases` | ❌ |
| 2 | 0.238900 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 3 | 0.202957 | `delete_azure_database_admin_configurations` | ❌ |
| 4 | 0.202122 | `create_azure_database_admin_configurations` | ❌ |
| 5 | 0.192344 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 68

**Expected Tool:** `create_azure_sql_databases_and_servers`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.544634 | `create_azure_sql_databases_and_servers` | ✅ **EXPECTED** |
| 2 | 0.496642 | `rename_azure_sql_databases` | ❌ |
| 3 | 0.474577 | `create_azure_database_admin_configurations` | ❌ |
| 4 | 0.398467 | `delete_azure_database_admin_configurations` | ❌ |
| 5 | 0.395746 | `create_azure_key_vault_secrets` | ❌ |

---

## Test 69

**Expected Tool:** `create_azure_sql_databases_and_servers`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520646 | `rename_azure_sql_databases` | ❌ |
| 2 | 0.513516 | `create_azure_sql_databases_and_servers` | ✅ **EXPECTED** |
| 3 | 0.384104 | `add_azure_app_service_database` | ❌ |
| 4 | 0.381160 | `edit_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.376755 | `create_azure_database_admin_configurations` | ❌ |

---

## Test 70

**Expected Tool:** `create_azure_sql_databases_and_servers`  
**Prompt:** Create a new SQL database named <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492741 | `rename_azure_sql_databases` | ❌ |
| 2 | 0.448210 | `create_azure_sql_databases_and_servers` | ✅ **EXPECTED** |
| 3 | 0.371279 | `edit_azure_sql_databases_and_servers` | ❌ |
| 4 | 0.349862 | `create_azure_database_admin_configurations` | ❌ |
| 5 | 0.339719 | `edit_azure_databases` | ❌ |

---

## Test 71

**Expected Tool:** `create_azure_sql_databases_and_servers`  
**Prompt:** Create a SQL database <database_name> with Basic tier in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586664 | `create_azure_sql_databases_and_servers` | ✅ **EXPECTED** |
| 2 | 0.448354 | `rename_azure_sql_databases` | ❌ |
| 3 | 0.429547 | `edit_azure_sql_databases_and_servers` | ❌ |
| 4 | 0.414021 | `edit_azure_databases` | ❌ |
| 5 | 0.390204 | `add_azure_app_service_database` | ❌ |

---

## Test 72

**Expected Tool:** `create_azure_sql_databases_and_servers`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502365 | `create_azure_sql_databases_and_servers` | ✅ **EXPECTED** |
| 2 | 0.468619 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.424808 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.411480 | `edit_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.411012 | `rename_azure_sql_databases` | ❌ |

---

## Test 73

**Expected Tool:** `create_azure_sql_databases_and_servers`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545618 | `create_azure_sql_databases_and_servers` | ✅ **EXPECTED** |
| 2 | 0.485689 | `rename_azure_sql_databases` | ❌ |
| 3 | 0.442793 | `create_azure_database_admin_configurations` | ❌ |
| 4 | 0.407416 | `edit_azure_databases` | ❌ |
| 5 | 0.391741 | `add_azure_app_service_database` | ❌ |

---

## Test 74

**Expected Tool:** `rename_azure_sql_databases`  
**Prompt:** Rename my Azure SQL database <database_name> to <new_database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.794632 | `rename_azure_sql_databases` | ✅ **EXPECTED** |
| 2 | 0.541921 | `edit_azure_sql_databases_and_servers` | ❌ |
| 3 | 0.505707 | `edit_azure_databases` | ❌ |
| 4 | 0.475553 | `create_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.437481 | `create_azure_database_admin_configurations` | ❌ |

---

## Test 75

**Expected Tool:** `rename_azure_sql_databases`  
**Prompt:** Rename the SQL database <database_name> on server <server_name> to <new_database_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.689327 | `rename_azure_sql_databases` | ✅ **EXPECTED** |
| 2 | 0.436705 | `edit_azure_sql_databases_and_servers` | ❌ |
| 3 | 0.396117 | `edit_azure_databases` | ❌ |
| 4 | 0.358714 | `create_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.323652 | `get_azure_databases_details` | ❌ |

---

## Test 76

**Expected Tool:** `edit_azure_sql_databases_and_servers`  
**Prompt:** Delete SQL server <server_name> permanently  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.477760 | `edit_azure_sql_databases_and_servers` | ✅ **EXPECTED** |
| 2 | 0.442463 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.353822 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.350061 | `create_azure_database_admin_configurations` | ❌ |
| 5 | 0.314143 | `edit_azure_databases` | ❌ |

---

## Test 77

**Expected Tool:** `edit_azure_sql_databases_and_servers`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530932 | `delete_azure_database_admin_configurations` | ❌ |
| 2 | 0.524407 | `edit_azure_sql_databases_and_servers` | ✅ **EXPECTED** |
| 3 | 0.470752 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.453689 | `create_azure_database_admin_configurations` | ❌ |
| 5 | 0.417277 | `edit_azure_data_analytics_resources` | ❌ |

---

## Test 78

**Expected Tool:** `edit_azure_sql_databases_and_servers`  
**Prompt:** Delete the database called <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.425106 | `edit_azure_sql_databases_and_servers` | ✅ **EXPECTED** |
| 2 | 0.402144 | `rename_azure_sql_databases` | ❌ |
| 3 | 0.303504 | `get_azure_databases_details` | ❌ |
| 4 | 0.292939 | `edit_azure_databases` | ❌ |
| 5 | 0.285850 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 79

**Expected Tool:** `edit_azure_sql_databases_and_servers`  
**Prompt:** Delete the SQL database <database_name> from server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480068 | `edit_azure_sql_databases_and_servers` | ✅ **EXPECTED** |
| 2 | 0.441425 | `rename_azure_sql_databases` | ❌ |
| 3 | 0.354135 | `delete_azure_database_admin_configurations` | ❌ |
| 4 | 0.316716 | `edit_azure_databases` | ❌ |
| 5 | 0.312687 | `get_azure_databases_details` | ❌ |

---

## Test 80

**Expected Tool:** `edit_azure_sql_databases_and_servers`  
**Prompt:** Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.479510 | `rename_azure_sql_databases` | ❌ |
| 2 | 0.465178 | `edit_azure_sql_databases_and_servers` | ✅ **EXPECTED** |
| 3 | 0.420526 | `delete_azure_database_admin_configurations` | ❌ |
| 4 | 0.361867 | `edit_azure_data_analytics_resources` | ❌ |
| 5 | 0.356097 | `create_azure_sql_databases_and_servers` | ❌ |

---

## Test 81

**Expected Tool:** `edit_azure_sql_databases_and_servers`  
**Prompt:** Remove the SQL server <server_name> from my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.466825 | `delete_azure_database_admin_configurations` | ❌ |
| 2 | 0.450380 | `edit_azure_sql_databases_and_servers` | ✅ **EXPECTED** |
| 3 | 0.395039 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.393448 | `edit_azure_databases` | ❌ |
| 5 | 0.370137 | `create_azure_database_admin_configurations` | ❌ |

---

## Test 82

**Expected Tool:** `edit_azure_sql_databases_and_servers`  
**Prompt:** Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.447543 | `rename_azure_sql_databases` | ❌ |
| 2 | 0.446459 | `edit_azure_databases` | ❌ |
| 3 | 0.444263 | `edit_azure_sql_databases_and_servers` | ✅ **EXPECTED** |
| 4 | 0.434377 | `create_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.348517 | `get_azure_databases_details` | ❌ |

---

## Test 83

**Expected Tool:** `edit_azure_sql_databases_and_servers`  
**Prompt:** Update the performance tier of SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595423 | `create_azure_sql_databases_and_servers` | ❌ |
| 2 | 0.550251 | `edit_azure_sql_databases_and_servers` | ✅ **EXPECTED** |
| 3 | 0.515796 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.506574 | `edit_azure_databases` | ❌ |
| 5 | 0.372785 | `get_azure_databases_details` | ❌ |

---

## Test 84

**Expected Tool:** `edit_azure_databases`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.340843 | `edit_azure_databases` | ✅ **EXPECTED** |
| 2 | 0.250565 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.234603 | `get_azure_databases_details` | ❌ |
| 4 | 0.220379 | `add_azure_app_service_database` | ❌ |
| 5 | 0.212755 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 85

**Expected Tool:** `edit_azure_databases`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.380868 | `edit_azure_databases` | ✅ **EXPECTED** |
| 2 | 0.269323 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.251993 | `delete_azure_database_admin_configurations` | ❌ |
| 4 | 0.213410 | `get_azure_databases_details` | ❌ |
| 5 | 0.195757 | `rename_azure_sql_databases` | ❌ |

---

## Test 86

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518830 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.421973 | `create_azure_monitor_webtests` | ❌ |
| 3 | 0.402104 | `create_azure_load_testing` | ❌ |
| 4 | 0.398005 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.397774 | `deploy_azure_resources_and_applications` | ❌ |

---

## Test 87

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510827 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.472245 | `create_azure_monitor_webtests` | ❌ |
| 3 | 0.459723 | `update_azure_monitor_webtests` | ❌ |
| 4 | 0.438668 | `get_azure_capacity` | ❌ |
| 5 | 0.381326 | `get_azure_load_testing_details` | ❌ |

---

## Test 88

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.345277 | `get_azure_messaging_service_details` | ❌ |
| 2 | 0.302173 | `get_azure_storage_details` | ❌ |
| 3 | 0.289476 | `get_azure_capacity` | ❌ |
| 4 | 0.285925 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 5 | 0.276066 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 89

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.314030 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.257651 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.253620 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.244006 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.239936 | `get_azure_signalr_details` | ❌ |

---

## Test 90

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.380856 | `get_azure_capacity` | ❌ |
| 2 | 0.338439 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 3 | 0.336667 | `get_azure_storage_details` | ❌ |
| 4 | 0.325422 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.312349 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 91

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.486070 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.406567 | `create_azure_monitor_webtests` | ❌ |
| 3 | 0.405853 | `get_azure_capacity` | ❌ |
| 4 | 0.384944 | `update_azure_monitor_webtests` | ❌ |
| 5 | 0.375039 | `create_azure_load_testing` | ❌ |

---

## Test 92

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List active service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.508303 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.487631 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.384016 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.380883 | `get_azure_signalr_details` | ❌ |
| 5 | 0.379623 | `publish_azure_eventgrid_events` | ❌ |

---

## Test 93

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.423129 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.407964 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.398563 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.396444 | `get_azure_databases_details` | ❌ |
| 5 | 0.383614 | `create_azure_workbooks` | ❌ |

---

## Test 94

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.455463 | `get_azure_databases_details` | ❌ |
| 2 | 0.449685 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 3 | 0.445863 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.438518 | `get_azure_security_configurations` | ❌ |
| 5 | 0.432854 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 95

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492563 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.463935 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.452333 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.437304 | `create_azure_workbooks` | ❌ |
| 5 | 0.419394 | `get_azure_security_configurations` | ❌ |

---

## Test 96

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.400161 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.356965 | `get_azure_load_testing_details` | ❌ |
| 3 | 0.349385 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.343187 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.318702 | `get_azure_messaging_service_details` | ❌ |

---

## Test 97

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.514546 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.488013 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.381633 | `get_azure_signalr_details` | ❌ |
| 4 | 0.372903 | `publish_azure_eventgrid_events` | ❌ |
| 5 | 0.371822 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 98

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.429517 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.418441 | `get_azure_workbooks_details` | ❌ |
| 3 | 0.398664 | `create_azure_workbooks` | ❌ |
| 4 | 0.393598 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.376632 | `get_azure_databases_details` | ❌ |

---

## Test 99

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525913 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.514752 | `get_azure_capacity` | ❌ |
| 3 | 0.482171 | `get_azure_storage_details` | ❌ |
| 4 | 0.480031 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.465854 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |

---

## Test 100

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.474724 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.430555 | `deploy_azure_resources_and_applications` | ❌ |
| 3 | 0.423122 | `get_azure_best_practices` | ❌ |
| 4 | 0.407367 | `update_azure_monitor_webtests` | ❌ |
| 5 | 0.368902 | `create_azure_monitor_webtests` | ❌ |

---

## Test 101

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531640 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.453872 | `deploy_azure_resources_and_applications` | ❌ |
| 3 | 0.439990 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.418057 | `get_azure_capacity` | ❌ |
| 5 | 0.416257 | `get_azure_security_configurations` | ❌ |

---

## Test 102

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List the activity logs of the last month for <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.340942 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.293858 | `get_azure_load_testing_details` | ❌ |
| 3 | 0.279505 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.269020 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.267492 | `get_azure_capacity` | ❌ |

---

## Test 103

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.348670 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.247328 | `deploy_azure_resources_and_applications` | ❌ |
| 3 | 0.246532 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.235685 | `update_azure_monitor_webtests` | ❌ |
| 5 | 0.235128 | `get_azure_app_resource_details` | ❌ |

---

## Test 104

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.330121 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.264069 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.253364 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.252330 | `get_azure_capacity` | ❌ |
| 5 | 0.245600 | `get_azure_messaging_service_details` | ❌ |

---

## Test 105

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.558407 | `get_azure_storage_details` | ❌ |
| 2 | 0.447481 | `get_azure_capacity` | ❌ |
| 3 | 0.407697 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.403095 | `create_azure_storage` | ❌ |
| 5 | 0.390433 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |

---

## Test 106

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496308 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.467743 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.424333 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.392324 | `publish_azure_eventgrid_events` | ❌ |
| 5 | 0.390496 | `get_azure_capacity` | ❌ |

---

## Test 107

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me code optimization recommendations for all Application Insights resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.508215 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.492356 | `deploy_azure_resources_and_applications` | ❌ |
| 3 | 0.464604 | `get_azure_best_practices` | ❌ |
| 4 | 0.418559 | `create_azure_monitor_webtests` | ❌ |
| 5 | 0.413326 | `get_azure_capacity` | ❌ |

---

## Test 108

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.506190 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.488696 | `create_azure_workbooks` | ❌ |
| 3 | 0.451336 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.410409 | `get_azure_security_configurations` | ❌ |
| 5 | 0.405448 | `edit_azure_workbooks` | ❌ |

---

## Test 109

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.472052 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.434828 | `update_azure_monitor_webtests` | ❌ |
| 3 | 0.411997 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.398711 | `create_azure_monitor_webtests` | ❌ |
| 5 | 0.396961 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |

---

## Test 110

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478758 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.468817 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.445424 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.438080 | `get_azure_capacity` | ❌ |
| 5 | 0.435005 | `get_azure_app_resource_details` | ❌ |

---

## Test 111

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.428340 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.405799 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.396006 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.387063 | `create_azure_workbooks` | ❌ |
| 5 | 0.384415 | `get_azure_databases_details` | ❌ |

---

## Test 112

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.575009 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.499791 | `get_azure_capacity` | ❌ |
| 3 | 0.475264 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.474203 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.471465 | `get_azure_security_configurations` | ❌ |

---

## Test 113

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the health status of entity <entity_id> using the health model <health_model_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.311450 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.230296 | `get_azure_ai_resources_details` | ❌ |
| 3 | 0.201724 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.193404 | `audit_azure_resources_compliance` | ❌ |
| 5 | 0.190307 | `create_azure_monitor_webtests` | ❌ |

---

## Test 114

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.494473 | `get_azure_storage_details` | ❌ |
| 2 | 0.403725 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 3 | 0.395150 | `create_azure_storage` | ❌ |
| 4 | 0.371528 | `get_azure_capacity` | ❌ |
| 5 | 0.339439 | `get_azure_security_configurations` | ❌ |

---

## Test 115

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.513546 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.479440 | `create_azure_workbooks` | ❌ |
| 3 | 0.458853 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.456725 | `get_azure_workbooks_details` | ❌ |
| 5 | 0.418907 | `browse_azure_marketplace_products` | ❌ |

---

## Test 116

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.464430 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.365333 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.333899 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.329728 | `get_azure_workbooks_details` | ❌ |
| 5 | 0.327688 | `get_azure_signalr_details` | ❌ |

---

## Test 117

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.453704 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.375709 | `create_azure_workbooks` | ❌ |
| 3 | 0.357168 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.336058 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.324855 | `edit_azure_workbooks` | ❌ |

---

## Test 118

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.398557 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.354368 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.347808 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.346010 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.324679 | `get_azure_capacity` | ❌ |

---

## Test 119

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433671 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.420831 | `get_azure_workbooks_details` | ❌ |
| 3 | 0.400964 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.400018 | `create_azure_workbooks` | ❌ |
| 5 | 0.367191 | `get_azure_databases_details` | ❌ |

---

## Test 120

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Use app lens to check why my app is slow?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.317206 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.234984 | `get_azure_app_resource_details` | ❌ |
| 3 | 0.221280 | `deploy_azure_resources_and_applications` | ❌ |
| 4 | 0.214378 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.213578 | `get_azure_databases_details` | ❌ |

---

## Test 121

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.299595 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.250948 | `get_azure_app_resource_details` | ❌ |
| 3 | 0.212919 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.205979 | `add_azure_app_service_database` | ❌ |
| 5 | 0.201566 | `get_azure_signalr_details` | ❌ |

---

## Test 122

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.404977 | `get_azure_virtual_desktop_details` | ❌ |
| 2 | 0.381848 | `get_azure_capacity` | ❌ |
| 3 | 0.380088 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.377460 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 5 | 0.363367 | `get_azure_signalr_details` | ❌ |

---

## Test 123

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.519763 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.415402 | `create_azure_monitor_webtests` | ❌ |
| 3 | 0.411354 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.410131 | `update_azure_monitor_webtests` | ❌ |
| 5 | 0.409756 | `get_azure_capacity` | ❌ |

---

## Test 124

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.505430 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.411932 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.408064 | `get_azure_capacity` | ❌ |
| 4 | 0.389687 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.382850 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 125

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What service issues have occurred in the last 30 days?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.274285 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.256350 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.237971 | `get_azure_signalr_details` | ❌ |
| 4 | 0.225301 | `get_azure_container_details` | ❌ |
| 5 | 0.224471 | `get_azure_capacity` | ❌ |

---

## Test 126

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.438760 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.399002 | `get_azure_capacity` | ❌ |
| 3 | 0.371290 | `get_azure_signalr_details` | ❌ |
| 4 | 0.370369 | `create_azure_load_testing` | ❌ |
| 5 | 0.363028 | `create_azure_monitor_webtests` | ❌ |

---

## Test 127

**Expected Tool:** `deploy_azure_resources_and_applications`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640058 | `deploy_azure_resources_and_applications` | ✅ **EXPECTED** |
| 2 | 0.479918 | `get_azure_best_practices` | ❌ |
| 3 | 0.461377 | `create_azure_monitor_webtests` | ❌ |
| 4 | 0.454825 | `add_azure_app_service_database` | ❌ |
| 5 | 0.453039 | `design_azure_architecture` | ❌ |

---

## Test 128

**Expected Tool:** `deploy_azure_resources_and_applications`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578477 | `deploy_azure_resources_and_applications` | ✅ **EXPECTED** |
| 2 | 0.428250 | `add_azure_app_service_database` | ❌ |
| 3 | 0.410719 | `get_azure_best_practices` | ❌ |
| 4 | 0.409389 | `deploy_azure_ai_models` | ❌ |
| 5 | 0.372647 | `generate_azure_cli_commands` | ❌ |

---

## Test 129

**Expected Tool:** `deploy_azure_resources_and_applications`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522934 | `deploy_azure_resources_and_applications` | ✅ **EXPECTED** |
| 2 | 0.442200 | `get_azure_resource_and_app_health_status` | ❌ |
| 3 | 0.402454 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.392628 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.385063 | `get_azure_container_details` | ❌ |

---

## Test 130

**Expected Tool:** `deploy_azure_resources_and_applications`  
**Prompt:** Show me the rules to generate bicep scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488008 | `get_azure_best_practices` | ❌ |
| 2 | 0.384841 | `deploy_azure_resources_and_applications` | ✅ **EXPECTED** |
| 3 | 0.381084 | `generate_azure_cli_commands` | ❌ |
| 4 | 0.325324 | `create_azure_database_admin_configurations` | ❌ |
| 5 | 0.280777 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 131

**Expected Tool:** `deploy_azure_ai_models`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.307463 | `deploy_azure_resources_and_applications` | ❌ |
| 2 | 0.299302 | `create_azure_load_testing` | ❌ |
| 3 | 0.281815 | `deploy_azure_ai_models` | ✅ **EXPECTED** |
| 4 | 0.240425 | `edit_azure_databases` | ❌ |
| 5 | 0.236281 | `get_azure_best_practices` | ❌ |

---

## Test 132

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549804 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.418698 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.401422 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.388733 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.379923 | `get_azure_messaging_service_details` | ❌ |

---

## Test 133

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.605174 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.469735 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.413315 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.323275 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.310593 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 134

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** List all key-value settings with key name starting with 'prod-' in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492499 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.375253 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.345143 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.293230 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.274244 | `get_azure_key_vault_items` | ❌ |

---

## Test 135

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.517123 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.397359 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.350023 | `add_azure_app_service_database` | ❌ |
| 4 | 0.318242 | `edit_azure_app_config_settings` | ❌ |
| 5 | 0.299920 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 136

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564754 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.445478 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.410403 | `add_azure_app_service_database` | ❌ |
| 4 | 0.382377 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.377241 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 137

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619236 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.496884 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.413994 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.320917 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.318437 | `get_azure_key_vault_secret_values` | ❌ |

---

## Test 138

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.473674 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.397489 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.351004 | `get_azure_key_vault_secret_values` | ❌ |
| 4 | 0.319847 | `edit_azure_app_config_settings` | ❌ |
| 5 | 0.293222 | `get_azure_key_vault_items` | ❌ |

---

## Test 139

**Expected Tool:** `edit_azure_app_config_settings`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480490 | `edit_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.419225 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.386233 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.246854 | `edit_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.245528 | `add_azure_app_service_database` | ❌ |

---

## Test 140

**Expected Tool:** `edit_azure_app_config_settings`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.454141 | `lock_unlock_azure_app_config_settings` | ❌ |
| 2 | 0.419890 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.418151 | `edit_azure_app_config_settings` | ✅ **EXPECTED** |
| 4 | 0.282688 | `update_azure_managed_lustre_filesystems` | ❌ |
| 5 | 0.270519 | `add_azure_app_service_database` | ❌ |

---

## Test 141

**Expected Tool:** `lock_unlock_azure_app_config_settings`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.523446 | `lock_unlock_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.367924 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.324653 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.243851 | `add_azure_app_service_database` | ❌ |
| 5 | 0.238376 | `update_azure_managed_lustre_filesystems` | ❌ |

---

## Test 142

**Expected Tool:** `lock_unlock_azure_app_config_settings`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552583 | `lock_unlock_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.393938 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.339108 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.276616 | `get_azure_key_vault_secret_values` | ❌ |
| 5 | 0.242903 | `create_azure_key_vault_secrets` | ❌ |

---

## Test 143

**Expected Tool:** `edit_azure_workbooks`  
**Prompt:** Delete the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.505878 | `edit_azure_workbooks` | ✅ **EXPECTED** |
| 2 | 0.375642 | `create_azure_workbooks` | ❌ |
| 3 | 0.362979 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.265457 | `edit_azure_app_config_settings` | ❌ |
| 5 | 0.252814 | `edit_azure_data_analytics_resources` | ❌ |

---

## Test 144

**Expected Tool:** `edit_azure_workbooks`  
**Prompt:** Update the workbook <workbook_resource_id> with a new text step  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496535 | `edit_azure_workbooks` | ✅ **EXPECTED** |
| 2 | 0.413187 | `create_azure_workbooks` | ❌ |
| 3 | 0.327796 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.247622 | `update_azure_managed_lustre_filesystems` | ❌ |
| 5 | 0.236165 | `update_azure_load_testing_configurations` | ❌ |

---

## Test 145

**Expected Tool:** `create_azure_workbooks`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555073 | `create_azure_workbooks` | ✅ **EXPECTED** |
| 2 | 0.400619 | `edit_azure_workbooks` | ❌ |
| 3 | 0.371495 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.225508 | `create_azure_key_vault_secrets` | ❌ |
| 5 | 0.194912 | `create_azure_key_vault_items` | ❌ |

---

## Test 146

**Expected Tool:** `get_azure_workbooks_details`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512253 | `get_azure_workbooks_details` | ✅ **EXPECTED** |
| 2 | 0.409967 | `edit_azure_workbooks` | ❌ |
| 3 | 0.409085 | `create_azure_workbooks` | ❌ |
| 4 | 0.299382 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.294878 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 147

**Expected Tool:** `get_azure_workbooks_details`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552702 | `get_azure_workbooks_details` | ✅ **EXPECTED** |
| 2 | 0.514558 | `create_azure_workbooks` | ❌ |
| 3 | 0.441697 | `edit_azure_workbooks` | ❌ |
| 4 | 0.426606 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.396091 | `get_azure_security_configurations` | ❌ |

---

## Test 148

**Expected Tool:** `get_azure_workbooks_details`  
**Prompt:** Show me the workbook with display name <workbook_display_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.474463 | `get_azure_workbooks_details` | ✅ **EXPECTED** |
| 2 | 0.454790 | `create_azure_workbooks` | ❌ |
| 3 | 0.422536 | `edit_azure_workbooks` | ❌ |
| 4 | 0.201213 | `get_azure_security_configurations` | ❌ |
| 5 | 0.181802 | `browse_azure_marketplace_products` | ❌ |

---

## Test 149

**Expected Tool:** `get_azure_workbooks_details`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549690 | `get_azure_workbooks_details` | ✅ **EXPECTED** |
| 2 | 0.529693 | `create_azure_workbooks` | ❌ |
| 3 | 0.453173 | `edit_azure_workbooks` | ❌ |
| 4 | 0.438514 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.391845 | `get_azure_security_configurations` | ❌ |

---

## Test 150

**Expected Tool:** `audit_azure_resources_compliance`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557893 | `get_azure_capacity` | ❌ |
| 2 | 0.546941 | `audit_azure_resources_compliance` | ✅ **EXPECTED** |
| 3 | 0.500223 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.477388 | `get_azure_best_practices` | ❌ |
| 5 | 0.474918 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 151

**Expected Tool:** `audit_azure_resources_compliance`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536483 | `audit_azure_resources_compliance` | ✅ **EXPECTED** |
| 2 | 0.511039 | `get_azure_best_practices` | ❌ |
| 3 | 0.490293 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.487381 | `get_azure_capacity` | ❌ |
| 5 | 0.463219 | `deploy_azure_resources_and_applications` | ❌ |

---

## Test 152

**Expected Tool:** `audit_azure_resources_compliance`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592611 | `audit_azure_resources_compliance` | ✅ **EXPECTED** |
| 2 | 0.508765 | `get_azure_best_practices` | ❌ |
| 3 | 0.502901 | `get_azure_capacity` | ❌ |
| 4 | 0.492553 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.455630 | `deploy_azure_resources_and_applications` | ❌ |

---

## Test 153

**Expected Tool:** `generate_azure_cli_commands`  
**Prompt:** Get Azure CLI command to create a Storage account with name <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.559527 | `create_azure_storage` | ❌ |
| 2 | 0.490440 | `get_azure_storage_details` | ❌ |
| 3 | 0.486197 | `generate_azure_cli_commands` | ✅ **EXPECTED** |
| 4 | 0.428212 | `install_azure_cli_extensions` | ❌ |
| 5 | 0.417777 | `create_azure_key_vault_secrets` | ❌ |

---

## Test 154

**Expected Tool:** `generate_azure_cli_commands`  
**Prompt:** Show me how to use Azure CLI to list all virtual machines in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.505461 | `generate_azure_cli_commands` | ✅ **EXPECTED** |
| 2 | 0.466689 | `install_azure_cli_extensions` | ❌ |
| 3 | 0.460094 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.439158 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.433504 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 155

**Expected Tool:** `generate_azure_cli_commands`  
**Prompt:** Show me the details of the storage account <account_name> with Azure CLI commands  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612935 | `get_azure_storage_details` | ❌ |
| 2 | 0.497121 | `generate_azure_cli_commands` | ✅ **EXPECTED** |
| 3 | 0.475109 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.471795 | `create_azure_storage` | ❌ |
| 5 | 0.455795 | `get_azure_container_details` | ❌ |

---

## Test 156

**Expected Tool:** `get_azure_security_configurations`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.734080 | `get_azure_security_configurations` | ✅ **EXPECTED** |
| 2 | 0.460399 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.414687 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.365074 | `get_azure_capacity` | ❌ |
| 5 | 0.356352 | `browse_azure_marketplace_products` | ❌ |

---

## Test 157

**Expected Tool:** `get_azure_security_configurations`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702749 | `get_azure_security_configurations` | ✅ **EXPECTED** |
| 2 | 0.485211 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.431073 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.400579 | `get_azure_capacity` | ❌ |
| 5 | 0.388410 | `browse_azure_marketplace_products` | ❌ |

---

## Test 158

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Display the certificate details for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533821 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.508482 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.465773 | `get_azure_key_vault_secret_values` | ❌ |
| 4 | 0.426168 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.393742 | `get_azure_app_config_settings` | ❌ |

---

## Test 159

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Display the key details for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.459787 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.447147 | `get_azure_key_vault_secret_values` | ❌ |
| 3 | 0.393173 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.343627 | `create_azure_key_vault_secrets` | ❌ |
| 5 | 0.342336 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 160

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Enumerate certificates in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572193 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.513646 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.495222 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.438478 | `create_azure_key_vault_secrets` | ❌ |
| 5 | 0.423154 | `get_azure_key_vault_secret_values` | ❌ |

---

## Test 161

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Enumerate keys in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.501656 | `create_azure_key_vault_secrets` | ❌ |
| 2 | 0.492799 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.477019 | `get_azure_key_vault_secret_values` | ❌ |
| 4 | 0.466847 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.447167 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 162

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Enumerate secrets in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.614347 | `create_azure_key_vault_secrets` | ❌ |
| 2 | 0.522680 | `get_azure_key_vault_secret_values` | ❌ |
| 3 | 0.504917 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 4 | 0.450082 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.405839 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 163

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Get the account settings for my key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509964 | `get_azure_app_config_settings` | ❌ |
| 2 | 0.469179 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.446761 | `get_azure_key_vault_secret_values` | ❌ |
| 4 | 0.391479 | `lock_unlock_azure_app_config_settings` | ❌ |
| 5 | 0.377164 | `create_azure_key_vault_secrets` | ❌ |

---

## Test 164

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Get the certificate <certificate_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547751 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.482982 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.476094 | `get_azure_key_vault_secret_values` | ❌ |
| 4 | 0.435609 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.378696 | `create_azure_key_vault_secrets` | ❌ |

---

## Test 165

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Get the key <key_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.424659 | `get_azure_key_vault_secret_values` | ❌ |
| 2 | 0.371826 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.356181 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.355054 | `import_azure_key_vault_certificates` | ❌ |
| 5 | 0.315829 | `create_azure_key_vault_items` | ❌ |

---

## Test 166

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542408 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.529198 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.485747 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.425943 | `create_azure_key_vault_secrets` | ❌ |
| 5 | 0.418508 | `get_azure_key_vault_secret_values` | ❌ |

---

## Test 167

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.503471 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.468167 | `create_azure_key_vault_secrets` | ❌ |
| 3 | 0.459043 | `get_azure_key_vault_secret_values` | ❌ |
| 4 | 0.442074 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.427597 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 168

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580241 | `create_azure_key_vault_secrets` | ❌ |
| 2 | 0.494605 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.490556 | `get_azure_key_vault_secret_values` | ❌ |
| 4 | 0.413675 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.372548 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 169

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** List certificate names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.526271 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.522418 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.482074 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.447164 | `get_azure_key_vault_secret_values` | ❌ |
| 5 | 0.435835 | `create_azure_key_vault_secrets` | ❌ |

---

## Test 170

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** List key names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520132 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.475321 | `get_azure_key_vault_secret_values` | ❌ |
| 3 | 0.465846 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.441517 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.407002 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 171

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** List secrets names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545546 | `create_azure_key_vault_secrets` | ❌ |
| 2 | 0.497984 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.490924 | `get_azure_key_vault_secret_values` | ❌ |
| 4 | 0.430161 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.381031 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 172

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.526739 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.521127 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.458662 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.449940 | `get_azure_key_vault_secret_values` | ❌ |
| 5 | 0.352644 | `create_azure_key_vault_secrets` | ❌ |

---

## Test 173

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Retrieve key metadata for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.448461 | `get_azure_key_vault_secret_values` | ❌ |
| 2 | 0.440374 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.360597 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.347427 | `create_azure_key_vault_secrets` | ❌ |
| 5 | 0.345646 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 174

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Show certificate names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557161 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.535803 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.480958 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.470813 | `get_azure_key_vault_secret_values` | ❌ |
| 5 | 0.436295 | `create_azure_key_vault_secrets` | ❌ |

---

## Test 175

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Show key names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532996 | `get_azure_key_vault_secret_values` | ❌ |
| 2 | 0.527983 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.476688 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.443413 | `import_azure_key_vault_certificates` | ❌ |
| 5 | 0.438251 | `create_azure_key_vault_items` | ❌ |

---

## Test 176

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Show me the account settings for managed HSM keyvault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.459394 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.434426 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.393297 | `get_azure_key_vault_secret_values` | ❌ |
| 4 | 0.376496 | `lock_unlock_azure_app_config_settings` | ❌ |
| 5 | 0.354255 | `create_azure_key_vault_secrets` | ❌ |

---

## Test 177

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543362 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.508426 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.454583 | `get_azure_key_vault_secret_values` | ❌ |
| 4 | 0.453867 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.386384 | `create_azure_key_vault_secrets` | ❌ |

---

## Test 178

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552928 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.535077 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.480063 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.475424 | `get_azure_key_vault_secret_values` | ❌ |
| 5 | 0.430555 | `create_azure_key_vault_secrets` | ❌ |

---

## Test 179

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527455 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.490672 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.460248 | `get_azure_key_vault_secret_values` | ❌ |
| 4 | 0.432966 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.409324 | `get_azure_app_config_settings` | ❌ |

---

## Test 180

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480223 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.471361 | `get_azure_key_vault_secret_values` | ❌ |
| 3 | 0.426201 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.394979 | `create_azure_key_vault_secrets` | ❌ |
| 5 | 0.377132 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 181

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.464736 | `get_azure_key_vault_secret_values` | ❌ |
| 2 | 0.449713 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.407701 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.399319 | `import_azure_key_vault_certificates` | ❌ |
| 5 | 0.379740 | `create_azure_key_vault_items` | ❌ |

---

## Test 182

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.517414 | `get_azure_key_vault_secret_values` | ❌ |
| 2 | 0.502886 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.480193 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.448371 | `import_azure_key_vault_certificates` | ❌ |
| 5 | 0.441572 | `create_azure_key_vault_items` | ❌ |

---

## Test 183

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587359 | `create_azure_key_vault_secrets` | ❌ |
| 2 | 0.574899 | `get_azure_key_vault_secret_values` | ❌ |
| 3 | 0.526650 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 4 | 0.440536 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.426022 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 184

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** Show secrets names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595751 | `create_azure_key_vault_secrets` | ❌ |
| 2 | 0.581382 | `get_azure_key_vault_secret_values` | ❌ |
| 3 | 0.517138 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 4 | 0.434379 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.427291 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 185

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** What certificates are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.583734 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.570872 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.555201 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.477077 | `create_azure_key_vault_secrets` | ❌ |
| 5 | 0.476712 | `get_azure_key_vault_secret_values` | ❌ |

---

## Test 186

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** What keys are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551036 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.543936 | `create_azure_key_vault_secrets` | ❌ |
| 3 | 0.534063 | `get_azure_key_vault_secret_values` | ❌ |
| 4 | 0.520805 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.484768 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 187

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** What secrets are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622270 | `create_azure_key_vault_secrets` | ❌ |
| 2 | 0.581477 | `get_azure_key_vault_secret_values` | ❌ |
| 3 | 0.564796 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 4 | 0.495618 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.438877 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 188

**Expected Tool:** `get_azure_key_vault_items`  
**Prompt:** What's the value of the <setting_name> setting in my key vault with name <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498642 | `get_azure_app_config_settings` | ❌ |
| 2 | 0.468577 | `get_azure_key_vault_secret_values` | ❌ |
| 3 | 0.448342 | `lock_unlock_azure_app_config_settings` | ❌ |
| 4 | 0.444459 | `get_azure_key_vault_items` | ✅ **EXPECTED** |
| 5 | 0.409443 | `create_azure_key_vault_secrets` | ❌ |

---

## Test 189

**Expected Tool:** `get_azure_key_vault_secret_values`  
**Prompt:** Display the secret details for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586366 | `get_azure_key_vault_secret_values` | ✅ **EXPECTED** |
| 2 | 0.526985 | `get_azure_key_vault_items` | ❌ |
| 3 | 0.518051 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.440667 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.389702 | `create_azure_key_vault_items` | ❌ |

---

## Test 190

**Expected Tool:** `get_azure_key_vault_secret_values`  
**Prompt:** Get the secret <secret_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563408 | `get_azure_key_vault_secret_values` | ✅ **EXPECTED** |
| 2 | 0.504391 | `create_azure_key_vault_secrets` | ❌ |
| 3 | 0.450877 | `get_azure_key_vault_items` | ❌ |
| 4 | 0.381027 | `import_azure_key_vault_certificates` | ❌ |
| 5 | 0.364969 | `create_azure_key_vault_items` | ❌ |

---

## Test 191

**Expected Tool:** `get_azure_key_vault_secret_values`  
**Prompt:** Retrieve secret metadata for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.558674 | `get_azure_key_vault_secret_values` | ✅ **EXPECTED** |
| 2 | 0.487736 | `create_azure_key_vault_secrets` | ❌ |
| 3 | 0.486559 | `get_azure_key_vault_items` | ❌ |
| 4 | 0.389038 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.365020 | `get_azure_app_config_settings` | ❌ |

---

## Test 192

**Expected Tool:** `get_azure_key_vault_secret_values`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542116 | `get_azure_key_vault_secret_values` | ✅ **EXPECTED** |
| 2 | 0.507830 | `get_azure_key_vault_items` | ❌ |
| 3 | 0.493688 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.433356 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.375736 | `create_azure_key_vault_items` | ❌ |

---

## Test 193

**Expected Tool:** `get_azure_key_vault_secret_values`  
**Prompt:** Show me the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552814 | `get_azure_key_vault_secret_values` | ✅ **EXPECTED** |
| 2 | 0.516089 | `create_azure_key_vault_secrets` | ❌ |
| 3 | 0.487940 | `get_azure_key_vault_items` | ❌ |
| 4 | 0.389282 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.386391 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 194

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602182 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.536615 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.536205 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.416317 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.323442 | `get_azure_key_vault_secret_values` | ❌ |

---

## Test 195

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527766 | `create_azure_key_vault_secrets` | ❌ |
| 2 | 0.505954 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.417468 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.354704 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.330865 | `get_azure_key_vault_secret_values` | ❌ |

---

## Test 196

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Create an EC key with name <key_name> in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492989 | `create_azure_key_vault_secrets` | ❌ |
| 2 | 0.470079 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.407173 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.350415 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.338214 | `get_azure_key_vault_secret_values` | ❌ |

---

## Test 197

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Create an oct key in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560056 | `create_azure_key_vault_secrets` | ❌ |
| 2 | 0.506522 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.450938 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.422702 | `get_azure_key_vault_secret_values` | ❌ |
| 5 | 0.392705 | `get_azure_key_vault_items` | ❌ |

---

## Test 198

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Create an RSA key in the vault <key_vault_account_name> with name <key_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545338 | `create_azure_key_vault_secrets` | ❌ |
| 2 | 0.534535 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.437054 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.390812 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.372207 | `get_azure_key_vault_secret_values` | ❌ |

---

## Test 199

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Generate a certificate named <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.559592 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.530817 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.487915 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.426824 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.367947 | `get_azure_key_vault_secret_values` | ❌ |

---

## Test 200

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Generate a key <key_name> with type <key_type> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.484318 | `create_azure_key_vault_secrets` | ❌ |
| 2 | 0.468180 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.387664 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.356823 | `get_azure_key_vault_secret_values` | ❌ |
| 5 | 0.351880 | `get_azure_key_vault_items` | ❌ |

---

## Test 201

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Issue a certificate <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.559527 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.546365 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.476974 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.452962 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.372106 | `get_azure_key_vault_secret_values` | ❌ |

---

## Test 202

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555645 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.554305 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.500254 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.410218 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.325473 | `get_azure_key_vault_secret_values` | ❌ |

---

## Test 203

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Request creation of certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563546 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.545314 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.490569 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.439216 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.343811 | `get_azure_key_vault_secret_values` | ❌ |

---

## Test 204

**Expected Tool:** `create_azure_key_vault_secrets`  
**Prompt:** Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629675 | `create_azure_key_vault_secrets` | ✅ **EXPECTED** |
| 2 | 0.479338 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.407829 | `get_azure_key_vault_secret_values` | ❌ |
| 4 | 0.391533 | `import_azure_key_vault_certificates` | ❌ |
| 5 | 0.363103 | `append_azure_confidential_ledger_entries` | ❌ |

---

## Test 205

**Expected Tool:** `create_azure_key_vault_secrets`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674434 | `create_azure_key_vault_secrets` | ✅ **EXPECTED** |
| 2 | 0.528671 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.442879 | `get_azure_key_vault_secret_values` | ❌ |
| 4 | 0.386205 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.385343 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 206

**Expected Tool:** `create_azure_key_vault_secrets`  
**Prompt:** Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572357 | `create_azure_key_vault_secrets` | ✅ **EXPECTED** |
| 2 | 0.454539 | `get_azure_key_vault_secret_values` | ❌ |
| 3 | 0.415396 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.387475 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.357474 | `lock_unlock_azure_app_config_settings` | ❌ |

---

## Test 207

**Expected Tool:** `create_azure_key_vault_secrets`  
**Prompt:** Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588807 | `create_azure_key_vault_secrets` | ✅ **EXPECTED** |
| 2 | 0.475024 | `get_azure_key_vault_secret_values` | ❌ |
| 3 | 0.452599 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.402378 | `import_azure_key_vault_certificates` | ❌ |
| 5 | 0.395541 | `get_azure_key_vault_items` | ❌ |

---

## Test 208

**Expected Tool:** `create_azure_key_vault_secrets`  
**Prompt:** Update secret <secret_name> to value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.514684 | `create_azure_key_vault_secrets` | ✅ **EXPECTED** |
| 2 | 0.453288 | `get_azure_key_vault_secret_values` | ❌ |
| 3 | 0.390780 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.376210 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.365541 | `lock_unlock_azure_app_config_settings` | ❌ |

---

## Test 209

**Expected Tool:** `import_azure_key_vault_certificates`  
**Prompt:** Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560409 | `import_azure_key_vault_certificates` | ✅ **EXPECTED** |
| 2 | 0.451439 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.389225 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.351729 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.275281 | `get_azure_key_vault_secret_values` | ❌ |

---

## Test 210

**Expected Tool:** `import_azure_key_vault_certificates`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660982 | `import_azure_key_vault_certificates` | ✅ **EXPECTED** |
| 2 | 0.486040 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.420000 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.383841 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.352274 | `get_azure_key_vault_secret_values` | ❌ |

---

## Test 211

**Expected Tool:** `import_azure_key_vault_certificates`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645826 | `import_azure_key_vault_certificates` | ✅ **EXPECTED** |
| 2 | 0.444963 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.405491 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.367234 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.360848 | `get_azure_key_vault_secret_values` | ❌ |

---

## Test 212

**Expected Tool:** `import_azure_key_vault_certificates`  
**Prompt:** Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630847 | `import_azure_key_vault_certificates` | ✅ **EXPECTED** |
| 2 | 0.464585 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.419381 | `get_azure_key_vault_items` | ❌ |
| 4 | 0.390178 | `get_azure_key_vault_secret_values` | ❌ |
| 5 | 0.388744 | `create_azure_key_vault_secrets` | ❌ |

---

## Test 213

**Expected Tool:** `import_azure_key_vault_certificates`  
**Prompt:** Upload certificate file <file_path> to key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587413 | `import_azure_key_vault_certificates` | ✅ **EXPECTED** |
| 2 | 0.473576 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.423146 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.395735 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.362752 | `get_azure_key_vault_secret_values` | ❌ |

---

## Test 214

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.735005 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.524465 | `deploy_azure_resources_and_applications` | ❌ |
| 3 | 0.439480 | `generate_azure_cli_commands` | ❌ |
| 4 | 0.436798 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.435782 | `get_azure_app_config_settings` | ❌ |

---

## Test 215

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690117 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.539669 | `deploy_azure_resources_and_applications` | ❌ |
| 3 | 0.508718 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.478557 | `get_azure_capacity` | ❌ |
| 5 | 0.460726 | `get_azure_storage_details` | ❌ |

---

## Test 216

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713392 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.529580 | `deploy_azure_resources_and_applications` | ❌ |
| 3 | 0.470093 | `design_azure_architecture` | ❌ |
| 4 | 0.437567 | `generate_azure_cli_commands` | ❌ |
| 5 | 0.435577 | `browse_azure_marketplace_products` | ❌ |

---

## Test 217

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.683437 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.614180 | `deploy_azure_resources_and_applications` | ❌ |
| 3 | 0.465496 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.446368 | `get_azure_capacity` | ❌ |
| 5 | 0.427992 | `generate_azure_cli_commands` | ❌ |

---

## Test 218

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682026 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.557930 | `get_azure_app_resource_details` | ❌ |
| 3 | 0.505735 | `deploy_azure_resources_and_applications` | ❌ |
| 4 | 0.443359 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.432776 | `get_azure_capacity` | ❌ |

---

## Test 219

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685214 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.499419 | `get_azure_app_resource_details` | ❌ |
| 3 | 0.480287 | `deploy_azure_resources_and_applications` | ❌ |
| 4 | 0.425068 | `generate_azure_cli_commands` | ❌ |
| 5 | 0.397939 | `design_azure_architecture` | ❌ |

---

## Test 220

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.675358 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.571007 | `deploy_azure_resources_and_applications` | ❌ |
| 3 | 0.537884 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.415218 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.413866 | `get_azure_capacity` | ❌ |

---

## Test 221

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612873 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.518435 | `deploy_azure_resources_and_applications` | ❌ |
| 3 | 0.424667 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.418814 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.417600 | `get_azure_app_resource_details` | ❌ |

---

## Test 222

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.489465 | `deploy_azure_resources_and_applications` | ❌ |
| 2 | 0.486962 | `use_azure_openai_models` | ❌ |
| 3 | 0.480742 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 4 | 0.451552 | `generate_azure_cli_commands` | ❌ |
| 5 | 0.441313 | `get_azure_ai_resources_details` | ❌ |

---

## Test 223

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618330 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.535723 | `create_azure_key_vault_secrets` | ❌ |
| 3 | 0.531963 | `get_azure_key_vault_secret_values` | ❌ |
| 4 | 0.465939 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.462316 | `create_azure_key_vault_items` | ❌ |

---

## Test 224

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628027 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.475164 | `get_azure_app_resource_details` | ❌ |
| 3 | 0.453910 | `deploy_azure_resources_and_applications` | ❌ |
| 4 | 0.378761 | `get_azure_capacity` | ❌ |
| 5 | 0.363076 | `generate_azure_cli_commands` | ❌ |

---

## Test 225

**Expected Tool:** `design_azure_architecture`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676638 | `design_azure_architecture` | ✅ **EXPECTED** |
| 2 | 0.481643 | `get_azure_best_practices` | ❌ |
| 3 | 0.465832 | `deploy_azure_resources_and_applications` | ❌ |
| 4 | 0.406230 | `create_azure_monitor_webtests` | ❌ |
| 5 | 0.403087 | `generate_azure_cli_commands` | ❌ |

---

## Test 226

**Expected Tool:** `design_azure_architecture`  
**Prompt:** Help me create a cloud service that will serve as ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.269652 | `create_azure_storage` | ❌ |
| 2 | 0.259518 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.248253 | `design_azure_architecture` | ✅ **EXPECTED** |
| 4 | 0.244271 | `generate_azure_cli_commands` | ❌ |
| 5 | 0.231667 | `create_azure_sql_databases_and_servers` | ❌ |

---

## Test 227

**Expected Tool:** `design_azure_architecture`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.427652 | `upload_azure_storage_blobs` | ❌ |
| 2 | 0.424080 | `create_azure_storage` | ❌ |
| 3 | 0.418547 | `get_azure_storage_details` | ❌ |
| 4 | 0.413253 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.405913 | `design_azure_architecture` | ✅ **EXPECTED** |

---

## Test 228

**Expected Tool:** `design_azure_architecture`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.342485 | `browse_azure_marketplace_products` | ❌ |
| 2 | 0.289308 | `design_azure_architecture` | ✅ **EXPECTED** |
| 3 | 0.276203 | `deploy_azure_resources_and_applications` | ❌ |
| 4 | 0.251719 | `add_azure_app_service_database` | ❌ |
| 5 | 0.231297 | `get_azure_app_resource_details` | ❌ |

---

## Test 229

**Expected Tool:** `design_azure_architecture`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.323311 | `upload_azure_storage_blobs` | ❌ |
| 2 | 0.305813 | `create_azure_storage` | ❌ |
| 3 | 0.296166 | `design_azure_architecture` | ✅ **EXPECTED** |
| 4 | 0.261114 | `get_azure_storage_details` | ❌ |
| 5 | 0.235001 | `update_azure_managed_lustre_filesystems` | ❌ |

---

## Test 230

**Expected Tool:** `get_azure_load_testing_details`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609260 | `get_azure_load_testing_details` | ✅ **EXPECTED** |
| 2 | 0.567757 | `create_azure_load_testing` | ❌ |
| 3 | 0.448056 | `update_azure_load_testing_configurations` | ❌ |
| 4 | 0.366305 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.353288 | `create_azure_monitor_webtests` | ❌ |

---

## Test 231

**Expected Tool:** `get_azure_load_testing_details`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599651 | `create_azure_load_testing` | ❌ |
| 2 | 0.581081 | `get_azure_load_testing_details` | ✅ **EXPECTED** |
| 3 | 0.457483 | `update_azure_load_testing_configurations` | ❌ |
| 4 | 0.357813 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.350739 | `create_azure_monitor_webtests` | ❌ |

---

## Test 232

**Expected Tool:** `get_azure_load_testing_details`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612800 | `create_azure_load_testing` | ❌ |
| 2 | 0.592725 | `get_azure_load_testing_details` | ✅ **EXPECTED** |
| 3 | 0.421873 | `update_azure_load_testing_configurations` | ❌ |
| 4 | 0.362425 | `create_azure_monitor_webtests` | ❌ |
| 5 | 0.349117 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 233

**Expected Tool:** `get_azure_load_testing_details`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669717 | `get_azure_load_testing_details` | ✅ **EXPECTED** |
| 2 | 0.609875 | `create_azure_load_testing` | ❌ |
| 3 | 0.493520 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.427548 | `get_azure_capacity` | ❌ |
| 5 | 0.426809 | `get_azure_messaging_service_details` | ❌ |

---

## Test 234

**Expected Tool:** `create_azure_load_testing`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542817 | `create_azure_load_testing` | ✅ **EXPECTED** |
| 2 | 0.491654 | `create_azure_monitor_webtests` | ❌ |
| 3 | 0.431906 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.425527 | `update_azure_load_testing_configurations` | ❌ |
| 5 | 0.409288 | `update_azure_monitor_webtests` | ❌ |

---

## Test 235

**Expected Tool:** `create_azure_load_testing`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660181 | `create_azure_load_testing` | ✅ **EXPECTED** |
| 2 | 0.530657 | `get_azure_load_testing_details` | ❌ |
| 3 | 0.430087 | `create_azure_monitor_webtests` | ❌ |
| 4 | 0.411267 | `update_azure_load_testing_configurations` | ❌ |
| 5 | 0.374033 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 236

**Expected Tool:** `create_azure_load_testing`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585612 | `create_azure_load_testing` | ✅ **EXPECTED** |
| 2 | 0.496772 | `update_azure_load_testing_configurations` | ❌ |
| 3 | 0.460907 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.420018 | `create_azure_monitor_webtests` | ❌ |
| 5 | 0.334546 | `update_azure_monitor_webtests` | ❌ |

---

## Test 237

**Expected Tool:** `update_azure_load_testing_configurations`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577419 | `update_azure_load_testing_configurations` | ✅ **EXPECTED** |
| 2 | 0.501316 | `create_azure_load_testing` | ❌ |
| 3 | 0.443800 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.369297 | `update_azure_monitor_webtests` | ❌ |
| 5 | 0.360753 | `rename_azure_sql_databases` | ❌ |

---

## Test 238

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Get details for AI Foundry resource <resource_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599211 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.449921 | `get_azure_signalr_details` | ❌ |
| 3 | 0.443802 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.429913 | `deploy_azure_ai_models` | ❌ |
| 5 | 0.425558 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 239

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Get the details of knowledge base <agent-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659138 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 2 | 0.646622 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.486694 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.442025 | `get_azure_app_resource_details` | ❌ |
| 5 | 0.441556 | `get_azure_signalr_details` | ❌ |

---

## Test 240

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Get the details of knowledge source <source-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600909 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.585328 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 3 | 0.440398 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.437073 | `get_azure_signalr_details` | ❌ |
| 5 | 0.416989 | `get_azure_app_resource_details` | ❌ |

---

## Test 241

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.316895 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 2 | 0.310095 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.269536 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 4 | 0.268668 | `get_azure_best_practices` | ❌ |
| 5 | 0.262166 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 242

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all agents in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521635 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 2 | 0.510202 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 3 | 0.502701 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 4 | 0.498233 | `connect_azure_ai_foundry_agents` | ❌ |
| 5 | 0.406924 | `deploy_azure_ai_models` | ❌ |

---

## Test 243

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579535 | `deploy_azure_ai_models` | ❌ |
| 2 | 0.562287 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.448518 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.440588 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.412607 | `connect_azure_ai_foundry_agents` | ❌ |

---

## Test 244

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.504981 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.482742 | `deploy_azure_ai_models` | ❌ |
| 3 | 0.403986 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.398748 | `use_azure_openai_models` | ❌ |
| 5 | 0.397053 | `evaluate_azure_ai_foundry_agents` | ❌ |

---

## Test 245

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all AI Foundry resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579817 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.418960 | `deploy_azure_ai_models` | ❌ |
| 3 | 0.406256 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.405825 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.403897 | `connect_azure_ai_foundry_agents` | ❌ |

---

## Test 246

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all available OpenAI models in my Azure resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603264 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.498031 | `use_azure_openai_models` | ❌ |
| 3 | 0.484432 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.443802 | `deploy_azure_ai_models` | ❌ |
| 5 | 0.435368 | `get_azure_capacity` | ❌ |

---

## Test 247

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.534733 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.454264 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.444086 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.419389 | `get_azure_app_resource_details` | ❌ |
| 5 | 0.416112 | `get_azure_signalr_details` | ❌ |

---

## Test 248

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.482325 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.398786 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 3 | 0.364422 | `get_azure_signalr_details` | ❌ |
| 4 | 0.357577 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.356614 | `get_azure_security_configurations` | ❌ |

---

## Test 249

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.667963 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 2 | 0.621099 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.450304 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.444408 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.401759 | `get_azure_databases_details` | ❌ |

---

## Test 250

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531138 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 2 | 0.448783 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.337241 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.323032 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.320822 | `get_azure_container_details` | ❌ |

---

## Test 251

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.515763 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.410647 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 3 | 0.404503 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.396554 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.395810 | `deploy_azure_ai_models` | ❌ |

---

## Test 252

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654111 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.615349 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 3 | 0.446627 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.422491 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.416060 | `get_azure_signalr_details` | ❌ |

---

## Test 253

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.453376 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.433866 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 3 | 0.321611 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.300443 | `get_azure_container_details` | ❌ |
| 5 | 0.295001 | `get_azure_signalr_details` | ❌ |

---

## Test 254

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.507589 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 2 | 0.425742 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.309441 | `get_azure_resource_and_app_health_status` | ❌ |
| 4 | 0.298698 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.291702 | `use_azure_openai_models` | ❌ |

---

## Test 255

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599374 | `deploy_azure_ai_models` | ❌ |
| 2 | 0.552427 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.462343 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.458855 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.435445 | `use_azure_openai_models` | ❌ |

---

## Test 256

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492932 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.431878 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.414065 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 4 | 0.376911 | `get_azure_signalr_details` | ❌ |
| 5 | 0.364678 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 257

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the AI Foundry resources in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588933 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.510164 | `deploy_azure_ai_models` | ❌ |
| 3 | 0.472600 | `connect_azure_ai_foundry_agents` | ❌ |
| 4 | 0.467078 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.463365 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |

---

## Test 258

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the available agents in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546352 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 2 | 0.534425 | `connect_azure_ai_foundry_agents` | ❌ |
| 3 | 0.530860 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.525204 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 5 | 0.429424 | `deploy_azure_ai_models` | ❌ |

---

## Test 259

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the available AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555128 | `deploy_azure_ai_models` | ❌ |
| 2 | 0.522598 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.509261 | `use_azure_openai_models` | ❌ |
| 4 | 0.460129 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.459774 | `evaluate_azure_ai_foundry_agents` | ❌ |

---

## Test 260

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543850 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.507116 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.468386 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 4 | 0.448711 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.425779 | `get_azure_app_resource_details` | ❌ |

---

## Test 261

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.494174 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.450784 | `get_azure_signalr_details` | ❌ |
| 3 | 0.423479 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.417724 | `get_azure_app_resource_details` | ❌ |
| 5 | 0.406388 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 262

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.470270 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.407167 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 3 | 0.378470 | `get_azure_signalr_details` | ❌ |
| 4 | 0.373514 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.360971 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 263

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496619 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 2 | 0.435586 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.310044 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.302527 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.290298 | `get_azure_container_details` | ❌ |

---

## Test 264

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685811 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 2 | 0.620774 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.463308 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.439438 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.410728 | `get_azure_signalr_details` | ❌ |

---

## Test 265

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557843 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 2 | 0.456176 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.377134 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.337980 | `get_azure_container_details` | ❌ |
| 5 | 0.327510 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 266

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502671 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.463948 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 3 | 0.445783 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.423817 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 5 | 0.406434 | `use_azure_openai_models` | ❌ |

---

## Test 267

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the knowledge source <source-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.417775 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 2 | 0.373515 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.282160 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.261925 | `get_azure_container_details` | ❌ |
| 5 | 0.256054 | `get_azure_signalr_details` | ❌ |

---

## Test 268

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642314 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.626649 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 3 | 0.459611 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.420373 | `get_azure_signalr_details` | ❌ |
| 5 | 0.417992 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 269

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.465949 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 2 | 0.459306 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.363115 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.321897 | `get_azure_signalr_details` | ❌ |
| 5 | 0.307899 | `get_azure_container_details` | ❌ |

---

## Test 270

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the OpenAI model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.454384 | `deploy_azure_ai_models` | ❌ |
| 2 | 0.442176 | `use_azure_openai_models` | ❌ |
| 3 | 0.414312 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 4 | 0.364747 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.351290 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |

---

## Test 271

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the schema for knowledge index <index-name> in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478034 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.412935 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 3 | 0.343793 | `deploy_azure_ai_models` | ❌ |
| 4 | 0.324853 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.324788 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 272

**Expected Tool:** `retrieve_azure_ai_knowledge_base_content`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566491 | `retrieve_azure_ai_knowledge_base_content` | ✅ **EXPECTED** |
| 2 | 0.464178 | `get_azure_ai_resources_details` | ❌ |
| 3 | 0.312344 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.305023 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.304988 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |

---

## Test 273

**Expected Tool:** `retrieve_azure_ai_knowledge_base_content`  
**Prompt:** Find information about <query> using knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564315 | `retrieve_azure_ai_knowledge_base_content` | ✅ **EXPECTED** |
| 2 | 0.477702 | `get_azure_ai_resources_details` | ❌ |
| 3 | 0.330689 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.327179 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.321705 | `get_azure_messaging_service_details` | ❌ |

---

## Test 274

**Expected Tool:** `retrieve_azure_ai_knowledge_base_content`  
**Prompt:** Query knowledge base <agent-name> in search service <service-name> about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516858 | `retrieve_azure_ai_knowledge_base_content` | ✅ **EXPECTED** |
| 2 | 0.465631 | `get_azure_ai_resources_details` | ❌ |
| 3 | 0.353677 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.324901 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.306547 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 275

**Expected Tool:** `retrieve_azure_ai_knowledge_base_content`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in Azure AI Search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669464 | `retrieve_azure_ai_knowledge_base_content` | ✅ **EXPECTED** |
| 2 | 0.561928 | `get_azure_ai_resources_details` | ❌ |
| 3 | 0.450609 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.412094 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.390953 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 276

**Expected Tool:** `retrieve_azure_ai_knowledge_base_content`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.524146 | `retrieve_azure_ai_knowledge_base_content` | ✅ **EXPECTED** |
| 2 | 0.399606 | `get_azure_ai_resources_details` | ❌ |
| 3 | 0.322840 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.267991 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.264021 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 277

**Expected Tool:** `retrieve_azure_ai_knowledge_base_content`  
**Prompt:** Search knowledge base <agent-name> in Azure AI Search service <service-name> for <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691488 | `retrieve_azure_ai_knowledge_base_content` | ✅ **EXPECTED** |
| 2 | 0.627605 | `get_azure_ai_resources_details` | ❌ |
| 3 | 0.462028 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.421014 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.414851 | `get_azure_messaging_service_details` | ❌ |

---

## Test 278

**Expected Tool:** `retrieve_azure_ai_knowledge_base_content`  
**Prompt:** What does knowledge base <agent-name> in search service <service-name> know about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530883 | `retrieve_azure_ai_knowledge_base_content` | ✅ **EXPECTED** |
| 2 | 0.439737 | `get_azure_ai_resources_details` | ❌ |
| 3 | 0.308946 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.286399 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.282951 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 279

**Expected Tool:** `use_azure_openai_models`  
**Prompt:** Create a chat completion with the message "Hello, how are you today?"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.389976 | `use_azure_openai_models` | ✅ **EXPECTED** |
| 2 | 0.254263 | `send_azure_communication_messages` | ❌ |
| 3 | 0.205840 | `generate_azure_cli_commands` | ❌ |
| 4 | 0.184587 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.150659 | `connect_azure_ai_foundry_agents` | ❌ |

---

## Test 280

**Expected Tool:** `use_azure_openai_models`  
**Prompt:** Create a completion with the prompt "What is Azure?"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.517506 | `generate_azure_cli_commands` | ❌ |
| 2 | 0.472396 | `use_azure_openai_models` | ✅ **EXPECTED** |
| 3 | 0.388792 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.377460 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.367700 | `install_azure_cli_extensions` | ❌ |

---

## Test 281

**Expected Tool:** `use_azure_openai_models`  
**Prompt:** Create vector embeddings for my text using Azure OpenAI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682093 | `use_azure_openai_models` | ✅ **EXPECTED** |
| 2 | 0.414225 | `recognize_speech_from_audio` | ❌ |
| 3 | 0.403170 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 4 | 0.401921 | `generate_azure_cli_commands` | ❌ |
| 5 | 0.398779 | `deploy_azure_ai_models` | ❌ |

---

## Test 282

**Expected Tool:** `use_azure_openai_models`  
**Prompt:** Generate embeddings for the text "Azure OpenAI Service"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620210 | `use_azure_openai_models` | ✅ **EXPECTED** |
| 2 | 0.417187 | `recognize_speech_from_audio` | ❌ |
| 3 | 0.416775 | `generate_azure_cli_commands` | ❌ |
| 4 | 0.388115 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 5 | 0.376550 | `get_azure_ai_resources_details` | ❌ |

---

## Test 283

**Expected Tool:** `connect_azure_ai_foundry_agents`  
**Prompt:** Query an agent in my AI foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.575247 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 2 | 0.514647 | `connect_azure_ai_foundry_agents` | ✅ **EXPECTED** |
| 3 | 0.510270 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.495795 | `get_azure_ai_resources_details` | ❌ |
| 5 | 0.392199 | `deploy_azure_ai_models` | ❌ |

---

## Test 284

**Expected Tool:** `query_and_evaluate_azure_ai_foundry_agents`  
**Prompt:** Query and evaluate an agent in my AI Foundry project for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.596620 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 2 | 0.590134 | `query_and_evaluate_azure_ai_foundry_agents` | ✅ **EXPECTED** |
| 3 | 0.423700 | `connect_azure_ai_foundry_agents` | ❌ |
| 4 | 0.383588 | `get_azure_ai_resources_details` | ❌ |
| 5 | 0.365679 | `deploy_azure_ai_models` | ❌ |

---

## Test 285

**Expected Tool:** `evaluate_azure_ai_foundry_agents`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.425995 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 2 | 0.364688 | `evaluate_azure_ai_foundry_agents` | ✅ **EXPECTED** |
| 3 | 0.262513 | `get_azure_resource_and_app_health_status` | ❌ |
| 4 | 0.249584 | `get_azure_ai_resources_details` | ❌ |
| 5 | 0.239002 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 286

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623815 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.475469 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.433314 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.430956 | `get_azure_app_resource_details` | ❌ |
| 5 | 0.427589 | `get_azure_messaging_service_details` | ❌ |

---

## Test 287

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.601381 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.478682 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.448552 | `get_azure_container_details` | ❌ |
| 4 | 0.427685 | `create_azure_storage` | ❌ |
| 5 | 0.415661 | `get_azure_app_config_settings` | ❌ |

---

## Test 288

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510965 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.478409 | `create_azure_storage` | ❌ |
| 3 | 0.453205 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.357190 | `get_azure_container_details` | ❌ |
| 5 | 0.336886 | `get_azure_security_configurations` | ❌ |

---

## Test 289

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.497304 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.463738 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.458514 | `create_azure_storage` | ❌ |
| 4 | 0.338640 | `get_azure_container_details` | ❌ |
| 5 | 0.309742 | `get_azure_security_configurations` | ❌ |

---

## Test 290

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611409 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.455104 | `get_azure_capacity` | ❌ |
| 3 | 0.444366 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.441300 | `create_azure_storage` | ❌ |
| 5 | 0.401717 | `get_azure_messaging_service_details` | ❌ |

---

## Test 291

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.583632 | `update_azure_managed_lustre_filesystems` | ❌ |
| 2 | 0.577262 | `create_azure_storage` | ❌ |
| 3 | 0.573528 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 4 | 0.432052 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.417417 | `get_azure_messaging_service_details` | ❌ |

---

## Test 292

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.605946 | `update_azure_managed_lustre_filesystems` | ❌ |
| 2 | 0.596710 | `create_azure_storage` | ❌ |
| 3 | 0.593813 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 4 | 0.436583 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.431655 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 293

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List the Azure Managed Lustre SKUs available in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.597221 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.541443 | `update_azure_managed_lustre_filesystems` | ❌ |
| 3 | 0.531024 | `create_azure_storage` | ❌ |
| 4 | 0.463214 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.412477 | `get_azure_capacity` | ❌ |

---

## Test 294

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.493767 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.426243 | `create_azure_storage` | ❌ |
| 3 | 0.384612 | `get_azure_security_configurations` | ❌ |
| 4 | 0.382781 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.377358 | `get_azure_capacity` | ❌ |

---

## Test 295

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.501412 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.472639 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.454001 | `create_azure_storage` | ❌ |
| 4 | 0.397280 | `get_azure_container_details` | ❌ |
| 5 | 0.328169 | `get_azure_confidential_ledger_entries` | ❌ |

---

## Test 296

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520158 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.480540 | `create_azure_storage` | ❌ |
| 3 | 0.418173 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.408751 | `get_azure_container_details` | ❌ |
| 5 | 0.354495 | `get_azure_security_configurations` | ❌ |

---

## Test 297

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591984 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.449230 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.419483 | `create_azure_storage` | ❌ |
| 4 | 0.419250 | `get_azure_container_details` | ❌ |
| 5 | 0.417457 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 298

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546641 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.457241 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.439747 | `create_azure_storage` | ❌ |
| 4 | 0.405931 | `get_azure_container_details` | ❌ |
| 5 | 0.348687 | `get_azure_app_config_settings` | ❌ |

---

## Test 299

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.559658 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.450881 | `create_azure_storage` | ❌ |
| 3 | 0.434495 | `get_azure_container_details` | ❌ |
| 4 | 0.398764 | `upload_azure_storage_blobs` | ❌ |
| 5 | 0.373892 | `get_azure_capacity` | ❌ |

---

## Test 300

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512980 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.430206 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.425170 | `create_azure_storage` | ❌ |
| 4 | 0.401776 | `get_azure_capacity` | ❌ |
| 5 | 0.397521 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 301

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Tell me how many IP addresses I need for an Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.584062 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.543203 | `create_azure_storage` | ❌ |
| 3 | 0.528023 | `update_azure_managed_lustre_filesystems` | ❌ |
| 4 | 0.382127 | `get_azure_capacity` | ❌ |
| 5 | 0.331802 | `generate_azure_cli_commands` | ❌ |

---

## Test 302

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Validate if the network <subnet_id> can host Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.574214 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.504246 | `update_azure_managed_lustre_filesystems` | ❌ |
| 3 | 0.472221 | `create_azure_storage` | ❌ |
| 4 | 0.368401 | `get_azure_capacity` | ❌ |
| 5 | 0.316710 | `publish_azure_eventgrid_events` | ❌ |

---

## Test 303

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.462833 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.431996 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.408422 | `get_azure_storage_details` | ❌ |
| 4 | 0.339283 | `create_azure_key_vault_secrets` | ❌ |
| 5 | 0.318823 | `create_azure_key_vault_items` | ❌ |

---

## Test 304

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.460058 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.396664 | `get_azure_storage_details` | ❌ |
| 3 | 0.358873 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.328252 | `create_azure_monitor_webtests` | ❌ |
| 5 | 0.324048 | `create_azure_key_vault_items` | ❌ |

---

## Test 305

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555692 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.477943 | `get_azure_storage_details` | ❌ |
| 3 | 0.469204 | `create_azure_sql_databases_and_servers` | ❌ |
| 4 | 0.441689 | `create_azure_key_vault_secrets` | ❌ |
| 5 | 0.436232 | `create_azure_key_vault_items` | ❌ |

---

## Test 306

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521592 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.483638 | `create_azure_sql_databases_and_servers` | ❌ |
| 3 | 0.468735 | `get_azure_storage_details` | ❌ |
| 4 | 0.369974 | `get_azure_capacity` | ❌ |
| 5 | 0.356649 | `create_azure_load_testing` | ❌ |

---

## Test 307

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create an Azure Managed Lustre filesystem with name <filesystem_name>, size <filesystem_size>, SKU <sku>, and subnet <subnet_id> for availability zone <zone> in location <location>. Maintenance should occur on <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640777 | `update_azure_managed_lustre_filesystems` | ❌ |
| 2 | 0.626919 | `create_azure_storage` | ✅ **EXPECTED** |
| 3 | 0.533351 | `get_azure_storage_details` | ❌ |
| 4 | 0.363841 | `create_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.318408 | `create_azure_load_testing` | ❌ |

---

## Test 308

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532844 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.487471 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.440527 | `get_azure_storage_details` | ❌ |
| 4 | 0.345035 | `create_azure_key_vault_secrets` | ❌ |
| 5 | 0.326507 | `get_azure_container_details` | ❌ |

---

## Test 309

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.534269 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.450592 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.430175 | `get_azure_storage_details` | ❌ |
| 4 | 0.356380 | `create_azure_key_vault_secrets` | ❌ |
| 5 | 0.328880 | `create_azure_key_vault_items` | ❌ |

---

## Test 310

**Expected Tool:** `update_azure_managed_lustre_filesystems`  
**Prompt:** Update the maintenance window of the Azure Managed Lustre filesystem <filesystem_name> to <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.664019 | `update_azure_managed_lustre_filesystems` | ✅ **EXPECTED** |
| 2 | 0.448808 | `create_azure_storage` | ❌ |
| 3 | 0.368488 | `get_azure_storage_details` | ❌ |
| 4 | 0.327730 | `edit_azure_databases` | ❌ |
| 5 | 0.323192 | `edit_azure_workbooks` | ❌ |

---

## Test 311

**Expected Tool:** `upload_azure_storage_blobs`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623181 | `upload_azure_storage_blobs` | ✅ **EXPECTED** |
| 2 | 0.466442 | `create_azure_storage` | ❌ |
| 3 | 0.419370 | `get_azure_storage_details` | ❌ |
| 4 | 0.308477 | `update_azure_managed_lustre_filesystems` | ❌ |
| 5 | 0.268633 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 312

**Expected Tool:** `browse_azure_marketplace_products`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.424825 | `browse_azure_marketplace_products` | ✅ **EXPECTED** |
| 2 | 0.376519 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.359701 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.324878 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.323149 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 313

**Expected Tool:** `browse_azure_marketplace_products`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712278 | `browse_azure_marketplace_products` | ✅ **EXPECTED** |
| 2 | 0.379892 | `get_azure_ai_resources_details` | ❌ |
| 3 | 0.364792 | `deploy_azure_resources_and_applications` | ❌ |
| 4 | 0.353621 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 5 | 0.344772 | `get_azure_databases_details` | ❌ |

---

## Test 314

**Expected Tool:** `browse_azure_marketplace_products`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492651 | `browse_azure_marketplace_products` | ✅ **EXPECTED** |
| 2 | 0.226351 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.218384 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.215330 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 5 | 0.210581 | `get_azure_workbooks_details` | ❌ |

---

## Test 315

**Expected Tool:** `get_azure_capacity`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530253 | `get_azure_capacity` | ✅ **EXPECTED** |
| 2 | 0.397452 | `get_azure_storage_details` | ❌ |
| 3 | 0.353830 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.353146 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.350349 | `get_azure_messaging_service_details` | ❌ |

---

## Test 316

**Expected Tool:** `get_azure_capacity`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.430316 | `get_azure_capacity` | ✅ **EXPECTED** |
| 2 | 0.332100 | `get_azure_load_testing_details` | ❌ |
| 3 | 0.326530 | `get_azure_storage_details` | ❌ |
| 4 | 0.322400 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.317967 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 317

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Get the details of my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550433 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.537248 | `edit_azure_data_analytics_resources` | ❌ |
| 3 | 0.417076 | `get_azure_signalr_details` | ❌ |
| 4 | 0.413392 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.405636 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 318

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Get the details of my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555243 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.510649 | `edit_azure_data_analytics_resources` | ❌ |
| 3 | 0.453511 | `get_azure_signalr_details` | ❌ |
| 4 | 0.423299 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.415266 | `get_azure_app_resource_details` | ❌ |

---

## Test 319

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Get the details of my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509618 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.468594 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.458941 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.450924 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.432453 | `get_azure_signalr_details` | ❌ |

---

## Test 320

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all consumer groups in my event hub <event_hub_name> in namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552301 | `edit_azure_data_analytics_resources` | ❌ |
| 2 | 0.470393 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 3 | 0.379261 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.360237 | `publish_azure_eventgrid_events` | ❌ |
| 5 | 0.338916 | `get_azure_signalr_details` | ❌ |

---

## Test 321

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.445663 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.431671 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.414508 | `publish_azure_eventgrid_events` | ❌ |
| 4 | 0.350435 | `get_azure_security_configurations` | ❌ |
| 5 | 0.335767 | `edit_azure_data_analytics_resources` | ❌ |

---

## Test 322

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.514479 | `publish_azure_eventgrid_events` | ❌ |
| 2 | 0.511417 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 3 | 0.433919 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.380839 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.375303 | `get_azure_security_configurations` | ❌ |

---

## Test 323

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.517196 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.473204 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 3 | 0.440107 | `publish_azure_eventgrid_events` | ❌ |
| 4 | 0.367530 | `edit_azure_data_analytics_resources` | ❌ |
| 5 | 0.337400 | `get_azure_load_testing_details` | ❌ |

---

## Test 324

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.484235 | `publish_azure_eventgrid_events` | ❌ |
| 2 | 0.474480 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 3 | 0.397469 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.335248 | `edit_azure_data_analytics_resources` | ❌ |
| 5 | 0.329802 | `get_azure_security_configurations` | ❌ |

---

## Test 325

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Hubs in my namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.538223 | `edit_azure_data_analytics_resources` | ❌ |
| 2 | 0.490081 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 3 | 0.381229 | `publish_azure_eventgrid_events` | ❌ |
| 4 | 0.361397 | `get_azure_signalr_details` | ❌ |
| 5 | 0.332270 | `get_azure_security_configurations` | ❌ |

---

## Test 326

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Hubs namespaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.517926 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.508146 | `edit_azure_data_analytics_resources` | ❌ |
| 3 | 0.415741 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.373001 | `get_azure_security_configurations` | ❌ |
| 5 | 0.366988 | `publish_azure_eventgrid_events` | ❌ |

---

## Test 327

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.443596 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.440872 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 3 | 0.412665 | `publish_azure_eventgrid_events` | ❌ |
| 4 | 0.352198 | `get_azure_storage_details` | ❌ |
| 5 | 0.351500 | `get_azure_security_configurations` | ❌ |

---

## Test 328

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.499123 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.494840 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 3 | 0.473120 | `publish_azure_eventgrid_events` | ❌ |
| 4 | 0.366263 | `get_azure_security_configurations` | ❌ |
| 5 | 0.347265 | `edit_azure_data_analytics_resources` | ❌ |

---

## Test 329

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.473535 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.469527 | `publish_azure_eventgrid_events` | ❌ |
| 3 | 0.422468 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.341614 | `get_azure_security_configurations` | ❌ |
| 5 | 0.319377 | `browse_azure_marketplace_products` | ❌ |

---

## Test 330

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.481543 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.470487 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.453733 | `publish_azure_eventgrid_events` | ❌ |
| 4 | 0.429263 | `get_azure_security_configurations` | ❌ |
| 5 | 0.402453 | `browse_azure_marketplace_products` | ❌ |

---

## Test 331

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566739 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.483454 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 3 | 0.405819 | `publish_azure_eventgrid_events` | ❌ |
| 4 | 0.383949 | `get_azure_security_configurations` | ❌ |
| 5 | 0.375086 | `edit_azure_data_analytics_resources` | ❌ |

---

## Test 332

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498127 | `publish_azure_eventgrid_events` | ❌ |
| 2 | 0.476345 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 3 | 0.404707 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.344019 | `get_azure_security_configurations` | ❌ |
| 5 | 0.337543 | `browse_azure_marketplace_products` | ❌ |

---

## Test 333

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577320 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.422716 | `get_azure_signalr_details` | ❌ |
| 3 | 0.384758 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.365499 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.364952 | `get_azure_app_config_settings` | ❌ |

---

## Test 334

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565533 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.432405 | `get_azure_signalr_details` | ❌ |
| 3 | 0.397741 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.380696 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.379021 | `get_azure_app_config_settings` | ❌ |

---

## Test 335

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.569639 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.408956 | `get_azure_signalr_details` | ❌ |
| 3 | 0.375835 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.363340 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.347870 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 336

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529422 | `publish_azure_eventgrid_events` | ❌ |
| 2 | 0.520326 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 3 | 0.444029 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.417590 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.364815 | `get_azure_security_configurations` | ❌ |

---

## Test 337

**Expected Tool:** `edit_azure_data_analytics_resources`  
**Prompt:** Create a new consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567860 | `edit_azure_data_analytics_resources` | ✅ **EXPECTED** |
| 2 | 0.401665 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.380821 | `publish_azure_eventgrid_events` | ❌ |
| 4 | 0.343917 | `create_azure_key_vault_secrets` | ❌ |
| 5 | 0.334668 | `create_azure_key_vault_items` | ❌ |

---

## Test 338

**Expected Tool:** `edit_azure_data_analytics_resources`  
**Prompt:** Create a new event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537219 | `edit_azure_data_analytics_resources` | ✅ **EXPECTED** |
| 2 | 0.420484 | `publish_azure_eventgrid_events` | ❌ |
| 3 | 0.400204 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.365801 | `create_azure_key_vault_secrets` | ❌ |
| 5 | 0.364315 | `create_azure_key_vault_items` | ❌ |

---

## Test 339

**Expected Tool:** `edit_azure_data_analytics_resources`  
**Prompt:** Create an new namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.380823 | `edit_azure_data_analytics_resources` | ✅ **EXPECTED** |
| 2 | 0.380229 | `create_azure_key_vault_secrets` | ❌ |
| 3 | 0.351558 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.329203 | `create_azure_load_testing` | ❌ |
| 5 | 0.323437 | `get_azure_messaging_service_details` | ❌ |

---

## Test 340

**Expected Tool:** `edit_azure_data_analytics_resources`  
**Prompt:** Delete my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669651 | `edit_azure_data_analytics_resources` | ✅ **EXPECTED** |
| 2 | 0.383213 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.330286 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.311518 | `publish_azure_eventgrid_events` | ❌ |
| 5 | 0.305524 | `edit_azure_workbooks` | ❌ |

---

## Test 341

**Expected Tool:** `edit_azure_data_analytics_resources`  
**Prompt:** Delete my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658817 | `edit_azure_data_analytics_resources` | ✅ **EXPECTED** |
| 2 | 0.371998 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.359687 | `publish_azure_eventgrid_events` | ❌ |
| 4 | 0.356153 | `edit_azure_app_config_settings` | ❌ |
| 5 | 0.340131 | `edit_azure_workbooks` | ❌ |

---

## Test 342

**Expected Tool:** `edit_azure_data_analytics_resources`  
**Prompt:** Delete my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.508241 | `edit_azure_data_analytics_resources` | ✅ **EXPECTED** |
| 2 | 0.364331 | `edit_azure_app_config_settings` | ❌ |
| 3 | 0.328435 | `edit_azure_workbooks` | ❌ |
| 4 | 0.325612 | `delete_azure_database_admin_configurations` | ❌ |
| 5 | 0.323482 | `edit_azure_sql_databases_and_servers` | ❌ |

---

## Test 343

**Expected Tool:** `edit_azure_data_analytics_resources`  
**Prompt:** Update my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738618 | `edit_azure_data_analytics_resources` | ✅ **EXPECTED** |
| 2 | 0.439672 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.386800 | `update_azure_managed_lustre_filesystems` | ❌ |
| 4 | 0.373932 | `publish_azure_eventgrid_events` | ❌ |
| 5 | 0.362896 | `edit_azure_workbooks` | ❌ |

---

## Test 344

**Expected Tool:** `edit_azure_data_analytics_resources`  
**Prompt:** Update my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.708436 | `edit_azure_data_analytics_resources` | ✅ **EXPECTED** |
| 2 | 0.440297 | `update_azure_managed_lustre_filesystems` | ❌ |
| 3 | 0.431754 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.423825 | `publish_azure_eventgrid_events` | ❌ |
| 5 | 0.389301 | `edit_azure_workbooks` | ❌ |

---

## Test 345

**Expected Tool:** `edit_azure_data_analytics_resources`  
**Prompt:** Update my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.538963 | `edit_azure_data_analytics_resources` | ✅ **EXPECTED** |
| 2 | 0.416006 | `update_azure_managed_lustre_filesystems` | ❌ |
| 3 | 0.368404 | `edit_azure_workbooks` | ❌ |
| 4 | 0.368253 | `rename_azure_sql_databases` | ❌ |
| 5 | 0.352082 | `edit_azure_sql_databases_and_servers` | ❌ |

---

## Test 346

**Expected Tool:** `publish_azure_eventgrid_events`  
**Prompt:** Publish an event to Event Grid topic <topic_name> using <event_schema> with the following data <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732024 | `publish_azure_eventgrid_events` | ✅ **EXPECTED** |
| 2 | 0.365389 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.327559 | `edit_azure_data_analytics_resources` | ❌ |
| 4 | 0.289012 | `get_azure_best_practices` | ❌ |
| 5 | 0.286585 | `send_azure_communication_messages` | ❌ |

---

## Test 347

**Expected Tool:** `publish_azure_eventgrid_events`  
**Prompt:** Publish event to my Event Grid topic <topic_name> with the following events <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.647400 | `publish_azure_eventgrid_events` | ✅ **EXPECTED** |
| 2 | 0.382138 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.352434 | `edit_azure_data_analytics_resources` | ❌ |
| 4 | 0.297312 | `send_azure_communication_messages` | ❌ |
| 5 | 0.294962 | `upload_azure_storage_blobs` | ❌ |

---

## Test 348

**Expected Tool:** `publish_azure_eventgrid_events`  
**Prompt:** Send an event to Event Grid topic <topic_name> in resource group <resource_group_name> with <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593921 | `publish_azure_eventgrid_events` | ✅ **EXPECTED** |
| 2 | 0.386869 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.369923 | `edit_azure_data_analytics_resources` | ❌ |
| 4 | 0.295613 | `send_azure_communication_messages` | ❌ |
| 5 | 0.289053 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 349

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589762 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.413814 | `get_azure_databases_details` | ❌ |
| 3 | 0.398499 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.379203 | `get_azure_container_details` | ❌ |
| 5 | 0.379108 | `browse_azure_marketplace_products` | ❌ |

---

## Test 350

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546030 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.462094 | `get_azure_databases_details` | ❌ |
| 3 | 0.304208 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.295340 | `get_azure_container_details` | ❌ |
| 5 | 0.281556 | `add_azure_app_service_database` | ❌ |

---

## Test 351

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.526929 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.410927 | `get_azure_databases_details` | ❌ |
| 3 | 0.256124 | `get_azure_container_details` | ❌ |
| 4 | 0.244911 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.243279 | `rename_azure_sql_databases` | ❌ |

---

## Test 352

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512442 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.313016 | `get_azure_databases_details` | ❌ |
| 3 | 0.263223 | `get_azure_confidential_ledger_entries` | ❌ |
| 4 | 0.248948 | `get_azure_container_details` | ❌ |
| 5 | 0.229602 | `browse_azure_marketplace_products` | ❌ |

---

## Test 353

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.428985 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.359004 | `retrieve_azure_ai_knowledge_base_content` | ❌ |
| 3 | 0.305757 | `get_azure_databases_details` | ❌ |
| 4 | 0.297599 | `get_azure_ai_resources_details` | ❌ |
| 5 | 0.264111 | `browse_azure_marketplace_products` | ❌ |

---

## Test 354

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533350 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.337173 | `get_azure_databases_details` | ❌ |
| 3 | 0.337078 | `get_azure_container_details` | ❌ |
| 4 | 0.305856 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.288691 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 355

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.584941 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.420000 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.415745 | `get_azure_databases_details` | ❌ |
| 4 | 0.404683 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.400036 | `get_azure_container_details` | ❌ |

---

## Test 356

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.535152 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.443460 | `get_azure_databases_details` | ❌ |
| 3 | 0.301627 | `get_azure_container_details` | ❌ |
| 4 | 0.295149 | `rename_azure_sql_databases` | ❌ |
| 5 | 0.293066 | `add_azure_app_service_database` | ❌ |

---

## Test 357

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603734 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.416961 | `get_azure_container_details` | ❌ |
| 3 | 0.365816 | `get_azure_databases_details` | ❌ |
| 4 | 0.363498 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.363158 | `get_azure_signalr_details` | ❌ |

---

## Test 358

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.475232 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.367299 | `get_azure_databases_details` | ❌ |
| 3 | 0.251410 | `get_azure_best_practices` | ❌ |
| 4 | 0.242322 | `design_azure_architecture` | ❌ |
| 5 | 0.241889 | `publish_azure_eventgrid_events` | ❌ |

---

## Test 359

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521279 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.401926 | `get_azure_databases_details` | ❌ |
| 3 | 0.271168 | `get_azure_container_details` | ❌ |
| 4 | 0.258235 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.253747 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 360

**Expected Tool:** `create_azure_database_admin_configurations`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619667 | `create_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.497535 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.339582 | `edit_azure_databases` | ❌ |
| 4 | 0.339334 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.310935 | `rename_azure_sql_databases` | ❌ |

---

## Test 361

**Expected Tool:** `create_azure_database_admin_configurations`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769894 | `create_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.659595 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.476817 | `edit_azure_databases` | ❌ |
| 4 | 0.455047 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.452299 | `create_azure_sql_databases_and_servers` | ❌ |

---

## Test 362

**Expected Tool:** `create_azure_database_admin_configurations`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670172 | `create_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.546667 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.409535 | `create_azure_sql_databases_and_servers` | ❌ |
| 4 | 0.399140 | `rename_azure_sql_databases` | ❌ |
| 5 | 0.370078 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 363

**Expected Tool:** `delete_azure_database_admin_configurations`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.725925 | `delete_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.684225 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.507485 | `edit_azure_sql_databases_and_servers` | ❌ |
| 4 | 0.446796 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.434498 | `rename_azure_sql_databases` | ❌ |

---

## Test 364

**Expected Tool:** `delete_azure_database_admin_configurations`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691123 | `delete_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.657272 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.411100 | `edit_azure_sql_databases_and_servers` | ❌ |
| 4 | 0.410644 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.398776 | `rename_azure_sql_databases` | ❌ |

---

## Test 365

**Expected Tool:** `delete_azure_database_admin_configurations`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662278 | `delete_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.610044 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.374118 | `edit_azure_sql_databases_and_servers` | ❌ |
| 4 | 0.368223 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.351139 | `rename_azure_sql_databases` | ❌ |

---

## Test 366

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550801 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.437477 | `get_azure_databases_details` | ❌ |
| 3 | 0.418188 | `create_azure_sql_databases_and_servers` | ❌ |
| 4 | 0.411871 | `rename_azure_sql_databases` | ❌ |
| 5 | 0.370734 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 367

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659544 | `create_azure_database_admin_configurations` | ❌ |
| 2 | 0.635949 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.509175 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 4 | 0.344890 | `get_azure_security_configurations` | ❌ |
| 5 | 0.332632 | `get_azure_databases_details` | ❌ |

---

## Test 368

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498412 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.362041 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.358939 | `get_azure_security_configurations` | ❌ |
| 4 | 0.343594 | `get_azure_databases_details` | ❌ |
| 5 | 0.334656 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 369

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602455 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.445269 | `create_azure_sql_databases_and_servers` | ❌ |
| 3 | 0.424559 | `get_azure_databases_details` | ❌ |
| 4 | 0.414641 | `edit_azure_databases` | ❌ |
| 5 | 0.400359 | `get_azure_container_details` | ❌ |

---

## Test 370

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498397 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.325040 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.300449 | `add_azure_app_service_database` | ❌ |
| 4 | 0.299005 | `get_azure_databases_details` | ❌ |
| 5 | 0.294052 | `get_azure_security_configurations` | ❌ |

---

## Test 371

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659102 | `create_azure_database_admin_configurations` | ❌ |
| 2 | 0.611917 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.486410 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 4 | 0.361115 | `edit_azure_databases` | ❌ |
| 5 | 0.322908 | `get_azure_security_configurations` | ❌ |

---

## Test 372

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.515387 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.442752 | `create_azure_sql_databases_and_servers` | ❌ |
| 3 | 0.412941 | `edit_azure_databases` | ❌ |
| 4 | 0.411655 | `get_azure_databases_details` | ❌ |
| 5 | 0.370955 | `get_azure_capacity` | ❌ |

---

## Test 373

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657075 | `create_azure_database_admin_configurations` | ❌ |
| 2 | 0.595199 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.493185 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 4 | 0.358803 | `edit_azure_databases` | ❌ |
| 5 | 0.329180 | `edit_azure_sql_databases_and_servers` | ❌ |

---

## Test 374

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.451771 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.340143 | `add_azure_app_service_database` | ❌ |
| 3 | 0.332608 | `create_azure_database_admin_configurations` | ❌ |
| 4 | 0.331531 | `edit_azure_sql_databases_and_servers` | ❌ |
| 5 | 0.331515 | `create_azure_sql_databases_and_servers` | ❌ |

---

## Test 375

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591451 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.489539 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.461238 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.439012 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.434254 | `get_azure_signalr_details` | ❌ |

---

## Test 376

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525642 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.515594 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.423830 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.384930 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.373640 | `lock_unlock_azure_app_config_settings` | ❌ |

---

## Test 377

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.541013 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.472911 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.459564 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.420414 | `get_azure_security_configurations` | ❌ |
| 5 | 0.403360 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 378

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585525 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.460024 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.428348 | `get_azure_storage_details` | ❌ |
| 4 | 0.427150 | `get_azure_security_configurations` | ❌ |
| 5 | 0.410049 | `browse_azure_marketplace_products` | ❌ |

---

## Test 379

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.514903 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.394679 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.385192 | `get_azure_storage_details` | ❌ |
| 4 | 0.351749 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.345305 | `get_azure_security_configurations` | ❌ |

---

## Test 380

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.489483 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.382508 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.365207 | `get_azure_storage_details` | ❌ |
| 4 | 0.349921 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.338102 | `get_azure_security_configurations` | ❌ |

---

## Test 381

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542243 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.417443 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.385526 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.371461 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.362639 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 382

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.452265 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.277163 | `get_azure_storage_details` | ❌ |
| 3 | 0.265440 | `get_azure_security_configurations` | ❌ |
| 4 | 0.261770 | `create_azure_storage` | ❌ |
| 5 | 0.260828 | `browse_azure_marketplace_products` | ❌ |

---

## Test 383

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593337 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.419226 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.395307 | `get_azure_storage_details` | ❌ |
| 4 | 0.390515 | `get_azure_security_configurations` | ❌ |
| 5 | 0.378209 | `get_azure_databases_details` | ❌ |

---

## Test 384

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536299 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.472664 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.416305 | `get_azure_signalr_details` | ❌ |
| 4 | 0.415764 | `get_azure_security_configurations` | ❌ |
| 5 | 0.400655 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 385

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485764 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.310577 | `get_azure_security_configurations` | ❌ |
| 3 | 0.310265 | `get_azure_storage_details` | ❌ |
| 4 | 0.294726 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.292917 | `create_azure_storage` | ❌ |

---

## Test 386

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516023 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.442551 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.410279 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.394278 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.360171 | `get_azure_signalr_details` | ❌ |

---

## Test 387

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547796 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.433036 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.391606 | `get_azure_storage_details` | ❌ |
| 4 | 0.385491 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.362622 | `get_azure_messaging_service_details` | ❌ |

---

## Test 388

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510429 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.431510 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.357940 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.353883 | `get_azure_storage_details` | ❌ |
| 5 | 0.346827 | `get_azure_capacity` | ❌ |

---

## Test 389

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579908 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.459131 | `get_azure_signalr_details` | ❌ |
| 3 | 0.444584 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.424320 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.422049 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 390

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.452245 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.375014 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.357934 | `get_azure_signalr_details` | ❌ |
| 4 | 0.343177 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.322237 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 391

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542863 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.412997 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.387109 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.373553 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.363902 | `get_azure_security_configurations` | ❌ |

---

## Test 392

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.463224 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.277348 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.273850 | `get_azure_storage_details` | ❌ |
| 4 | 0.267446 | `get_azure_key_vault_items` | ❌ |
| 5 | 0.258361 | `get_azure_databases_details` | ❌ |

---

## Test 393

**Expected Tool:** `get_azure_container_details`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580085 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.423943 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.351001 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.349750 | `get_azure_storage_details` | ❌ |
| 5 | 0.349339 | `get_azure_signalr_details` | ❌ |

---

## Test 394

**Expected Tool:** `get_azure_container_details`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607448 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.511645 | `get_azure_signalr_details` | ❌ |
| 3 | 0.458216 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.457991 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.451232 | `get_azure_storage_details` | ❌ |

---

## Test 395

**Expected Tool:** `get_azure_container_details`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.489130 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.347326 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.338923 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.317925 | `get_azure_signalr_details` | ❌ |
| 5 | 0.315321 | `generate_azure_cli_commands` | ❌ |

---

## Test 396

**Expected Tool:** `get_azure_container_details`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532532 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.389232 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.362397 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.358841 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.355124 | `get_azure_signalr_details` | ❌ |

---

## Test 397

**Expected Tool:** `get_azure_virtual_desktop_details`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550251 | `get_azure_virtual_desktop_details` | ✅ **EXPECTED** |
| 2 | 0.453600 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.442910 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.412967 | `get_azure_container_details` | ❌ |
| 5 | 0.394678 | `get_azure_messaging_service_details` | ❌ |

---

## Test 398

**Expected Tool:** `get_azure_virtual_desktop_details`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607532 | `get_azure_virtual_desktop_details` | ✅ **EXPECTED** |
| 2 | 0.364740 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.319120 | `get_azure_security_configurations` | ❌ |
| 4 | 0.312479 | `get_azure_signalr_details` | ❌ |
| 5 | 0.307420 | `get_azure_container_details` | ❌ |

---

## Test 399

**Expected Tool:** `get_azure_virtual_desktop_details`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611133 | `get_azure_virtual_desktop_details` | ✅ **EXPECTED** |
| 2 | 0.335895 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.313084 | `get_azure_security_configurations` | ❌ |
| 4 | 0.283932 | `get_azure_signalr_details` | ❌ |
| 5 | 0.265858 | `get_azure_container_details` | ❌ |

---

## Test 400

**Expected Tool:** `get_azure_signalr_details`  
**Prompt:** Describe the SignalR runtime <signalr_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711335 | `get_azure_signalr_details` | ✅ **EXPECTED** |
| 2 | 0.438790 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.387114 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.373297 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.370390 | `edit_azure_data_analytics_resources` | ❌ |

---

## Test 401

**Expected Tool:** `get_azure_signalr_details`  
**Prompt:** Get information about my SignalR runtime <signalr_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729127 | `get_azure_signalr_details` | ✅ **EXPECTED** |
| 2 | 0.467563 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.452790 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.447727 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.433126 | `get_azure_app_config_settings` | ❌ |

---

## Test 402

**Expected Tool:** `get_azure_signalr_details`  
**Prompt:** List all SignalRs in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.497376 | `get_azure_signalr_details` | ✅ **EXPECTED** |
| 2 | 0.391290 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.360482 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.336211 | `get_azure_security_configurations` | ❌ |
| 5 | 0.314121 | `browse_azure_marketplace_products` | ❌ |

---

## Test 403

**Expected Tool:** `get_azure_signalr_details`  
**Prompt:** Show all the SignalRs information in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641102 | `get_azure_signalr_details` | ✅ **EXPECTED** |
| 2 | 0.500333 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.461902 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.433484 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.420390 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 404

**Expected Tool:** `get_azure_signalr_details`  
**Prompt:** Show me the details of SignalR <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.605571 | `get_azure_signalr_details` | ✅ **EXPECTED** |
| 2 | 0.428387 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.388473 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.379322 | `get_azure_app_resource_details` | ❌ |
| 5 | 0.362474 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 405

**Expected Tool:** `get_azure_signalr_details`  
**Prompt:** Show me the network information of SignalR runtime <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639126 | `get_azure_signalr_details` | ✅ **EXPECTED** |
| 2 | 0.332452 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.321748 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.317889 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.293598 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 406

**Expected Tool:** `get_azure_confidential_ledger_entries`  
**Prompt:** Get entry from Confidential Ledger for transaction <transaction_id> on ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551776 | `get_azure_confidential_ledger_entries` | ✅ **EXPECTED** |
| 2 | 0.478164 | `append_azure_confidential_ledger_entries` | ❌ |
| 3 | 0.212123 | `get_azure_key_vault_secret_values` | ❌ |
| 4 | 0.171645 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.141977 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 407

**Expected Tool:** `get_azure_confidential_ledger_entries`  
**Prompt:** Get transaction <transaction_id> from ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.398313 | `get_azure_confidential_ledger_entries` | ✅ **EXPECTED** |
| 2 | 0.394587 | `append_azure_confidential_ledger_entries` | ❌ |
| 3 | 0.154670 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.140490 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.137979 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 408

**Expected Tool:** `append_azure_confidential_ledger_entries`  
**Prompt:** Append {"hello": "from mcp"} to my confidential ledger <ledger_name> in collection <collection_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521636 | `append_azure_confidential_ledger_entries` | ✅ **EXPECTED** |
| 2 | 0.358953 | `get_azure_confidential_ledger_entries` | ❌ |
| 3 | 0.217335 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.216388 | `update_azure_managed_lustre_filesystems` | ❌ |
| 5 | 0.204048 | `edit_azure_app_config_settings` | ❌ |

---

## Test 409

**Expected Tool:** `append_azure_confidential_ledger_entries`  
**Prompt:** Append an entry to my ledger <ledger_name> with data {"key": "value"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.456250 | `append_azure_confidential_ledger_entries` | ✅ **EXPECTED** |
| 2 | 0.289949 | `get_azure_confidential_ledger_entries` | ❌ |
| 3 | 0.214352 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.197816 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.193278 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 410

**Expected Tool:** `append_azure_confidential_ledger_entries`  
**Prompt:** Create an immutable ledger entry in <ledger_name> with content {"audit": "log"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.440329 | `append_azure_confidential_ledger_entries` | ✅ **EXPECTED** |
| 2 | 0.370414 | `get_azure_confidential_ledger_entries` | ❌ |
| 3 | 0.195062 | `publish_azure_eventgrid_events` | ❌ |
| 4 | 0.186084 | `audit_azure_resources_compliance` | ❌ |
| 5 | 0.184154 | `create_azure_key_vault_items` | ❌ |

---

## Test 411

**Expected Tool:** `append_azure_confidential_ledger_entries`  
**Prompt:** Write a tamper-proof entry to ledger <ledger_name> containing {"transaction": "data"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.538340 | `append_azure_confidential_ledger_entries` | ✅ **EXPECTED** |
| 2 | 0.440278 | `get_azure_confidential_ledger_entries` | ❌ |
| 3 | 0.195564 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.181246 | `publish_azure_eventgrid_events` | ❌ |
| 5 | 0.179857 | `create_azure_key_vault_secrets` | ❌ |

---

## Test 412

**Expected Tool:** `append_azure_confidential_ledger_entries`  
**Prompt:** Write an entry to confidential ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533611 | `append_azure_confidential_ledger_entries` | ✅ **EXPECTED** |
| 2 | 0.455037 | `get_azure_confidential_ledger_entries` | ❌ |
| 3 | 0.241912 | `create_azure_key_vault_secrets` | ❌ |
| 4 | 0.179351 | `update_azure_managed_lustre_filesystems` | ❌ |
| 5 | 0.174724 | `edit_azure_app_config_settings` | ❌ |

---

## Test 413

**Expected Tool:** `send_azure_communication_messages`  
**Prompt:** Send an email from my communication service to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500057 | `send_azure_communication_messages` | ✅ **EXPECTED** |
| 2 | 0.197359 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.196910 | `publish_azure_eventgrid_events` | ❌ |
| 4 | 0.186658 | `connect_azure_ai_foundry_agents` | ❌ |
| 5 | 0.167947 | `recognize_speech_from_audio` | ❌ |

---

## Test 414

**Expected Tool:** `send_azure_communication_messages`  
**Prompt:** Send an email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.310846 | `send_azure_communication_messages` | ✅ **EXPECTED** |
| 2 | 0.159858 | `publish_azure_eventgrid_events` | ❌ |
| 3 | 0.126443 | `generate_azure_cli_commands` | ❌ |
| 4 | 0.117853 | `upload_azure_storage_blobs` | ❌ |
| 5 | 0.111964 | `edit_azure_app_config_settings` | ❌ |

---

## Test 415

**Expected Tool:** `send_azure_communication_messages`  
**Prompt:** Send an email with BCC recipients  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.351177 | `send_azure_communication_messages` | ✅ **EXPECTED** |
| 2 | 0.168924 | `append_azure_confidential_ledger_entries` | ❌ |
| 3 | 0.162872 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.156211 | `import_azure_key_vault_certificates` | ❌ |
| 5 | 0.152314 | `create_azure_workbooks` | ❌ |

---

## Test 416

**Expected Tool:** `send_azure_communication_messages`  
**Prompt:** Send an SMS message to <phone-number> saying "Hello"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.349510 | `send_azure_communication_messages` | ✅ **EXPECTED** |
| 2 | 0.135823 | `generate_azure_cli_commands` | ❌ |
| 3 | 0.127312 | `use_azure_openai_models` | ❌ |
| 4 | 0.103749 | `lock_unlock_azure_app_config_settings` | ❌ |
| 5 | 0.099666 | `get_azure_messaging_service_details` | ❌ |

---

## Test 417

**Expected Tool:** `send_azure_communication_messages`  
**Prompt:** Send an SMS with delivery receipt tracking  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553571 | `send_azure_communication_messages` | ✅ **EXPECTED** |
| 2 | 0.228969 | `append_azure_confidential_ledger_entries` | ❌ |
| 3 | 0.177229 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.168556 | `publish_azure_eventgrid_events` | ❌ |
| 5 | 0.166446 | `create_azure_load_testing` | ❌ |

---

## Test 418

**Expected Tool:** `send_azure_communication_messages`  
**Prompt:** Send broadcast SMS to <phone-number-1> and <phone-number-2> saying "Urgent notification"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.368364 | `send_azure_communication_messages` | ✅ **EXPECTED** |
| 2 | 0.136766 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.120215 | `publish_azure_eventgrid_events` | ❌ |
| 4 | 0.118743 | `create_azure_database_admin_configurations` | ❌ |
| 5 | 0.114166 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 419

**Expected Tool:** `send_azure_communication_messages`  
**Prompt:** Send email to multiple recipients: <email-address-1>, <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.329715 | `send_azure_communication_messages` | ✅ **EXPECTED** |
| 2 | 0.087988 | `edit_azure_databases` | ❌ |
| 3 | 0.083453 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.077784 | `create_azure_workbooks` | ❌ |
| 5 | 0.076582 | `use_azure_openai_models` | ❌ |

---

## Test 420

**Expected Tool:** `send_azure_communication_messages`  
**Prompt:** Send email with CC to <email-address-1> and <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.344829 | `send_azure_communication_messages` | ✅ **EXPECTED** |
| 2 | 0.107081 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.086260 | `create_azure_database_admin_configurations` | ❌ |
| 4 | 0.085471 | `append_azure_confidential_ledger_entries` | ❌ |
| 5 | 0.084815 | `upload_azure_storage_blobs` | ❌ |

---

## Test 421

**Expected Tool:** `send_azure_communication_messages`  
**Prompt:** Send email with custom sender name <sender-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.267887 | `send_azure_communication_messages` | ✅ **EXPECTED** |
| 2 | 0.159272 | `rename_azure_sql_databases` | ❌ |
| 3 | 0.128130 | `edit_azure_databases` | ❌ |
| 4 | 0.116966 | `create_azure_database_admin_configurations` | ❌ |
| 5 | 0.113303 | `publish_azure_eventgrid_events` | ❌ |

---

## Test 422

**Expected Tool:** `send_azure_communication_messages`  
**Prompt:** Send email with reply-to address set to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.258019 | `send_azure_communication_messages` | ✅ **EXPECTED** |
| 2 | 0.145024 | `edit_azure_app_config_settings` | ❌ |
| 3 | 0.139142 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.118904 | `edit_azure_databases` | ❌ |
| 5 | 0.116188 | `create_azure_database_admin_configurations` | ❌ |

---

## Test 423

**Expected Tool:** `send_azure_communication_messages`  
**Prompt:** Send HTML-formatted email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.240446 | `send_azure_communication_messages` | ✅ **EXPECTED** |
| 2 | 0.122166 | `publish_azure_eventgrid_events` | ❌ |
| 3 | 0.093117 | `generate_azure_cli_commands` | ❌ |
| 4 | 0.092980 | `create_azure_monitor_webtests` | ❌ |
| 5 | 0.081360 | `use_azure_openai_models` | ❌ |

---

## Test 424

**Expected Tool:** `send_azure_communication_messages`  
**Prompt:** Send SMS from my communication service to <phone-number-1>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502877 | `send_azure_communication_messages` | ✅ **EXPECTED** |
| 2 | 0.190918 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.171099 | `connect_azure_ai_foundry_agents` | ❌ |
| 4 | 0.160623 | `use_azure_openai_models` | ❌ |
| 5 | 0.141456 | `add_azure_app_service_database` | ❌ |

---

## Test 425

**Expected Tool:** `send_azure_communication_messages`  
**Prompt:** Send SMS message with custom tracking tag "campaign1"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.450517 | `send_azure_communication_messages` | ✅ **EXPECTED** |
| 2 | 0.149276 | `append_azure_confidential_ledger_entries` | ❌ |
| 3 | 0.136341 | `use_azure_openai_models` | ❌ |
| 4 | 0.134519 | `create_azure_load_testing` | ❌ |
| 5 | 0.133742 | `create_azure_monitor_webtests` | ❌ |

---

## Test 426

**Expected Tool:** `send_azure_communication_messages`  
**Prompt:** Send SMS to <phone-number-2> from <phone-number-1> with message "Test message"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.397926 | `send_azure_communication_messages` | ✅ **EXPECTED** |
| 2 | 0.160968 | `create_azure_load_testing` | ❌ |
| 3 | 0.140980 | `rename_azure_sql_databases` | ❌ |
| 4 | 0.136239 | `create_azure_monitor_webtests` | ❌ |
| 5 | 0.134762 | `update_azure_load_testing_configurations` | ❌ |

---

## Test 427

**Expected Tool:** `send_azure_communication_messages`  
**Prompt:** Send SMS to multiple recipients: <phone-number-1>, <phone-number-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.432998 | `send_azure_communication_messages` | ✅ **EXPECTED** |
| 2 | 0.107918 | `edit_azure_databases` | ❌ |
| 3 | 0.105595 | `use_azure_openai_models` | ❌ |
| 4 | 0.098489 | `create_azure_database_admin_configurations` | ❌ |
| 5 | 0.093663 | `install_azure_cli_extensions` | ❌ |

---

## Test 428

**Expected Tool:** `send_azure_communication_messages`  
**Prompt:** Send SMS with delivery reporting enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.508975 | `send_azure_communication_messages` | ✅ **EXPECTED** |
| 2 | 0.198925 | `audit_azure_resources_compliance` | ❌ |
| 3 | 0.182134 | `create_azure_monitor_webtests` | ❌ |
| 4 | 0.180919 | `publish_azure_eventgrid_events` | ❌ |
| 5 | 0.179284 | `get_azure_messaging_service_details` | ❌ |

---

## Test 429

**Expected Tool:** `recognize_speech_from_audio`  
**Prompt:** Convert speech to text from audio file <file_path> using endpoint <endpoint>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536383 | `recognize_speech_from_audio` | ✅ **EXPECTED** |
| 2 | 0.242680 | `use_azure_openai_models` | ❌ |
| 3 | 0.208042 | `generate_azure_cli_commands` | ❌ |
| 4 | 0.194268 | `deploy_azure_ai_models` | ❌ |
| 5 | 0.186059 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |

---

## Test 430

**Expected Tool:** `recognize_speech_from_audio`  
**Prompt:** Convert speech to text with comma-separated phrase hints: "Azure, cognitive services, API"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527723 | `recognize_speech_from_audio` | ✅ **EXPECTED** |
| 2 | 0.447401 | `use_azure_openai_models` | ❌ |
| 3 | 0.438006 | `generate_azure_cli_commands` | ❌ |
| 4 | 0.357181 | `send_azure_communication_messages` | ❌ |
| 5 | 0.327587 | `retrieve_azure_ai_knowledge_base_content` | ❌ |

---

## Test 431

**Expected Tool:** `recognize_speech_from_audio`  
**Prompt:** Convert speech to text with detailed output format from audio file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512680 | `recognize_speech_from_audio` | ✅ **EXPECTED** |
| 2 | 0.209836 | `generate_azure_cli_commands` | ❌ |
| 3 | 0.177107 | `use_azure_openai_models` | ❌ |
| 4 | 0.156184 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.133229 | `get_azure_ai_resources_details` | ❌ |

---

## Test 432

**Expected Tool:** `recognize_speech_from_audio`  
**Prompt:** Convert this audio file to text using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.764832 | `recognize_speech_from_audio` | ✅ **EXPECTED** |
| 2 | 0.405604 | `use_azure_openai_models` | ❌ |
| 3 | 0.393590 | `generate_azure_cli_commands` | ❌ |
| 4 | 0.376904 | `upload_azure_storage_blobs` | ❌ |
| 5 | 0.371891 | `send_azure_communication_messages` | ❌ |

---

## Test 433

**Expected Tool:** `recognize_speech_from_audio`  
**Prompt:** Recognize speech from <file_path> with phrase hints for better accuracy  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.397874 | `recognize_speech_from_audio` | ✅ **EXPECTED** |
| 2 | 0.257631 | `use_azure_openai_models` | ❌ |
| 3 | 0.253810 | `generate_azure_cli_commands` | ❌ |
| 4 | 0.217932 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.183380 | `retrieve_azure_ai_knowledge_base_content` | ❌ |

---

## Test 434

**Expected Tool:** `recognize_speech_from_audio`  
**Prompt:** Recognize speech from my audio file with language detection  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.493098 | `recognize_speech_from_audio` | ✅ **EXPECTED** |
| 2 | 0.245003 | `use_azure_openai_models` | ❌ |
| 3 | 0.222850 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.183681 | `deploy_azure_ai_models` | ❌ |
| 5 | 0.172007 | `generate_azure_cli_commands` | ❌ |

---

## Test 435

**Expected Tool:** `recognize_speech_from_audio`  
**Prompt:** Transcribe audio using multiple phrase hints: "Azure", "cognitive services", "machine learning"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.539450 | `recognize_speech_from_audio` | ✅ **EXPECTED** |
| 2 | 0.453170 | `generate_azure_cli_commands` | ❌ |
| 3 | 0.451471 | `use_azure_openai_models` | ❌ |
| 4 | 0.361161 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.348465 | `deploy_azure_ai_models` | ❌ |

---

## Test 436

**Expected Tool:** `recognize_speech_from_audio`  
**Prompt:** Transcribe audio with raw profanity output from file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.359302 | `recognize_speech_from_audio` | ✅ **EXPECTED** |
| 2 | 0.194397 | `generate_azure_cli_commands` | ❌ |
| 3 | 0.170664 | `use_azure_openai_models` | ❌ |
| 4 | 0.146774 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.134187 | `deploy_azure_ai_models` | ❌ |

---

## Test 437

**Expected Tool:** `recognize_speech_from_audio`  
**Prompt:** Transcribe speech from audio file <file_path> with profanity filtering  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.380445 | `recognize_speech_from_audio` | ✅ **EXPECTED** |
| 2 | 0.188454 | `use_azure_openai_models` | ❌ |
| 3 | 0.170544 | `generate_azure_cli_commands` | ❌ |
| 4 | 0.161234 | `query_and_evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.124053 | `deploy_azure_ai_models` | ❌ |

---

## Test 438

**Expected Tool:** `recognize_speech_from_audio`  
**Prompt:** Transcribe the audio file <file_path> in Spanish language  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.352922 | `recognize_speech_from_audio` | ✅ **EXPECTED** |
| 2 | 0.198862 | `generate_azure_cli_commands` | ❌ |
| 3 | 0.146925 | `use_azure_openai_models` | ❌ |
| 4 | 0.122758 | `create_azure_storage` | ❌ |
| 5 | 0.119987 | `update_azure_managed_lustre_filesystems` | ❌ |

---

## Summary

**Total Prompts Tested:** 438  
**Analysis Execution Time:** 85.3316282s  

### Success Rate Metrics

**Top Choice Success:** 76.3% (334/438 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 0.0% (0/438 tests)  
**🎯 High Confidence (≥0.7):** 3.2% (14/438 tests)  
**✅ Good Confidence (≥0.6):** 18.0% (79/438 tests)  
**👍 Fair Confidence (≥0.5):** 59.4% (260/438 tests)  
**👌 Acceptable Confidence (≥0.4):** 87.0% (381/438 tests)  
**❌ Low Confidence (<0.4):** 13.0% (57/438 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 0.0% (0/438 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 3.2% (14/438 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 17.1% (75/438 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 50.0% (219/438 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 67.8% (297/438 tests)  

### Success Rate Analysis

🟠 **Fair** - The tool selection system needs significant improvement.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

