---
name: upload-file
description: Uploads a file the user provides (for example a server inventory CSV or Excel export, an RVTools file or any other import file) to Azure Migrate. Use when the user attaches or shares a file and asks to upload, import or send it to Azure Migrate.
---

# Upload a file to Azure Migrate

Azure Migrate gives a short-lived, pre-authorized upload URL. Upload the user's file to it directly. The file content never goes through the chat or the tool call.

## Rules
- Never show the upload URL (or any part of its query string) to the user, and never write it in a file, message or summary. It is a temporary credential.
- Never mention tool names, function names, argument names, connector names or JSON field names. Use plain language, for example "Getting an upload location...".
- Upload the original file bytes unchanged. Do not convert, summarise or re-encode the file.
- One upload URL per file. If the user gives several files, repeat the steps for each file.

## How
1. Make sure you have the file. If the user asked to upload but did not attach or point to a file, ask for it.
2. Call the Azure Migrate plugin's get-upload-URL tool with the file's name (with extension) as `file-name`.
3. If `success` is `false`, tell the user uploads are not available right now and quote `error`. Stop.
4. Upload the file with an HTTP `PUT` to `uploadUrl`:
   - Request body: the raw file bytes.
   - Header: `x-ms-blob-type: BlockBlob` (plus every header in `headers`).
   - Example with your code tools: `curl -sS -o /dev/null -w "%{http_code}" -X PUT -H "x-ms-blob-type: BlockBlob" --data-binary @"<local file path>" "<uploadUrl>"`
   - Or in Python: `requests.put(uploadUrl, data=open(path, "rb"), headers={"x-ms-blob-type": "BlockBlob"})`.
5. Check the result:
   - HTTP `201`: tell the user the file was uploaded, with its name and size, and the stored name `blobName`.
   - HTTP `403`: the upload URL has expired or is not valid. Get a new one once (step 2) and retry. If it fails again, tell the user.
   - Network error, or you cannot make outbound HTTP calls: tell the user the file could not be uploaded from here.
   - Anything else: tell the user the upload failed with that status code.
