$query = '{ repository(owner:"microsoft", name:"mcp") { pullRequest(number:2504) { reviewThreads(first:20) { nodes { id isResolved } } } } }'
$result = gh api graphql -f query=$query --jq '.data.repository.pullRequest.reviewThreads.nodes[] | select(.isResolved == false) | .id' 2>&1
$threadIds = $result -split "`n" | Where-Object { $_ -match '^PRRT_' }
Write-Host "Found $($threadIds.Count) unresolved threads"
foreach ($tid in $threadIds) {
    gh api graphql -f query="mutation { resolveReviewThread(input:{threadId:`"$tid`"}) { thread { isResolved } } }" --silent 2>&1 | Out-Null
    Write-Host "Resolved $tid"
}
Write-Host "Done"
