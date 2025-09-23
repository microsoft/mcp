# Tool Selection Analysis Setup

**Setup completed:** 2025-09-23 17:27:15  
**Tool count:** 40  
**Database setup time:** 1.2917668s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-09-23 17:27:15  
**Tool count:** 40  

## Table of Contents

- [Test 1: get_azure_subscriptions_and_resource_groups](#test-1)
- [Test 2: get_azure_subscriptions_and_resource_groups](#test-2)
- [Test 3: get_azure_subscriptions_and_resource_groups](#test-3)
- [Test 4: get_azure_subscriptions_and_resource_groups](#test-4)
- [Test 5: get_azure_subscriptions_and_resource_groups](#test-5)
- [Test 6: get_azure_subscriptions_and_resource_groups](#test-6)
- [Test 7: get_azure_subscriptions_and_resource_groups](#test-7)
- [Test 8: get_application_platform_details](#test-8)
- [Test 9: get_application_platform_details](#test-9)
- [Test 10: get_application_platform_details](#test-10)
- [Test 11: get_application_platform_details](#test-11)
- [Test 12: get_application_platform_details](#test-12)
- [Test 13: get_application_platform_details](#test-13)
- [Test 14: get_application_platform_details](#test-14)
- [Test 15: get_application_platform_details](#test-15)
- [Test 16: get_application_platform_details](#test-16)
- [Test 17: get_application_platform_details](#test-17)
- [Test 18: get_application_platform_details](#test-18)
- [Test 19: get_application_platform_details](#test-19)
- [Test 20: get_azure_databases_details](#test-20)
- [Test 21: get_azure_databases_details](#test-21)
- [Test 22: get_azure_databases_details](#test-22)
- [Test 23: get_azure_databases_details](#test-23)
- [Test 24: get_azure_databases_details](#test-24)
- [Test 25: get_azure_databases_details](#test-25)
- [Test 26: get_azure_databases_details](#test-26)
- [Test 27: get_azure_databases_details](#test-27)
- [Test 28: get_azure_databases_details](#test-28)
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
- [Test 54: edit_azure_databases](#test-54)
- [Test 55: edit_azure_databases](#test-55)
- [Test 56: get_azure_resource_and_app_health_status](#test-56)
- [Test 57: get_azure_resource_and_app_health_status](#test-57)
- [Test 58: get_azure_resource_and_app_health_status](#test-58)
- [Test 59: get_azure_resource_and_app_health_status](#test-59)
- [Test 60: get_azure_resource_and_app_health_status](#test-60)
- [Test 61: get_azure_resource_and_app_health_status](#test-61)
- [Test 62: get_azure_resource_and_app_health_status](#test-62)
- [Test 63: get_azure_resource_and_app_health_status](#test-63)
- [Test 64: get_azure_resource_and_app_health_status](#test-64)
- [Test 65: get_azure_resource_and_app_health_status](#test-65)
- [Test 66: get_azure_resource_and_app_health_status](#test-66)
- [Test 67: get_azure_resource_and_app_health_status](#test-67)
- [Test 68: get_azure_resource_and_app_health_status](#test-68)
- [Test 69: get_azure_resource_and_app_health_status](#test-69)
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
- [Test 84: deploy_resources_and_applications_to_azure](#test-84)
- [Test 85: deploy_resources_and_applications_to_azure](#test-85)
- [Test 86: deploy_resources_and_applications_to_azure](#test-86)
- [Test 87: deploy_resources_and_applications_to_azure](#test-87)
- [Test 88: get_azure_app_config_settings](#test-88)
- [Test 89: get_azure_app_config_settings](#test-89)
- [Test 90: get_azure_app_config_settings](#test-90)
- [Test 91: get_azure_app_config_settings](#test-91)
- [Test 92: get_azure_app_config_settings](#test-92)
- [Test 93: get_azure_app_config_settings](#test-93)
- [Test 94: edit_azure_app_config_settings](#test-94)
- [Test 95: edit_azure_app_config_settings](#test-95)
- [Test 96: set_azure_app_config_settings_lock_state](#test-96)
- [Test 97: set_azure_app_config_settings_lock_state](#test-97)
- [Test 98: edit_azure_workbooks](#test-98)
- [Test 99: edit_azure_workbooks](#test-99)
- [Test 100: create_azure_workbooks](#test-100)
- [Test 101: get_azure_workbooks_details](#test-101)
- [Test 102: get_azure_workbooks_details](#test-102)
- [Test 103: get_azure_workbooks_details](#test-103)
- [Test 104: get_azure_workbooks_details](#test-104)
- [Test 105: audit_azure_resources_compliance](#test-105)
- [Test 106: audit_azure_resources_compliance](#test-106)
- [Test 107: audit_azure_resources_compliance](#test-107)
- [Test 108: get_azure_security_configurations](#test-108)
- [Test 109: get_azure_security_configurations](#test-109)
- [Test 110: get_azure_key_vault](#test-110)
- [Test 111: get_azure_key_vault](#test-111)
- [Test 112: get_azure_key_vault](#test-112)
- [Test 113: get_azure_key_vault](#test-113)
- [Test 114: get_azure_key_vault](#test-114)
- [Test 115: get_azure_key_vault](#test-115)
- [Test 116: get_azure_key_vault](#test-116)
- [Test 117: get_azure_key_vault](#test-117)
- [Test 118: create_azure_key_vault_items](#test-118)
- [Test 119: create_azure_key_vault_items](#test-119)
- [Test 120: create_azure_key_vault_items](#test-120)
- [Test 121: import_azure_key_vault_certificates](#test-121)
- [Test 122: import_azure_key_vault_certificates](#test-122)
- [Test 123: get_azure_best_practices](#test-123)
- [Test 124: get_azure_best_practices](#test-124)
- [Test 125: get_azure_best_practices](#test-125)
- [Test 126: get_azure_best_practices](#test-126)
- [Test 127: get_azure_best_practices](#test-127)
- [Test 128: get_azure_best_practices](#test-128)
- [Test 129: get_azure_best_practices](#test-129)
- [Test 130: get_azure_best_practices](#test-130)
- [Test 131: get_azure_best_practices](#test-131)
- [Test 132: get_azure_best_practices](#test-132)
- [Test 133: get_azure_best_practices](#test-133)
- [Test 134: get_azure_best_practices](#test-134)
- [Test 135: get_azure_best_practices](#test-135)
- [Test 136: design_azure_architecture](#test-136)
- [Test 137: design_azure_architecture](#test-137)
- [Test 138: design_azure_architecture](#test-138)
- [Test 139: design_azure_architecture](#test-139)
- [Test 140: design_azure_architecture](#test-140)
- [Test 141: get_azure_load_testing_details](#test-141)
- [Test 142: get_azure_load_testing_details](#test-142)
- [Test 143: get_azure_load_testing_details](#test-143)
- [Test 144: get_azure_load_testing_details](#test-144)
- [Test 145: create_azure_load_testing](#test-145)
- [Test 146: create_azure_load_testing](#test-146)
- [Test 147: create_azure_load_testing](#test-147)
- [Test 148: update_azure_load_testing_configurations](#test-148)
- [Test 149: get_azure_ai_resources_details](#test-149)
- [Test 150: get_azure_ai_resources_details](#test-150)
- [Test 151: get_azure_ai_resources_details](#test-151)
- [Test 152: get_azure_ai_resources_details](#test-152)
- [Test 153: get_azure_ai_resources_details](#test-153)
- [Test 154: get_azure_ai_resources_details](#test-154)
- [Test 155: get_azure_ai_resources_details](#test-155)
- [Test 156: get_azure_ai_resources_details](#test-156)
- [Test 157: get_azure_ai_resources_details](#test-157)
- [Test 158: get_azure_ai_resources_details](#test-158)
- [Test 159: get_azure_ai_resources_details](#test-159)
- [Test 160: get_azure_ai_resources_details](#test-160)
- [Test 161: get_azure_ai_resources_details](#test-161)
- [Test 162: get_azure_ai_resources_details](#test-162)
- [Test 163: get_azure_ai_resources_details](#test-163)
- [Test 164: deploy_azure_ai_models](#test-164)
- [Test 165: get_azure_storage_details](#test-165)
- [Test 166: get_azure_storage_details](#test-166)
- [Test 167: get_azure_storage_details](#test-167)
- [Test 168: get_azure_storage_details](#test-168)
- [Test 169: get_azure_storage_details](#test-169)
- [Test 170: get_azure_storage_details](#test-170)
- [Test 171: get_azure_storage_details](#test-171)
- [Test 172: get_azure_storage_details](#test-172)
- [Test 173: get_azure_storage_details](#test-173)
- [Test 174: get_azure_storage_details](#test-174)
- [Test 175: get_azure_storage_details](#test-175)
- [Test 176: get_azure_storage_details](#test-176)
- [Test 177: get_azure_storage_details](#test-177)
- [Test 178: get_azure_storage_details](#test-178)
- [Test 179: create_azure_storage](#test-179)
- [Test 180: create_azure_storage](#test-180)
- [Test 181: create_azure_storage](#test-181)
- [Test 182: create_azure_storage](#test-182)
- [Test 183: create_azure_storage](#test-183)
- [Test 184: create_azure_storage](#test-184)
- [Test 185: upload_azure_storage_blobs](#test-185)
- [Test 186: get_azure_cache_for_redis_details](#test-186)
- [Test 187: get_azure_cache_for_redis_details](#test-187)
- [Test 188: get_azure_cache_for_redis_details](#test-188)
- [Test 189: get_azure_cache_for_redis_details](#test-189)
- [Test 190: get_azure_cache_for_redis_details](#test-190)
- [Test 191: get_azure_cache_for_redis_details](#test-191)
- [Test 192: get_azure_cache_for_redis_details](#test-192)
- [Test 193: get_azure_cache_for_redis_details](#test-193)
- [Test 194: get_azure_cache_for_redis_details](#test-194)
- [Test 195: get_azure_cache_for_redis_details](#test-195)
- [Test 196: browse_azure_marketplace_products](#test-196)
- [Test 197: browse_azure_marketplace_products](#test-197)
- [Test 198: browse_azure_marketplace_products](#test-198)
- [Test 199: get_azure_capacity](#test-199)
- [Test 200: get_azure_capacity](#test-200)
- [Test 201: get_azure_capacity](#test-201)
- [Test 202: get_azure_messaging_service_details](#test-202)
- [Test 203: get_azure_messaging_service_details](#test-203)
- [Test 204: get_azure_messaging_service_details](#test-204)
- [Test 205: get_azure_messaging_service_details](#test-205)
- [Test 206: get_azure_messaging_service_details](#test-206)
- [Test 207: get_azure_messaging_service_details](#test-207)
- [Test 208: get_azure_messaging_service_details](#test-208)
- [Test 209: get_azure_messaging_service_details](#test-209)
- [Test 210: get_azure_messaging_service_details](#test-210)
- [Test 211: get_azure_messaging_service_details](#test-211)
- [Test 212: get_azure_messaging_service_details](#test-212)
- [Test 213: get_azure_messaging_service_details](#test-213)
- [Test 214: get_azure_messaging_service_details](#test-214)
- [Test 215: get_azure_messaging_service_details](#test-215)
- [Test 216: get_azure_data_explorer_kusto_details](#test-216)
- [Test 217: get_azure_data_explorer_kusto_details](#test-217)
- [Test 218: get_azure_data_explorer_kusto_details](#test-218)
- [Test 219: get_azure_data_explorer_kusto_details](#test-219)
- [Test 220: get_azure_data_explorer_kusto_details](#test-220)
- [Test 221: get_azure_data_explorer_kusto_details](#test-221)
- [Test 222: get_azure_data_explorer_kusto_details](#test-222)
- [Test 223: get_azure_data_explorer_kusto_details](#test-223)
- [Test 224: get_azure_data_explorer_kusto_details](#test-224)
- [Test 225: get_azure_data_explorer_kusto_details](#test-225)
- [Test 226: get_azure_data_explorer_kusto_details](#test-226)
- [Test 227: create_azure_sql_firewall_rules](#test-227)
- [Test 228: create_azure_sql_firewall_rules](#test-228)
- [Test 229: create_azure_sql_firewall_rules](#test-229)
- [Test 230: delete_azure_sql_firewall_rules](#test-230)
- [Test 231: delete_azure_sql_firewall_rules](#test-231)
- [Test 232: delete_azure_sql_firewall_rules](#test-232)
- [Test 233: get_azure_sql_server_details](#test-233)
- [Test 234: get_azure_sql_server_details](#test-234)
- [Test 235: get_azure_sql_server_details](#test-235)
- [Test 236: get_azure_sql_server_details](#test-236)
- [Test 237: get_azure_sql_server_details](#test-237)
- [Test 238: get_azure_sql_server_details](#test-238)
- [Test 239: get_azure_sql_server_details](#test-239)
- [Test 240: get_azure_sql_server_details](#test-240)
- [Test 241: get_azure_sql_server_details](#test-241)
- [Test 242: get_azure_container_details](#test-242)
- [Test 243: get_azure_container_details](#test-243)
- [Test 244: get_azure_container_details](#test-244)
- [Test 245: get_azure_container_details](#test-245)
- [Test 246: get_azure_container_details](#test-246)
- [Test 247: get_azure_container_details](#test-247)
- [Test 248: get_azure_container_details](#test-248)
- [Test 249: get_azure_container_details](#test-249)
- [Test 250: get_azure_container_details](#test-250)
- [Test 251: get_azure_container_details](#test-251)
- [Test 252: get_azure_container_details](#test-252)
- [Test 253: get_azure_container_details](#test-253)
- [Test 254: get_azure_container_details](#test-254)
- [Test 255: get_azure_container_details](#test-255)
- [Test 256: get_azure_container_details](#test-256)
- [Test 257: get_azure_container_details](#test-257)
- [Test 258: get_azure_container_details](#test-258)
- [Test 259: get_azure_container_details](#test-259)
- [Test 260: get_azure_container_details](#test-260)
- [Test 261: get_azure_virtual_desktop_details](#test-261)
- [Test 262: get_azure_virtual_desktop_details](#test-262)
- [Test 263: get_azure_virtual_desktop_details](#test-263)

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
| 4 | 0.382415 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.374074 | `get_azure_databases_details` | ❌ |

---

## Test 2

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.415793 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.384034 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.328705 | `get_azure_security_configurations` | ❌ |
| 4 | 0.317407 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.265107 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 3

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549609 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.418812 | `get_azure_security_configurations` | ❌ |
| 3 | 0.409009 | `get_azure_databases_details` | ❌ |
| 4 | 0.364712 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.358284 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 4

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.347384 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.305464 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.278209 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.242468 | `get_azure_security_configurations` | ❌ |
| 5 | 0.221398 | `get_azure_databases_details` | ❌ |

---

## Test 5

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671073 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.437160 | `get_azure_security_configurations` | ❌ |
| 3 | 0.407444 | `get_azure_databases_details` | ❌ |
| 4 | 0.399372 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.381269 | `get_azure_load_testing_details` | ❌ |

---

## Test 6

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.322378 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.291206 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.246134 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.205317 | `get_azure_resource_and_app_health_status` | ❌ |
| 5 | 0.201879 | `get_azure_capacity` | ❌ |

---

## Test 7

**Expected Tool:** `get_azure_subscriptions_and_resource_groups`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.380158 | `get_azure_subscriptions_and_resource_groups` | ✅ **EXPECTED** |
| 2 | 0.362085 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.274746 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.242685 | `get_azure_container_details` | ❌ |
| 5 | 0.229933 | `get_azure_databases_details` | ❌ |

---

## Test 8

**Expected Tool:** `get_application_platform_details`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567577 | `get_application_platform_details` | ✅ **EXPECTED** |
| 2 | 0.446709 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.438587 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.383826 | `get_azure_resource_and_app_health_status` | ❌ |
| 5 | 0.382315 | `get_azure_capacity` | ❌ |

---

## Test 9

**Expected Tool:** `get_application_platform_details`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622593 | `get_azure_app_config_settings` | ❌ |
| 2 | 0.565172 | `get_application_platform_details` | ✅ **EXPECTED** |
| 3 | 0.480175 | `set_azure_app_config_settings_lock_state` | ❌ |
| 4 | 0.434875 | `get_azure_best_practices` | ❌ |
| 5 | 0.415185 | `edit_azure_app_config_settings` | ❌ |

---

## Test 10

**Expected Tool:** `get_application_platform_details`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551165 | `get_application_platform_details` | ✅ **EXPECTED** |
| 2 | 0.444696 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.430633 | `get_azure_resource_and_app_health_status` | ❌ |
| 4 | 0.340781 | `set_azure_app_config_settings_lock_state` | ❌ |
| 5 | 0.330235 | `get_azure_messaging_service_details` | ❌ |

---

## Test 11

**Expected Tool:** `get_application_platform_details`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606770 | `get_application_platform_details` | ✅ **EXPECTED** |
| 2 | 0.516861 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.498775 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.416693 | `get_azure_resource_and_app_health_status` | ❌ |
| 5 | 0.416074 | `get_azure_messaging_service_details` | ❌ |

---

## Test 12

**Expected Tool:** `get_application_platform_details`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.558485 | `get_application_platform_details` | ✅ **EXPECTED** |
| 2 | 0.448010 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.427410 | `get_azure_security_configurations` | ❌ |
| 4 | 0.421965 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.405400 | `get_azure_app_config_settings` | ❌ |

---

## Test 13

**Expected Tool:** `get_application_platform_details`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545132 | `get_application_platform_details` | ✅ **EXPECTED** |
| 2 | 0.462941 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.447054 | `get_azure_resource_and_app_health_status` | ❌ |
| 4 | 0.385066 | `execute_azure_cli` | ❌ |
| 5 | 0.375743 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 14

**Expected Tool:** `get_application_platform_details`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630201 | `get_application_platform_details` | ✅ **EXPECTED** |
| 2 | 0.514721 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.407630 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.401230 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.385685 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 15

**Expected Tool:** `get_application_platform_details`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560507 | `get_application_platform_details` | ✅ **EXPECTED** |
| 2 | 0.462610 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.448413 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.444987 | `get_azure_security_configurations` | ❌ |
| 5 | 0.437162 | `get_azure_app_config_settings` | ❌ |

---

## Test 16

**Expected Tool:** `get_application_platform_details`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650735 | `get_application_platform_details` | ✅ **EXPECTED** |
| 2 | 0.570557 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.444990 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.393760 | `get_azure_sql_server_details` | ❌ |
| 5 | 0.383607 | `get_azure_container_details` | ❌ |

---

## Test 17

**Expected Tool:** `get_application_platform_details`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.534806 | `get_application_platform_details` | ✅ **EXPECTED** |
| 2 | 0.433055 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.408729 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.390900 | `get_azure_best_practices` | ❌ |
| 5 | 0.366092 | `execute_azure_cli` | ❌ |

---

## Test 18

**Expected Tool:** `get_application_platform_details`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.416096 | `get_application_platform_details` | ✅ **EXPECTED** |
| 2 | 0.290136 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.283297 | `get_azure_resource_and_app_health_status` | ❌ |
| 4 | 0.269663 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.262952 | `execute_azure_cli` | ❌ |

---

## Test 19

**Expected Tool:** `get_application_platform_details`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547155 | `get_application_platform_details` | ✅ **EXPECTED** |
| 2 | 0.437944 | `get_azure_resource_and_app_health_status` | ❌ |
| 3 | 0.419457 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.372139 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.360335 | `get_azure_best_practices` | ❌ |

---

## Test 20

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.501515 | `get_azure_sql_server_details` | ❌ |
| 2 | 0.449240 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.373528 | `edit_azure_databases` | ❌ |
| 4 | 0.287050 | `get_azure_workbooks_details` | ❌ |
| 5 | 0.286541 | `get_application_platform_details` | ❌ |

---

## Test 21

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.470240 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.469040 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 3 | 0.449118 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.442073 | `get_azure_storage_details` | ❌ |
| 5 | 0.439142 | `get_azure_security_configurations` | ❌ |

---

## Test 22

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.486576 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.459903 | `get_azure_sql_server_details` | ❌ |
| 3 | 0.420211 | `delete_azure_sql_firewall_rules` | ❌ |
| 4 | 0.415459 | `edit_azure_databases` | ❌ |
| 5 | 0.403496 | `create_azure_sql_firewall_rules` | ❌ |

---

## Test 23

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.389996 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.353887 | `edit_azure_databases` | ❌ |
| 3 | 0.279292 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.238100 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.224949 | `get_azure_security_configurations` | ❌ |

---

## Test 24

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.443193 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.418916 | `edit_azure_databases` | ❌ |
| 3 | 0.356004 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.322073 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.305719 | `get_azure_messaging_service_details` | ❌ |

---

## Test 25

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.344675 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.321958 | `edit_azure_databases` | ❌ |
| 3 | 0.261577 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.230828 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.203815 | `delete_azure_sql_firewall_rules` | ❌ |

---

## Test 26

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.414822 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.397754 | `edit_azure_databases` | ❌ |
| 3 | 0.359479 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.323642 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.319877 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 27

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.349342 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.319283 | `edit_azure_databases` | ❌ |
| 3 | 0.246849 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.224061 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.219311 | `delete_azure_sql_firewall_rules` | ❌ |

---

## Test 28

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.312488 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.295828 | `edit_azure_databases` | ❌ |
| 3 | 0.225183 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.217793 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.202330 | `delete_azure_sql_firewall_rules` | ❌ |

---

## Test 29

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.458617 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.426045 | `get_azure_storage_details` | ❌ |
| 3 | 0.423698 | `create_azure_storage` | ❌ |
| 4 | 0.410278 | `get_azure_container_details` | ❌ |
| 5 | 0.360627 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 30

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.486509 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.372579 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.372036 | `get_azure_storage_details` | ❌ |
| 4 | 0.356233 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.340541 | `get_azure_security_configurations` | ❌ |

---

## Test 31

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.388598 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.313531 | `edit_azure_databases` | ❌ |
| 3 | 0.261138 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.249198 | `get_azure_ai_resources_details` | ❌ |
| 5 | 0.224080 | `browse_azure_marketplace_products` | ❌ |

---

## Test 32

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.349715 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.282259 | `edit_azure_databases` | ❌ |
| 3 | 0.249869 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.222915 | `get_azure_ai_resources_details` | ❌ |
| 5 | 0.211605 | `create_azure_sql_firewall_rules` | ❌ |

---

## Test 33

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573657 | `get_azure_sql_server_details` | ❌ |
| 2 | 0.486536 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.468156 | `edit_azure_databases` | ❌ |
| 4 | 0.437278 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 5 | 0.398723 | `get_azure_messaging_service_details` | ❌ |

---

## Test 34

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.318903 | `edit_azure_databases` | ❌ |
| 2 | 0.227967 | `get_azure_sql_server_details` | ❌ |
| 3 | 0.191595 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 4 | 0.165104 | `create_azure_sql_firewall_rules` | ❌ |
| 5 | 0.164953 | `get_azure_app_config_settings` | ❌ |

---

## Test 35

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.499510 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.437033 | `get_azure_storage_details` | ❌ |
| 3 | 0.421777 | `get_azure_security_configurations` | ❌ |
| 4 | 0.401078 | `get_azure_container_details` | ❌ |
| 5 | 0.396453 | `get_azure_messaging_service_details` | ❌ |

---

## Test 36

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.381945 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.357185 | `edit_azure_databases` | ❌ |
| 3 | 0.294403 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.234680 | `delete_azure_sql_firewall_rules` | ❌ |
| 5 | 0.227130 | `get_azure_security_configurations` | ❌ |

---

## Test 37

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.355399 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.354214 | `edit_azure_databases` | ❌ |
| 3 | 0.294120 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.240477 | `delete_azure_sql_firewall_rules` | ❌ |
| 5 | 0.234628 | `create_azure_sql_firewall_rules` | ❌ |

---

## Test 38

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the configuration of MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.424259 | `edit_azure_databases` | ❌ |
| 2 | 0.346477 | `get_azure_sql_server_details` | ❌ |
| 3 | 0.285564 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 4 | 0.282061 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.217174 | `delete_azure_sql_firewall_rules` | ❌ |

---

## Test 39

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.396395 | `edit_azure_databases` | ❌ |
| 2 | 0.327525 | `get_azure_sql_server_details` | ❌ |
| 3 | 0.250062 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.242546 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 5 | 0.200991 | `create_azure_sql_firewall_rules` | ❌ |

---

## Test 40

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.462828 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.403564 | `create_azure_storage` | ❌ |
| 3 | 0.395139 | `get_azure_storage_details` | ❌ |
| 4 | 0.391660 | `get_azure_container_details` | ❌ |
| 5 | 0.327793 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 41

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496094 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.488113 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.458313 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.452018 | `get_azure_storage_details` | ❌ |
| 5 | 0.444953 | `get_azure_security_configurations` | ❌ |

---

## Test 42

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.490136 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.368375 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.362453 | `get_azure_storage_details` | ❌ |
| 4 | 0.336256 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.323139 | `get_azure_security_configurations` | ❌ |

---

## Test 43

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.435369 | `get_azure_sql_server_details` | ❌ |
| 2 | 0.382094 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 3 | 0.354815 | `edit_azure_databases` | ❌ |
| 4 | 0.343500 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.325907 | `get_azure_storage_details` | ❌ |

---

## Test 44

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.442213 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.388815 | `get_azure_ai_resources_details` | ❌ |
| 3 | 0.365295 | `get_azure_storage_details` | ❌ |
| 4 | 0.361081 | `get_azure_container_details` | ❌ |
| 5 | 0.334991 | `get_azure_key_vault` | ❌ |

---

## Test 45

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.398115 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.358413 | `edit_azure_databases` | ❌ |
| 3 | 0.273842 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.239673 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.215038 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 46

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.490987 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.461477 | `edit_azure_databases` | ❌ |
| 3 | 0.386152 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.353379 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.349028 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 47

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.363106 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.332482 | `edit_azure_databases` | ❌ |
| 3 | 0.265369 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.227159 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.203381 | `create_azure_sql_firewall_rules` | ❌ |

---

## Test 48

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.451350 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.433648 | `edit_azure_databases` | ❌ |
| 3 | 0.377900 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.343503 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.339133 | `browse_azure_marketplace_products` | ❌ |

---

## Test 49

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.333177 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.314466 | `edit_azure_databases` | ❌ |
| 3 | 0.240993 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.225610 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.221418 | `get_azure_best_practices` | ❌ |

---

## Test 50

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.294539 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.283335 | `edit_azure_databases` | ❌ |
| 3 | 0.219516 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.213897 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.193859 | `get_azure_best_practices` | ❌ |

---

## Test 51

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.393677 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.335190 | `edit_azure_databases` | ❌ |
| 3 | 0.271047 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.243773 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.210444 | `get_azure_security_configurations` | ❌ |

---

## Test 52

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.352654 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 2 | 0.310370 | `edit_azure_databases` | ❌ |
| 3 | 0.244377 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.230608 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.188657 | `create_azure_sql_firewall_rules` | ❌ |

---

## Test 53

**Expected Tool:** `get_azure_databases_details`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.364740 | `edit_azure_databases` | ❌ |
| 2 | 0.206938 | `get_azure_databases_details` | ✅ **EXPECTED** |
| 3 | 0.205971 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.202957 | `delete_azure_sql_firewall_rules` | ❌ |
| 5 | 0.202122 | `create_azure_sql_firewall_rules` | ❌ |

---

## Test 54

**Expected Tool:** `edit_azure_databases`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.340869 | `edit_azure_databases` | ✅ **EXPECTED** |
| 2 | 0.250546 | `create_azure_sql_firewall_rules` | ❌ |
| 3 | 0.212740 | `delete_azure_sql_firewall_rules` | ❌ |
| 4 | 0.172018 | `get_azure_sql_server_details` | ❌ |
| 5 | 0.170591 | `get_azure_databases_details` | ❌ |

---

## Test 55

**Expected Tool:** `edit_azure_databases`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.380868 | `edit_azure_databases` | ✅ **EXPECTED** |
| 2 | 0.269323 | `create_azure_sql_firewall_rules` | ❌ |
| 3 | 0.251993 | `delete_azure_sql_firewall_rules` | ❌ |
| 4 | 0.175184 | `get_azure_databases_details` | ❌ |
| 5 | 0.171002 | `get_azure_sql_server_details` | ❌ |

---

## Test 56

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.451202 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.405561 | `get_azure_capacity` | ❌ |
| 3 | 0.402104 | `create_azure_load_testing` | ❌ |
| 4 | 0.398005 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.385907 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 57

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.475173 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.435791 | `get_azure_capacity` | ❌ |
| 3 | 0.381293 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.371078 | `get_azure_ai_resources_details` | ❌ |
| 5 | 0.368152 | `create_azure_load_testing` | ❌ |

---

## Test 58

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.315424 | `get_azure_storage_details` | ❌ |
| 2 | 0.295675 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 3 | 0.279543 | `get_azure_capacity` | ❌ |
| 4 | 0.272938 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.269671 | `get_azure_messaging_service_details` | ❌ |

---

## Test 59

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.318502 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.253703 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.245821 | `get_azure_databases_details` | ❌ |
| 4 | 0.245524 | `get_azure_storage_details` | ❌ |
| 5 | 0.244056 | `get_azure_load_testing_details` | ❌ |

---

## Test 60

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.434203 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.361614 | `get_azure_capacity` | ❌ |
| 3 | 0.350486 | `get_azure_storage_details` | ❌ |
| 4 | 0.325422 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.305299 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 61

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.427290 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.405155 | `get_azure_capacity` | ❌ |
| 3 | 0.379113 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.375177 | `create_azure_load_testing` | ❌ |
| 5 | 0.370855 | `get_azure_load_testing_details` | ❌ |

---

## Test 62

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.438756 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.436208 | `get_azure_databases_details` | ❌ |
| 3 | 0.407964 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.398563 | `get_azure_workbooks_details` | ❌ |
| 5 | 0.383577 | `create_azure_workbooks` | ❌ |

---

## Test 63

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.482062 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.461521 | `get_azure_databases_details` | ❌ |
| 3 | 0.445863 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.441003 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.438518 | `get_azure_security_configurations` | ❌ |

---

## Test 64

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512722 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.463935 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.452333 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.437283 | `create_azure_workbooks` | ❌ |
| 5 | 0.419394 | `get_azure_security_configurations` | ❌ |

---

## Test 65

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.484962 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.392732 | `get_azure_databases_details` | ❌ |
| 3 | 0.356965 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.349385 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.321615 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 66

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.441261 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.418441 | `get_azure_workbooks_details` | ❌ |
| 3 | 0.415147 | `get_azure_databases_details` | ❌ |
| 4 | 0.398640 | `create_azure_workbooks` | ❌ |
| 5 | 0.393598 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 67

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525913 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.507695 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 3 | 0.465101 | `get_azure_capacity` | ❌ |
| 4 | 0.460682 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.455781 | `get_azure_load_testing_details` | ❌ |

---

## Test 68

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.338738 | `get_azure_databases_details` | ❌ |
| 2 | 0.333991 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 3 | 0.267238 | `get_azure_capacity` | ❌ |
| 4 | 0.264069 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.253990 | `get_azure_storage_details` | ❌ |

---

## Test 69

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551584 | `get_azure_storage_details` | ❌ |
| 2 | 0.431185 | `get_azure_capacity` | ❌ |
| 3 | 0.393636 | `get_azure_container_details` | ❌ |
| 4 | 0.392555 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.383711 | `get_azure_app_config_settings` | ❌ |

---

## Test 70

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.528584 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.488687 | `create_azure_workbooks` | ❌ |
| 3 | 0.451336 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.410409 | `get_azure_security_configurations` | ❌ |
| 5 | 0.409488 | `get_azure_databases_details` | ❌ |

---

## Test 71

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.449939 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.439021 | `get_azure_databases_details` | ❌ |
| 3 | 0.405799 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.396006 | `get_azure_workbooks_details` | ❌ |
| 5 | 0.387039 | `create_azure_workbooks` | ❌ |

---

## Test 72

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610196 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.499251 | `get_azure_databases_details` | ❌ |
| 3 | 0.490462 | `get_azure_capacity` | ❌ |
| 4 | 0.475264 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.471465 | `get_azure_security_configurations` | ❌ |

---

## Test 73

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the health status of entity <entity_id> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.598944 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.359140 | `get_azure_workbooks_details` | ❌ |
| 3 | 0.340267 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.334028 | `create_azure_workbooks` | ❌ |
| 5 | 0.331249 | `get_azure_capacity` | ❌ |

---

## Test 74

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496367 | `get_azure_storage_details` | ❌ |
| 2 | 0.431978 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 3 | 0.400710 | `create_azure_storage` | ❌ |
| 4 | 0.382684 | `get_azure_capacity` | ❌ |
| 5 | 0.339439 | `get_azure_security_configurations` | ❌ |

---

## Test 75

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527120 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.479435 | `create_azure_workbooks` | ❌ |
| 3 | 0.458853 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.456725 | `get_azure_workbooks_details` | ❌ |
| 5 | 0.418907 | `browse_azure_marketplace_products` | ❌ |

---

## Test 76

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469281 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.376703 | `get_azure_databases_details` | ❌ |
| 3 | 0.365333 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.355340 | `get_azure_capacity` | ❌ |
| 5 | 0.333899 | `get_azure_load_testing_details` | ❌ |

---

## Test 77

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.446681 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.375661 | `create_azure_workbooks` | ❌ |
| 3 | 0.357168 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.338168 | `get_azure_databases_details` | ❌ |
| 5 | 0.336058 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 78

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.484089 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.406616 | `get_azure_databases_details` | ❌ |
| 3 | 0.354368 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.346010 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.319276 | `get_azure_capacity` | ❌ |

---

## Test 79

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.447749 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.433218 | `get_azure_databases_details` | ❌ |
| 3 | 0.420831 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.400964 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.400003 | `create_azure_workbooks` | ❌ |

---

## Test 80

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.437518 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.416952 | `get_azure_capacity` | ❌ |
| 3 | 0.404977 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.380088 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.343837 | `get_azure_ai_resources_details` | ❌ |

---

## Test 81

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.421297 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.410786 | `get_azure_capacity` | ❌ |
| 3 | 0.382899 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.381994 | `get_application_platform_details` | ❌ |
| 5 | 0.380026 | `get_azure_ai_resources_details` | ❌ |

---

## Test 82

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532228 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 2 | 0.437141 | `get_azure_capacity` | ❌ |
| 3 | 0.389687 | `get_azure_virtual_desktop_details` | ❌ |
| 4 | 0.382850 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.374415 | `get_azure_databases_details` | ❌ |

---

## Test 83

**Expected Tool:** `get_azure_resource_and_app_health_status`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.422305 | `get_azure_capacity` | ❌ |
| 2 | 0.388641 | `get_azure_resource_and_app_health_status` | ✅ **EXPECTED** |
| 3 | 0.369677 | `create_azure_load_testing` | ❌ |
| 4 | 0.353073 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.349272 | `get_application_platform_details` | ❌ |

---

## Test 84

**Expected Tool:** `deploy_resources_and_applications_to_azure`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642708 | `deploy_resources_and_applications_to_azure` | ✅ **EXPECTED** |
| 2 | 0.519278 | `deploy_azure_ai_models` | ❌ |
| 3 | 0.515742 | `design_azure_architecture` | ❌ |
| 4 | 0.479904 | `get_azure_best_practices` | ❌ |
| 5 | 0.454754 | `execute_azure_developer_cli` | ❌ |

---

## Test 85

**Expected Tool:** `deploy_resources_and_applications_to_azure`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591844 | `deploy_resources_and_applications_to_azure` | ✅ **EXPECTED** |
| 2 | 0.477740 | `deploy_azure_ai_models` | ❌ |
| 3 | 0.437318 | `execute_azure_developer_cli` | ❌ |
| 4 | 0.410713 | `get_azure_best_practices` | ❌ |
| 5 | 0.401777 | `execute_azure_cli` | ❌ |

---

## Test 86

**Expected Tool:** `deploy_resources_and_applications_to_azure`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533141 | `execute_azure_developer_cli` | ❌ |
| 2 | 0.524236 | `deploy_resources_and_applications_to_azure` | ✅ **EXPECTED** |
| 3 | 0.427108 | `get_azure_resource_and_app_health_status` | ❌ |
| 4 | 0.402454 | `get_application_platform_details` | ❌ |
| 5 | 0.396308 | `execute_azure_cli` | ❌ |

---

## Test 87

**Expected Tool:** `deploy_resources_and_applications_to_azure`  
**Prompt:** Show me the rules to generate bicep scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488000 | `get_azure_best_practices` | ❌ |
| 2 | 0.336943 | `design_azure_architecture` | ❌ |
| 3 | 0.328879 | `deploy_resources_and_applications_to_azure` | ✅ **EXPECTED** |
| 4 | 0.325324 | `create_azure_sql_firewall_rules` | ❌ |
| 5 | 0.315430 | `execute_azure_cli` | ❌ |

---

## Test 88

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549804 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.418698 | `set_azure_app_config_settings_lock_state` | ❌ |
| 3 | 0.401422 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.400070 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.397547 | `get_azure_sql_server_details` | ❌ |

---

## Test 89

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.605174 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.469735 | `set_azure_app_config_settings_lock_state` | ❌ |
| 3 | 0.413315 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.316476 | `get_azure_sql_server_details` | ❌ |
| 5 | 0.304659 | `update_azure_load_testing_configurations` | ❌ |

---

## Test 90

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.517123 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.397359 | `set_azure_app_config_settings_lock_state` | ❌ |
| 3 | 0.318242 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.307380 | `get_azure_sql_server_details` | ❌ |
| 5 | 0.296300 | `browse_azure_marketplace_products` | ❌ |

---

## Test 91

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564754 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.445478 | `set_azure_app_config_settings_lock_state` | ❌ |
| 3 | 0.382377 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.378099 | `get_azure_sql_server_details` | ❌ |
| 5 | 0.368144 | `get_azure_messaging_service_details` | ❌ |

---

## Test 92

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619236 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.496884 | `set_azure_app_config_settings_lock_state` | ❌ |
| 3 | 0.413994 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.323271 | `get_azure_sql_server_details` | ❌ |
| 5 | 0.303617 | `update_azure_load_testing_configurations` | ❌ |

---

## Test 93

**Expected Tool:** `get_azure_app_config_settings`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.474288 | `get_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.398144 | `set_azure_app_config_settings_lock_state` | ❌ |
| 3 | 0.320151 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.291243 | `get_azure_key_vault` | ❌ |
| 5 | 0.237162 | `get_azure_container_details` | ❌ |

---

## Test 94

**Expected Tool:** `edit_azure_app_config_settings`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480490 | `edit_azure_app_config_settings` | ✅ **EXPECTED** |
| 2 | 0.419225 | `set_azure_app_config_settings_lock_state` | ❌ |
| 3 | 0.386233 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.236794 | `edit_azure_workbooks` | ❌ |
| 5 | 0.226127 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 95

**Expected Tool:** `edit_azure_app_config_settings`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.454669 | `set_azure_app_config_settings_lock_state` | ❌ |
| 2 | 0.419517 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.418814 | `edit_azure_app_config_settings` | ✅ **EXPECTED** |
| 4 | 0.251832 | `update_azure_load_testing_configurations` | ❌ |
| 5 | 0.227122 | `edit_azure_databases` | ❌ |

---

## Test 96

**Expected Tool:** `set_azure_app_config_settings_lock_state`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.523446 | `set_azure_app_config_settings_lock_state` | ✅ **EXPECTED** |
| 2 | 0.367924 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.324653 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.206576 | `import_azure_key_vault_certificates` | ❌ |
| 5 | 0.186093 | `update_azure_load_testing_configurations` | ❌ |

---

## Test 97

**Expected Tool:** `set_azure_app_config_settings_lock_state`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552583 | `set_azure_app_config_settings_lock_state` | ✅ **EXPECTED** |
| 2 | 0.393938 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.339108 | `edit_azure_app_config_settings` | ❌ |
| 4 | 0.240636 | `import_azure_key_vault_certificates` | ❌ |
| 5 | 0.232320 | `get_azure_key_vault` | ❌ |

---

## Test 98

**Expected Tool:** `edit_azure_workbooks`  
**Prompt:** Delete the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.505878 | `edit_azure_workbooks` | ✅ **EXPECTED** |
| 2 | 0.375703 | `create_azure_workbooks` | ❌ |
| 3 | 0.362979 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.265457 | `edit_azure_app_config_settings` | ❌ |
| 5 | 0.188350 | `create_azure_load_testing` | ❌ |

---

## Test 99

**Expected Tool:** `edit_azure_workbooks`  
**Prompt:** Update the workbook <workbook_resource_id> with a new text step  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496535 | `edit_azure_workbooks` | ✅ **EXPECTED** |
| 2 | 0.413267 | `create_azure_workbooks` | ❌ |
| 3 | 0.327796 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.236165 | `update_azure_load_testing_configurations` | ❌ |
| 5 | 0.216298 | `edit_azure_app_config_settings` | ❌ |

---

## Test 100

**Expected Tool:** `create_azure_workbooks`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555091 | `create_azure_workbooks` | ✅ **EXPECTED** |
| 2 | 0.400619 | `edit_azure_workbooks` | ❌ |
| 3 | 0.371495 | `get_azure_workbooks_details` | ❌ |
| 4 | 0.196704 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.157512 | `create_azure_storage` | ❌ |

---

## Test 101

**Expected Tool:** `get_azure_workbooks_details`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512253 | `get_azure_workbooks_details` | ✅ **EXPECTED** |
| 2 | 0.409967 | `edit_azure_workbooks` | ❌ |
| 3 | 0.409127 | `create_azure_workbooks` | ❌ |
| 4 | 0.299382 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.294878 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 102

**Expected Tool:** `get_azure_workbooks_details`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552702 | `get_azure_workbooks_details` | ✅ **EXPECTED** |
| 2 | 0.514603 | `create_azure_workbooks` | ❌ |
| 3 | 0.441697 | `edit_azure_workbooks` | ❌ |
| 4 | 0.426606 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.396091 | `get_azure_security_configurations` | ❌ |

---

## Test 103

**Expected Tool:** `get_azure_workbooks_details`  
**Prompt:** Show me the workbook with display name <workbook_display_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.474463 | `get_azure_workbooks_details` | ✅ **EXPECTED** |
| 2 | 0.454862 | `create_azure_workbooks` | ❌ |
| 3 | 0.422536 | `edit_azure_workbooks` | ❌ |
| 4 | 0.201213 | `get_azure_security_configurations` | ❌ |
| 5 | 0.181802 | `browse_azure_marketplace_products` | ❌ |

---

## Test 104

**Expected Tool:** `get_azure_workbooks_details`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549690 | `get_azure_workbooks_details` | ✅ **EXPECTED** |
| 2 | 0.529735 | `create_azure_workbooks` | ❌ |
| 3 | 0.453173 | `edit_azure_workbooks` | ❌ |
| 4 | 0.438514 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.391845 | `get_azure_security_configurations` | ❌ |

---

## Test 105

**Expected Tool:** `audit_azure_resources_compliance`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552351 | `get_azure_capacity` | ❌ |
| 2 | 0.546941 | `audit_azure_resources_compliance` | ✅ **EXPECTED** |
| 3 | 0.500223 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.477351 | `get_azure_best_practices` | ❌ |
| 5 | 0.452732 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 106

**Expected Tool:** `audit_azure_resources_compliance`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536483 | `audit_azure_resources_compliance` | ✅ **EXPECTED** |
| 2 | 0.511009 | `get_azure_best_practices` | ❌ |
| 3 | 0.490293 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.488706 | `get_azure_capacity` | ❌ |
| 5 | 0.452616 | `execute_azure_cli` | ❌ |

---

## Test 107

**Expected Tool:** `audit_azure_resources_compliance`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592611 | `audit_azure_resources_compliance` | ✅ **EXPECTED** |
| 2 | 0.508724 | `get_azure_best_practices` | ❌ |
| 3 | 0.502378 | `get_azure_capacity` | ❌ |
| 4 | 0.492553 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.472186 | `execute_azure_cli` | ❌ |

---

## Test 108

**Expected Tool:** `get_azure_security_configurations`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.734114 | `get_azure_security_configurations` | ✅ **EXPECTED** |
| 2 | 0.460374 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.408590 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.368186 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.356351 | `browse_azure_marketplace_products` | ❌ |

---

## Test 109

**Expected Tool:** `get_azure_security_configurations`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702749 | `get_azure_security_configurations` | ✅ **EXPECTED** |
| 2 | 0.485211 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.427639 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.388410 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.380793 | `get_azure_messaging_service_details` | ❌ |

---

## Test 110

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542408 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.539243 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 3 | 0.461491 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.385604 | `get_azure_security_configurations` | ❌ |
| 5 | 0.293023 | `get_azure_storage_details` | ❌ |

---

## Test 111

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.497101 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.429278 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.427597 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.378915 | `get_azure_security_configurations` | ❌ |
| 5 | 0.337198 | `get_azure_storage_details` | ❌ |

---

## Test 112

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498894 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.434446 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.372548 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.348550 | `get_azure_security_configurations` | ❌ |
| 5 | 0.299993 | `get_azure_app_config_settings` | ❌ |

---

## Test 113

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542386 | `import_azure_key_vault_certificates` | ❌ |
| 2 | 0.526170 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 3 | 0.434878 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.300457 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.297388 | `get_azure_security_configurations` | ❌ |

---

## Test 114

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557832 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.552928 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.461463 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.368299 | `get_azure_security_configurations` | ❌ |
| 5 | 0.320935 | `get_azure_app_config_settings` | ❌ |

---

## Test 115

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.519309 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.490672 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.420402 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.409324 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.352123 | `get_azure_messaging_service_details` | ❌ |

---

## Test 116

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518093 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.448371 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.432123 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.344132 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.337168 | `get_azure_security_configurations` | ❌ |

---

## Test 117

**Expected Tool:** `get_azure_key_vault`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545032 | `get_azure_key_vault` | ✅ **EXPECTED** |
| 2 | 0.460930 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.426022 | `import_azure_key_vault_certificates` | ❌ |
| 4 | 0.366899 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.357027 | `get_azure_storage_details` | ❌ |

---

## Test 118

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577228 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.536615 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.432370 | `get_azure_key_vault` | ❌ |
| 4 | 0.283179 | `create_azure_storage` | ❌ |
| 5 | 0.282124 | `create_azure_workbooks` | ❌ |

---

## Test 119

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.493929 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.417468 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.344129 | `get_azure_key_vault` | ❌ |
| 4 | 0.281591 | `create_azure_storage` | ❌ |
| 5 | 0.230664 | `create_azure_workbooks` | ❌ |

---

## Test 120

**Expected Tool:** `create_azure_key_vault_items`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551337 | `create_azure_key_vault_items` | ✅ **EXPECTED** |
| 2 | 0.385343 | `import_azure_key_vault_certificates` | ❌ |
| 3 | 0.383659 | `get_azure_key_vault` | ❌ |
| 4 | 0.301886 | `set_azure_app_config_settings_lock_state` | ❌ |
| 5 | 0.294044 | `create_azure_storage` | ❌ |

---

## Test 121

**Expected Tool:** `import_azure_key_vault_certificates`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660888 | `import_azure_key_vault_certificates` | ✅ **EXPECTED** |
| 2 | 0.459725 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.400860 | `get_azure_key_vault` | ❌ |
| 4 | 0.256688 | `deploy_azure_ai_models` | ❌ |
| 5 | 0.240517 | `create_azure_sql_firewall_rules` | ❌ |

---

## Test 122

**Expected Tool:** `import_azure_key_vault_certificates`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645826 | `import_azure_key_vault_certificates` | ✅ **EXPECTED** |
| 2 | 0.425682 | `create_azure_key_vault_items` | ❌ |
| 3 | 0.392675 | `get_azure_key_vault` | ❌ |
| 4 | 0.249209 | `upload_azure_storage_blobs` | ❌ |
| 5 | 0.248738 | `deploy_azure_ai_models` | ❌ |

---

## Test 123

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Create the plan for creating a simple HTTP-triggered function app in javascript that returns a random compliment from a predefined list in a JSON response. And deploy it to azure eventually. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.393322 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.391704 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.381749 | `get_application_platform_details` | ❌ |
| 4 | 0.350969 | `design_azure_architecture` | ❌ |
| 5 | 0.340836 | `deploy_azure_ai_models` | ❌ |

---

## Test 124

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Create the plan for creating a to-do list app. And deploy it to azure as a container app. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.464897 | `deploy_resources_and_applications_to_azure` | ❌ |
| 2 | 0.390048 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 3 | 0.385816 | `design_azure_architecture` | ❌ |
| 4 | 0.384385 | `deploy_azure_ai_models` | ❌ |
| 5 | 0.380941 | `create_azure_storage` | ❌ |

---

## Test 125

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.734966 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.484098 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.474532 | `search_microsoft_docs` | ❌ |
| 4 | 0.459688 | `execute_azure_cli` | ❌ |
| 5 | 0.436798 | `browse_azure_marketplace_products` | ❌ |

---

## Test 126

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690042 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.601714 | `search_microsoft_docs` | ❌ |
| 3 | 0.524057 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.508718 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.487540 | `get_azure_capacity` | ❌ |

---

## Test 127

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713365 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.543696 | `search_microsoft_docs` | ❌ |
| 3 | 0.525038 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.512007 | `design_azure_architecture` | ❌ |
| 5 | 0.435613 | `browse_azure_marketplace_products` | ❌ |

---

## Test 128

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.683345 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.619725 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.558588 | `search_microsoft_docs` | ❌ |
| 4 | 0.478904 | `design_azure_architecture` | ❌ |
| 5 | 0.465496 | `browse_azure_marketplace_products` | ❌ |

---

## Test 129

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.681951 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.557930 | `get_application_platform_details` | ❌ |
| 3 | 0.556021 | `search_microsoft_docs` | ❌ |
| 4 | 0.489786 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.443359 | `browse_azure_marketplace_products` | ❌ |

---

## Test 130

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685176 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.499419 | `get_application_platform_details` | ❌ |
| 3 | 0.486074 | `search_microsoft_docs` | ❌ |
| 4 | 0.469229 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.455695 | `design_azure_architecture` | ❌ |

---

## Test 131

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.675293 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.570960 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.537928 | `get_application_platform_details` | ❌ |
| 4 | 0.527886 | `search_microsoft_docs` | ❌ |
| 5 | 0.440919 | `design_azure_architecture` | ❌ |

---

## Test 132

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612793 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.520938 | `search_microsoft_docs` | ❌ |
| 3 | 0.501695 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.424667 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.418814 | `get_azure_app_config_settings` | ❌ |

---

## Test 133

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480733 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.447110 | `get_azure_ai_resources_details` | ❌ |
| 3 | 0.436599 | `deploy_azure_ai_models` | ❌ |
| 4 | 0.423060 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.391152 | `get_azure_capacity` | ❌ |

---

## Test 134

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618313 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.496677 | `get_azure_key_vault` | ❌ |
| 3 | 0.478248 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.444000 | `import_azure_key_vault_certificates` | ❌ |
| 5 | 0.412101 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 135

**Expected Tool:** `get_azure_best_practices`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627987 | `get_azure_best_practices` | ✅ **EXPECTED** |
| 2 | 0.475164 | `get_application_platform_details` | ❌ |
| 3 | 0.451968 | `search_microsoft_docs` | ❌ |
| 4 | 0.443178 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.394211 | `get_azure_capacity` | ❌ |

---

## Test 136

**Expected Tool:** `design_azure_architecture`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736950 | `design_azure_architecture` | ✅ **EXPECTED** |
| 2 | 0.481581 | `get_azure_best_practices` | ❌ |
| 3 | 0.457503 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.386793 | `execute_azure_developer_cli` | ❌ |
| 5 | 0.385718 | `audit_azure_resources_compliance` | ❌ |

---

## Test 137

**Expected Tool:** `design_azure_architecture`  
**Prompt:** Help me create a cloud service that will serve as ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.306056 | `design_azure_architecture` | ✅ **EXPECTED** |
| 2 | 0.259518 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.241293 | `create_azure_storage` | ❌ |
| 4 | 0.221449 | `deploy_resources_and_applications_to_azure` | ❌ |
| 5 | 0.221006 | `search_microsoft_docs` | ❌ |

---

## Test 138

**Expected Tool:** `design_azure_architecture`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.427652 | `upload_azure_storage_blobs` | ❌ |
| 2 | 0.414019 | `design_azure_architecture` | ✅ **EXPECTED** |
| 3 | 0.413253 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.410459 | `create_azure_storage` | ❌ |
| 5 | 0.410342 | `search_microsoft_docs` | ❌ |

---

## Test 139

**Expected Tool:** `design_azure_architecture`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.342485 | `browse_azure_marketplace_products` | ❌ |
| 2 | 0.318718 | `design_azure_architecture` | ✅ **EXPECTED** |
| 3 | 0.275570 | `deploy_resources_and_applications_to_azure` | ❌ |
| 4 | 0.231297 | `get_application_platform_details` | ❌ |
| 5 | 0.224297 | `get_azure_best_practices` | ❌ |

---

## Test 140

**Expected Tool:** `design_azure_architecture`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.365156 | `design_azure_architecture` | ✅ **EXPECTED** |
| 2 | 0.323311 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.245346 | `create_azure_storage` | ❌ |
| 4 | 0.235691 | `get_azure_storage_details` | ❌ |
| 5 | 0.225571 | `get_azure_capacity` | ❌ |

---

## Test 141

**Expected Tool:** `get_azure_load_testing_details`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609521 | `get_azure_load_testing_details` | ✅ **EXPECTED** |
| 2 | 0.568027 | `create_azure_load_testing` | ❌ |
| 3 | 0.447987 | `update_azure_load_testing_configurations` | ❌ |
| 4 | 0.366478 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.334726 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 142

**Expected Tool:** `get_azure_load_testing_details`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599651 | `create_azure_load_testing` | ❌ |
| 2 | 0.581081 | `get_azure_load_testing_details` | ✅ **EXPECTED** |
| 3 | 0.457483 | `update_azure_load_testing_configurations` | ❌ |
| 4 | 0.357813 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.328938 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 143

**Expected Tool:** `get_azure_load_testing_details`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612800 | `create_azure_load_testing` | ❌ |
| 2 | 0.592725 | `get_azure_load_testing_details` | ✅ **EXPECTED** |
| 3 | 0.421873 | `update_azure_load_testing_configurations` | ❌ |
| 4 | 0.349117 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.340432 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 144

**Expected Tool:** `get_azure_load_testing_details`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669717 | `get_azure_load_testing_details` | ✅ **EXPECTED** |
| 2 | 0.609875 | `create_azure_load_testing` | ❌ |
| 3 | 0.493520 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.427197 | `get_azure_capacity` | ❌ |
| 5 | 0.411161 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 145

**Expected Tool:** `create_azure_load_testing`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542817 | `create_azure_load_testing` | ✅ **EXPECTED** |
| 2 | 0.431906 | `get_azure_load_testing_details` | ❌ |
| 3 | 0.425527 | `update_azure_load_testing_configurations` | ❌ |
| 4 | 0.303407 | `create_azure_workbooks` | ❌ |
| 5 | 0.294287 | `get_azure_capacity` | ❌ |

---

## Test 146

**Expected Tool:** `create_azure_load_testing`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659657 | `create_azure_load_testing` | ✅ **EXPECTED** |
| 2 | 0.530030 | `get_azure_load_testing_details` | ❌ |
| 3 | 0.410883 | `update_azure_load_testing_configurations` | ❌ |
| 4 | 0.374253 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.342719 | `get_azure_capacity` | ❌ |

---

## Test 147

**Expected Tool:** `create_azure_load_testing`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585054 | `create_azure_load_testing` | ✅ **EXPECTED** |
| 2 | 0.497079 | `update_azure_load_testing_configurations` | ❌ |
| 3 | 0.460357 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.319141 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.297099 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 148

**Expected Tool:** `update_azure_load_testing_configurations`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577419 | `update_azure_load_testing_configurations` | ✅ **EXPECTED** |
| 2 | 0.501316 | `create_azure_load_testing` | ❌ |
| 3 | 0.443800 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.303358 | `edit_azure_workbooks` | ❌ |
| 5 | 0.257467 | `edit_azure_databases` | ❌ |

---

## Test 149

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.310095 | `get_azure_app_config_settings` | ❌ |
| 2 | 0.268639 | `get_azure_best_practices` | ❌ |
| 3 | 0.262166 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.249688 | `get_azure_workbooks_details` | ❌ |
| 5 | 0.245377 | `get_azure_sql_server_details` | ❌ |

---

## Test 150

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619824 | `deploy_azure_ai_models` | ❌ |
| 2 | 0.496307 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.362424 | `design_azure_architecture` | ❌ |
| 4 | 0.359607 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.356163 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 151

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.472872 | `deploy_azure_ai_models` | ❌ |
| 2 | 0.454365 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.338517 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.259499 | `design_azure_architecture` | ❌ |
| 5 | 0.251448 | `get_azure_databases_details` | ❌ |

---

## Test 152

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.528577 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.474808 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.444086 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.419389 | `get_application_platform_details` | ❌ |
| 5 | 0.399308 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 153

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.468505 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.356671 | `get_azure_security_configurations` | ❌ |
| 3 | 0.350943 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.345507 | `get_azure_databases_details` | ❌ |
| 5 | 0.344029 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 154

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.473670 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.407003 | `deploy_azure_ai_models` | ❌ |
| 3 | 0.288627 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.286997 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.271542 | `get_azure_databases_details` | ❌ |

---

## Test 155

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.430979 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.280912 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.279030 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.277447 | `get_azure_databases_details` | ❌ |
| 5 | 0.272738 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 156

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612664 | `deploy_azure_ai_models` | ❌ |
| 2 | 0.493076 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.389499 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.364626 | `design_azure_architecture` | ❌ |
| 5 | 0.358370 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 157

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.483332 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.431878 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.374112 | `get_azure_databases_details` | ❌ |
| 4 | 0.364824 | `get_azure_container_details` | ❌ |
| 5 | 0.363484 | `get_application_platform_details` | ❌ |

---

## Test 158

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the available AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533639 | `deploy_azure_ai_models` | ❌ |
| 2 | 0.471847 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 3 | 0.384895 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.274684 | `design_azure_architecture` | ❌ |
| 5 | 0.261365 | `get_azure_databases_details` | ❌ |

---

## Test 159

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.539532 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.507116 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.458450 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.425779 | `get_application_platform_details` | ❌ |
| 5 | 0.411862 | `get_azure_databases_details` | ❌ |

---

## Test 160

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496260 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.430990 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.417792 | `get_application_platform_details` | ❌ |
| 4 | 0.406409 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.398647 | `get_azure_app_config_settings` | ❌ |

---

## Test 161

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.456784 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.366135 | `get_azure_databases_details` | ❌ |
| 3 | 0.362259 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.360971 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.335022 | `get_azure_security_configurations` | ❌ |

---

## Test 162

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.468719 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.414405 | `deploy_azure_ai_models` | ❌ |
| 3 | 0.299211 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.290181 | `get_azure_capacity` | ❌ |
| 5 | 0.287340 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 163

**Expected Tool:** `get_azure_ai_resources_details`  
**Prompt:** Show me the schema for knowledge index <index-name> in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.418102 | `get_azure_ai_resources_details` | ✅ **EXPECTED** |
| 2 | 0.373160 | `deploy_azure_ai_models` | ❌ |
| 3 | 0.324788 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.308690 | `get_azure_best_practices` | ❌ |
| 5 | 0.289972 | `get_azure_databases_details` | ❌ |

---

## Test 164

**Expected Tool:** `deploy_azure_ai_models`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.387447 | `deploy_azure_ai_models` | ✅ **EXPECTED** |
| 2 | 0.301223 | `deploy_resources_and_applications_to_azure` | ❌ |
| 3 | 0.299302 | `create_azure_load_testing` | ❌ |
| 4 | 0.240425 | `edit_azure_databases` | ❌ |
| 5 | 0.239872 | `get_azure_capacity` | ❌ |

---

## Test 165

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.648174 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.475469 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.449058 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.430956 | `get_application_platform_details` | ❌ |
| 5 | 0.429429 | `get_azure_container_details` | ❌ |

---

## Test 166

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619030 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.529062 | `create_azure_storage` | ❌ |
| 3 | 0.478682 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.466323 | `get_azure_container_details` | ❌ |
| 5 | 0.415661 | `get_azure_app_config_settings` | ❌ |

---

## Test 167

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587279 | `create_azure_storage` | ❌ |
| 2 | 0.518468 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 3 | 0.453137 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.376493 | `get_azure_container_details` | ❌ |
| 5 | 0.336839 | `get_azure_security_configurations` | ❌ |

---

## Test 168

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551096 | `create_azure_storage` | ❌ |
| 2 | 0.508411 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 3 | 0.463738 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.348803 | `get_azure_container_details` | ❌ |
| 5 | 0.309742 | `get_azure_security_configurations` | ❌ |

---

## Test 169

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.535565 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.459444 | `create_azure_storage` | ❌ |
| 3 | 0.444366 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.417742 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.400577 | `browse_azure_marketplace_products` | ❌ |

---

## Test 170

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.614534 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.432052 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.422723 | `get_azure_capacity` | ❌ |
| 4 | 0.412758 | `get_azure_databases_details` | ❌ |
| 5 | 0.408527 | `get_azure_load_testing_details` | ❌ |

---

## Test 171

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607043 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.455284 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.431655 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.427369 | `get_azure_capacity` | ❌ |
| 5 | 0.395359 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 172

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.490615 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.404977 | `create_azure_storage` | ❌ |
| 3 | 0.390632 | `get_azure_capacity` | ❌ |
| 4 | 0.384612 | `get_azure_security_configurations` | ❌ |
| 5 | 0.361530 | `get_azure_container_details` | ❌ |

---

## Test 173

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546896 | `create_azure_storage` | ❌ |
| 2 | 0.509039 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 3 | 0.472639 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.408261 | `get_azure_container_details` | ❌ |
| 5 | 0.323929 | `get_azure_key_vault` | ❌ |

---

## Test 174

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543409 | `create_azure_storage` | ❌ |
| 2 | 0.527491 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 3 | 0.428357 | `get_azure_container_details` | ❌ |
| 4 | 0.418173 | `upload_azure_storage_blobs` | ❌ |
| 5 | 0.354495 | `get_azure_security_configurations` | ❌ |

---

## Test 175

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.601969 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.449230 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.434194 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.431374 | `get_azure_container_details` | ❌ |
| 5 | 0.419042 | `create_azure_storage` | ❌ |

---

## Test 176

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549426 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.521396 | `create_azure_storage` | ❌ |
| 3 | 0.457227 | `upload_azure_storage_blobs` | ❌ |
| 4 | 0.420784 | `get_azure_container_details` | ❌ |
| 5 | 0.348613 | `get_azure_app_config_settings` | ❌ |

---

## Test 177

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.559871 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.498174 | `create_azure_storage` | ❌ |
| 3 | 0.451684 | `get_azure_container_details` | ❌ |
| 4 | 0.398764 | `upload_azure_storage_blobs` | ❌ |
| 5 | 0.374824 | `get_azure_capacity` | ❌ |

---

## Test 178

**Expected Tool:** `get_azure_storage_details`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.495299 | `get_azure_storage_details` | ✅ **EXPECTED** |
| 2 | 0.476386 | `create_azure_storage` | ❌ |
| 3 | 0.430206 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.399080 | `get_azure_sql_server_details` | ❌ |
| 5 | 0.397760 | `get_azure_messaging_service_details` | ❌ |

---

## Test 179

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546204 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.431996 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.404325 | `get_azure_storage_details` | ❌ |
| 4 | 0.318029 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.304649 | `search_microsoft_docs` | ❌ |

---

## Test 180

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478014 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.375790 | `get_azure_storage_details` | ❌ |
| 3 | 0.329543 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.307994 | `create_azure_load_testing` | ❌ |
| 5 | 0.292741 | `upload_azure_storage_blobs` | ❌ |

---

## Test 181

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557679 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.451935 | `get_azure_storage_details` | ❌ |
| 3 | 0.432518 | `create_azure_key_vault_items` | ❌ |
| 4 | 0.423712 | `upload_azure_storage_blobs` | ❌ |
| 5 | 0.395078 | `create_azure_workbooks` | ❌ |

---

## Test 182

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488344 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.436857 | `get_azure_storage_details` | ❌ |
| 3 | 0.413299 | `get_azure_capacity` | ❌ |
| 4 | 0.356649 | `create_azure_load_testing` | ❌ |
| 5 | 0.346863 | `create_azure_key_vault_items` | ❌ |

---

## Test 183

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.631937 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.487471 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.425315 | `get_azure_storage_details` | ❌ |
| 4 | 0.332783 | `get_azure_container_details` | ❌ |
| 5 | 0.316986 | `create_azure_key_vault_items` | ❌ |

---

## Test 184

**Expected Tool:** `create_azure_storage`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607036 | `create_azure_storage` | ✅ **EXPECTED** |
| 2 | 0.450592 | `upload_azure_storage_blobs` | ❌ |
| 3 | 0.427732 | `get_azure_storage_details` | ❌ |
| 4 | 0.325408 | `create_azure_key_vault_items` | ❌ |
| 5 | 0.313058 | `get_azure_container_details` | ❌ |

---

## Test 185

**Expected Tool:** `upload_azure_storage_blobs`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623181 | `upload_azure_storage_blobs` | ✅ **EXPECTED** |
| 2 | 0.528682 | `create_azure_storage` | ❌ |
| 3 | 0.419405 | `get_azure_storage_details` | ❌ |
| 4 | 0.292612 | `deploy_azure_ai_models` | ❌ |
| 5 | 0.268633 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 186

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** List all access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.598233 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.335003 | `get_azure_security_configurations` | ❌ |
| 3 | 0.295502 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.285710 | `get_azure_key_vault` | ❌ |
| 5 | 0.272415 | `get_azure_container_details` | ❌ |

---

## Test 187

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** List all databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.470782 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.387732 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.371289 | `get_azure_databases_details` | ❌ |
| 4 | 0.281471 | `get_azure_container_details` | ❌ |
| 5 | 0.254950 | `get_azure_security_configurations` | ❌ |

---

## Test 188

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** List all Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580586 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.364783 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.342661 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.325858 | `get_azure_databases_details` | ❌ |
| 5 | 0.312887 | `get_azure_container_details` | ❌ |

---

## Test 189

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** List all Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567767 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.435563 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.414456 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.406105 | `get_azure_container_details` | ❌ |
| 5 | 0.383811 | `get_azure_messaging_service_details` | ❌ |

---

## Test 190

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me my Redis Caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520329 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.307314 | `get_azure_databases_details` | ❌ |
| 3 | 0.267242 | `get_azure_container_details` | ❌ |
| 4 | 0.263546 | `get_azure_key_vault` | ❌ |
| 5 | 0.252356 | `get_azure_security_configurations` | ❌ |

---

## Test 191

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me my Redis Clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498407 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.354127 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.348501 | `get_azure_container_details` | ❌ |
| 4 | 0.325563 | `get_azure_databases_details` | ❌ |
| 5 | 0.287089 | `get_azure_sql_server_details` | ❌ |

---

## Test 192

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me the access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600085 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.316812 | `get_azure_security_configurations` | ❌ |
| 3 | 0.315264 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.313256 | `get_azure_key_vault` | ❌ |
| 5 | 0.305484 | `get_azure_app_config_settings` | ❌ |

---

## Test 193

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me the databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.456986 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.381255 | `get_azure_databases_details` | ❌ |
| 3 | 0.379480 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.283078 | `get_azure_container_details` | ❌ |
| 5 | 0.256540 | `get_azure_sql_server_details` | ❌ |

---

## Test 194

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me the Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553755 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.360815 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.347079 | `get_azure_databases_details` | ❌ |
| 4 | 0.335247 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.328705 | `get_azure_messaging_service_details` | ❌ |

---

## Test 195

**Expected Tool:** `get_azure_cache_for_redis_details`  
**Prompt:** Show me the Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.538790 | `get_azure_cache_for_redis_details` | ✅ **EXPECTED** |
| 2 | 0.424900 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.415817 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.408194 | `get_azure_container_details` | ❌ |
| 5 | 0.386110 | `get_azure_databases_details` | ❌ |

---

## Test 196

**Expected Tool:** `browse_azure_marketplace_products`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.424825 | `browse_azure_marketplace_products` | ✅ **EXPECTED** |
| 2 | 0.376519 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.359701 | `get_application_platform_details` | ❌ |
| 4 | 0.343877 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.300278 | `get_azure_storage_details` | ❌ |

---

## Test 197

**Expected Tool:** `browse_azure_marketplace_products`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712278 | `browse_azure_marketplace_products` | ✅ **EXPECTED** |
| 2 | 0.464133 | `search_microsoft_docs` | ❌ |
| 3 | 0.394415 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.350764 | `get_azure_databases_details` | ❌ |
| 5 | 0.338328 | `deploy_resources_and_applications_to_azure` | ❌ |

---

## Test 198

**Expected Tool:** `browse_azure_marketplace_products`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492651 | `browse_azure_marketplace_products` | ✅ **EXPECTED** |
| 2 | 0.227530 | `get_azure_messaging_service_details` | ❌ |
| 3 | 0.216860 | `get_azure_databases_details` | ❌ |
| 4 | 0.211973 | `get_azure_ai_resources_details` | ❌ |
| 5 | 0.210581 | `get_azure_workbooks_details` | ❌ |

---

## Test 199

**Expected Tool:** `get_azure_capacity`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.449864 | `get_azure_capacity` | ✅ **EXPECTED** |
| 2 | 0.397168 | `get_azure_storage_details` | ❌ |
| 3 | 0.353830 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.350026 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.321104 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 200

**Expected Tool:** `get_azure_capacity`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.332100 | `get_azure_load_testing_details` | ❌ |
| 2 | 0.328688 | `get_azure_databases_details` | ❌ |
| 3 | 0.320050 | `get_azure_capacity` | ✅ **EXPECTED** |
| 4 | 0.318017 | `get_azure_ai_resources_details` | ❌ |
| 5 | 0.313365 | `get_azure_subscriptions_and_resource_groups` | ❌ |

---

## Test 201

**Expected Tool:** `get_azure_capacity`  
**Prompt:** Tell me how many IP addresses I need for <filesystem_size> of <amlfs_sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.299130 | `get_azure_storage_details` | ❌ |
| 2 | 0.278726 | `get_azure_capacity` | ✅ **EXPECTED** |
| 3 | 0.225047 | `execute_azure_cli` | ❌ |
| 4 | 0.215121 | `edit_azure_databases` | ❌ |
| 5 | 0.206770 | `get_azure_container_details` | ❌ |

---

## Test 202

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510142 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.431684 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.350456 | `get_azure_security_configurations` | ❌ |
| 4 | 0.326854 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.306960 | `get_application_platform_details` | ❌ |

---

## Test 203

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.583567 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.433919 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.380839 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.375303 | `get_azure_security_configurations` | ❌ |
| 5 | 0.355380 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 204

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.517196 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.485648 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 3 | 0.337400 | `get_azure_load_testing_details` | ❌ |
| 4 | 0.332975 | `get_azure_security_configurations` | ❌ |
| 5 | 0.331312 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 205

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.540786 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.397469 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.329802 | `get_azure_security_configurations` | ❌ |
| 4 | 0.325418 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.309353 | `get_azure_load_testing_details` | ❌ |

---

## Test 206

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.508206 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.443596 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.351500 | `get_azure_security_configurations` | ❌ |
| 4 | 0.335511 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.332395 | `get_application_platform_details` | ❌ |

---

## Test 207

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.539053 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.499123 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.366263 | `get_azure_security_configurations` | ❌ |
| 4 | 0.341727 | `get_azure_load_testing_details` | ❌ |
| 5 | 0.331468 | `get_azure_capacity` | ❌ |

---

## Test 208

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551826 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.422468 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.341614 | `get_azure_security_configurations` | ❌ |
| 4 | 0.319377 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.318300 | `get_application_platform_details` | ❌ |

---

## Test 209

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553625 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.470487 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.429263 | `get_azure_security_configurations` | ❌ |
| 4 | 0.402453 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.364305 | `get_azure_databases_details` | ❌ |

---

## Test 210

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566739 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 2 | 0.506376 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 3 | 0.383949 | `get_azure_security_configurations` | ❌ |
| 4 | 0.357645 | `get_azure_databases_details` | ❌ |
| 5 | 0.356476 | `get_azure_capacity` | ❌ |

---

## Test 211

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552969 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.404707 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.344019 | `get_azure_security_configurations` | ❌ |
| 4 | 0.337543 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.328806 | `get_application_platform_details` | ❌ |

---

## Test 212

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602510 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.384758 | `get_application_platform_details` | ❌ |
| 3 | 0.374093 | `get_azure_container_details` | ❌ |
| 4 | 0.373005 | `get_azure_sql_server_details` | ❌ |
| 5 | 0.364952 | `get_azure_app_config_settings` | ❌ |

---

## Test 213

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622685 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.397741 | `get_application_platform_details` | ❌ |
| 3 | 0.380696 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.379021 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.365824 | `get_azure_container_details` | ❌ |

---

## Test 214

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615509 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.375764 | `get_application_platform_details` | ❌ |
| 3 | 0.363286 | `get_azure_app_config_settings` | ❌ |
| 4 | 0.361894 | `get_azure_container_details` | ❌ |
| 5 | 0.347380 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 215

**Expected Tool:** `get_azure_messaging_service_details`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587610 | `get_azure_messaging_service_details` | ✅ **EXPECTED** |
| 2 | 0.444005 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.417523 | `browse_azure_marketplace_products` | ❌ |
| 4 | 0.364752 | `get_azure_security_configurations` | ❌ |
| 5 | 0.349067 | `get_azure_databases_details` | ❌ |

---

## Test 216

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589678 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.426314 | `get_azure_databases_details` | ❌ |
| 3 | 0.398411 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.395808 | `get_azure_container_details` | ❌ |
| 5 | 0.385122 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 217

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546030 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.447128 | `get_azure_databases_details` | ❌ |
| 3 | 0.337758 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.305376 | `get_azure_container_details` | ❌ |
| 5 | 0.304244 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 218

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527040 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.396515 | `get_azure_databases_details` | ❌ |
| 3 | 0.305606 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.265201 | `get_azure_resource_and_app_health_status` | ❌ |
| 5 | 0.262194 | `get_azure_container_details` | ❌ |

---

## Test 219

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512442 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.353150 | `get_azure_databases_details` | ❌ |
| 3 | 0.247154 | `get_azure_container_details` | ❌ |
| 4 | 0.242332 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.234571 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 220

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.428871 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.346074 | `get_azure_databases_details` | ❌ |
| 3 | 0.293825 | `get_azure_ai_resources_details` | ❌ |
| 4 | 0.264339 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.249109 | `get_azure_container_details` | ❌ |

---

## Test 221

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533350 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.381865 | `get_azure_databases_details` | ❌ |
| 3 | 0.347867 | `get_azure_container_details` | ❌ |
| 4 | 0.315634 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.312009 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 222

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.584941 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.454271 | `get_azure_databases_details` | ❌ |
| 3 | 0.420000 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.415851 | `get_azure_container_details` | ❌ |
| 5 | 0.404683 | `browse_azure_marketplace_products` | ❌ |

---

## Test 223

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.535152 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.456484 | `get_azure_databases_details` | ❌ |
| 3 | 0.328037 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.314178 | `get_azure_container_details` | ❌ |
| 5 | 0.296908 | `get_azure_resource_and_app_health_status` | ❌ |

---

## Test 224

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603734 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.419498 | `get_azure_container_details` | ❌ |
| 3 | 0.382673 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.366595 | `get_azure_sql_server_details` | ❌ |
| 5 | 0.359077 | `get_azure_storage_details` | ❌ |

---

## Test 225

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.475232 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.366188 | `get_azure_databases_details` | ❌ |
| 3 | 0.251412 | `get_azure_best_practices` | ❌ |
| 4 | 0.241156 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.239322 | `get_azure_sql_server_details` | ❌ |

---

## Test 226

**Expected Tool:** `get_azure_data_explorer_kusto_details`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521279 | `get_azure_data_explorer_kusto_details` | ✅ **EXPECTED** |
| 2 | 0.422581 | `get_azure_databases_details` | ❌ |
| 3 | 0.301709 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.279126 | `get_azure_container_details` | ❌ |
| 5 | 0.274870 | `get_azure_sql_server_details` | ❌ |

---

## Test 227

**Expected Tool:** `create_azure_sql_firewall_rules`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619588 | `create_azure_sql_firewall_rules` | ✅ **EXPECTED** |
| 2 | 0.497433 | `delete_azure_sql_firewall_rules` | ❌ |
| 3 | 0.348092 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.339577 | `edit_azure_databases` | ❌ |
| 5 | 0.204700 | `import_azure_key_vault_certificates` | ❌ |

---

## Test 228

**Expected Tool:** `create_azure_sql_firewall_rules`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769917 | `create_azure_sql_firewall_rules` | ✅ **EXPECTED** |
| 2 | 0.659584 | `delete_azure_sql_firewall_rules` | ❌ |
| 3 | 0.476820 | `edit_azure_databases` | ❌ |
| 4 | 0.461699 | `get_azure_sql_server_details` | ❌ |
| 5 | 0.322692 | `execute_azure_cli` | ❌ |

---

## Test 229

**Expected Tool:** `create_azure_sql_firewall_rules`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670172 | `create_azure_sql_firewall_rules` | ✅ **EXPECTED** |
| 2 | 0.546667 | `delete_azure_sql_firewall_rules` | ❌ |
| 3 | 0.378044 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.333972 | `edit_azure_databases` | ❌ |
| 5 | 0.250880 | `create_azure_workbooks` | ❌ |

---

## Test 230

**Expected Tool:** `delete_azure_sql_firewall_rules`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.725947 | `delete_azure_sql_firewall_rules` | ✅ **EXPECTED** |
| 2 | 0.684182 | `create_azure_sql_firewall_rules` | ❌ |
| 3 | 0.449221 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.432983 | `edit_azure_databases` | ❌ |
| 5 | 0.365219 | `edit_azure_workbooks` | ❌ |

---

## Test 231

**Expected Tool:** `delete_azure_sql_firewall_rules`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691123 | `delete_azure_sql_firewall_rules` | ✅ **EXPECTED** |
| 2 | 0.657272 | `create_azure_sql_firewall_rules` | ❌ |
| 3 | 0.415990 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.364151 | `edit_azure_databases` | ❌ |
| 5 | 0.287812 | `get_azure_security_configurations` | ❌ |

---

## Test 232

**Expected Tool:** `delete_azure_sql_firewall_rules`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662278 | `delete_azure_sql_firewall_rules` | ✅ **EXPECTED** |
| 2 | 0.610044 | `create_azure_sql_firewall_rules` | ❌ |
| 3 | 0.373541 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.299807 | `edit_azure_databases` | ❌ |
| 5 | 0.250392 | `get_azure_security_configurations` | ❌ |

---

## Test 233

**Expected Tool:** `get_azure_sql_server_details`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580147 | `get_azure_sql_server_details` | ✅ **EXPECTED** |
| 2 | 0.408684 | `get_azure_databases_details` | ❌ |
| 3 | 0.370734 | `delete_azure_sql_firewall_rules` | ❌ |
| 4 | 0.369513 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.368293 | `edit_azure_databases` | ❌ |

---

## Test 234

**Expected Tool:** `get_azure_sql_server_details`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659544 | `create_azure_sql_firewall_rules` | ❌ |
| 2 | 0.635949 | `delete_azure_sql_firewall_rules` | ❌ |
| 3 | 0.523251 | `get_azure_sql_server_details` | ✅ **EXPECTED** |
| 4 | 0.344890 | `get_azure_security_configurations` | ❌ |
| 5 | 0.329505 | `edit_azure_databases` | ❌ |

---

## Test 235

**Expected Tool:** `get_azure_sql_server_details`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.490224 | `get_azure_sql_server_details` | ✅ **EXPECTED** |
| 2 | 0.362041 | `create_azure_sql_firewall_rules` | ❌ |
| 3 | 0.358939 | `get_azure_security_configurations` | ❌ |
| 4 | 0.334656 | `delete_azure_sql_firewall_rules` | ❌ |
| 5 | 0.293586 | `get_azure_databases_details` | ❌ |

---

## Test 236

**Expected Tool:** `get_azure_sql_server_details`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.631996 | `get_azure_sql_server_details` | ✅ **EXPECTED** |
| 2 | 0.414631 | `edit_azure_databases` | ❌ |
| 3 | 0.397080 | `get_azure_databases_details` | ❌ |
| 4 | 0.396196 | `get_azure_container_details` | ❌ |
| 5 | 0.372737 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 237

**Expected Tool:** `get_azure_sql_server_details`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500753 | `get_azure_sql_server_details` | ✅ **EXPECTED** |
| 2 | 0.325040 | `create_azure_sql_firewall_rules` | ❌ |
| 3 | 0.294052 | `get_azure_security_configurations` | ❌ |
| 4 | 0.287675 | `delete_azure_sql_firewall_rules` | ❌ |
| 5 | 0.271374 | `edit_azure_databases` | ❌ |

---

## Test 238

**Expected Tool:** `get_azure_sql_server_details`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659102 | `create_azure_sql_firewall_rules` | ❌ |
| 2 | 0.611917 | `delete_azure_sql_firewall_rules` | ❌ |
| 3 | 0.498815 | `get_azure_sql_server_details` | ✅ **EXPECTED** |
| 4 | 0.361115 | `edit_azure_databases` | ❌ |
| 5 | 0.322908 | `get_azure_security_configurations` | ❌ |

---

## Test 239

**Expected Tool:** `get_azure_sql_server_details`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.539130 | `get_azure_sql_server_details` | ✅ **EXPECTED** |
| 2 | 0.412902 | `edit_azure_databases` | ❌ |
| 3 | 0.388607 | `get_azure_databases_details` | ❌ |
| 4 | 0.376653 | `get_azure_capacity` | ❌ |
| 5 | 0.336862 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 240

**Expected Tool:** `get_azure_sql_server_details`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657075 | `create_azure_sql_firewall_rules` | ❌ |
| 2 | 0.595199 | `delete_azure_sql_firewall_rules` | ❌ |
| 3 | 0.501082 | `get_azure_sql_server_details` | ✅ **EXPECTED** |
| 4 | 0.358803 | `edit_azure_databases` | ❌ |
| 5 | 0.289735 | `get_azure_security_configurations` | ❌ |

---

## Test 241

**Expected Tool:** `get_azure_sql_server_details`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.451403 | `get_azure_sql_server_details` | ✅ **EXPECTED** |
| 2 | 0.332608 | `create_azure_sql_firewall_rules` | ❌ |
| 3 | 0.326446 | `edit_azure_databases` | ❌ |
| 4 | 0.281938 | `delete_azure_sql_firewall_rules` | ❌ |
| 5 | 0.274248 | `search_microsoft_docs` | ❌ |

---

## Test 242

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530281 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.515594 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.437388 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.385731 | `execute_azure_cli` | ❌ |
| 5 | 0.384930 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 243

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.544572 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.472911 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.459564 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.420414 | `get_azure_security_configurations` | ❌ |
| 5 | 0.417368 | `get_azure_messaging_service_details` | ❌ |

---

## Test 244

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.601907 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.460024 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.427150 | `get_azure_security_configurations` | ❌ |
| 4 | 0.420761 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.410049 | `browse_azure_marketplace_products` | ❌ |

---

## Test 245

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525816 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.393971 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.360943 | `get_azure_storage_details` | ❌ |
| 4 | 0.351352 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.349855 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 246

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.499101 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.382508 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.365125 | `get_azure_storage_details` | ❌ |
| 4 | 0.356818 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.349921 | `get_azure_load_testing_details` | ❌ |

---

## Test 247

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.513638 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.417443 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.385526 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.382904 | `get_azure_sql_server_details` | ❌ |
| 5 | 0.372789 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 248

**Expected Tool:** `get_azure_container_details`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.456406 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.285995 | `get_azure_storage_details` | ❌ |
| 3 | 0.285116 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.268189 | `get_azure_databases_details` | ❌ |
| 5 | 0.266007 | `get_azure_key_vault` | ❌ |

---

## Test 249

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611384 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.419226 | `browse_azure_marketplace_products` | ❌ |
| 3 | 0.404127 | `get_azure_databases_details` | ❌ |
| 4 | 0.400669 | `create_azure_storage` | ❌ |
| 5 | 0.400616 | `get_azure_key_vault` | ❌ |

---

## Test 250

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550382 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.472664 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.415764 | `get_azure_security_configurations` | ❌ |
| 4 | 0.405849 | `get_azure_databases_details` | ❌ |
| 5 | 0.403526 | `get_azure_messaging_service_details` | ❌ |

---

## Test 251

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.494408 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.336265 | `create_azure_storage` | ❌ |
| 3 | 0.320400 | `get_azure_key_vault` | ❌ |
| 4 | 0.313948 | `get_azure_storage_details` | ❌ |
| 5 | 0.310577 | `get_azure_security_configurations` | ❌ |

---

## Test 252

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.562127 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.433036 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.386547 | `get_azure_messaging_service_details` | ❌ |
| 4 | 0.385491 | `browse_azure_marketplace_products` | ❌ |
| 5 | 0.363088 | `get_azure_storage_details` | ❌ |

---

## Test 253

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521562 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.431510 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 3 | 0.369215 | `get_azure_databases_details` | ❌ |
| 4 | 0.362549 | `get_azure_storage_details` | ❌ |
| 5 | 0.355126 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 254

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572001 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.444584 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.424565 | `get_azure_cache_for_redis_details` | ❌ |
| 4 | 0.424320 | `get_azure_virtual_desktop_details` | ❌ |
| 5 | 0.415038 | `get_azure_app_config_settings` | ❌ |

---

## Test 255

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.448335 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.375014 | `get_azure_app_config_settings` | ❌ |
| 3 | 0.355399 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.344379 | `execute_azure_cli` | ❌ |
| 5 | 0.322237 | `get_azure_data_explorer_kusto_details` | ❌ |

---

## Test 256

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500237 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.412997 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.393722 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.387109 | `get_azure_data_explorer_kusto_details` | ❌ |
| 5 | 0.378801 | `get_azure_cache_for_redis_details` | ❌ |

---

## Test 257

**Expected Tool:** `get_azure_container_details`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.467436 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.296376 | `get_azure_key_vault` | ❌ |
| 3 | 0.296313 | `get_azure_databases_details` | ❌ |
| 4 | 0.287702 | `get_azure_cache_for_redis_details` | ❌ |
| 5 | 0.280113 | `get_azure_storage_details` | ❌ |

---

## Test 258

**Expected Tool:** `get_azure_container_details`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570830 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.423943 | `get_azure_data_explorer_kusto_details` | ❌ |
| 3 | 0.405032 | `execute_azure_cli` | ❌ |
| 4 | 0.355304 | `get_azure_resource_and_app_health_status` | ❌ |
| 5 | 0.351001 | `get_azure_virtual_desktop_details` | ❌ |

---

## Test 259

**Expected Tool:** `get_azure_container_details`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602681 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.458216 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.457991 | `get_azure_data_explorer_kusto_details` | ❌ |
| 4 | 0.448234 | `get_azure_app_config_settings` | ❌ |
| 5 | 0.443673 | `get_application_platform_details` | ❌ |

---

## Test 260

**Expected Tool:** `get_azure_container_details`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.501318 | `get_azure_container_details` | ✅ **EXPECTED** |
| 2 | 0.389162 | `get_azure_virtual_desktop_details` | ❌ |
| 3 | 0.375633 | `get_azure_sql_server_details` | ❌ |
| 4 | 0.362346 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 5 | 0.358984 | `get_azure_capacity` | ❌ |

---

## Test 261

**Expected Tool:** `get_azure_virtual_desktop_details`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550251 | `get_azure_virtual_desktop_details` | ✅ **EXPECTED** |
| 2 | 0.479082 | `get_azure_sql_server_details` | ❌ |
| 3 | 0.442910 | `get_azure_subscriptions_and_resource_groups` | ❌ |
| 4 | 0.408007 | `get_azure_container_details` | ❌ |
| 5 | 0.404019 | `get_azure_messaging_service_details` | ❌ |

---

## Test 262

**Expected Tool:** `get_azure_virtual_desktop_details`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607532 | `get_azure_virtual_desktop_details` | ✅ **EXPECTED** |
| 2 | 0.389302 | `get_azure_sql_server_details` | ❌ |
| 3 | 0.319120 | `get_azure_security_configurations` | ❌ |
| 4 | 0.305126 | `get_azure_capacity` | ❌ |
| 5 | 0.295866 | `get_azure_container_details` | ❌ |

---

## Test 263

**Expected Tool:** `get_azure_virtual_desktop_details`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611133 | `get_azure_virtual_desktop_details` | ✅ **EXPECTED** |
| 2 | 0.348565 | `get_azure_sql_server_details` | ❌ |
| 3 | 0.313084 | `get_azure_security_configurations` | ❌ |
| 4 | 0.262040 | `get_azure_messaging_service_details` | ❌ |
| 5 | 0.262035 | `get_azure_container_details` | ❌ |

---

## Summary

**Total Prompts Tested:** 263  
**Analysis Execution Time:** 63.2173824s  

### Success Rate Metrics

**Top Choice Success:** 84.0% (221/263 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 0.0% (0/263 tests)  
**🎯 High Confidence (≥0.7):** 3.0% (8/263 tests)  
**✅ Good Confidence (≥0.6):** 20.2% (53/263 tests)  
**👍 Fair Confidence (≥0.5):** 56.3% (148/263 tests)  
**👌 Acceptable Confidence (≥0.4):** 84.8% (223/263 tests)  
**❌ Low Confidence (<0.4):** 15.2% (40/263 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 0.0% (0/263 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 3.0% (8/263 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 20.2% (53/263 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 50.6% (133/263 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 74.9% (197/263 tests)  

### Success Rate Analysis

🟠 **Fair** - The tool selection system needs significant improvement.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

