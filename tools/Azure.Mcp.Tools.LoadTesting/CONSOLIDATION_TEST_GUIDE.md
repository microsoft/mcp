# Load Testing Tools Consolidation Test Guide

## Overview
This guide helps verify the consolidated Load Testing tools work correctly after merging:
- `testrun get` + `testrun list` → single `testrun get` command
- `testrun create` + `testrun update` → single `testrun update` command

## Prerequisites
1. Azure Load Testing resource exists
2. At least one test exists in the resource
3. Azure CLI is authenticated (`az login`)

## Test Scenarios

### 1. Test `loadtesting_testrun_get` - Get Single Test Run

**Command (via azmcp CLI):**
```bash
azmcp.exe loadtesting testrun get \
  --load-test-resource <resource-name> \
  --resource-group <rg-name> \
  --testrun-id <existing-testrun-id>
```

**Expected Behavior:**
- Returns details of the specified test run
- Shows all test run properties (status, startDateTime, endDateTime, etc.)

**Validation:**
- ✅ Command executes without error
- ✅ Returns single test run object
- ✅ Test run ID matches the provided ID

### 2. Test `loadtesting_testrun_get` - List All Test Runs for a Test

**Command (via azmcp CLI):**
```bash
azmcp.exe loadtesting testrun get \
  --load-test-resource <resource-name> \
  --resource-group <rg-name> \
  --test-id <existing-test-id>
```

**Expected Behavior:**
- Returns list of all test runs for the specified test
- Shows multiple test runs if they exist

**Validation:**
- ✅ Command executes without error
- ✅ Returns array of test runs
- ✅ All returned test runs belong to the specified test ID

### 3. Test `loadtesting_testrun_update` - Create New Test Run

**Command (via azmcp CLI):**
```bash
azmcp.exe loadtesting testrun update \
  --load-test-resource <resource-name> \
  --resource-group <rg-name> \
  --test-id <existing-test-id> \
  --testrun-id <new-unique-id> \
  --display-name "New Test Run" \
  --description "Created via consolidated command"
```

**Expected Behavior:**
- Creates a new test run with the provided details
- Returns the newly created test run object

**Validation:**
- ✅ Command executes without error
- ✅ Test run is created successfully
- ✅ Returned object shows the correct display name and description

### 4. Test `loadtesting_testrun_update` - Create from Existing Run (Rerun)

**Command (via azmcp CLI):**
```bash
azmcp.exe loadtesting testrun update \
  --load-test-resource <resource-name> \
  --resource-group <rg-name> \
  --test-id <existing-test-id> \
  --testrun-id <new-unique-id> \
  --old-testrun-id <existing-testrun-id> \
  --display-name "Rerun Test"
```

**Expected Behavior:**
- Creates a new test run based on an existing test run
- Copies configuration from the old test run

**Validation:**
- ✅ Command executes without error
- ✅ New test run is created with settings from old test run
- ✅ Display name is updated to "Rerun Test"

### 5. Validate Error Handling

#### Test mutual exclusivity validator for `get` command
```bash
# Should fail - neither testrun-id nor test-id provided
azmcp.exe loadtesting testrun get \
  --load-test-resource <resource-name> \
  --resource-group <rg-name>
```

**Expected:** Error message indicating at least one parameter is required

```bash
# Should fail - both parameters provided
azmcp.exe loadtesting testrun get \
  --load-test-resource <resource-name> \
  --resource-group <rg-name> \
  --testrun-id <id1> \
  --test-id <id2>
```

**Expected:** Error message indicating parameters are mutually exclusive

## Manual Testing via MCP Protocol

If testing via direct tool invocation through MCP client (Claude Desktop, etc.):

### Get Single Test Run
```json
{ 
  "name": "loadtesting_testrun_get",
  "arguments": {
    "loadTestResource": "<resource-name>",
    "resourceGroup": "<rg-name>",
    "testRunId": "<existing-testrun-id>"
  }
}
```

### List Test Runs for Test
```json
{
  "name": "loadtesting_testrun_get",
  "arguments": {
    "loadTestResource": "<resource-name>",
    "resourceGroup": "<rg-name>",
    "testId": "<existing-test-id>"
  }
}
```

### Create New Test Run
```json
{
  "name": "loadtesting_testrun_update",
  "arguments": {
    "loadTestResource": "<resource-name>",
    "resourceGroup": "<rg-name>",
    "testId": "<existing-test-id>",
    "testRunId": "<new-unique-id>",
    "displayName": "New Test Run",
    "description": "Created via consolidated command"
  }
}
```

### Create from Existing (Rerun)
```json
{
  "name": "loadtesting_testrun_update",
  "arguments": {
    "loadTestResource": "<resource-name>",
    "resourceGroup": "<rg-name>",
    "testId": "<existing-test-id>",
    "testRunId": "<new-unique-id>",
    "oldTestRunId": "<existing-testrun-id>",
    "displayName": "Rerun Test"
  }
}
```

## Success Criteria

All tests should pass with:
- ✅ No compilation errors
- ✅ Commands execute successfully
- ✅ Correct data returned for each scenario
- ✅ Validators work as expected (mutual exclusivity)
- ✅ Service integration works correctly
- ✅ Error messages are clear and helpful

## Notes

1. The consolidated commands are backward compatible - they support all the same functionality as the original separate commands
2. The `--test-id` parameter in `get` command enables listing all test runs for a test (previously `testrun list`)
3. The `--old-testrun-id` parameter in `update` command enables creating a new run from an existing one (rerun scenario)
4. All parameter names follow the existing naming conventions

## Rollback Plan

If issues are found:
1. Restore the deleted commands from git history
2. Re-add them to LoadTestingSetup.cs
3. Update LoadTestingJsonContext.cs to include old serialization types
4. Revert e2eTestPrompts.md changes
