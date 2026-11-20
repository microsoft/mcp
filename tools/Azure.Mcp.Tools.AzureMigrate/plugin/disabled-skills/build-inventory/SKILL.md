---
name: build-inventory
description: Builds an Azure Migrate server inventory from a file the user provides (for example a CMDB export, a server list CSV or Excel file, an RVTools file or a zip of such files). Use when the user attaches or shares an inventory file and asks to build, import, discover or upload an inventory to Azure Migrate.
---

# Build an Azure Migrate inventory

One call creates a new Azure Migrate session for the signed-in user, starts the inventory build and returns a short-lived, pre-authorized upload URL. Uploading the file to that URL hands it to the build. The file content never goes through the chat or the tool call.

## Rules
- Never show the upload URL (or any part of its query string) to the user, and never write it in a file, message or summary. It is a temporary credential.
- Never mention tool names, function names, argument names, connector names or JSON field names. Use plain language, for example "Starting the inventory build...".
- Upload the original file bytes unchanged. Do not convert, summarise or re-encode the file.
- One build per file. If the user gives several files, ask whether to zip them together into one file first (one build), or run one build per file.

## How
1. Make sure you have the file. If the user asked to build an inventory but did not attach or point to a file, ask for it.
2. Call the Azure Migrate plugin's build-inventory tool with the file's name (with extension) as `file-name`. If the user gave extra instructions (for example "only production servers"), pass them as `message`.
3. If `success` is `false`, tell the user the inventory build could not be started and quote `error`. If `failedStep` is `auth`, say they need to sign in again. Stop.
4. Upload the file with an HTTP `PUT` to `uploadUrl`:
   - Request body: the raw file bytes.
   - Header: `x-ms-blob-type: BlockBlob` (plus every header in `headers`).
   - Example with your code tools: `curl -sS -o /dev/null -w "%{http_code}" -X PUT -H "x-ms-blob-type: BlockBlob" --data-binary @"<local file path>" "<uploadUrl>"`
   - Or in Python: `requests.put(uploadUrl, data=open(path, "rb"), headers={"x-ms-blob-type": "BlockBlob"})`.
5. Check the result:
   - HTTP `201`: tell the user the file was uploaded and the inventory build has started and runs in the background. Give the session ID (`sessionId`) and operation ID (`operationId`) so they can reference it.
   - HTTP `403`: the upload URL has expired or is not valid. Call the tool again once (step 2, which starts a new session) and retry. If it fails again, tell the user.
   - Network error, or you cannot make outbound HTTP calls: tell the user the file could not be uploaded from here.
   - Anything else: tell the user the upload failed with that status code.
6. The build result is not returned to the chat yet. Do not claim the inventory is ready or invent its contents.
