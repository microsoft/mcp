# Tool Selection Analysis Setup

**Setup completed:** 2025-09-30 13:34:15  
**Tool count:** 42  
**Database setup time:** 1.6216539s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-09-30 13:34:15  
**Tool count:** 42  

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
- [Test 20: get_azure_app_resource_details](#test-20)
- [Test 21: get_azure_app_resource_details](#test-21)
- [Test 22: get_azure_app_resource_details](#test-22)
- [Test 23: get_azure_app_resource_details](#test-23)
- [Test 24: get_azure_app_resource_details](#test-24)
- [Test 25: get_azure_app_resource_details](#test-25)
- [Test 26: get_azure_app_resource_details](#test-26)
- [Test 27: get_azure_app_resource_details](#test-27)
- [Test 28: get_azure_app_resource_details](#test-28)
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
- [Test 68: edit_azure_databases](#test-68)
- [Test 69: edit_azure_databases](#test-69)
- [Test 70: get_azure_resource_and_app_health_status](#test-70)
- [Test 71: get_azure_resource_and_app_health_status](#test-71)
- [Test 72: get_azure_resource_and_app_health_status](#test-72)
- [Test 73: get_azure_resource_and_app_health_status](#test-73)
- [Test 74: get_azure_resource_and_app_health_status](#test-74)
- [Test 75: get_azure_resource_and_app_health_status](#test-75)
- [Test 76: get_azure_resource_and_app_health_status](#test-76)
- [Test 77: get_azure_resource_and_app_health_status](#test-77)
- [Test 78: get_azure_resource_and_app_health_status](#test-78)
- [Test 79: get_azure_resource_and_app_health_status](#test-79)
- [Test 80: get_azure_resource_and_app_health_status](#test-80)
- [Test 81: get_azure_resource_and_app_health_status](#test-81)
- [Test 82: get_azure_resource_and_app_health_status](#test-82)
- [Test 83: get_azure_resource_and_app_health_status](#test-83)
- [Test 84: get_azure_resource_and_app_health_status](#test-84)
- [Test 85: get_azure_resource_and_app_health_status](#test-85)
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
- [Test 105: deploy_resources_and_applications_to_azure](#test-105)
- [Test 106: deploy_resources_and_applications_to_azure](#test-106)
- [Test 107: deploy_resources_and_applications_to_azure](#test-107)
- [Test 108: deploy_resources_and_applications_to_azure](#test-108)
- [Test 109: get_azure_app_config_settings](#test-109)
- [Test 110: get_azure_app_config_settings](#test-110)
- [Test 111: get_azure_app_config_settings](#test-111)
- [Test 112: get_azure_app_config_settings](#test-112)
- [Test 113: get_azure_app_config_settings](#test-113)
- [Test 114: get_azure_app_config_settings](#test-114)
- [Test 115: edit_azure_app_config_settings](#test-115)
- [Test 116: edit_azure_app_config_settings](#test-116)
- [Test 117: lock_unlock_azure_app_config_settings](#test-117)
- [Test 118: lock_unlock_azure_app_config_settings](#test-118)
- [Test 119: edit_azure_workbooks](#test-119)
- [Test 120: edit_azure_workbooks](#test-120)
- [Test 121: create_azure_workbooks](#test-121)
- [Test 122: get_azure_workbooks_details](#test-122)
- [Test 123: get_azure_workbooks_details](#test-123)
- [Test 124: get_azure_workbooks_details](#test-124)
- [Test 125: get_azure_workbooks_details](#test-125)
- [Test 126: audit_azure_resources_compliance](#test-126)
- [Test 127: audit_azure_resources_compliance](#test-127)
- [Test 128: audit_azure_resources_compliance](#test-128)
- [Test 129: get_azure_security_configurations](#test-129)
- [Test 130: get_azure_security_configurations](#test-130)
- [Test 131: get_azure_key_vault](#test-131)
- [Test 132: get_azure_key_vault](#test-132)
- [Test 133: get_azure_key_vault](#test-133)
- [Test 134: get_azure_key_vault](#test-134)
- [Test 135: get_azure_key_vault](#test-135)
- [Test 136: get_azure_key_vault](#test-136)
- [Test 137: get_azure_key_vault](#test-137)
- [Test 138: get_azure_key_vault](#test-138)
- [Test 139: get_azure_key_vault](#test-139)
- [Test 140: get_azure_key_vault](#test-140)
- [Test 141: get_azure_key_vault](#test-141)
- [Test 142: get_azure_key_vault](#test-142)
- [Test 143: get_azure_key_vault](#test-143)
- [Test 144: get_azure_key_vault](#test-144)
- [Test 145: get_azure_key_vault](#test-145)
- [Test 146: get_azure_key_vault](#test-146)
- [Test 147: get_azure_key_vault](#test-147)
- [Test 148: get_azure_key_vault](#test-148)
- [Test 149: get_azure_key_vault](#test-149)
- [Test 150: get_azure_key_vault](#test-150)
- [Test 151: get_azure_key_vault](#test-151)
- [Test 152: get_azure_key_vault](#test-152)
- [Test 153: get_azure_key_vault](#test-153)
- [Test 154: get_azure_key_vault](#test-154)
- [Test 155: get_azure_key_vault](#test-155)
- [Test 156: get_azure_key_vault](#test-156)
- [Test 157: get_azure_key_vault](#test-157)
- [Test 158: get_azure_key_vault](#test-158)
- [Test 159: create_azure_key_vault_items](#test-159)
- [Test 160: create_azure_key_vault_items](#test-160)
- [Test 161: create_azure_key_vault_items](#test-161)
- [Test 162: create_azure_key_vault_items](#test-162)
- [Test 163: create_azure_key_vault_items](#test-163)
- [Test 164: create_azure_key_vault_items](#test-164)
- [Test 165: create_azure_key_vault_items](#test-165)
- [Test 166: create_azure_key_vault_items](#test-166)
- [Test 167: create_azure_key_vault_items](#test-167)
- [Test 168: create_azure_key_vault_items](#test-168)
- [Test 169: create_azure_key_vault_items](#test-169)
- [Test 170: create_azure_key_vault_items](#test-170)
- [Test 171: create_azure_key_vault_items](#test-171)
- [Test 172: create_azure_key_vault_items](#test-172)
- [Test 173: create_azure_key_vault_items](#test-173)
- [Test 174: import_azure_key_vault_certificates](#test-174)
- [Test 175: import_azure_key_vault_certificates](#test-175)
- [Test 176: import_azure_key_vault_certificates](#test-176)
- [Test 177: import_azure_key_vault_certificates](#test-177)
- [Test 178: import_azure_key_vault_certificates](#test-178)
- [Test 179: get_azure_best_practices](#test-179)
- [Test 180: get_azure_best_practices](#test-180)
- [Test 181: get_azure_best_practices](#test-181)
- [Test 182: get_azure_best_practices](#test-182)
- [Test 183: get_azure_best_practices](#test-183)
- [Test 184: get_azure_best_practices](#test-184)
- [Test 185: get_azure_best_practices](#test-185)
- [Test 186: get_azure_best_practices](#test-186)
- [Test 187: get_azure_best_practices](#test-187)
- [Test 188: get_azure_best_practices](#test-188)
- [Test 189: get_azure_best_practices](#test-189)
- [Test 190: design_azure_architecture](#test-190)
- [Test 191: design_azure_architecture](#test-191)
- [Test 192: design_azure_architecture](#test-192)
- [Test 193: design_azure_architecture](#test-193)
- [Test 194: design_azure_architecture](#test-194)
- [Test 195: get_azure_load_testing_details](#test-195)
- [Test 196: get_azure_load_testing_details](#test-196)
- [Test 197: get_azure_load_testing_details](#test-197)
- [Test 198: get_azure_load_testing_details](#test-198)
- [Test 199: create_azure_load_testing](#test-199)
- [Test 200: create_azure_load_testing](#test-200)
- [Test 201: create_azure_load_testing](#test-201)
- [Test 202: update_azure_load_testing_configurations](#test-202)
- [Test 203: get_azure_ai_resources_details](#test-203)
- [Test 204: get_azure_ai_resources_details](#test-204)
- [Test 205: get_azure_ai_resources_details](#test-205)
- [Test 206: get_azure_ai_resources_details](#test-206)
- [Test 207: get_azure_ai_resources_details](#test-207)
- [Test 208: get_azure_ai_resources_details](#test-208)
- [Test 209: get_azure_ai_resources_details](#test-209)
- [Test 210: get_azure_ai_resources_details](#test-210)
- [Test 211: get_azure_ai_resources_details](#test-211)
- [Test 212: get_azure_ai_resources_details](#test-212)
- [Test 213: get_azure_ai_resources_details](#test-213)
- [Test 214: get_azure_ai_resources_details](#test-214)
- [Test 215: get_azure_ai_resources_details](#test-215)
- [Test 216: get_azure_ai_resources_details](#test-216)
- [Test 217: get_azure_ai_resources_details](#test-217)
- [Test 218: deploy_azure_ai_models](#test-218)
- [Test 219: connect_azure_ai_foundry_agents](#test-219)
- [Test 220: evaluate_azure_ai_foundry_agents](#test-220)
- [Test 221: get_azure_storage_details](#test-221)
- [Test 222: get_azure_storage_details](#test-222)
- [Test 223: get_azure_storage_details](#test-223)
- [Test 224: get_azure_storage_details](#test-224)
- [Test 225: get_azure_storage_details](#test-225)
- [Test 226: get_azure_storage_details](#test-226)
- [Test 227: get_azure_storage_details](#test-227)
- [Test 228: get_azure_storage_details](#test-228)
- [Test 229: get_azure_storage_details](#test-229)
- [Test 230: get_azure_storage_details](#test-230)
- [Test 231: get_azure_storage_details](#test-231)
- [Test 232: get_azure_storage_details](#test-232)
- [Test 233: get_azure_storage_details](#test-233)
- [Test 234: get_azure_storage_details](#test-234)
- [Test 235: get_azure_storage_details](#test-235)
- [Test 236: create_azure_storage](#test-236)
- [Test 237: create_azure_storage](#test-237)
- [Test 238: create_azure_storage](#test-238)
- [Test 239: create_azure_storage](#test-239)
- [Test 240: create_azure_storage](#test-240)
- [Test 241: create_azure_storage](#test-241)
- [Test 242: upload_azure_storage_blobs](#test-242)
- [Test 243: get_azure_cache_for_redis_details](#test-243)
- [Test 244: get_azure_cache_for_redis_details](#test-244)
- [Test 245: get_azure_cache_for_redis_details](#test-245)
- [Test 246: get_azure_cache_for_redis_details](#test-246)
- [Test 247: get_azure_cache_for_redis_details](#test-247)
- [Test 248: get_azure_cache_for_redis_details](#test-248)
- [Test 249: get_azure_cache_for_redis_details](#test-249)
- [Test 250: get_azure_cache_for_redis_details](#test-250)
- [Test 251: get_azure_cache_for_redis_details](#test-251)
- [Test 252: get_azure_cache_for_redis_details](#test-252)
- [Test 253: browse_azure_marketplace_products](#test-253)
- [Test 254: browse_azure_marketplace_products](#test-254)
- [Test 255: browse_azure_marketplace_products](#test-255)
- [Test 256: get_azure_capacity](#test-256)
- [Test 257: get_azure_capacity](#test-257)
- [Test 258: get_azure_capacity](#test-258)
- [Test 259: get_azure_messaging_service_details](#test-259)
- [Test 260: get_azure_messaging_service_details](#test-260)
- [Test 261: get_azure_messaging_service_details](#test-261)
- [Test 262: get_azure_messaging_service_details](#test-262)
- [Test 263: get_azure_messaging_service_details](#test-263)
- [Test 264: get_azure_messaging_service_details](#test-264)
- [Test 265: get_azure_messaging_service_details](#test-265)
- [Test 266: get_azure_messaging_service_details](#test-266)
- [Test 267: get_azure_messaging_service_details](#test-267)
- [Test 268: get_azure_messaging_service_details](#test-268)
- [Test 269: get_azure_messaging_service_details](#test-269)
- [Test 270: get_azure_messaging_service_details](#test-270)
- [Test 271: get_azure_messaging_service_details](#test-271)
- [Test 272: get_azure_messaging_service_details](#test-272)
- [Test 273: get_azure_data_explorer_kusto_details](#test-273)
- [Test 274: get_azure_data_explorer_kusto_details](#test-274)
- [Test 275: get_azure_data_explorer_kusto_details](#test-275)
- [Test 276: get_azure_data_explorer_kusto_details](#test-276)
- [Test 277: get_azure_data_explorer_kusto_details](#test-277)
- [Test 278: get_azure_data_explorer_kusto_details](#test-278)
- [Test 279: get_azure_data_explorer_kusto_details](#test-279)
- [Test 280: get_azure_data_explorer_kusto_details](#test-280)
- [Test 281: get_azure_data_explorer_kusto_details](#test-281)
- [Test 282: get_azure_data_explorer_kusto_details](#test-282)
- [Test 283: get_azure_data_explorer_kusto_details](#test-283)
- [Test 284: create_azure_database_admin_configurations](#test-284)
- [Test 285: create_azure_database_admin_configurations](#test-285)
- [Test 286: create_azure_database_admin_configurations](#test-286)
- [Test 287: delete_azure_database_admin_configurations](#test-287)
- [Test 288: delete_azure_database_admin_configurations](#test-288)
- [Test 289: delete_azure_database_admin_configurations](#test-289)
- [Test 290: get_azure_database_admin_configuration_details](#test-290)
- [Test 291: get_azure_database_admin_configuration_details](#test-291)
- [Test 292: get_azure_database_admin_configuration_details](#test-292)
- [Test 293: get_azure_database_admin_configuration_details](#test-293)
- [Test 294: get_azure_database_admin_configuration_details](#test-294)
- [Test 295: get_azure_database_admin_configuration_details](#test-295)
- [Test 296: get_azure_database_admin_configuration_details](#test-296)
- [Test 297: get_azure_database_admin_configuration_details](#test-297)
- [Test 298: get_azure_database_admin_configuration_details](#test-298)
- [Test 299: get_azure_container_details](#test-299)
- [Test 300: get_azure_container_details](#test-300)
- [Test 301: get_azure_container_details](#test-301)
- [Test 302: get_azure_container_details](#test-302)
- [Test 303: get_azure_container_details](#test-303)
- [Test 304: get_azure_container_details](#test-304)
- [Test 305: get_azure_container_details](#test-305)
- [Test 306: get_azure_container_details](#test-306)
- [Test 307: get_azure_container_details](#test-307)
- [Test 308: get_azure_container_details](#test-308)
- [Test 309: get_azure_container_details](#test-309)
- [Test 310: get_azure_container_details](#test-310)
- [Test 311: get_azure_container_details](#test-311)
- [Test 312: get_azure_container_details](#test-312)
- [Test 313: get_azure_container_details](#test-313)
- [Test 314: get_azure_container_details](#test-314)
- [Test 315: get_azure_container_details](#test-315)
- [Test 316: get_azure_container_details](#test-316)
- [Test 317: get_azure_container_details](#test-317)
- [Test 318: get_azure_container_details](#test-318)
- [Test 319: get_azure_container_details](#test-319)
- [Test 320: get_azure_container_details](#test-320)
- [Test 321: get_azure_virtual_desktop_details](#test-321)
- [Test 322: get_azure_virtual_desktop_details](#test-322)
- [Test 323: get_azure_virtual_desktop_details](#test-323)

---

## Test 1

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** List all resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638889 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.420089 | `get_azure_security_configurations` | ❌ |
| 3 | 0.384567 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.382376 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.368405 | `get_azure_storage_details` | ❌ |

---

## Test 2

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.415793 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.383878 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.328704 | `get_azure_security_configurations` | ❌ |
| 4 | 0.317407 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.290558 | `get_azure_storage_details` | ❌ |

---

## Test 3

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549609 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.418811 | `get_azure_security_configurations` | ❌ |
| 3 | 0.364712 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.363601 | `get_azure_storage_details` | ❌ |
| 5 | 0.358284 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 4

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.347561 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.305506 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.278264 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.242552 | `get_azure_security_configurations` | ❌ |
| 5 | 0.206525 | `get_azure_key_vault` | ❌ |

---

## Test 5

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671044 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.437174 | `get_azure_security_configurations` | ❌ |
| 3 | 0.399337 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.381286 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.379037 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 6

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.322378 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.291113 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.246133 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.199535 | `get_azure_resource_and_app_health_status` | ❌ |
| 5 | 0.193922 | `get_azure_capacity` | ❌ |

---

## Test 7

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.380158 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.361939 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.274746 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.227908 | `get_azure_container_details` | ❌ |
| 5 | 0.227901 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 8

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Add a CosmosDB database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.475947 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.460199 | `get_azure_databases_details` | ❌ |
| 3 | 0.441557 | `edit_azure_databases` | ❌ |
| 4 | 0.360392 | `lock_unlock_azure_app_config_settings` | ❌ |
| 5 | 0.353615 | `create_azure_database_admin_configurations` | ❌ |

---

## Test 9

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Add a database connection to my app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.416690 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.399204 | `edit_azure_databases` | ❌ |
| 3 | 0.339497 | `get_azure_databases_details` | ❌ |
| 4 | 0.330529 | `create_azure_database_admin_configurations` | ❌ |
| 5 | 0.325063 | `lock_unlock_azure_app_config_settings` | ❌ |

---

## Test 10

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Add a MySQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510959 | `edit_azure_databases` | ❌ |
| 2 | 0.461314 | `get_azure_databases_details` | ❌ |
| 3 | 0.447485 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 4 | 0.328092 | `create_azure_database_admin_configurations` | ❌ |
| 5 | 0.319773 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 11

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Add a PostgreSQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488067 | `edit_azure_databases` | ❌ |
| 2 | 0.423578 | `get_azure_databases_details` | ❌ |
| 3 | 0.415055 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 4 | 0.298856 | `create_azure_database_admin_configurations` | ❌ |
| 5 | 0.282619 | `deploy_azure_ai_models` | ❌ |

---

## Test 12

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Add database <database_name> on server <database_server> to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.445528 | `edit_azure_databases` | ❌ |
| 2 | 0.440446 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 3 | 0.436024 | `get_azure_databases_details` | ❌ |
| 4 | 0.360971 | `create_azure_database_admin_configurations` | ❌ |
| 5 | 0.305096 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 13

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Add database <database_name> with retry policy to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.409196 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.380948 | `edit_azure_databases` | ❌ |
| 3 | 0.359762 | `get_azure_databases_details` | ❌ |
| 4 | 0.330075 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.318303 | `lock_unlock_azure_app_config_settings` | ❌ |

---

## Test 14

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Configure a SQL Server database for app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512464 | `edit_azure_databases` | ❌ |
| 2 | 0.497479 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 3 | 0.456119 | `lock_unlock_azure_app_config_settings` | ❌ |
| 4 | 0.438142 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.429642 | `create_azure_database_admin_configurations` | ❌ |

---

## Test 15

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Configure tenant <tenant> for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.438242 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.427201 | `edit_azure_databases` | ❌ |
| 3 | 0.397353 | `lock_unlock_azure_app_config_settings` | ❌ |
| 4 | 0.364309 | `get_azure_databases_details` | ❌ |
| 5 | 0.357462 | `get_azure_app_config_settings` | ❌ |

---

## Test 16

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.535574 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.464895 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.438587 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.412412 | `get_azure_resource_and_app_health_status` | ❌ |
| 5 | 0.376779 | `get_azure_best_practices` | ❌ |

---

## Test 17

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622593 | `get_azure_app_config_settings` | ❌ |
| 2 | 0.517288 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 3 | 0.480175 | `lock_unlock_azure_app_config_settings` | ❌ |
| 4 | 0.434907 | `get_azure_best_practices` | ❌ |
| 5 | 0.415185 | `edit_azure_app_config_settings` | ❌ |

---

## Test 18

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488083 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.444736 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.419295 | `get_azure_resource_and_app_health_status` | ❌ |
| 4 | 0.360835 | `get_azure_storage_details` | ❌ |
| 5 | 0.345517 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 19

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.528620 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.516861 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.498775 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.433480 | `get_azure_storage_details` | ❌ |
| 5 | 0.427611 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 20

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.508222 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.448105 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.427352 | `get_azure_security_configurations` | ❌ |
| 4 | 0.421921 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.409234 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 21

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.494647 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.462941 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.441714 | `get_azure_resource_and_app_health_status` | ❌ |
| 4 | 0.385066 | `execute_azure_cli` | ❌ |
| 5 | 0.383867 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 22

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Set connection string for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.468293 | `edit_azure_databases` | ❌ |
| 2 | 0.420307 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 3 | 0.395735 | `lock_unlock_azure_app_config_settings` | ❌ |
| 4 | 0.366333 | `get_azure_databases_details` | ❌ |
| 5 | 0.364732 | `get_azure_app_config_settings` | ❌ |

---

## Test 23

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581479 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.514721 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.430445 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.411448 | `get_azure_resource_and_app_health_status` | ❌ |
| 5 | 0.401384 | `get_azure_messaging_service_details` | ❌ |

---

## Test 24

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520882 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.469871 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.462611 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.444986 | `get_azure_security_configurations` | ❌ |
| 5 | 0.437162 | `get_azure_app_config_settings` | ❌ |

---

## Test 25

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595243 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.570557 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.445049 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.394452 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.391607 | `get_azure_storage_details` | ❌ |

---

## Test 26

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.499709 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.433102 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.428962 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.390991 | `get_azure_capacity` | ❌ |
| 5 | 0.390919 | `get_azure_best_practices` | ❌ |

---

## Test 27

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.404196 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.315885 | `get_azure_resource_and_app_health_status` | ❌ |
| 3 | 0.305243 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.269663 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.262952 | `execute_azure_cli` | ❌ |

---

## Test 28

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.517747 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.438212 | `get_azure_resource_and_app_health_status` | ❌ |
| 3 | 0.419467 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.388259 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.360351 | `get_azure_best_practices` | ❌ |

---

## Test 29

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.394526 | `get_azure_database_admin_configuration_details` | ❌ |
| 2 | 0.378321 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 3 | 0.346100 | `edit_azure_databases` | ❌ |
| 4 | 0.312577 | `create_azure_database_admin_configurations` | ❌ |
| 5 | 0.311368 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 30

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516062 | `get_azure_database_admin_configuration_details` | ❌ |
| 2 | 0.515479 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.369218 | `edit_azure_databases` | ❌ |
| 4 | 0.347208 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 5 | 0.341826 | `get_azure_app_resource_details` | ❌ |

---

## Test 31

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478446 | `get_azure_database_admin_configuration_details` | ❌ |
| 2 | 0.449240 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.375171 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 4 | 0.373528 | `edit_azure_databases` | ❌ |
| 5 | 0.339970 | `get_azure_app_resource_details` | ❌ |

---

## Test 32

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all Azure SQL servers in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.437870 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.423838 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 3 | 0.422601 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.420824 | `create_azure_database_admin_configurations` | ❌ |
| 5 | 0.417101 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 33

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.483707 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.470240 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.466822 | `get_azure_storage_details` | ❌ |
| 4 | 0.449054 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.439142 | `get_azure_security_configurations` | ❌ |

---

## Test 34

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550049 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.447529 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.420211 | `delete_azure_database_admin_configurations` | ❌ |
| 4 | 0.415459 | `edit_azure_databases` | ❌ |
| 5 | 0.403496 | `create_azure_database_admin_configurations` | ❌ |

---

## Test 35

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.459813 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.353887 | `edit_azure_databases` | ❌ |
| 3 | 0.255202 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.238100 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.224949 | `get_azure_security_configurations` | ❌ |

---

## Test 36

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496481 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.418841 | `edit_azure_databases` | ❌ |
| 3 | 0.335264 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.322067 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.305614 | `get_azure_messaging_service_details` | ❌ |

---

## Test 37

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.411217 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.321958 | `edit_azure_databases` | ❌ |
| 3 | 0.243270 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.230828 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.203815 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 38

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.455154 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.397754 | `edit_azure_databases` | ❌ |
| 3 | 0.341460 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.323444 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.319877 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 39

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.416288 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.319283 | `edit_azure_databases` | ❌ |
| 3 | 0.224069 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.224061 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.219312 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 40

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.375280 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.295866 | `edit_azure_databases` | ❌ |
| 3 | 0.217704 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.204516 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.202386 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 41

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485364 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.427326 | `get_azure_storage_details` | ❌ |
| 3 | 0.423698 | `create_azure_storage` | ❌ |
| 4 | 0.393277 | `get_azure_container_details` | ❌ |
| 5 | 0.360627 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 42

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521537 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.388467 | `get_azure_storage_details` | ❌ |
| 3 | 0.372579 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.356233 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.340541 | `get_azure_security_configurations` | ❌ |

---

## Test 43

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.400787 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.313451 | `edit_azure_databases` | ❌ |
| 3 | 0.260782 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.239619 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.224217 | `browse_azure_marketplace_products` | ❌ |

---

## Test 44

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.357470 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.282259 | `edit_azure_databases` | ❌ |
| 3 | 0.237418 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.230730 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.211605 | `create_azure_database_admin_configurations` | ❌ |

---

## Test 45

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.558434 | `get_azure_database_admin_configuration_details` | ❌ |
| 2 | 0.530328 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 3 | 0.486536 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.468156 | `edit_azure_databases` | ❌ |
| 5 | 0.421194 | `get_azure_app_resource_details` | ❌ |

---

## Test 46

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me every SQL server available in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.459302 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.424009 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.422684 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 4 | 0.399212 | `get_azure_storage_details` | ❌ |
| 5 | 0.389735 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 47

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.318903 | `edit_azure_databases` | ❌ |
| 2 | 0.251389 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 3 | 0.215865 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.165104 | `create_azure_database_admin_configurations` | ❌ |
| 5 | 0.164953 | `get_azure_app_config_settings` | ❌ |

---

## Test 48

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.494153 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.446933 | `get_azure_storage_details` | ❌ |
| 3 | 0.421776 | `get_azure_security_configurations` | ❌ |
| 4 | 0.396421 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.389268 | `browse_azure_marketplace_products` | ❌ |

---

## Test 49

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.404968 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.357185 | `edit_azure_databases` | ❌ |
| 3 | 0.270829 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.234680 | `delete_azure_database_admin_configurations` | ❌ |
| 5 | 0.227130 | `get_azure_security_configurations` | ❌ |

---

## Test 50

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.380591 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.354214 | `edit_azure_databases` | ❌ |
| 3 | 0.274988 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.240477 | `delete_azure_database_admin_configurations` | ❌ |
| 5 | 0.234628 | `create_azure_database_admin_configurations` | ❌ |

---

## Test 51

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the configuration of MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.424259 | `edit_azure_databases` | ❌ |
| 2 | 0.345881 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 3 | 0.319072 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.282061 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.217174 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 52

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.396395 | `edit_azure_databases` | ❌ |
| 2 | 0.305067 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.302014 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 4 | 0.250062 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.200991 | `create_azure_database_admin_configurations` | ❌ |

---

## Test 53

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.467713 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.403564 | `create_azure_storage` | ❌ |
| 3 | 0.391129 | `get_azure_storage_details` | ❌ |
| 4 | 0.377338 | `get_azure_container_details` | ❌ |
| 5 | 0.327793 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 54

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488113 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.481986 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 3 | 0.473445 | `get_azure_storage_details` | ❌ |
| 4 | 0.458261 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.444953 | `get_azure_security_configurations` | ❌ |

---

## Test 55

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496822 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.373904 | `get_azure_storage_details` | ❌ |
| 3 | 0.368375 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.336256 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.323139 | `get_azure_security_configurations` | ❌ |

---

## Test 56

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549663 | `get_azure_database_admin_configuration_details` | ❌ |
| 2 | 0.489035 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.483888 | `get_azure_storage_details` | ❌ |
| 4 | 0.476130 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.464348 | `get_azure_app_config_settings` | ❌ |

---

## Test 57

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.430729 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.415590 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.358991 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.354816 | `edit_azure_databases` | ❌ |
| 5 | 0.343500 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 58

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.425531 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.382341 | `get_azure_ai_resources_details` | ❌ |
| 3 | 0.377916 | `get_azure_storage_details` | ❌ |
| 4 | 0.346397 | `get_azure_container_details` | ❌ |
| 5 | 0.328296 | `browse_azure_marketplace_products` | ❌ |

---

## Test 59

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.443295 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.358413 | `edit_azure_databases` | ❌ |
| 3 | 0.250030 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.239673 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.215038 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 60

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.511744 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.461478 | `edit_azure_databases` | ❌ |
| 3 | 0.364350 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.353379 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.349028 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 61

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.406462 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.332482 | `edit_azure_databases` | ❌ |
| 3 | 0.245580 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.227159 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.203381 | `create_azure_database_admin_configurations` | ❌ |

---

## Test 62

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469147 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.433769 | `edit_azure_databases` | ❌ |
| 3 | 0.360519 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.343634 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.339215 | `browse_azure_marketplace_products` | ❌ |

---

## Test 63

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.383189 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.314466 | `edit_azure_databases` | ❌ |
| 3 | 0.225609 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.221419 | `get_azure_best_practices` | ❌ |
| 5 | 0.219501 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 64

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.343871 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.284665 | `edit_azure_databases` | ❌ |
| 3 | 0.214231 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.199822 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.193724 | `get_azure_best_practices` | ❌ |

---

## Test 65

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.421279 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.335190 | `edit_azure_databases` | ❌ |
| 3 | 0.249102 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.243773 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.210444 | `get_azure_security_configurations` | ❌ |

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
| 4 | 0.224141 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.188657 | `create_azure_database_admin_configurations` | ❌ |

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
| 5 | 0.192494 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 68

**Expected Tool:** `edit_azure_databases`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.340843 | `edit_azure_databases` | ✅ **EXPECTED** |
| 2 | 0.250565 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.234603 | `get_azure_databases_details` | ❌ |
| 4 | 0.212755 | `delete_azure_database_admin_configurations` | ❌ |
| 5 | 0.160632 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 69

**Expected Tool:** `edit_azure_databases`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.380868 | `edit_azure_databases` | ✅ **EXPECTED** |
| 2 | 0.269323 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.251993 | `delete_azure_database_admin_configurations` | ❌ |
| 4 | 0.213409 | `get_azure_databases_details` | ❌ |
| 5 | 0.174321 | `connect_azure_ai_foundry_agents` | ❌ |

---

## Test 70

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521504 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.402104 | `create_azure_load_testing` | ❌ |
| 3 | 0.398005 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.397775 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.383437 | `get_azure_capacity` | ❌ |

---

## Test 71

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542512 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.426884 | `get_azure_capacity` | ❌ |
| 3 | 0.381282 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.370434 | `get_azure_ai_resources_details` | ❌ |
| 5 | 0.368141 | `create_azure_load_testing` | ❌ |

---

## Test 72

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.339028 | `get_azure_storage_details` | ❌ |
| 2 | 0.315432 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 3 | 0.272938 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.271450 | `get_azure_capacity` | ❌ |
| 5 | 0.269633 | `get_azure_messaging_service_details` | ❌ |

---

## Test 73

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.337793 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.260055 | `get_azure_storage_details` | ❌ |
| 3 | 0.253643 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.243989 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.237046 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 74

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.409198 | `get_azure_storage_details` | ❌ |
| 2 | 0.379831 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 3 | 0.374257 | `get_azure_capacity` | ❌ |
| 4 | 0.325422 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.305299 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 75

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.486374 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.389526 | `get_azure_capacity` | ❌ |
| 3 | 0.375039 | `create_azure_load_testing` | ❌ |
| 4 | 0.370705 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.369547 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 76

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.427197 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.407964 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.398563 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.396444 | `get_azure_databases_details` | ❌ |
| 5 | 0.383614 | `create_azure_workbooks` | ❌ |

---

## Test 77

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.470427 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.455463 | `get_azure_databases_details` | ❌ |
| 3 | 0.445863 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.440818 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.438518 | `get_azure_security_configurations` | ❌ |

---

## Test 78

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498165 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.463936 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.452333 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.437304 | `create_azure_workbooks` | ❌ |
| 5 | 0.419394 | `get_azure_security_configurations` | ❌ |

---

## Test 79

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.419335 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.356965 | `get_azure_load_testing_details` | ❌ |
| 3 | 0.349385 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.331350 | `get_azure_storage_details` | ❌ |
| 5 | 0.321614 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 80

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.428737 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.418527 | `get_azure_workbooks_details` | ❌ |
| 3 | 0.398748 | `create_azure_workbooks` | ❌ |
| 4 | 0.393663 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.376728 | `get_azure_databases_details` | ❌ |

---

## Test 81

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536715 | `get_azure_storage_details` | ❌ |
| 2 | 0.525913 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.486032 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 4 | 0.478110 | `get_azure_capacity` | ❌ |
| 5 | 0.460629 | `get_azure_messaging_service_details` | ❌ |

---

## Test 82

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476038 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.430555 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.423122 | `get_azure_best_practices` | ❌ |
| 4 | 0.369889 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.368140 | `browse_azure_marketplace_products` | ❌ |

---

## Test 83

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.548400 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.453872 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.440214 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.416257 | `get_azure_security_configurations` | ❌ |
| 5 | 0.416176 | `get_azure_capacity` | ❌ |

---

## Test 84

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.378029 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.272313 | `get_azure_app_resource_details` | ❌ |
| 3 | 0.249202 | `execute_azure_cli` | ❌ |
| 4 | 0.247328 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.246532 | `get_azure_app_config_settings` | ❌ |

---

## Test 85

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.344644 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.269942 | `get_azure_storage_details` | ❌ |
| 3 | 0.264069 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.253364 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.252579 | `get_azure_capacity` | ❌ |

---

## Test 86

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577907 | `get_azure_storage_details` | ❌ |
| 2 | 0.430781 | `get_azure_capacity` | ❌ |
| 3 | 0.399711 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 4 | 0.392511 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.383711 | `get_azure_app_config_settings` | ❌ |

---

## Test 87

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me code optimization recommendations for all Application Insights resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512832 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.492356 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.464604 | `get_azure_best_practices` | ❌ |
| 4 | 0.421210 | `get_azure_capacity` | ❌ |
| 5 | 0.400675 | `browse_azure_marketplace_products` | ❌ |

---

## Test 88

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520253 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.488696 | `create_azure_workbooks` | ❌ |
| 3 | 0.451337 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.410409 | `get_azure_security_configurations` | ❌ |
| 5 | 0.405448 | `edit_azure_workbooks` | ❌ |

---

## Test 89

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485261 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.431076 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 3 | 0.392623 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.368812 | `get_azure_capacity` | ❌ |
| 5 | 0.356368 | `get_azure_best_practices` | ❌ |

---

## Test 90

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.438406 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.405799 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.396006 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.387063 | `create_azure_workbooks` | ❌ |
| 5 | 0.384415 | `get_azure_databases_details` | ❌ |

---

## Test 91

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582586 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.551904 | `get_azure_storage_details` | ❌ |
| 3 | 0.488089 | `get_azure_capacity` | ❌ |
| 4 | 0.475264 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.471465 | `get_azure_security_configurations` | ❌ |

---

## Test 92

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the health status of entity <entity_id> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.569558 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.364021 | `get_azure_storage_details` | ❌ |
| 3 | 0.359140 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.340267 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.334046 | `create_azure_workbooks` | ❌ |

---

## Test 93

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560849 | `get_azure_storage_details` | ❌ |
| 2 | 0.406534 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 3 | 0.400710 | `create_azure_storage` | ❌ |
| 4 | 0.368175 | `get_azure_capacity` | ❌ |
| 5 | 0.339439 | `get_azure_security_configurations` | ❌ |

---

## Test 94

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.523533 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.479440 | `create_azure_workbooks` | ❌ |
| 3 | 0.458853 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.456725 | `get_azure_workbooks_details` | ❌ |
| 5 | 0.418907 | `browse_azure_marketplace_products` | ❌ |

---

## Test 95

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.457886 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.365358 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.333901 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.332481 | `get_azure_capacity` | ❌ |
| 5 | 0.329720 | `get_azure_workbooks_details` | ❌ |

---

## Test 96

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.445778 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.375709 | `create_azure_workbooks` | ❌ |
| 3 | 0.357168 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.336058 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.324855 | `edit_azure_workbooks` | ❌ |

---

## Test 97

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.422735 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.354368 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.346010 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.321471 | `get_azure_storage_details` | ❌ |
| 5 | 0.316126 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 98

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.435717 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.420831 | `get_azure_workbooks_details` | ❌ |
| 3 | 0.400964 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.400018 | `create_azure_workbooks` | ❌ |
| 5 | 0.367191 | `get_azure_databases_details` | ❌ |

---

## Test 99

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Use app lens to check why my app is slow?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.357303 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.258469 | `get_azure_app_resource_details` | ❌ |
| 3 | 0.234720 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.221280 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.214378 | `get_azure_app_config_settings` | ❌ |

---

## Test 100

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.322056 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.275921 | `get_azure_app_resource_details` | ❌ |
| 3 | 0.212918 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.205828 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.164353 | `get_azure_databases_details` | ❌ |

---

## Test 101

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.421696 | `get_azure_capacity` | ❌ |
| 2 | 0.410736 | `get_azure_storage_details` | ❌ |
| 3 | 0.408348 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 4 | 0.404977 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.380088 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 102

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.517790 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.397417 | `get_azure_capacity` | ❌ |
| 3 | 0.382899 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.379035 | `get_azure_storage_details` | ❌ |
| 5 | 0.378286 | `get_azure_ai_resources_details` | ❌ |

---

## Test 103

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510381 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.445813 | `get_azure_storage_details` | ❌ |
| 3 | 0.423077 | `get_azure_capacity` | ❌ |
| 4 | 0.389687 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.382850 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 104

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.434104 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.396015 | `get_azure_capacity` | ❌ |
| 3 | 0.369685 | `create_azure_load_testing` | ❌ |
| 4 | 0.353095 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.326466 | `get_azure_ai_resources_details` | ❌ |

---

## Test 105

**Expected Tool:** `deploy_resources_and_applications_to_azure`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640058 | `deploy_resources_and_applications_to_azure` | ✅ **EXPECTED** |
| 2 | 0.519278 | `deploy_azure_ai_models` | ❌ |
| 3 | 0.479918 | `get_azure_best_practices` | ❌ |
| 4 | 0.454755 | `execute_azure_developer_cli` | ❌ |
| 5 | 0.453039 | `design_azure_architecture` | ❌ |

---

## Test 106

**Expected Tool:** `deploy_resources_and_applications_to_azure`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578477 | `deploy_resources_and_applications_to_azure` | ✅ **EXPECTED** |
| 2 | 0.477740 | `deploy_azure_ai_models` | ❌ |
| 3 | 0.437318 | `execute_azure_developer_cli` | ❌ |
| 4 | 0.410719 | `get_azure_best_practices` | ❌ |
| 5 | 0.401777 | `execute_azure_cli` | ❌ |

---

## Test 107

**Expected Tool:** `deploy_resources_and_applications_to_azure`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533141 | `execute_azure_developer_cli` | ❌ |
| 2 | 0.522934 | `deploy_resources_and_applications_to_azure` | ✅ **EXPECTED** |
| 3 | 0.449771 | `get_azure_resource_and_app_health_status` | ❌ |
| 4 | 0.396308 | `execute_azure_cli` | ❌ |
| 5 | 0.393841 | `deploy_azure_ai_models` | ❌ |

---

## Test 108

**Expected Tool:** `deploy_resources_and_applications_to_azure`  
**Prompt:** Show me the rules to generate bicep scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488008 | `get_azure_best_practices` | ❌ |
| 2 | 0.384841 | `deploy_resources_and_applications_to_azure` | ✅ **EXPECTED** |
| 3 | 0.325324 | `create_azure_database_admin_configurations` | ❌ |
| 4 | 0.315430 | `execute_azure_cli` | ❌ |
| 5 | 0.313187 | `search_microsoft_docs` | ❌ |

---

## Test 109

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549804 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.418698 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.401422 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.400003 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.388838 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 110

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.605174 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.469735 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.413315 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.310582 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.304660 | `update_azure_load_testing_configurations` | ❌ |

---

## Test 111

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.517123 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.397359 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.318242 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.302856 | `get_azure_app_resource_details` | ❌ |
| 5 | 0.300071 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 112

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564754 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.445478 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.396571 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.382377 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.377431 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 113

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619236 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.496884 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.413994 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.320934 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.303617 | `update_azure_load_testing_configurations` | ❌ |

---

## Test 114

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.473674 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.397489 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.319847 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.290485 | `get_azure_key_vault` | ❌ |
| 5 | 0.227918 | `get_azure_container_details` | ❌ |

---

## Test 115

**Expected Tool:** `edit_azure_app_config_settings`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480490 | `edit_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.419225 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.386234 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.236794 | `edit_azure_workbooks` | ❌ |
| 5 | 0.226127 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 116

**Expected Tool:** `edit_azure_app_config_settings`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.454677 | `lock_unlock_azure_app_config_settings` | ❌ |
| 2 | 0.419522 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.418815 | `edit_azure_app_config_settings` | ✅ **EXPECTED** |
| 4 | 0.251838 | `update_azure_load_testing_configurations` | ❌ |
| 5 | 0.227102 | `edit_azure_databases` | ❌ |

---

## Test 117

**Expected Tool:** `lock_unlock_azure_app_config_settings`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.523446 | `lock_unlock_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.367924 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.324652 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.206577 | `import_azure_key_vault_certificates` | ❌ |
| 5 | 0.186093 | `update_azure_load_testing_configurations` | ❌ |

---

## Test 118

**Expected Tool:** `lock_unlock_azure_app_config_settings`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552583 | `lock_unlock_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.393938 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.339108 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.240636 | `import_azure_key_vault_certificates` | ❌ |
| 5 | 0.224555 | `get_azure_key_vault` | ❌ |

---

## Test 119

**Expected Tool:** `edit_azure_workbooks`  
**Prompt:** Delete the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.505878 | `edit_azure_workbooks` | ✅ **EXPECTED** |
| 2 | 0.375642 | `create_azure_workbooks` | ❌ |
| 3 | 0.362979 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.265457 | `edit_azure_app_config_settings` | ❌ |
| 5 | 0.188350 | `create_azure_load_testing` | ❌ |

---

## Test 120

**Expected Tool:** `edit_azure_workbooks`  
**Prompt:** Update the workbook <workbook_resource_id> with a new text step  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496535 | `edit_azure_workbooks` | ✅ **EXPECTED** |
| 2 | 0.413187 | `create_azure_workbooks` | ❌ |
| 3 | 0.327796 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.236165 | `update_azure_load_testing_configurations` | ❌ |
| 5 | 0.216298 | `edit_azure_app_config_settings` | ❌ |

---

## Test 121

**Expected Tool:** `create_azure_workbooks`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555073 | `create_azure_workbooks` | ✅ **EXPECTED** |
| 2 | 0.400619 | `edit_azure_workbooks` | ❌ |
| 3 | 0.371495 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.196704 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.157512 | `create_azure_storage` | ❌ |

---

## Test 122

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

## Test 123

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

## Test 124

**Expected Tool:** `get_azure_workbooks_details`  
**Prompt:** Show me the workbook with display name <workbook_display_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.474463 | `get_azure_workbooks_details` | ✅ **EXPECTED** |
| 2 | 0.454790 | `create_azure_workbooks` | ❌ |
| 3 | 0.422535 | `edit_azure_workbooks` | ❌ |
| 4 | 0.201213 | `get_azure_security_configurations` | ❌ |
| 5 | 0.181802 | `browse_azure_marketplace_products` | ❌ |

---

## Test 125

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

## Test 126

**Expected Tool:** `audit_azure_resources_compliance`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546941 | `audit_azure_resources_compliance` | ✅ **EXPECTED** |
| 2 | 0.541006 | `get_azure_capacity` | ❌ |
| 3 | 0.500223 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.477992 | `get_azure_resource_and_app_health_status` | ❌ |
| 5 | 0.477388 | `get_azure_best_practices` | ❌ |

---

## Test 127

**Expected Tool:** `audit_azure_resources_compliance`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536483 | `audit_azure_resources_compliance` | ✅ **EXPECTED** |
| 2 | 0.511039 | `get_azure_best_practices` | ❌ |
| 3 | 0.490293 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.476949 | `get_azure_capacity` | ❌ |
| 5 | 0.463219 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 128

**Expected Tool:** `audit_azure_resources_compliance`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592611 | `audit_azure_resources_compliance` | ✅ **EXPECTED** |
| 2 | 0.508765 | `get_azure_best_practices` | ❌ |
| 3 | 0.492553 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.490633 | `get_azure_capacity` | ❌ |
| 5 | 0.472186 | `execute_azure_cli` | ❌ |

---

## Test 129

**Expected Tool:** `get_azure_security_configurations`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.734098 | `get_azure_security_configurations` | ✅ **EXPECTED** |
| 2 | 0.460328 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.414685 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.368134 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.356299 | `browse_azure_marketplace_products` | ❌ |

---

## Test 130

**Expected Tool:** `get_azure_security_configurations`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702749 | `get_azure_security_configurations` | ✅ **EXPECTED** |
| 2 | 0.485210 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.431017 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.388410 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.380797 | `get_azure_messaging_service_details` | ❌ |

---

## Test 131

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Display the certificate details for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527620 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.508692 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.415809 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.393808 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.326756 | `get_azure_storage_details` | ❌ |

---

## Test 132

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Display the key details for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.452262 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.393173 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.342336 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.331707 | `get_azure_storage_details` | ❌ |
| 5 | 0.325902 | `create_azure_key_vault_items` | ❌ |

---

## Test 133

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Enumerate certificates in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572328 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.516793 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 3 | 0.473039 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.374233 | `get_azure_security_configurations` | ❌ |
| 5 | 0.320632 | `get_azure_storage_details` | ❌ |

---

## Test 134

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Enumerate keys in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.486561 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.456995 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.447167 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.358826 | `get_azure_storage_details` | ❌ |
| 5 | 0.355400 | `get_azure_security_configurations` | ❌ |

---

## Test 135

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Enumerate secrets in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.508682 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.475020 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.405839 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.352439 | `get_azure_security_configurations` | ❌ |
| 5 | 0.347744 | `get_azure_storage_details` | ❌ |

---

## Test 136

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Get the certificate <certificate_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547751 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.477245 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 3 | 0.415517 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.305052 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.273675 | `get_azure_storage_details` | ❌ |

---

## Test 137

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Get the key <key_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.362855 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.355054 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.306974 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.269754 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.240656 | `get_azure_storage_details` | ❌ |

---

## Test 138

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542408 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.534958 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 3 | 0.461491 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.385604 | `get_azure_security_configurations` | ❌ |
| 5 | 0.322461 | `get_azure_storage_details` | ❌ |

---

## Test 139

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502884 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.429278 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.427597 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.378915 | `get_azure_security_configurations` | ❌ |
| 5 | 0.362933 | `get_azure_storage_details` | ❌ |

---

## Test 140

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502924 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.434446 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.372548 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.348550 | `get_azure_security_configurations` | ❌ |
| 5 | 0.316564 | `get_azure_storage_details` | ❌ |

---

## Test 141

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** List certificate names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522418 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.515660 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 3 | 0.464982 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.392176 | `get_azure_security_configurations` | ❌ |
| 5 | 0.341570 | `get_azure_storage_details` | ❌ |

---

## Test 142

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** List key names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500762 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.437312 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.409157 | `get_azure_storage_details` | ❌ |
| 4 | 0.407003 | `import_azure_key_vault_certificates` | ❌ |
| 5 | 0.389283 | `get_azure_security_configurations` | ❌ |

---

## Test 143

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** List secrets names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.489437 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.443592 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.381031 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.369698 | `get_azure_storage_details` | ❌ |
| 5 | 0.365721 | `get_azure_security_configurations` | ❌ |

---

## Test 144

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.526739 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.500287 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 3 | 0.439402 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.331729 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.303016 | `get_azure_storage_details` | ❌ |

---

## Test 145

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Retrieve key metadata for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.416058 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.353865 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.345646 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.343080 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.340225 | `get_azure_storage_details` | ❌ |

---

## Test 146

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show certificate names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557161 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.522419 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 3 | 0.462161 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.379854 | `get_azure_security_configurations` | ❌ |
| 5 | 0.331591 | `get_azure_storage_details` | ❌ |

---

## Test 147

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show key names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.508375 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.443413 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.434842 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.402007 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.394387 | `get_azure_storage_details` | ❌ |

---

## Test 148

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543362 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.500953 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 3 | 0.436563 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.300959 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.298500 | `get_azure_security_configurations` | ❌ |

---

## Test 149

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552928 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.537878 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 3 | 0.461463 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.368299 | `get_azure_security_configurations` | ❌ |
| 5 | 0.330062 | `get_azure_storage_details` | ❌ |

---

## Test 150

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.514998 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.490672 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.420402 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.409324 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.358500 | `get_azure_storage_details` | ❌ |

---

## Test 151

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.467928 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.426201 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.377132 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.374666 | `get_azure_storage_details` | ❌ |
| 5 | 0.367478 | `create_azure_key_vault_items` | ❌ |

---

## Test 152

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.436642 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.399191 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.371997 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.301224 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.286946 | `get_azure_storage_details` | ❌ |

---

## Test 153

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.499555 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.448371 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.432123 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.344132 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.341486 | `get_azure_storage_details` | ❌ |

---

## Test 154

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530658 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.461601 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.426648 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.367244 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.360507 | `get_azure_storage_details` | ❌ |

---

## Test 155

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show secrets names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.505159 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.453385 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.427298 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.356917 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.339292 | `get_azure_storage_details` | ❌ |

---

## Test 156

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** What certificates are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570872 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.566285 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 3 | 0.542657 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.354249 | `get_azure_storage_details` | ❌ |
| 5 | 0.338482 | `get_azure_security_configurations` | ❌ |

---

## Test 157

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** What keys are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532438 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.517834 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.484768 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.375040 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.371608 | `get_azure_storage_details` | ❌ |

---

## Test 158

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** What secrets are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545085 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.520862 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.438877 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.412144 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.401361 | `get_azure_storage_details` | ❌ |

---

## Test 159

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.504694 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.391544 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.340089 | `get_azure_key_vault` | ❌ |
| 4 | 0.337182 | `lock_unlock_azure_app_config_settings` | ❌ |
| 5 | 0.315313 | `edit_azure_app_config_settings` | ❌ |

---

## Test 160

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577228 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.536615 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.403721 | `get_azure_key_vault` | ❌ |
| 4 | 0.283179 | `create_azure_storage` | ❌ |
| 5 | 0.282142 | `create_azure_workbooks` | ❌ |

---

## Test 161

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.494654 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.417922 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.335176 | `get_azure_key_vault` | ❌ |
| 4 | 0.282042 | `create_azure_storage` | ❌ |
| 5 | 0.231202 | `create_azure_workbooks` | ❌ |

---

## Test 162

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551337 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.385343 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.367275 | `get_azure_key_vault` | ❌ |
| 4 | 0.301886 | `lock_unlock_azure_app_config_settings` | ❌ |
| 5 | 0.294044 | `create_azure_storage` | ❌ |

---

## Test 163

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Create an EC key with name <key_name> in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.455389 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.407173 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.338543 | `get_azure_key_vault` | ❌ |
| 4 | 0.274881 | `create_azure_storage` | ❌ |
| 5 | 0.232171 | `lock_unlock_azure_app_config_settings` | ❌ |

---

## Test 164

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Create an oct key in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502036 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.450939 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.385868 | `get_azure_key_vault` | ❌ |
| 4 | 0.301799 | `lock_unlock_azure_app_config_settings` | ❌ |
| 5 | 0.289852 | `create_azure_workbooks` | ❌ |

---

## Test 165

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Create an RSA key in the vault <key_vault_account_name> with name <key_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.519876 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.437054 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.366580 | `get_azure_key_vault` | ❌ |
| 4 | 0.280091 | `create_azure_storage` | ❌ |
| 5 | 0.249914 | `lock_unlock_azure_app_config_settings` | ❌ |

---

## Test 166

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Generate a certificate named <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.534280 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.530813 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.417740 | `get_azure_key_vault` | ❌ |
| 4 | 0.282987 | `create_azure_storage` | ❌ |
| 5 | 0.270041 | `audit_azure_resources_compliance` | ❌ |

---

## Test 167

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Generate a key <key_name> with type <key_type> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.460685 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.387664 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.343094 | `get_azure_key_vault` | ❌ |
| 4 | 0.272969 | `create_azure_storage` | ❌ |
| 5 | 0.243119 | `lock_unlock_azure_app_config_settings` | ❌ |

---

## Test 168

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Issue a certificate <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.559527 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.521991 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.440867 | `get_azure_key_vault` | ❌ |
| 4 | 0.277842 | `lock_unlock_azure_app_config_settings` | ❌ |
| 5 | 0.277213 | `create_azure_database_admin_configurations` | ❌ |

---

## Test 169

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555685 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.527468 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.396987 | `get_azure_key_vault` | ❌ |
| 4 | 0.264595 | `create_azure_storage` | ❌ |
| 5 | 0.263716 | `deploy_azure_ai_models` | ❌ |

---

## Test 170

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Request creation of certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545314 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.540640 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 3 | 0.428272 | `get_azure_key_vault` | ❌ |
| 4 | 0.286085 | `create_azure_storage` | ❌ |
| 5 | 0.283203 | `create_azure_workbooks` | ❌ |

---

## Test 171

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.439305 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.357475 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.356853 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.347919 | `get_azure_key_vault` | ❌ |
| 5 | 0.347006 | `edit_azure_app_config_settings` | ❌ |

---

## Test 172

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476932 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.402382 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.372842 | `get_azure_key_vault` | ❌ |
| 4 | 0.329065 | `lock_unlock_azure_app_config_settings` | ❌ |
| 5 | 0.301220 | `edit_azure_app_config_settings` | ❌ |

---

## Test 173

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Update secret <secret_name> to value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.411404 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.365562 | `lock_unlock_azure_app_config_settings` | ❌ |
| 3 | 0.356877 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.352415 | `get_azure_key_vault` | ❌ |
| 5 | 0.342010 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 174

**Expected Tool:** `import_azure_key_vault_certificates`  
**Prompt:** Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560409 | `import_azure_key_vault_certificates` | ✅ **EXPECTED** |
| 2 | 0.430852 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.339090 | `get_azure_key_vault` | ❌ |
| 4 | 0.221680 | `upload_azure_storage_blobs` | ❌ |
| 5 | 0.216397 | `lock_unlock_azure_app_config_settings` | ❌ |

---

## Test 175

**Expected Tool:** `import_azure_key_vault_certificates`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660982 | `import_azure_key_vault_certificates` | ✅ **EXPECTED** |
| 2 | 0.459787 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.378360 | `get_azure_key_vault` | ❌ |
| 4 | 0.256701 | `deploy_azure_ai_models` | ❌ |
| 5 | 0.240543 | `create_azure_database_admin_configurations` | ❌ |

---

## Test 176

**Expected Tool:** `import_azure_key_vault_certificates`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645826 | `import_azure_key_vault_certificates` | ✅ **EXPECTED** |
| 2 | 0.425682 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.376495 | `get_azure_key_vault` | ❌ |
| 4 | 0.249209 | `upload_azure_storage_blobs` | ❌ |
| 5 | 0.248738 | `deploy_azure_ai_models` | ❌ |

---

## Test 177

**Expected Tool:** `import_azure_key_vault_certificates`  
**Prompt:** Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630975 | `import_azure_key_vault_certificates` | ✅ **EXPECTED** |
| 2 | 0.443601 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.422833 | `get_azure_key_vault` | ❌ |
| 4 | 0.233437 | `lock_unlock_azure_app_config_settings` | ❌ |
| 5 | 0.222987 | `deploy_azure_ai_models` | ❌ |

---

## Test 178

**Expected Tool:** `import_azure_key_vault_certificates`  
**Prompt:** Upload certificate file <file_path> to key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587412 | `import_azure_key_vault_certificates` | ✅ **EXPECTED** |
| 2 | 0.451230 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.396327 | `get_azure_key_vault` | ❌ |
| 4 | 0.334084 | `upload_azure_storage_blobs` | ❌ |
| 5 | 0.287607 | `deploy_azure_ai_models` | ❌ |

---

## Test 179

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.735075 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.524574 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.474628 | `search_microsoft_docs` | ❌ |
| 4 | 0.459823 | `execute_azure_cli` | ❌ |
| 5 | 0.437042 | `get_azure_capacity` | ❌ |

---

## Test 180

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690117 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.601714 | `search_microsoft_docs` | ❌ |
| 3 | 0.539669 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.508718 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.483779 | `get_azure_capacity` | ❌ |

---

## Test 181

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713406 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.543696 | `search_microsoft_docs` | ❌ |
| 3 | 0.529617 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.470109 | `design_azure_architecture` | ❌ |
| 5 | 0.435613 | `browse_azure_marketplace_products` | ❌ |

---

## Test 182

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.683437 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.614180 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.558589 | `search_microsoft_docs` | ❌ |
| 4 | 0.465496 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.450131 | `get_azure_capacity` | ❌ |

---

## Test 183

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682026 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.556022 | `search_microsoft_docs` | ❌ |
| 3 | 0.509047 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.505735 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.443358 | `browse_azure_marketplace_products` | ❌ |

---

## Test 184

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685214 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.486075 | `search_microsoft_docs` | ❌ |
| 3 | 0.480287 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.448692 | `get_azure_app_resource_details` | ❌ |
| 5 | 0.416416 | `execute_azure_developer_cli` | ❌ |

---

## Test 185

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.675358 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.571007 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.527827 | `search_microsoft_docs` | ❌ |
| 4 | 0.497925 | `get_azure_app_resource_details` | ❌ |
| 5 | 0.435563 | `deploy_azure_ai_models` | ❌ |

---

## Test 186

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612873 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.520938 | `search_microsoft_docs` | ❌ |
| 3 | 0.518435 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.424667 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.421748 | `get_azure_app_resource_details` | ❌ |

---

## Test 187

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.489465 | `deploy_resources_and_applications_to_azure` | ❌ |
| 2 | 0.480742 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 3 | 0.440374 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.436599 | `deploy_azure_ai_models` | ❌ |
| 5 | 0.432577 | `evaluate_azure_ai_foundry_agents` | ❌ |

---

## Test 188

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618224 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.489003 | `get_azure_key_vault` | ❌ |
| 3 | 0.478352 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.444083 | `import_azure_key_vault_certificates` | ❌ |
| 5 | 0.435792 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 189

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628027 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.454934 | `get_azure_app_resource_details` | ❌ |
| 3 | 0.453910 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.451968 | `search_microsoft_docs` | ❌ |
| 5 | 0.391204 | `execute_azure_cli` | ❌ |

---

## Test 190

**Expected Tool:** `design_azure_architecture`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676638 | `design_azure_architecture` | ✅ **EXPECTED** |
| 2 | 0.481643 | `get_azure_best_practices` | ❌ |
| 3 | 0.465832 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.386867 | `execute_azure_developer_cli` | ❌ |
| 5 | 0.385801 | `audit_azure_resources_compliance` | ❌ |

---

## Test 191

**Expected Tool:** `design_azure_architecture`  
**Prompt:** Help me create a cloud service that will serve as ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.259518 | `browse_azure_marketplace_products` | ❌ |
| 2 | 0.248253 | `design_azure_architecture` | ✅ **EXPECTED** |
| 3 | 0.241293 | `create_azure_storage` | ❌ |
| 4 | 0.229913 | `get_azure_app_resource_details` | ❌ |
| 5 | 0.226031 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 192

**Expected Tool:** `design_azure_architecture`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.427652 | `upload_azure_storage_blobs` | ❌ |
| 2 | 0.413253 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.410459 | `create_azure_storage` | ❌ |
| 4 | 0.410342 | `search_microsoft_docs` | ❌ |
| 5 | 0.405913 | `design_azure_architecture` | ✅ **EXPECTED** |

---

## Test 193

**Expected Tool:** `design_azure_architecture`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.342485 | `browse_azure_marketplace_products` | ❌ |
| 2 | 0.289308 | `design_azure_architecture` | ✅ **EXPECTED** |
| 3 | 0.277705 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.276204 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.224286 | `get_azure_best_practices` | ❌ |

---

## Test 194

**Expected Tool:** `design_azure_architecture`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.323311 | `upload_azure_storage_blobs` | ❌ |
| 2 | 0.296166 | `design_azure_architecture` | ✅ **EXPECTED** |
| 3 | 0.245346 | `create_azure_storage` | ❌ |
| 4 | 0.224135 | `get_azure_capacity` | ❌ |
| 5 | 0.207400 | `get_azure_best_practices` | ❌ |

---

## Test 195

**Expected Tool:** `get_azure_load_testing_details`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609585 | `get_azure_load_testing_details` | ✅ **EXPECTED** |
| 2 | 0.568056 | `create_azure_load_testing` | ❌ |
| 3 | 0.448055 | `update_azure_load_testing_configurations` | ❌ |
| 4 | 0.366497 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.329986 | `get_azure_storage_details` | ❌ |

---

## Test 196

**Expected Tool:** `get_azure_load_testing_details`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599651 | `create_azure_load_testing` | ❌ |
| 2 | 0.581081 | `get_azure_load_testing_details` | ✅ **EXPECTED** |
| 3 | 0.457483 | `update_azure_load_testing_configurations` | ❌ |
| 4 | 0.357813 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.321301 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 197

**Expected Tool:** `get_azure_load_testing_details`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612800 | `create_azure_load_testing` | ❌ |
| 2 | 0.592725 | `get_azure_load_testing_details` | ✅ **EXPECTED** |
| 3 | 0.421873 | `update_azure_load_testing_configurations` | ❌ |
| 4 | 0.349117 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.333908 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 198

**Expected Tool:** `get_azure_load_testing_details`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669717 | `get_azure_load_testing_details` | ✅ **EXPECTED** |
| 2 | 0.609875 | `create_azure_load_testing` | ❌ |
| 3 | 0.493520 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.421963 | `get_azure_capacity` | ❌ |
| 5 | 0.411835 | `get_azure_storage_details` | ❌ |

---

## Test 199

**Expected Tool:** `create_azure_load_testing`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542817 | `create_azure_load_testing` | ✅ **EXPECTED** |
| 2 | 0.431906 | `get_azure_load_testing_details` | ❌ |
| 3 | 0.425527 | `update_azure_load_testing_configurations` | ❌ |
| 4 | 0.328438 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.303424 | `create_azure_workbooks` | ❌ |

---

## Test 200

**Expected Tool:** `create_azure_load_testing`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659823 | `create_azure_load_testing` | ✅ **EXPECTED** |
| 2 | 0.529798 | `get_azure_load_testing_details` | ❌ |
| 3 | 0.410977 | `update_azure_load_testing_configurations` | ❌ |
| 4 | 0.374224 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.334648 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 201

**Expected Tool:** `create_azure_load_testing`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585612 | `create_azure_load_testing` | ✅ **EXPECTED** |
| 2 | 0.496772 | `update_azure_load_testing_configurations` | ❌ |
| 3 | 0.460907 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.319822 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.297908 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 202

**Expected Tool:** `update_azure_load_testing_configurations`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577419 | `update_azure_load_testing_configurations` | ✅ **EXPECTED** |
| 2 | 0.501316 | `create_azure_load_testing` | ❌ |
| 3 | 0.443800 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.303358 | `edit_azure_workbooks` | ❌ |
| 5 | 0.257550 | `evaluate_azure_ai_foundry_agents` | ❌ |

---

## Test 203

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.310095 | `get_azure_app_config_settings` | ❌ |
| 2 | 0.294949 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.268668 | `get_azure_best_practices` | ❌ |
| 4 | 0.262165 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.249688 | `get_azure_workbooks_details` | ❌ |

---

## Test 204

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619823 | `deploy_azure_ai_models` | ❌ |
| 2 | 0.575424 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.474409 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.412586 | `connect_azure_ai_foundry_agents` | ❌ |
| 5 | 0.359607 | `browse_azure_marketplace_products` | ❌ |

---

## Test 205

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.517204 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.472873 | `deploy_azure_ai_models` | ❌ |
| 3 | 0.424302 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.367885 | `connect_azure_ai_foundry_agents` | ❌ |
| 5 | 0.338517 | `browse_azure_marketplace_products` | ❌ |

---

## Test 206

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530525 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.474685 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.444086 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.399309 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.391196 | `get_azure_security_configurations` | ❌ |

---

## Test 207

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.482355 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.356614 | `get_azure_security_configurations` | ❌ |
| 3 | 0.350774 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.349687 | `get_azure_databases_details` | ❌ |
| 5 | 0.343922 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 208

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522175 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.426054 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 3 | 0.407003 | `deploy_azure_ai_models` | ❌ |
| 4 | 0.330976 | `connect_azure_ai_foundry_agents` | ❌ |
| 5 | 0.288627 | `browse_azure_marketplace_products` | ❌ |

---

## Test 209

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.426451 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.296542 | `get_azure_resource_and_app_health_status` | ❌ |
| 3 | 0.280948 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.278893 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.268471 | `evaluate_azure_ai_foundry_agents` | ❌ |

---

## Test 210

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612664 | `deploy_azure_ai_models` | ❌ |
| 2 | 0.565595 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.496487 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.425410 | `connect_azure_ai_foundry_agents` | ❌ |
| 5 | 0.389499 | `browse_azure_marketplace_products` | ❌ |

---

## Test 211

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485141 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.431878 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.370196 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.362364 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.357646 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 212

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the available AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533639 | `deploy_azure_ai_models` | ❌ |
| 2 | 0.529802 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.499511 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.408332 | `connect_azure_ai_foundry_agents` | ❌ |
| 5 | 0.384895 | `browse_azure_marketplace_products` | ❌ |

---

## Test 213

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.541256 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.507116 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.458382 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.414408 | `get_azure_capacity` | ❌ |
| 5 | 0.413622 | `get_azure_app_resource_details` | ❌ |

---

## Test 214

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509173 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.430939 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.406409 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.398647 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.378065 | `get_azure_app_resource_details` | ❌ |

---

## Test 215

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.472474 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.362349 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.361055 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.349980 | `get_azure_resource_and_app_health_status` | ❌ |
| 5 | 0.345299 | `get_azure_databases_details` | ❌ |

---

## Test 216

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.511182 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.474227 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 3 | 0.414405 | `deploy_azure_ai_models` | ❌ |
| 4 | 0.337209 | `connect_azure_ai_foundry_agents` | ❌ |
| 5 | 0.316338 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 217

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the schema for knowledge index <index-name> in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498412 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.373160 | `deploy_azure_ai_models` | ❌ |
| 3 | 0.341967 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.324788 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.308722 | `get_azure_best_practices` | ❌ |

---

## Test 218

**Expected Tool:** `deploy_azure_ai_models`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.387447 | `deploy_azure_ai_models` | ✅ **EXPECTED** |
| 2 | 0.307463 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.299302 | `create_azure_load_testing` | ❌ |
| 4 | 0.240425 | `edit_azure_databases` | ❌ |
| 5 | 0.236281 | `get_azure_best_practices` | ❌ |

---

## Test 219

**Expected Tool:** `connect_azure_ai_foundry_agents`  
**Prompt:** Query an agent in my AI foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.561982 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 2 | 0.514602 | `connect_azure_ai_foundry_agents` | ✅ **EXPECTED** |
| 3 | 0.452053 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.382785 | `deploy_azure_ai_models` | ❌ |
| 5 | 0.282804 | `execute_azure_cli` | ❌ |

---

## Test 220

**Expected Tool:** `evaluate_azure_ai_foundry_agents`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.347263 | `evaluate_azure_ai_foundry_agents` | ✅ **EXPECTED** |
| 2 | 0.280105 | `get_azure_resource_and_app_health_status` | ❌ |
| 3 | 0.245804 | `execute_azure_cli` | ❌ |
| 4 | 0.238878 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.229140 | `audit_azure_resources_compliance` | ❌ |

---

## Test 221

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627423 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.475469 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.449003 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.429033 | `create_azure_storage` | ❌ |
| 5 | 0.412908 | `get_azure_container_details` | ❌ |

---

## Test 222

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580769 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.529061 | `create_azure_storage` | ❌ |
| 3 | 0.478682 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.448552 | `get_azure_container_details` | ❌ |
| 5 | 0.415661 | `get_azure_app_config_settings` | ❌ |

---

## Test 223

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587289 | `create_azure_storage` | ❌ |
| 2 | 0.514466 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 3 | 0.453206 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.357190 | `get_azure_container_details` | ❌ |
| 5 | 0.336886 | `get_azure_security_configurations` | ❌ |

---

## Test 224

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551075 | `create_azure_storage` | ❌ |
| 2 | 0.493746 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 3 | 0.463728 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.338653 | `get_azure_container_details` | ❌ |
| 5 | 0.309756 | `get_azure_security_configurations` | ❌ |

---

## Test 225

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599918 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.459444 | `create_azure_storage` | ❌ |
| 3 | 0.444366 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.428351 | `get_azure_capacity` | ❌ |
| 5 | 0.417674 | `get_azure_messaging_service_details` | ❌ |

---

## Test 226

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.575069 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.432148 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.421195 | `get_azure_capacity` | ❌ |
| 4 | 0.408405 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.389895 | `get_azure_databases_details` | ❌ |

---

## Test 227

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572276 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.455172 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.431655 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.422149 | `get_azure_capacity` | ❌ |
| 5 | 0.400784 | `get_azure_databases_details` | ❌ |

---

## Test 228

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List the Azure Managed Lustre SKUs available in <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600781 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.492224 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.455935 | `get_azure_capacity` | ❌ |
| 4 | 0.404690 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.390248 | `get_azure_ai_resources_details` | ❌ |

---

## Test 229

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.506913 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.404888 | `create_azure_storage` | ❌ |
| 3 | 0.388202 | `get_azure_capacity` | ❌ |
| 4 | 0.384632 | `get_azure_security_configurations` | ❌ |
| 5 | 0.356058 | `get_azure_messaging_service_details` | ❌ |

---

## Test 230

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546896 | `create_azure_storage` | ❌ |
| 2 | 0.483715 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 3 | 0.472639 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.397280 | `get_azure_container_details` | ❌ |
| 5 | 0.309579 | `get_azure_security_configurations` | ❌ |

---

## Test 231

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543409 | `create_azure_storage` | ❌ |
| 2 | 0.520343 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 3 | 0.418173 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.408752 | `get_azure_container_details` | ❌ |
| 5 | 0.354495 | `get_azure_security_configurations` | ❌ |

---

## Test 232

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592782 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.449229 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.434143 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.419250 | `get_azure_container_details` | ❌ |
| 5 | 0.419042 | `create_azure_storage` | ❌ |

---

## Test 233

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.524375 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.521425 | `create_azure_storage` | ❌ |
| 3 | 0.457312 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.405959 | `get_azure_container_details` | ❌ |
| 5 | 0.348768 | `get_azure_app_config_settings` | ❌ |

---

## Test 234

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543792 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.498174 | `create_azure_storage` | ❌ |
| 3 | 0.434495 | `get_azure_container_details` | ❌ |
| 4 | 0.398764 | `upload_azure_storage_blobs` | ❌ |
| 5 | 0.368710 | `get_azure_app_config_settings` | ❌ |

---

## Test 235

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520985 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.476386 | `create_azure_storage` | ❌ |
| 3 | 0.430206 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.397596 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.397401 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 236

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546204 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.431996 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.379209 | `get_azure_storage_details` | ❌ |
| 4 | 0.318029 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.304649 | `search_microsoft_docs` | ❌ |

---

## Test 237

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478014 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.354593 | `get_azure_storage_details` | ❌ |
| 3 | 0.329543 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.307994 | `create_azure_load_testing` | ❌ |
| 5 | 0.306614 | `get_azure_capacity` | ❌ |

---

## Test 238

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557678 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.441188 | `get_azure_storage_details` | ❌ |
| 3 | 0.432518 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.423712 | `upload_azure_storage_blobs` | ❌ |
| 5 | 0.395124 | `create_azure_workbooks` | ❌ |

---

## Test 239

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488344 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.437403 | `get_azure_storage_details` | ❌ |
| 3 | 0.406118 | `get_azure_capacity` | ❌ |
| 4 | 0.356649 | `create_azure_load_testing` | ❌ |
| 5 | 0.346863 | `create_azure_key_vault_items` | ❌ |

---

## Test 240

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.631937 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.487471 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.396535 | `get_azure_storage_details` | ❌ |
| 4 | 0.326507 | `get_azure_container_details` | ❌ |
| 5 | 0.316986 | `create_azure_key_vault_items` | ❌ |

---

## Test 241

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607096 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.450602 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.403164 | `get_azure_storage_details` | ❌ |
| 4 | 0.325431 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.308541 | `get_azure_container_details` | ❌ |

---

## Test 242

**Expected Tool:** `upload_azure_storage_blobs`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623181 | `upload_azure_storage_blobs` | ✅ **EXPECTED** |
| 2 | 0.528682 | `create_azure_storage` | ❌ |
| 3 | 0.375458 | `get_azure_storage_details` | ❌ |
| 4 | 0.292612 | `deploy_azure_ai_models` | ❌ |
| 5 | 0.268633 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 243

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** List all access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.598233 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.335003 | `get_azure_security_configurations` | ❌ |
| 3 | 0.304910 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.292789 | `get_azure_key_vault` | ❌ |
| 5 | 0.267608 | `get_azure_container_details` | ❌ |

---

## Test 244

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** List all databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.470782 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.389069 | `get_azure_databases_details` | ❌ |
| 3 | 0.387732 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.280568 | `get_azure_container_details` | ❌ |
| 5 | 0.254949 | `get_azure_security_configurations` | ❌ |

---

## Test 245

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** List all Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580587 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.364783 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.342455 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.340686 | `get_azure_databases_details` | ❌ |
| 5 | 0.310757 | `browse_azure_marketplace_products` | ❌ |

---

## Test 246

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** List all Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567767 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.435562 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.414456 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.396583 | `get_azure_container_details` | ❌ |
| 5 | 0.383637 | `get_azure_messaging_service_details` | ❌ |

---

## Test 247

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me my Redis Caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520317 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.290258 | `get_azure_databases_details` | ❌ |
| 3 | 0.261894 | `get_azure_container_details` | ❌ |
| 4 | 0.252540 | `get_azure_key_vault` | ❌ |
| 5 | 0.252359 | `get_azure_security_configurations` | ❌ |

---

## Test 248

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me my Redis Clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498407 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.354127 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.342390 | `get_azure_container_details` | ❌ |
| 4 | 0.298268 | `get_azure_databases_details` | ❌ |
| 5 | 0.272676 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 249

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me the access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600085 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.322520 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.316812 | `get_azure_security_configurations` | ❌ |
| 4 | 0.305854 | `get_azure_key_vault` | ❌ |
| 5 | 0.305484 | `get_azure_app_config_settings` | ❌ |

---

## Test 250

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me the databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.456959 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.379510 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.372181 | `get_azure_databases_details` | ❌ |
| 4 | 0.280782 | `get_azure_container_details` | ❌ |
| 5 | 0.238852 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 251

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me the Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553755 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.360815 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.335247 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.333569 | `get_azure_databases_details` | ❌ |
| 5 | 0.328519 | `get_azure_messaging_service_details` | ❌ |

---

## Test 252

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me the Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.538790 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.424900 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.415817 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.400228 | `get_azure_container_details` | ❌ |
| 5 | 0.380760 | `get_azure_messaging_service_details` | ❌ |

---

## Test 253

**Expected Tool:** `browse_azure_marketplace_products`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.424825 | `browse_azure_marketplace_products` | ✅ **EXPECTED** |
| 2 | 0.376519 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.344490 | `get_azure_storage_details` | ❌ |
| 4 | 0.343786 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.302227 | `get_azure_ai_resources_details` | ❌ |

---

## Test 254

**Expected Tool:** `browse_azure_marketplace_products`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712278 | `browse_azure_marketplace_products` | ✅ **EXPECTED** |
| 2 | 0.464133 | `search_microsoft_docs` | ❌ |
| 3 | 0.387115 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.364792 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.344772 | `get_azure_databases_details` | ❌ |

---

## Test 255

**Expected Tool:** `browse_azure_marketplace_products`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492651 | `browse_azure_marketplace_products` | ✅ **EXPECTED** |
| 2 | 0.227398 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.217240 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.210581 | `get_azure_workbooks_details` | ❌ |
| 5 | 0.205515 | `get_azure_storage_details` | ❌ |

---

## Test 256

**Expected Tool:** `get_azure_capacity`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.484870 | `get_azure_capacity` | ✅ **EXPECTED** |
| 2 | 0.419610 | `get_azure_storage_details` | ❌ |
| 3 | 0.353830 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.350026 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.314300 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 257

**Expected Tool:** `get_azure_capacity`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.398403 | `get_azure_capacity` | ✅ **EXPECTED** |
| 2 | 0.347081 | `get_azure_storage_details` | ❌ |
| 3 | 0.332100 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.313365 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.311621 | `get_azure_ai_resources_details` | ❌ |

---

## Test 258

**Expected Tool:** `get_azure_capacity`  
**Prompt:** Tell me how many IP addresses I need for <filesystem_size> of <amlfs_sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.304686 | `get_azure_storage_details` | ❌ |
| 2 | 0.266002 | `get_azure_capacity` | ✅ **EXPECTED** |
| 3 | 0.225046 | `execute_azure_cli` | ❌ |
| 4 | 0.215121 | `edit_azure_databases` | ❌ |
| 5 | 0.212666 | `get_azure_container_details` | ❌ |

---

## Test 259

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510172 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.431684 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.350456 | `get_azure_security_configurations` | ❌ |
| 4 | 0.326854 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.305112 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 260

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.583585 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.433919 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.380839 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.375303 | `get_azure_security_configurations` | ❌ |
| 5 | 0.355380 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 261

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.517003 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.485740 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 3 | 0.337256 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.332930 | `get_azure_security_configurations` | ❌ |
| 5 | 0.330989 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 262

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.540770 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.397469 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.329801 | `get_azure_security_configurations` | ❌ |
| 4 | 0.325418 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.312790 | `get_azure_storage_details` | ❌ |

---

## Test 263

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.508285 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.443596 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.351500 | `get_azure_security_configurations` | ❌ |
| 4 | 0.347938 | `get_azure_storage_details` | ❌ |
| 5 | 0.335511 | `get_azure_load_testing_details` | ❌ |

---

## Test 264

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.539106 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.499122 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.366263 | `get_azure_security_configurations` | ❌ |
| 4 | 0.341727 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.337568 | `get_azure_storage_details` | ❌ |

---

## Test 265

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551835 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.422468 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.341614 | `get_azure_security_configurations` | ❌ |
| 4 | 0.321405 | `get_azure_storage_details` | ❌ |
| 5 | 0.319377 | `browse_azure_marketplace_products` | ❌ |

---

## Test 266

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553724 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.470453 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.429269 | `get_azure_security_configurations` | ❌ |
| 4 | 0.402480 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.358337 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 267

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566739 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.506491 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 3 | 0.383948 | `get_azure_security_configurations` | ❌ |
| 4 | 0.348386 | `get_azure_storage_details` | ❌ |
| 5 | 0.346975 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 268

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552999 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.404707 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.344019 | `get_azure_security_configurations` | ❌ |
| 4 | 0.337543 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.317617 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 269

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602538 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.365590 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.364952 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.364635 | `get_azure_container_details` | ❌ |
| 5 | 0.354268 | `get_azure_storage_details` | ❌ |

---

## Test 270

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622635 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.380697 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.379021 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.357307 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.355318 | `get_azure_app_resource_details` | ❌ |

---

## Test 271

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615458 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.363340 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.347442 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.343578 | `get_azure_container_details` | ❌ |
| 5 | 0.336113 | `get_azure_storage_details` | ❌ |

---

## Test 272

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587666 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.444005 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.417523 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.364752 | `get_azure_security_configurations` | ❌ |
| 5 | 0.347933 | `get_azure_storage_details` | ❌ |

---

## Test 273

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589762 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.413814 | `get_azure_databases_details` | ❌ |
| 3 | 0.398499 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.385218 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.379204 | `get_azure_container_details` | ❌ |

---

## Test 274

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546030 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.462094 | `get_azure_databases_details` | ❌ |
| 3 | 0.337758 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.295340 | `get_azure_container_details` | ❌ |
| 5 | 0.284549 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 275

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527003 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.410919 | `get_azure_databases_details` | ❌ |
| 3 | 0.305694 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.263811 | `get_azure_storage_details` | ❌ |
| 5 | 0.256147 | `get_azure_container_details` | ❌ |

---

## Test 276

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512442 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.313016 | `get_azure_databases_details` | ❌ |
| 3 | 0.248948 | `get_azure_container_details` | ❌ |
| 4 | 0.242332 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.229602 | `browse_azure_marketplace_products` | ❌ |

---

## Test 277

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.428985 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.305757 | `get_azure_databases_details` | ❌ |
| 3 | 0.305238 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.264111 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.241148 | `get_azure_container_details` | ❌ |

---

## Test 278

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533350 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.337173 | `get_azure_databases_details` | ❌ |
| 3 | 0.337078 | `get_azure_container_details` | ❌ |
| 4 | 0.315634 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.313020 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 279

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.584941 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.420001 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.415745 | `get_azure_databases_details` | ❌ |
| 4 | 0.404684 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.400035 | `get_azure_container_details` | ❌ |

---

## Test 280

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.535152 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.443460 | `get_azure_databases_details` | ❌ |
| 3 | 0.328037 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.301627 | `get_azure_container_details` | ❌ |
| 5 | 0.284454 | `get_azure_storage_details` | ❌ |

---

## Test 281

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603734 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.416961 | `get_azure_container_details` | ❌ |
| 3 | 0.382673 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.365816 | `get_azure_databases_details` | ❌ |
| 5 | 0.359112 | `get_azure_storage_details` | ❌ |

---

## Test 282

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.475232 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.367299 | `get_azure_databases_details` | ❌ |
| 3 | 0.251410 | `get_azure_best_practices` | ❌ |
| 4 | 0.242322 | `design_azure_architecture` | ❌ |
| 5 | 0.241156 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 283

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521279 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.401926 | `get_azure_databases_details` | ❌ |
| 3 | 0.301709 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.271168 | `get_azure_container_details` | ❌ |
| 5 | 0.266107 | `get_azure_storage_details` | ❌ |

---

## Test 284

**Expected Tool:** `create_azure_database_admin_configurations`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619667 | `create_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.497535 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.339582 | `edit_azure_databases` | ❌ |
| 4 | 0.339238 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.210520 | `get_azure_databases_details` | ❌ |

---

## Test 285

**Expected Tool:** `create_azure_database_admin_configurations`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769894 | `create_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.659595 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.476818 | `edit_azure_databases` | ❌ |
| 4 | 0.455060 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.349711 | `get_azure_databases_details` | ❌ |

---

## Test 286

**Expected Tool:** `create_azure_database_admin_configurations`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670172 | `create_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.546667 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.370061 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.333972 | `edit_azure_databases` | ❌ |
| 5 | 0.250896 | `create_azure_workbooks` | ❌ |

---

## Test 287

**Expected Tool:** `delete_azure_database_admin_configurations`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.725926 | `delete_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.684224 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.446833 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.433064 | `edit_azure_databases` | ❌ |
| 5 | 0.365336 | `edit_azure_workbooks` | ❌ |

---

## Test 288

**Expected Tool:** `delete_azure_database_admin_configurations`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691123 | `delete_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.657273 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.410580 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.364151 | `edit_azure_databases` | ❌ |
| 5 | 0.287813 | `get_azure_security_configurations` | ❌ |

---

## Test 289

**Expected Tool:** `delete_azure_database_admin_configurations`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662278 | `delete_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.610044 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.368243 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.299807 | `edit_azure_databases` | ❌ |
| 5 | 0.250392 | `get_azure_security_configurations` | ❌ |

---

## Test 290

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550794 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.437477 | `get_azure_databases_details` | ❌ |
| 3 | 0.370734 | `delete_azure_database_admin_configurations` | ❌ |
| 4 | 0.369513 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.368293 | `edit_azure_databases` | ❌ |

---

## Test 291

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659544 | `create_azure_database_admin_configurations` | ❌ |
| 2 | 0.635949 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.509163 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 4 | 0.344890 | `get_azure_security_configurations` | ❌ |
| 5 | 0.332632 | `get_azure_databases_details` | ❌ |

---

## Test 292

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.497966 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.361563 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.358405 | `get_azure_security_configurations` | ❌ |
| 4 | 0.343348 | `get_azure_databases_details` | ❌ |
| 5 | 0.334175 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 293

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602459 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.424559 | `get_azure_databases_details` | ❌ |
| 3 | 0.414641 | `edit_azure_databases` | ❌ |
| 4 | 0.400359 | `get_azure_container_details` | ❌ |
| 5 | 0.372754 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 294

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498316 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.325040 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.299004 | `get_azure_databases_details` | ❌ |
| 4 | 0.294052 | `get_azure_security_configurations` | ❌ |
| 5 | 0.287675 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 295

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659102 | `create_azure_database_admin_configurations` | ❌ |
| 2 | 0.611917 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.486395 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 4 | 0.361115 | `edit_azure_databases` | ❌ |
| 5 | 0.322908 | `get_azure_security_configurations` | ❌ |

---

## Test 296

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.515336 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.412876 | `edit_azure_databases` | ❌ |
| 3 | 0.411604 | `get_azure_databases_details` | ❌ |
| 4 | 0.380410 | `get_azure_capacity` | ❌ |
| 5 | 0.346399 | `get_azure_container_details` | ❌ |

---

## Test 297

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657075 | `create_azure_database_admin_configurations` | ❌ |
| 2 | 0.595199 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.493189 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 4 | 0.358803 | `edit_azure_databases` | ❌ |
| 5 | 0.289735 | `get_azure_security_configurations` | ❌ |

---

## Test 298

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.451827 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.332608 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.326446 | `edit_azure_databases` | ❌ |
| 4 | 0.318149 | `get_azure_databases_details` | ❌ |
| 5 | 0.281938 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 299

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591399 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.489219 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.461213 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.439035 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.421425 | `get_azure_storage_details` | ❌ |

---

## Test 300

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525642 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.515594 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.423935 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.385731 | `execute_azure_cli` | ❌ |
| 5 | 0.384929 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 301

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.541013 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.472911 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.459563 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.420413 | `get_azure_security_configurations` | ❌ |
| 5 | 0.417176 | `get_azure_messaging_service_details` | ❌ |

---

## Test 302

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585525 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.460024 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.436814 | `get_azure_storage_details` | ❌ |
| 4 | 0.427150 | `get_azure_security_configurations` | ❌ |
| 5 | 0.420593 | `get_azure_messaging_service_details` | ❌ |

---

## Test 303

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.514903 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.394679 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.384478 | `get_azure_storage_details` | ❌ |
| 4 | 0.351749 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.349975 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 304

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.489483 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.386434 | `get_azure_storage_details` | ❌ |
| 3 | 0.382508 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.356818 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.349921 | `get_azure_load_testing_details` | ❌ |

---

## Test 305

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542243 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.417443 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.385526 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.372788 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.371461 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 306

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.452265 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.300828 | `get_azure_storage_details` | ❌ |
| 3 | 0.285116 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.265440 | `get_azure_security_configurations` | ❌ |
| 5 | 0.264853 | `create_azure_storage` | ❌ |

---

## Test 307

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593337 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.419226 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.400669 | `create_azure_storage` | ❌ |
| 4 | 0.397026 | `get_azure_storage_details` | ❌ |
| 5 | 0.390515 | `get_azure_security_configurations` | ❌ |

---

## Test 308

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536299 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.472664 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.415764 | `get_azure_security_configurations` | ❌ |
| 4 | 0.403373 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.400655 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 309

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485764 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.336273 | `create_azure_storage` | ❌ |
| 3 | 0.316603 | `get_azure_storage_details` | ❌ |
| 4 | 0.310570 | `get_azure_security_configurations` | ❌ |
| 5 | 0.302464 | `get_azure_key_vault` | ❌ |

---

## Test 310

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516160 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.442585 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.410309 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.394415 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.359805 | `get_azure_load_testing_details` | ❌ |

---

## Test 311

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547796 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.433036 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.386381 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.385491 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.382822 | `get_azure_storage_details` | ❌ |

---

## Test 312

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510432 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.431481 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.379202 | `get_azure_storage_details` | ❌ |
| 4 | 0.355078 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.343103 | `get_azure_load_testing_details` | ❌ |

---

## Test 313

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579908 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.444584 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.427430 | `get_azure_storage_details` | ❌ |
| 4 | 0.424565 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.424320 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 314

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.452245 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.375014 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.344379 | `execute_azure_cli` | ❌ |
| 4 | 0.343230 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.322236 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 315

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542863 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.412997 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.387109 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.378801 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.373530 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 316

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.463224 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.287702 | `get_azure_cache_for_redis_details` | ❌ |
| 3 | 0.287542 | `get_azure_storage_details` | ❌ |
| 4 | 0.277348 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.276767 | `get_azure_key_vault` | ❌ |

---

## Test 317

**Expected Tool:** `get_azure_container_details`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580086 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.423944 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.405032 | `execute_azure_cli` | ❌ |
| 4 | 0.351001 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.350966 | `get_azure_storage_details` | ❌ |

---

## Test 318

**Expected Tool:** `get_azure_container_details`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607448 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.458216 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.457991 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.457974 | `get_azure_storage_details` | ❌ |
| 5 | 0.448234 | `get_azure_app_config_settings` | ❌ |

---

## Test 319

**Expected Tool:** `get_azure_container_details`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.489130 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.347326 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.338797 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.315248 | `execute_azure_developer_cli` | ❌ |
| 5 | 0.314459 | `execute_azure_cli` | ❌ |

---

## Test 320

**Expected Tool:** `get_azure_container_details`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532532 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.389232 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.362397 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.358801 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.349892 | `get_azure_capacity` | ❌ |

---

## Test 321

**Expected Tool:** `get_azure_virtual_desktop_details`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550326 | `get_azure_virtual_desktop_details` | ✅ **EXPECTED** |
| 2 | 0.453746 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.442907 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.413008 | `get_azure_container_details` | ❌ |
| 5 | 0.403947 | `get_azure_messaging_service_details` | ❌ |

---

## Test 322

**Expected Tool:** `get_azure_virtual_desktop_details`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607532 | `get_azure_virtual_desktop_details` | ✅ **EXPECTED** |
| 2 | 0.364628 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.319120 | `get_azure_security_configurations` | ❌ |
| 4 | 0.307420 | `get_azure_container_details` | ❌ |
| 5 | 0.304479 | `get_azure_capacity` | ❌ |

---

## Test 323

**Expected Tool:** `get_azure_virtual_desktop_details`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611604 | `get_azure_virtual_desktop_details` | ✅ **EXPECTED** |
| 2 | 0.336240 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.313634 | `get_azure_security_configurations` | ❌ |
| 4 | 0.266594 | `get_azure_container_details` | ❌ |
| 5 | 0.262911 | `get_azure_messaging_service_details` | ❌ |

---

## Summary

**Total Prompts Tested:** 323  
**Analysis Execution Time:** 41.1595372s  

### Success Rate Metrics

**Top Choice Success:** 81.1% (262/323 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 0.0% (0/323 tests)  
**🎯 High Confidence (≥0.7):** 2.2% (7/323 tests)  
**✅ Good Confidence (≥0.6):** 13.9% (45/323 tests)  
**👍 Fair Confidence (≥0.5):** 57.0% (184/323 tests)  
**👌 Acceptable Confidence (≥0.4):** 88.9% (287/323 tests)  
**❌ Low Confidence (<0.4):** 11.1% (36/323 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 0.0% (0/323 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 2.2% (7/323 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 13.9% (45/323 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 49.5% (160/323 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 74.9% (242/323 tests)  

### Success Rate Analysis

🟠 **Fair** - The tool selection system needs significant improvement.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

