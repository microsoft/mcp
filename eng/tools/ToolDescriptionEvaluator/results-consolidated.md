# Tool Selection Analysis Setup

**Setup completed:** 2025-09-29 20:19:15  
**Tool count:** 42  
**Database setup time:** 0.8742365s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-09-29 20:19:15  
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
- [Test 141: create_azure_key_vault_items](#test-141)
- [Test 142: create_azure_key_vault_items](#test-142)
- [Test 143: create_azure_key_vault_items](#test-143)
- [Test 144: import_azure_key_vault_certificates](#test-144)
- [Test 145: import_azure_key_vault_certificates](#test-145)
- [Test 146: get_azure_best_practices](#test-146)
- [Test 147: get_azure_best_practices](#test-147)
- [Test 148: get_azure_best_practices](#test-148)
- [Test 149: get_azure_best_practices](#test-149)
- [Test 150: get_azure_best_practices](#test-150)
- [Test 151: get_azure_best_practices](#test-151)
- [Test 152: get_azure_best_practices](#test-152)
- [Test 153: get_azure_best_practices](#test-153)
- [Test 154: get_azure_best_practices](#test-154)
- [Test 155: get_azure_best_practices](#test-155)
- [Test 156: get_azure_best_practices](#test-156)
- [Test 157: design_azure_architecture](#test-157)
- [Test 158: design_azure_architecture](#test-158)
- [Test 159: design_azure_architecture](#test-159)
- [Test 160: design_azure_architecture](#test-160)
- [Test 161: design_azure_architecture](#test-161)
- [Test 162: get_azure_load_testing_details](#test-162)
- [Test 163: get_azure_load_testing_details](#test-163)
- [Test 164: get_azure_load_testing_details](#test-164)
- [Test 165: get_azure_load_testing_details](#test-165)
- [Test 166: create_azure_load_testing](#test-166)
- [Test 167: create_azure_load_testing](#test-167)
- [Test 168: create_azure_load_testing](#test-168)
- [Test 169: update_azure_load_testing_configurations](#test-169)
- [Test 170: get_azure_ai_resources_details](#test-170)
- [Test 171: get_azure_ai_resources_details](#test-171)
- [Test 172: get_azure_ai_resources_details](#test-172)
- [Test 173: get_azure_ai_resources_details](#test-173)
- [Test 174: get_azure_ai_resources_details](#test-174)
- [Test 175: get_azure_ai_resources_details](#test-175)
- [Test 176: get_azure_ai_resources_details](#test-176)
- [Test 177: get_azure_ai_resources_details](#test-177)
- [Test 178: get_azure_ai_resources_details](#test-178)
- [Test 179: get_azure_ai_resources_details](#test-179)
- [Test 180: get_azure_ai_resources_details](#test-180)
- [Test 181: get_azure_ai_resources_details](#test-181)
- [Test 182: get_azure_ai_resources_details](#test-182)
- [Test 183: get_azure_ai_resources_details](#test-183)
- [Test 184: get_azure_ai_resources_details](#test-184)
- [Test 185: deploy_azure_ai_models](#test-185)
- [Test 186: connect_azure_ai_foundry_agents](#test-186)
- [Test 187: evaluate_azure_ai_foundry_agents](#test-187)
- [Test 188: get_azure_storage_details](#test-188)
- [Test 189: get_azure_storage_details](#test-189)
- [Test 190: get_azure_storage_details](#test-190)
- [Test 191: get_azure_storage_details](#test-191)
- [Test 192: get_azure_storage_details](#test-192)
- [Test 193: get_azure_storage_details](#test-193)
- [Test 194: get_azure_storage_details](#test-194)
- [Test 195: get_azure_storage_details](#test-195)
- [Test 196: get_azure_storage_details](#test-196)
- [Test 197: get_azure_storage_details](#test-197)
- [Test 198: get_azure_storage_details](#test-198)
- [Test 199: get_azure_storage_details](#test-199)
- [Test 200: get_azure_storage_details](#test-200)
- [Test 201: get_azure_storage_details](#test-201)
- [Test 202: get_azure_storage_details](#test-202)
- [Test 203: create_azure_storage](#test-203)
- [Test 204: create_azure_storage](#test-204)
- [Test 205: create_azure_storage](#test-205)
- [Test 206: create_azure_storage](#test-206)
- [Test 207: create_azure_storage](#test-207)
- [Test 208: create_azure_storage](#test-208)
- [Test 209: upload_azure_storage_blobs](#test-209)
- [Test 210: get_azure_cache_for_redis_details](#test-210)
- [Test 211: get_azure_cache_for_redis_details](#test-211)
- [Test 212: get_azure_cache_for_redis_details](#test-212)
- [Test 213: get_azure_cache_for_redis_details](#test-213)
- [Test 214: get_azure_cache_for_redis_details](#test-214)
- [Test 215: get_azure_cache_for_redis_details](#test-215)
- [Test 216: get_azure_cache_for_redis_details](#test-216)
- [Test 217: get_azure_cache_for_redis_details](#test-217)
- [Test 218: get_azure_cache_for_redis_details](#test-218)
- [Test 219: get_azure_cache_for_redis_details](#test-219)
- [Test 220: browse_azure_marketplace_products](#test-220)
- [Test 221: browse_azure_marketplace_products](#test-221)
- [Test 222: browse_azure_marketplace_products](#test-222)
- [Test 223: get_azure_capacity](#test-223)
- [Test 224: get_azure_capacity](#test-224)
- [Test 225: get_azure_capacity](#test-225)
- [Test 226: get_azure_messaging_service_details](#test-226)
- [Test 227: get_azure_messaging_service_details](#test-227)
- [Test 228: get_azure_messaging_service_details](#test-228)
- [Test 229: get_azure_messaging_service_details](#test-229)
- [Test 230: get_azure_messaging_service_details](#test-230)
- [Test 231: get_azure_messaging_service_details](#test-231)
- [Test 232: get_azure_messaging_service_details](#test-232)
- [Test 233: get_azure_messaging_service_details](#test-233)
- [Test 234: get_azure_messaging_service_details](#test-234)
- [Test 235: get_azure_messaging_service_details](#test-235)
- [Test 236: get_azure_messaging_service_details](#test-236)
- [Test 237: get_azure_messaging_service_details](#test-237)
- [Test 238: get_azure_messaging_service_details](#test-238)
- [Test 239: get_azure_messaging_service_details](#test-239)
- [Test 240: get_azure_data_explorer_kusto_details](#test-240)
- [Test 241: get_azure_data_explorer_kusto_details](#test-241)
- [Test 242: get_azure_data_explorer_kusto_details](#test-242)
- [Test 243: get_azure_data_explorer_kusto_details](#test-243)
- [Test 244: get_azure_data_explorer_kusto_details](#test-244)
- [Test 245: get_azure_data_explorer_kusto_details](#test-245)
- [Test 246: get_azure_data_explorer_kusto_details](#test-246)
- [Test 247: get_azure_data_explorer_kusto_details](#test-247)
- [Test 248: get_azure_data_explorer_kusto_details](#test-248)
- [Test 249: get_azure_data_explorer_kusto_details](#test-249)
- [Test 250: get_azure_data_explorer_kusto_details](#test-250)
- [Test 251: create_azure_database_admin_configurations](#test-251)
- [Test 252: create_azure_database_admin_configurations](#test-252)
- [Test 253: create_azure_database_admin_configurations](#test-253)
- [Test 254: delete_azure_database_admin_configurations](#test-254)
- [Test 255: delete_azure_database_admin_configurations](#test-255)
- [Test 256: delete_azure_database_admin_configurations](#test-256)
- [Test 257: get_azure_database_admin_configuration_details](#test-257)
- [Test 258: get_azure_database_admin_configuration_details](#test-258)
- [Test 259: get_azure_database_admin_configuration_details](#test-259)
- [Test 260: get_azure_database_admin_configuration_details](#test-260)
- [Test 261: get_azure_database_admin_configuration_details](#test-261)
- [Test 262: get_azure_database_admin_configuration_details](#test-262)
- [Test 263: get_azure_database_admin_configuration_details](#test-263)
- [Test 264: get_azure_database_admin_configuration_details](#test-264)
- [Test 265: get_azure_database_admin_configuration_details](#test-265)
- [Test 266: get_azure_container_details](#test-266)
- [Test 267: get_azure_container_details](#test-267)
- [Test 268: get_azure_container_details](#test-268)
- [Test 269: get_azure_container_details](#test-269)
- [Test 270: get_azure_container_details](#test-270)
- [Test 271: get_azure_container_details](#test-271)
- [Test 272: get_azure_container_details](#test-272)
- [Test 273: get_azure_container_details](#test-273)
- [Test 274: get_azure_container_details](#test-274)
- [Test 275: get_azure_container_details](#test-275)
- [Test 276: get_azure_container_details](#test-276)
- [Test 277: get_azure_container_details](#test-277)
- [Test 278: get_azure_container_details](#test-278)
- [Test 279: get_azure_container_details](#test-279)
- [Test 280: get_azure_container_details](#test-280)
- [Test 281: get_azure_container_details](#test-281)
- [Test 282: get_azure_container_details](#test-282)
- [Test 283: get_azure_container_details](#test-283)
- [Test 284: get_azure_container_details](#test-284)
- [Test 285: get_azure_container_details](#test-285)
- [Test 286: get_azure_container_details](#test-286)
- [Test 287: get_azure_container_details](#test-287)
- [Test 288: get_azure_virtual_desktop_details](#test-288)
- [Test 289: get_azure_virtual_desktop_details](#test-289)
- [Test 290: get_azure_virtual_desktop_details](#test-290)

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
| 4 | 0.382375 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.368472 | `get_azure_storage_details` | ❌ |

---

## Test 2

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.415793 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.383875 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.328704 | `get_azure_security_configurations` | ❌ |
| 4 | 0.317407 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.290560 | `get_azure_storage_details` | ❌ |

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
| 4 | 0.363679 | `get_azure_storage_details` | ❌ |
| 5 | 0.358284 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 4

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.347561 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.305503 | `get_azure_messaging_service_details` | ❌ |
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
| 3 | 0.399332 | `get_azure_messaging_service_details` | ❌ |
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
| 2 | 0.291115 | `get_azure_messaging_service_details` | ❌ |
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
| 1 | 0.380155 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.362007 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.274839 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.227942 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.227919 | `get_azure_container_details` | ❌ |

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
| 5 | 0.305084 | `get_azure_database_admin_configuration_details` | ❌ |

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
| 4 | 0.330305 | `get_azure_cache_for_redis_details` | ❌ |
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
| 5 | 0.376776 | `get_azure_best_practices` | ❌ |

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
| 4 | 0.434901 | `get_azure_best_practices` | ❌ |
| 5 | 0.415185 | `edit_azure_app_config_settings` | ❌ |

---

## Test 18

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488025 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.444696 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.419275 | `get_azure_resource_and_app_health_status` | ❌ |
| 4 | 0.360911 | `get_azure_storage_details` | ❌ |
| 5 | 0.345466 | `deploy_resources_and_applications_to_azure` | ❌ |

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
| 4 | 0.433539 | `get_azure_storage_details` | ❌ |
| 5 | 0.427611 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 20

**Expected Tool:** `get_azure_app_resource_details`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.508135 | `get_azure_app_resource_details` | ✅ **EXPECTED** |
| 2 | 0.448072 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.427410 | `get_azure_security_configurations` | ❌ |
| 4 | 0.421965 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.409259 | `deploy_resources_and_applications_to_azure` | ❌ |

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
| 5 | 0.401383 | `get_azure_messaging_service_details` | ❌ |

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
| 3 | 0.445050 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.394452 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.391671 | `get_azure_storage_details` | ❌ |

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
| 5 | 0.390907 | `get_azure_best_practices` | ❌ |

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
| 5 | 0.360374 | `get_azure_best_practices` | ❌ |

---

## Test 29

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.394535 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 1 | 0.516072 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 1 | 0.478455 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 3 | 0.422628 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 3 | 0.466792 | `get_azure_storage_details` | ❌ |
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
| 2 | 0.447553 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 3 | 0.255211 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.238100 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.224949 | `get_azure_security_configurations` | ❌ |

---

## Test 36

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496395 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.418770 | `edit_azure_databases` | ❌ |
| 3 | 0.335274 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.322093 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.305576 | `get_azure_messaging_service_details` | ❌ |

---

## Test 37

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.411217 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.321958 | `edit_azure_databases` | ❌ |
| 3 | 0.243274 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 3 | 0.341482 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.323442 | `get_azure_messaging_service_details` | ❌ |
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
| 3 | 0.224080 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 4 | 0.204524 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.202386 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 41

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485399 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.427315 | `get_azure_storage_details` | ❌ |
| 3 | 0.423667 | `create_azure_storage` | ❌ |
| 4 | 0.393301 | `get_azure_container_details` | ❌ |
| 5 | 0.360784 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 42

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521537 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.388427 | `get_azure_storage_details` | ❌ |
| 3 | 0.372579 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.356404 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.340541 | `get_azure_security_configurations` | ❌ |

---

## Test 43

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.400848 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.313584 | `edit_azure_databases` | ❌ |
| 3 | 0.260685 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.239645 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.224229 | `browse_azure_marketplace_products` | ❌ |

---

## Test 44

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.357016 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.281932 | `edit_azure_databases` | ❌ |
| 3 | 0.237274 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.230483 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.211339 | `create_azure_database_admin_configurations` | ❌ |

---

## Test 45

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.558451 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 2 | 0.424023 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.422684 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 4 | 0.399238 | `get_azure_storage_details` | ❌ |
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
| 3 | 0.215872 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 2 | 0.446916 | `get_azure_storage_details` | ❌ |
| 3 | 0.421776 | `get_azure_security_configurations` | ❌ |
| 4 | 0.396416 | `get_azure_messaging_service_details` | ❌ |
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
| 3 | 0.270847 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 3 | 0.274993 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 3 | 0.319079 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 2 | 0.305069 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 3 | 0.391067 | `get_azure_storage_details` | ❌ |
| 4 | 0.377338 | `get_azure_container_details` | ❌ |
| 5 | 0.327978 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 54

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488113 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.481986 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 3 | 0.473411 | `get_azure_storage_details` | ❌ |
| 4 | 0.458257 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.444953 | `get_azure_security_configurations` | ❌ |

---

## Test 55

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496822 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.373845 | `get_azure_storage_details` | ❌ |
| 3 | 0.368375 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.336480 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.323139 | `get_azure_security_configurations` | ❌ |

---

## Test 56

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549668 | `get_azure_database_admin_configuration_details` | ❌ |
| 2 | 0.489035 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.483932 | `get_azure_storage_details` | ❌ |
| 4 | 0.476129 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.464348 | `get_azure_app_config_settings` | ❌ |

---

## Test 57

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.430729 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.415591 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 3 | 0.377864 | `get_azure_storage_details` | ❌ |
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
| 3 | 0.250033 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.239673 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.215258 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 60

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.511744 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.461478 | `edit_azure_databases` | ❌ |
| 3 | 0.364366 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 3 | 0.245578 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.227159 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.203381 | `create_azure_database_admin_configurations` | ❌ |

---

## Test 62

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469089 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.433734 | `edit_azure_databases` | ❌ |
| 3 | 0.360440 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.343516 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.339087 | `browse_azure_marketplace_products` | ❌ |

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
| 4 | 0.221415 | `get_azure_best_practices` | ❌ |
| 5 | 0.219511 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 64

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.343844 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.284626 | `edit_azure_databases` | ❌ |
| 3 | 0.214230 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.199866 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.193761 | `get_azure_best_practices` | ❌ |

---

## Test 65

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.421317 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.335214 | `edit_azure_databases` | ❌ |
| 3 | 0.249081 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.243774 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.210437 | `get_azure_security_configurations` | ❌ |

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
| 4 | 0.224139 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 5 | 0.192506 | `get_azure_database_admin_configuration_details` | ❌ |

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
| 5 | 0.160652 | `get_azure_database_admin_configuration_details` | ❌ |

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
| 1 | 0.542532 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.426841 | `get_azure_capacity` | ❌ |
| 3 | 0.381337 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.370327 | `get_azure_ai_resources_details` | ❌ |
| 5 | 0.368189 | `create_azure_load_testing` | ❌ |

---

## Test 72

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.339113 | `get_azure_storage_details` | ❌ |
| 2 | 0.315432 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 3 | 0.272938 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.271450 | `get_azure_capacity` | ❌ |
| 5 | 0.269638 | `get_azure_messaging_service_details` | ❌ |

---

## Test 73

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.337785 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.260120 | `get_azure_storage_details` | ❌ |
| 3 | 0.253631 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.243986 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.237039 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 74

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.409319 | `get_azure_storage_details` | ❌ |
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
| 4 | 0.440817 | `get_azure_messaging_service_details` | ❌ |
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
| 4 | 0.331387 | `get_azure_storage_details` | ❌ |
| 5 | 0.321578 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 80

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.428639 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.418441 | `get_azure_workbooks_details` | ❌ |
| 3 | 0.398664 | `create_azure_workbooks` | ❌ |
| 4 | 0.393598 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.376632 | `get_azure_databases_details` | ❌ |

---

## Test 81

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536844 | `get_azure_storage_details` | ❌ |
| 2 | 0.525913 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.486032 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 4 | 0.478110 | `get_azure_capacity` | ❌ |
| 5 | 0.460632 | `get_azure_messaging_service_details` | ❌ |

---

## Test 82

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476038 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.430555 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.423104 | `get_azure_best_practices` | ❌ |
| 4 | 0.369889 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 5 | 0.368140 | `browse_azure_marketplace_products` | ❌ |

---

## Test 83

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.548399 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.453897 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.440174 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.416282 | `get_azure_security_configurations` | ❌ |
| 5 | 0.416178 | `get_azure_capacity` | ❌ |

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
| 2 | 0.269987 | `get_azure_storage_details` | ❌ |
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
| 1 | 0.577912 | `get_azure_storage_details` | ❌ |
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
| 3 | 0.464578 | `get_azure_best_practices` | ❌ |
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
| 5 | 0.356376 | `get_azure_best_practices` | ❌ |

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
| 2 | 0.552006 | `get_azure_storage_details` | ❌ |
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
| 2 | 0.364066 | `get_azure_storage_details` | ❌ |
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
| 1 | 0.458069 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.365838 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.334156 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.332386 | `get_azure_capacity` | ❌ |
| 5 | 0.330072 | `get_azure_workbooks_details` | ❌ |

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
| 4 | 0.321501 | `get_azure_storage_details` | ❌ |
| 5 | 0.316211 | `get_azure_cache_for_redis_details` | ❌ |

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
| 4 | 0.205819 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.164353 | `get_azure_databases_details` | ❌ |

---

## Test 101

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.421696 | `get_azure_capacity` | ❌ |
| 2 | 0.410816 | `get_azure_storage_details` | ❌ |
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
| 4 | 0.379052 | `get_azure_storage_details` | ❌ |
| 5 | 0.378286 | `get_azure_ai_resources_details` | ❌ |

---

## Test 103

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510381 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.445914 | `get_azure_storage_details` | ❌ |
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
| 1 | 0.434123 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.395986 | `get_azure_capacity` | ❌ |
| 3 | 0.369677 | `create_azure_load_testing` | ❌ |
| 4 | 0.353073 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.326449 | `get_azure_ai_resources_details` | ❌ |

---

## Test 105

**Expected Tool:** `deploy_resources_and_applications_to_azure`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640058 | `deploy_resources_and_applications_to_azure` | ✅ **EXPECTED** |
| 2 | 0.519278 | `deploy_azure_ai_models` | ❌ |
| 3 | 0.479917 | `get_azure_best_practices` | ❌ |
| 4 | 0.454755 | `execute_azure_developer_cli` | ❌ |
| 5 | 0.453039 | `design_azure_architecture` | ❌ |

---

## Test 106

**Expected Tool:** `deploy_resources_and_applications_to_azure`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578541 | `deploy_resources_and_applications_to_azure` | ✅ **EXPECTED** |
| 2 | 0.477724 | `deploy_azure_ai_models` | ❌ |
| 3 | 0.437383 | `execute_azure_developer_cli` | ❌ |
| 4 | 0.410775 | `get_azure_best_practices` | ❌ |
| 5 | 0.401861 | `execute_azure_cli` | ❌ |

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
| 1 | 0.488004 | `get_azure_best_practices` | ❌ |
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
| 4 | 0.400007 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.388848 | `get_azure_database_admin_configuration_details` | ❌ |

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
| 4 | 0.310578 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 5 | 0.300069 | `get_azure_database_admin_configuration_details` | ❌ |

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
| 5 | 0.377426 | `get_azure_database_admin_configuration_details` | ❌ |

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
| 4 | 0.320929 | `get_azure_database_admin_configuration_details` | ❌ |
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
| 5 | 0.477397 | `get_azure_best_practices` | ❌ |

---

## Test 127

**Expected Tool:** `audit_azure_resources_compliance`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536483 | `audit_azure_resources_compliance` | ✅ **EXPECTED** |
| 2 | 0.511033 | `get_azure_best_practices` | ❌ |
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
| 2 | 0.508761 | `get_azure_best_practices` | ❌ |
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
| 1 | 0.734114 | `get_azure_security_configurations` | ✅ **EXPECTED** |
| 2 | 0.460374 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.414737 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.368185 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.356351 | `browse_azure_marketplace_products` | ❌ |

---

## Test 130

**Expected Tool:** `get_azure_security_configurations`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702749 | `get_azure_security_configurations` | ✅ **EXPECTED** |
| 2 | 0.485210 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.431032 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.388410 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.380791 | `get_azure_messaging_service_details` | ❌ |

---

## Test 131

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542408 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.534958 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 3 | 0.461491 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.385604 | `get_azure_security_configurations` | ❌ |
| 5 | 0.322487 | `get_azure_storage_details` | ❌ |

---

## Test 132

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502884 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.429278 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.427597 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.378915 | `get_azure_security_configurations` | ❌ |
| 5 | 0.362924 | `get_azure_storage_details` | ❌ |

---

## Test 133

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502924 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.434446 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.372548 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.348550 | `get_azure_security_configurations` | ❌ |
| 5 | 0.316556 | `get_azure_storage_details` | ❌ |

---

## Test 134

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

## Test 135

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552928 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.537878 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 3 | 0.461463 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.368299 | `get_azure_security_configurations` | ❌ |
| 5 | 0.330070 | `get_azure_storage_details` | ❌ |

---

## Test 136

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.515291 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.490749 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.420525 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.409646 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.358949 | `get_azure_storage_details` | ❌ |

---

## Test 137

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.467928 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.426201 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.377132 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.374678 | `get_azure_storage_details` | ❌ |
| 5 | 0.367478 | `create_azure_key_vault_items` | ❌ |

---

## Test 138

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

## Test 139

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.499555 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.448371 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.432123 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.344132 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.341471 | `get_azure_storage_details` | ❌ |

---

## Test 140

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529916 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.460930 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.426022 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.366899 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.360160 | `get_azure_storage_details` | ❌ |

---

## Test 141

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

## Test 142

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.493930 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.417468 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.334781 | `get_azure_key_vault` | ❌ |
| 4 | 0.281591 | `create_azure_storage` | ❌ |
| 5 | 0.230671 | `create_azure_workbooks` | ❌ |

---

## Test 143

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550455 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.384335 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.367336 | `get_azure_key_vault` | ❌ |
| 4 | 0.301538 | `lock_unlock_azure_app_config_settings` | ❌ |
| 5 | 0.292592 | `create_azure_storage` | ❌ |

---

## Test 144

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

## Test 145

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

## Test 146

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.734996 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.524465 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.474532 | `search_microsoft_docs` | ❌ |
| 4 | 0.459688 | `execute_azure_cli` | ❌ |
| 5 | 0.436925 | `get_azure_capacity` | ❌ |

---

## Test 147

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690141 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.601714 | `search_microsoft_docs` | ❌ |
| 3 | 0.539669 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.508718 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.483779 | `get_azure_capacity` | ❌ |

---

## Test 148

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713417 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.543696 | `search_microsoft_docs` | ❌ |
| 3 | 0.529617 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.470109 | `design_azure_architecture` | ❌ |
| 5 | 0.435613 | `browse_azure_marketplace_products` | ❌ |

---

## Test 149

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.683439 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.614180 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.558589 | `search_microsoft_docs` | ❌ |
| 4 | 0.465496 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.450131 | `get_azure_capacity` | ❌ |

---

## Test 150

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682046 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.556022 | `search_microsoft_docs` | ❌ |
| 3 | 0.509047 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.505735 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.443358 | `browse_azure_marketplace_products` | ❌ |

---

## Test 151

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685222 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.486075 | `search_microsoft_docs` | ❌ |
| 3 | 0.480287 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.448692 | `get_azure_app_resource_details` | ❌ |
| 5 | 0.416416 | `execute_azure_developer_cli` | ❌ |

---

## Test 152

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

## Test 153

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612893 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.520938 | `search_microsoft_docs` | ❌ |
| 3 | 0.518435 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.424667 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.421748 | `get_azure_app_resource_details` | ❌ |

---

## Test 154

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.489465 | `deploy_resources_and_applications_to_azure` | ❌ |
| 2 | 0.480777 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 3 | 0.440374 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.436599 | `deploy_azure_ai_models` | ❌ |
| 5 | 0.432577 | `evaluate_azure_ai_foundry_agents` | ❌ |

---

## Test 155

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618323 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.488919 | `get_azure_key_vault` | ❌ |
| 3 | 0.478248 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.444000 | `import_azure_key_vault_certificates` | ❌ |
| 5 | 0.435817 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 156

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628052 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.454934 | `get_azure_app_resource_details` | ❌ |
| 3 | 0.453910 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.451968 | `search_microsoft_docs` | ❌ |
| 5 | 0.391204 | `execute_azure_cli` | ❌ |

---

## Test 157

**Expected Tool:** `design_azure_architecture`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676638 | `design_azure_architecture` | ✅ **EXPECTED** |
| 2 | 0.481645 | `get_azure_best_practices` | ❌ |
| 3 | 0.465832 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.386867 | `execute_azure_developer_cli` | ❌ |
| 5 | 0.385801 | `audit_azure_resources_compliance` | ❌ |

---

## Test 158

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

## Test 159

**Expected Tool:** `design_azure_architecture`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.427594 | `upload_azure_storage_blobs` | ❌ |
| 2 | 0.413208 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.410389 | `create_azure_storage` | ❌ |
| 4 | 0.410321 | `search_microsoft_docs` | ❌ |
| 5 | 0.405889 | `design_azure_architecture` | ✅ **EXPECTED** |

---

## Test 160

**Expected Tool:** `design_azure_architecture`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.342485 | `browse_azure_marketplace_products` | ❌ |
| 2 | 0.289308 | `design_azure_architecture` | ✅ **EXPECTED** |
| 3 | 0.277705 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.276204 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.224271 | `get_azure_best_practices` | ❌ |

---

## Test 161

**Expected Tool:** `design_azure_architecture`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.323311 | `upload_azure_storage_blobs` | ❌ |
| 2 | 0.296166 | `design_azure_architecture` | ✅ **EXPECTED** |
| 3 | 0.245346 | `create_azure_storage` | ❌ |
| 4 | 0.224135 | `get_azure_capacity` | ❌ |
| 5 | 0.207392 | `get_azure_best_practices` | ❌ |

---

## Test 162

**Expected Tool:** `get_azure_load_testing_details`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609585 | `get_azure_load_testing_details` | ✅ **EXPECTED** |
| 2 | 0.568056 | `create_azure_load_testing` | ❌ |
| 3 | 0.448055 | `update_azure_load_testing_configurations` | ❌ |
| 4 | 0.366497 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.330068 | `get_azure_storage_details` | ❌ |

---

## Test 163

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

## Test 164

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

## Test 165

**Expected Tool:** `get_azure_load_testing_details`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669717 | `get_azure_load_testing_details` | ✅ **EXPECTED** |
| 2 | 0.609875 | `create_azure_load_testing` | ❌ |
| 3 | 0.493520 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.421963 | `get_azure_capacity` | ❌ |
| 5 | 0.411917 | `get_azure_storage_details` | ❌ |

---

## Test 166

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

## Test 167

**Expected Tool:** `create_azure_load_testing`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660182 | `create_azure_load_testing` | ✅ **EXPECTED** |
| 2 | 0.530656 | `get_azure_load_testing_details` | ❌ |
| 3 | 0.411267 | `update_azure_load_testing_configurations` | ❌ |
| 4 | 0.374033 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.334524 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 168

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

## Test 169

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

## Test 170

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.310095 | `get_azure_app_config_settings` | ❌ |
| 2 | 0.294949 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.268660 | `get_azure_best_practices` | ❌ |
| 4 | 0.262165 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.249688 | `get_azure_workbooks_details` | ❌ |

---

## Test 171

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

## Test 172

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

## Test 173

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530525 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.474686 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.444086 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.399309 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.391196 | `get_azure_security_configurations` | ❌ |

---

## Test 174

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.482355 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.356614 | `get_azure_security_configurations` | ❌ |
| 3 | 0.350779 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.349687 | `get_azure_databases_details` | ❌ |
| 5 | 0.343922 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 175

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

## Test 176

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.426476 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.296548 | `get_azure_resource_and_app_health_status` | ❌ |
| 3 | 0.280912 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.279030 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.268524 | `evaluate_azure_ai_foundry_agents` | ❌ |

---

## Test 177

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

## Test 178

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485141 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.431878 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.370196 | `get_azure_app_resource_details` | ❌ |
| 4 | 0.362361 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.357646 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 179

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

## Test 180

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.541256 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.507116 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.458381 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.414408 | `get_azure_capacity` | ❌ |
| 5 | 0.413622 | `get_azure_app_resource_details` | ❌ |

---

## Test 181

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509173 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.430944 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.406409 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.398647 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.378065 | `get_azure_app_resource_details` | ❌ |

---

## Test 182

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.472364 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.362187 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.360971 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.349891 | `get_azure_resource_and_app_health_status` | ❌ |
| 5 | 0.345205 | `get_azure_databases_details` | ❌ |

---

## Test 183

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

## Test 184

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the schema for knowledge index <index-name> in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498412 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.373160 | `deploy_azure_ai_models` | ❌ |
| 3 | 0.341967 | `evaluate_azure_ai_foundry_agents` | ❌ |
| 4 | 0.324788 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.308733 | `get_azure_best_practices` | ❌ |

---

## Test 185

**Expected Tool:** `deploy_azure_ai_models`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.387447 | `deploy_azure_ai_models` | ✅ **EXPECTED** |
| 2 | 0.307463 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.299302 | `create_azure_load_testing` | ❌ |
| 4 | 0.240425 | `edit_azure_databases` | ❌ |
| 5 | 0.236261 | `get_azure_best_practices` | ❌ |

---

## Test 186

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

## Test 187

**Expected Tool:** `evaluate_azure_ai_foundry_agents`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.347263 | `evaluate_azure_ai_foundry_agents` | ✅ **EXPECTED** |
| 2 | 0.280105 | `get_azure_resource_and_app_health_status` | ❌ |
| 3 | 0.245804 | `execute_azure_cli` | ❌ |
| 4 | 0.238874 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.229140 | `audit_azure_resources_compliance` | ❌ |

---

## Test 188

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627388 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.475469 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.448999 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.429033 | `create_azure_storage` | ❌ |
| 5 | 0.412908 | `get_azure_container_details` | ❌ |

---

## Test 189

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580698 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.529061 | `create_azure_storage` | ❌ |
| 3 | 0.478682 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.448552 | `get_azure_container_details` | ❌ |
| 5 | 0.415661 | `get_azure_app_config_settings` | ❌ |

---

## Test 190

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587289 | `create_azure_storage` | ❌ |
| 2 | 0.514372 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 3 | 0.453206 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.357190 | `get_azure_container_details` | ❌ |
| 5 | 0.336886 | `get_azure_security_configurations` | ❌ |

---

## Test 191

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551096 | `create_azure_storage` | ❌ |
| 2 | 0.493666 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 3 | 0.463738 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.338640 | `get_azure_container_details` | ❌ |
| 5 | 0.309741 | `get_azure_security_configurations` | ❌ |

---

## Test 192

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599933 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.459444 | `create_azure_storage` | ❌ |
| 3 | 0.444366 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.428351 | `get_azure_capacity` | ❌ |
| 5 | 0.417675 | `get_azure_messaging_service_details` | ❌ |

---

## Test 193

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.575344 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.432052 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.421279 | `get_azure_capacity` | ❌ |
| 4 | 0.408527 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.390093 | `get_azure_databases_details` | ❌ |

---

## Test 194

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572325 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.455173 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.431655 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.422149 | `get_azure_capacity` | ❌ |
| 5 | 0.400784 | `get_azure_databases_details` | ❌ |

---

## Test 195

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List the Azure Managed Lustre SKUs available in <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600844 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.492224 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.455935 | `get_azure_capacity` | ❌ |
| 4 | 0.404690 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.390248 | `get_azure_ai_resources_details` | ❌ |

---

## Test 196

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.506968 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.404977 | `create_azure_storage` | ❌ |
| 3 | 0.388185 | `get_azure_capacity` | ❌ |
| 4 | 0.384612 | `get_azure_security_configurations` | ❌ |
| 5 | 0.355987 | `get_azure_messaging_service_details` | ❌ |

---

## Test 197

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546896 | `create_azure_storage` | ❌ |
| 2 | 0.483606 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 3 | 0.472639 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.397280 | `get_azure_container_details` | ❌ |
| 5 | 0.309579 | `get_azure_security_configurations` | ❌ |

---

## Test 198

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543409 | `create_azure_storage` | ❌ |
| 2 | 0.520267 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 3 | 0.418173 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.408752 | `get_azure_container_details` | ❌ |
| 5 | 0.354495 | `get_azure_security_configurations` | ❌ |

---

## Test 199

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592829 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.449355 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.433866 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.419549 | `create_azure_storage` | ❌ |
| 5 | 0.419225 | `get_azure_container_details` | ❌ |

---

## Test 200

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.524228 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.521403 | `create_azure_storage` | ❌ |
| 3 | 0.457281 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.405975 | `get_azure_container_details` | ❌ |
| 5 | 0.348727 | `get_azure_app_config_settings` | ❌ |

---

## Test 201

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543735 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.498174 | `create_azure_storage` | ❌ |
| 3 | 0.434495 | `get_azure_container_details` | ❌ |
| 4 | 0.398764 | `upload_azure_storage_blobs` | ❌ |
| 5 | 0.368710 | `get_azure_app_config_settings` | ❌ |

---

## Test 202

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520995 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.476386 | `create_azure_storage` | ❌ |
| 3 | 0.430206 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.397591 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.397399 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 203

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546204 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.431996 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.379103 | `get_azure_storage_details` | ❌ |
| 4 | 0.318029 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.304649 | `search_microsoft_docs` | ❌ |

---

## Test 204

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478014 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.354545 | `get_azure_storage_details` | ❌ |
| 3 | 0.329543 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.307994 | `create_azure_load_testing` | ❌ |
| 5 | 0.306614 | `get_azure_capacity` | ❌ |

---

## Test 205

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557678 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.441131 | `get_azure_storage_details` | ❌ |
| 3 | 0.432518 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.423712 | `upload_azure_storage_blobs` | ❌ |
| 5 | 0.395124 | `create_azure_workbooks` | ❌ |

---

## Test 206

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488344 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.437405 | `get_azure_storage_details` | ❌ |
| 3 | 0.406118 | `get_azure_capacity` | ❌ |
| 4 | 0.356649 | `create_azure_load_testing` | ❌ |
| 5 | 0.346863 | `create_azure_key_vault_items` | ❌ |

---

## Test 207

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.631937 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.487471 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.396416 | `get_azure_storage_details` | ❌ |
| 4 | 0.326507 | `get_azure_container_details` | ❌ |
| 5 | 0.316986 | `create_azure_key_vault_items` | ❌ |

---

## Test 208

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607037 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.450592 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.403005 | `get_azure_storage_details` | ❌ |
| 4 | 0.325408 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.308422 | `get_azure_container_details` | ❌ |

---

## Test 209

**Expected Tool:** `upload_azure_storage_blobs`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623296 | `upload_azure_storage_blobs` | ✅ **EXPECTED** |
| 2 | 0.528854 | `create_azure_storage` | ❌ |
| 3 | 0.375096 | `get_azure_storage_details` | ❌ |
| 4 | 0.292417 | `deploy_azure_ai_models` | ❌ |
| 5 | 0.268224 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 210

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** List all access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.597841 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.335003 | `get_azure_security_configurations` | ❌ |
| 3 | 0.304885 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.292789 | `get_azure_key_vault` | ❌ |
| 5 | 0.267608 | `get_azure_container_details` | ❌ |

---

## Test 211

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** List all databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.470890 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.389069 | `get_azure_databases_details` | ❌ |
| 3 | 0.387732 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.280568 | `get_azure_container_details` | ❌ |
| 5 | 0.254949 | `get_azure_security_configurations` | ❌ |

---

## Test 212

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** List all Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580764 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.364783 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.342459 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.340686 | `get_azure_databases_details` | ❌ |
| 5 | 0.310757 | `browse_azure_marketplace_products` | ❌ |

---

## Test 213

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** List all Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.568036 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.435562 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.414456 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.396583 | `get_azure_container_details` | ❌ |
| 5 | 0.383636 | `get_azure_messaging_service_details` | ❌ |

---

## Test 214

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me my Redis Caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520582 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.290252 | `get_azure_databases_details` | ❌ |
| 3 | 0.261877 | `get_azure_container_details` | ❌ |
| 4 | 0.252495 | `get_azure_key_vault` | ❌ |
| 5 | 0.252330 | `get_azure_security_configurations` | ❌ |

---

## Test 215

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me my Redis Clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498683 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.354127 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.342390 | `get_azure_container_details` | ❌ |
| 4 | 0.298268 | `get_azure_databases_details` | ❌ |
| 5 | 0.272683 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 216

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me the access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599777 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.322488 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.316812 | `get_azure_security_configurations` | ❌ |
| 4 | 0.305854 | `get_azure_key_vault` | ❌ |
| 5 | 0.305484 | `get_azure_app_config_settings` | ❌ |

---

## Test 217

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me the databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.457201 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.379510 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.372181 | `get_azure_databases_details` | ❌ |
| 4 | 0.280782 | `get_azure_container_details` | ❌ |
| 5 | 0.238847 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 218

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me the Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553958 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.360815 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.335247 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.333569 | `get_azure_databases_details` | ❌ |
| 5 | 0.328521 | `get_azure_messaging_service_details` | ❌ |

---

## Test 219

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me the Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.539056 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.424900 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.415817 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.400228 | `get_azure_container_details` | ❌ |
| 5 | 0.380758 | `get_azure_messaging_service_details` | ❌ |

---

## Test 220

**Expected Tool:** `browse_azure_marketplace_products`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.424825 | `browse_azure_marketplace_products` | ✅ **EXPECTED** |
| 2 | 0.376519 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.344595 | `get_azure_storage_details` | ❌ |
| 4 | 0.343785 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.302227 | `get_azure_ai_resources_details` | ❌ |

---

## Test 221

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

## Test 222

**Expected Tool:** `browse_azure_marketplace_products`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492651 | `browse_azure_marketplace_products` | ✅ **EXPECTED** |
| 2 | 0.227400 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.217240 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.210581 | `get_azure_workbooks_details` | ❌ |
| 5 | 0.205593 | `get_azure_storage_details` | ❌ |

---

## Test 223

**Expected Tool:** `get_azure_capacity`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.484870 | `get_azure_capacity` | ✅ **EXPECTED** |
| 2 | 0.419684 | `get_azure_storage_details` | ❌ |
| 3 | 0.353830 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.350026 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.314369 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 224

**Expected Tool:** `get_azure_capacity`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.398403 | `get_azure_capacity` | ✅ **EXPECTED** |
| 2 | 0.347156 | `get_azure_storage_details` | ❌ |
| 3 | 0.332100 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.313365 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.311621 | `get_azure_ai_resources_details` | ❌ |

---

## Test 225

**Expected Tool:** `get_azure_capacity`  
**Prompt:** Tell me how many IP addresses I need for <filesystem_size> of <amlfs_sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.304753 | `get_azure_storage_details` | ❌ |
| 2 | 0.266002 | `get_azure_capacity` | ✅ **EXPECTED** |
| 3 | 0.225046 | `execute_azure_cli` | ❌ |
| 4 | 0.215121 | `edit_azure_databases` | ❌ |
| 5 | 0.212666 | `get_azure_container_details` | ❌ |

---

## Test 226

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510168 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.431684 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.350456 | `get_azure_security_configurations` | ❌ |
| 4 | 0.326854 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.305105 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 227

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.583583 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.433919 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.380839 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.375303 | `get_azure_security_configurations` | ❌ |
| 5 | 0.355380 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 228

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.517196 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.485690 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 3 | 0.337400 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.332975 | `get_azure_security_configurations` | ❌ |
| 5 | 0.331312 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 229

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.540766 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.397469 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.329801 | `get_azure_security_configurations` | ❌ |
| 4 | 0.325418 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.312824 | `get_azure_storage_details` | ❌ |

---

## Test 230

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.508284 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.443596 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.351500 | `get_azure_security_configurations` | ❌ |
| 4 | 0.348005 | `get_azure_storage_details` | ❌ |
| 5 | 0.335511 | `get_azure_load_testing_details` | ❌ |

---

## Test 231

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.539104 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.499122 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.366263 | `get_azure_security_configurations` | ❌ |
| 4 | 0.341727 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.337607 | `get_azure_storage_details` | ❌ |

---

## Test 232

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551834 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.422468 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.341614 | `get_azure_security_configurations` | ❌ |
| 4 | 0.321448 | `get_azure_storage_details` | ❌ |
| 5 | 0.319377 | `browse_azure_marketplace_products` | ❌ |

---

## Test 233

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553712 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.470487 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.429263 | `get_azure_security_configurations` | ❌ |
| 4 | 0.402453 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.358330 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 234

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566733 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.506484 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 3 | 0.383936 | `get_azure_security_configurations` | ❌ |
| 4 | 0.348412 | `get_azure_storage_details` | ❌ |
| 5 | 0.346967 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 235

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552994 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.404707 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.344019 | `get_azure_security_configurations` | ❌ |
| 4 | 0.337543 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.317601 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 236

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602537 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.365586 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.364952 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.364635 | `get_azure_container_details` | ❌ |
| 5 | 0.354304 | `get_azure_storage_details` | ❌ |

---

## Test 237

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622636 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.380697 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.379021 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.357295 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.355318 | `get_azure_app_resource_details` | ❌ |

---

## Test 238

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615458 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.363340 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.347442 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.343578 | `get_azure_container_details` | ❌ |
| 5 | 0.336162 | `get_azure_storage_details` | ❌ |

---

## Test 239

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587661 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.444005 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.417523 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.364752 | `get_azure_security_configurations` | ❌ |
| 5 | 0.347974 | `get_azure_storage_details` | ❌ |

---

## Test 240

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589762 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.413814 | `get_azure_databases_details` | ❌ |
| 3 | 0.398499 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.385321 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.379204 | `get_azure_container_details` | ❌ |

---

## Test 241

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546030 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.462094 | `get_azure_databases_details` | ❌ |
| 3 | 0.337843 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.295340 | `get_azure_container_details` | ❌ |
| 5 | 0.284549 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 242

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.526929 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.410926 | `get_azure_databases_details` | ❌ |
| 3 | 0.305775 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.263808 | `get_azure_storage_details` | ❌ |
| 5 | 0.256123 | `get_azure_container_details` | ❌ |

---

## Test 243

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512442 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.313016 | `get_azure_databases_details` | ❌ |
| 3 | 0.248948 | `get_azure_container_details` | ❌ |
| 4 | 0.242302 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.229602 | `browse_azure_marketplace_products` | ❌ |

---

## Test 244

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.429164 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.305911 | `get_azure_databases_details` | ❌ |
| 3 | 0.305251 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.264000 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.241299 | `get_azure_container_details` | ❌ |

---

## Test 245

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533350 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.337173 | `get_azure_databases_details` | ❌ |
| 3 | 0.337078 | `get_azure_container_details` | ❌ |
| 4 | 0.315760 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.313020 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 246

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

## Test 247

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.535152 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.443460 | `get_azure_databases_details` | ❌ |
| 3 | 0.328185 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.301627 | `get_azure_container_details` | ❌ |
| 5 | 0.284436 | `get_azure_storage_details` | ❌ |

---

## Test 248

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603734 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.416961 | `get_azure_container_details` | ❌ |
| 3 | 0.382717 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.365816 | `get_azure_databases_details` | ❌ |
| 5 | 0.359156 | `get_azure_storage_details` | ❌ |

---

## Test 249

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.475085 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.366605 | `get_azure_databases_details` | ❌ |
| 3 | 0.251781 | `get_azure_best_practices` | ❌ |
| 4 | 0.242419 | `design_azure_architecture` | ❌ |
| 5 | 0.241078 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 250

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521279 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.401926 | `get_azure_databases_details` | ❌ |
| 3 | 0.301799 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.271168 | `get_azure_container_details` | ❌ |
| 5 | 0.266098 | `get_azure_storage_details` | ❌ |

---

## Test 251

**Expected Tool:** `create_azure_database_admin_configurations`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619667 | `create_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.497535 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.339582 | `edit_azure_databases` | ❌ |
| 4 | 0.339239 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.210520 | `get_azure_databases_details` | ❌ |

---

## Test 252

**Expected Tool:** `create_azure_database_admin_configurations`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769894 | `create_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.659595 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.476818 | `edit_azure_databases` | ❌ |
| 4 | 0.455058 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.349711 | `get_azure_databases_details` | ❌ |

---

## Test 253

**Expected Tool:** `create_azure_database_admin_configurations`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670242 | `create_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.546745 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.370169 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.334013 | `edit_azure_databases` | ❌ |
| 5 | 0.250966 | `create_azure_workbooks` | ❌ |

---

## Test 254

**Expected Tool:** `delete_azure_database_admin_configurations`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.725926 | `delete_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.684224 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.446836 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.433064 | `edit_azure_databases` | ❌ |
| 5 | 0.365336 | `edit_azure_workbooks` | ❌ |

---

## Test 255

**Expected Tool:** `delete_azure_database_admin_configurations`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691123 | `delete_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.657273 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.410574 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.364151 | `edit_azure_databases` | ❌ |
| 5 | 0.287813 | `get_azure_security_configurations` | ❌ |

---

## Test 256

**Expected Tool:** `delete_azure_database_admin_configurations`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662278 | `delete_azure_database_admin_configurations` | ✅ **EXPECTED** |
| 2 | 0.610044 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.368252 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.299807 | `edit_azure_databases` | ❌ |
| 5 | 0.250392 | `get_azure_security_configurations` | ❌ |

---

## Test 257

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550810 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.437477 | `get_azure_databases_details` | ❌ |
| 3 | 0.370734 | `delete_azure_database_admin_configurations` | ❌ |
| 4 | 0.369513 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.368293 | `edit_azure_databases` | ❌ |

---

## Test 258

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659544 | `create_azure_database_admin_configurations` | ❌ |
| 2 | 0.635949 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.509176 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 4 | 0.344890 | `get_azure_security_configurations` | ❌ |
| 5 | 0.332632 | `get_azure_databases_details` | ❌ |

---

## Test 259

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498360 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.362041 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.358939 | `get_azure_security_configurations` | ❌ |
| 4 | 0.343594 | `get_azure_databases_details` | ❌ |
| 5 | 0.334656 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 260

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602479 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.424559 | `get_azure_databases_details` | ❌ |
| 3 | 0.414641 | `edit_azure_databases` | ❌ |
| 4 | 0.400359 | `get_azure_container_details` | ❌ |
| 5 | 0.372754 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 261

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498314 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.325040 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.299004 | `get_azure_databases_details` | ❌ |
| 4 | 0.294052 | `get_azure_security_configurations` | ❌ |
| 5 | 0.287675 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 262

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659102 | `create_azure_database_admin_configurations` | ❌ |
| 2 | 0.611917 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.486397 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 4 | 0.361115 | `edit_azure_databases` | ❌ |
| 5 | 0.322908 | `get_azure_security_configurations` | ❌ |

---

## Test 263

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.515319 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.412957 | `edit_azure_databases` | ❌ |
| 3 | 0.411657 | `get_azure_databases_details` | ❌ |
| 4 | 0.380417 | `get_azure_capacity` | ❌ |
| 5 | 0.346383 | `get_azure_container_details` | ❌ |

---

## Test 264

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657075 | `create_azure_database_admin_configurations` | ❌ |
| 2 | 0.595199 | `delete_azure_database_admin_configurations` | ❌ |
| 3 | 0.493201 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 4 | 0.358803 | `edit_azure_databases` | ❌ |
| 5 | 0.289735 | `get_azure_security_configurations` | ❌ |

---

## Test 265

**Expected Tool:** `get_azure_database_admin_configuration_details`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.451814 | `get_azure_database_admin_configuration_details` | ✅ **EXPECTED** |
| 2 | 0.332608 | `create_azure_database_admin_configurations` | ❌ |
| 3 | 0.326446 | `edit_azure_databases` | ❌ |
| 4 | 0.318149 | `get_azure_databases_details` | ❌ |
| 5 | 0.281938 | `delete_azure_database_admin_configurations` | ❌ |

---

## Test 266

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591418 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.489443 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.461090 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.439000 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.421388 | `get_azure_storage_details` | ❌ |

---

## Test 267

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525642 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.515594 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.423947 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.385731 | `execute_azure_cli` | ❌ |
| 5 | 0.384929 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 268

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

## Test 269

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585525 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.460024 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.436846 | `get_azure_storage_details` | ❌ |
| 4 | 0.427150 | `get_azure_security_configurations` | ❌ |
| 5 | 0.420593 | `get_azure_messaging_service_details` | ❌ |

---

## Test 270

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.514903 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.394679 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.384525 | `get_azure_storage_details` | ❌ |
| 4 | 0.351749 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.350200 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 271

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.489483 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.386495 | `get_azure_storage_details` | ❌ |
| 3 | 0.382508 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.356875 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.349921 | `get_azure_load_testing_details` | ❌ |

---

## Test 272

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542243 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.417443 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.385526 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.372812 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.371461 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 273

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.452265 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.300887 | `get_azure_storage_details` | ❌ |
| 3 | 0.285235 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.265440 | `get_azure_security_configurations` | ❌ |
| 5 | 0.264853 | `create_azure_storage` | ❌ |

---

## Test 274

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593337 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.419226 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.400669 | `create_azure_storage` | ❌ |
| 4 | 0.397056 | `get_azure_storage_details` | ❌ |
| 5 | 0.390515 | `get_azure_security_configurations` | ❌ |

---

## Test 275

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536299 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.472664 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.415764 | `get_azure_security_configurations` | ❌ |
| 4 | 0.403367 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.400655 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 276

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485764 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.336265 | `create_azure_storage` | ❌ |
| 3 | 0.316616 | `get_azure_storage_details` | ❌ |
| 4 | 0.310576 | `get_azure_security_configurations` | ❌ |
| 5 | 0.302471 | `get_azure_key_vault` | ❌ |

---

## Test 277

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516286 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.442823 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.409939 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.394130 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.359169 | `get_azure_load_testing_details` | ❌ |

---

## Test 278

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547796 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.433036 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.386378 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.385491 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.382855 | `get_azure_storage_details` | ❌ |

---

## Test 279

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510429 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.431510 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.379270 | `get_azure_storage_details` | ❌ |
| 4 | 0.355343 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.343124 | `get_azure_load_testing_details` | ❌ |

---

## Test 280

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579908 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.444584 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.427527 | `get_azure_storage_details` | ❌ |
| 4 | 0.424682 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.424320 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 281

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.452245 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.375014 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.344379 | `execute_azure_cli` | ❌ |
| 4 | 0.343243 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.322236 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 282

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542863 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.412997 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.387109 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.378828 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.373546 | `get_azure_database_admin_configuration_details` | ❌ |

---

## Test 283

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.463224 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.287889 | `get_azure_cache_for_redis_details` | ❌ |
| 3 | 0.287574 | `get_azure_storage_details` | ❌ |
| 4 | 0.277348 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.276767 | `get_azure_key_vault` | ❌ |

---

## Test 284

**Expected Tool:** `get_azure_container_details`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580086 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.423944 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.405032 | `execute_azure_cli` | ❌ |
| 4 | 0.351081 | `get_azure_storage_details` | ❌ |
| 5 | 0.351001 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 285

**Expected Tool:** `get_azure_container_details`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607448 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.458216 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.458077 | `get_azure_storage_details` | ❌ |
| 4 | 0.457991 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.448234 | `get_azure_app_config_settings` | ❌ |

---

## Test 286

**Expected Tool:** `get_azure_container_details`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.489505 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.347623 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.339115 | `get_azure_database_admin_configuration_details` | ❌ |
| 4 | 0.315916 | `execute_azure_developer_cli` | ❌ |
| 5 | 0.314839 | `execute_azure_cli` | ❌ |

---

## Test 287

**Expected Tool:** `get_azure_container_details`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532501 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.389162 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.362346 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.358748 | `get_azure_database_admin_configuration_details` | ❌ |
| 5 | 0.349770 | `get_azure_capacity` | ❌ |

---

## Test 288

**Expected Tool:** `get_azure_virtual_desktop_details`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550251 | `get_azure_virtual_desktop_details` | ✅ **EXPECTED** |
| 2 | 0.453728 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.442910 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.412967 | `get_azure_container_details` | ❌ |
| 5 | 0.403919 | `get_azure_messaging_service_details` | ❌ |

---

## Test 289

**Expected Tool:** `get_azure_virtual_desktop_details`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607532 | `get_azure_virtual_desktop_details` | ✅ **EXPECTED** |
| 2 | 0.364650 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.319120 | `get_azure_security_configurations` | ❌ |
| 4 | 0.307420 | `get_azure_container_details` | ❌ |
| 5 | 0.304479 | `get_azure_capacity` | ❌ |

---

## Test 290

**Expected Tool:** `get_azure_virtual_desktop_details`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611133 | `get_azure_virtual_desktop_details` | ✅ **EXPECTED** |
| 2 | 0.335749 | `get_azure_database_admin_configuration_details` | ❌ |
| 3 | 0.313084 | `get_azure_security_configurations` | ❌ |
| 4 | 0.265858 | `get_azure_container_details` | ❌ |
| 5 | 0.261914 | `get_azure_messaging_service_details` | ❌ |

---

## Summary

**Total Prompts Tested:** 290  
**Analysis Execution Time:** 38.9037599s  

### Success Rate Metrics

**Top Choice Success:** 82.1% (238/290 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 0.0% (0/290 tests)  
**🎯 High Confidence (≥0.7):** 2.4% (7/290 tests)  
**✅ Good Confidence (≥0.6):** 14.8% (43/290 tests)  
**👍 Fair Confidence (≥0.5):** 55.9% (162/290 tests)  
**👌 Acceptable Confidence (≥0.4):** 87.9% (255/290 tests)  
**❌ Low Confidence (<0.4):** 12.1% (35/290 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 0.0% (0/290 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 2.4% (7/290 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 14.8% (43/290 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 50.3% (146/290 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 75.5% (219/290 tests)  

### Success Rate Analysis

🟡 **Good** - The tool selection system is performing adequately but has room for improvement.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

