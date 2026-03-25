// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Azure;

public class BaseAzureResourceServiceTests
{
    private readonly TestAzureResourceService _service;

    public BaseAzureResourceServiceTests()
    {
        var subscriptionService = Substitute.For<ISubscriptionService>();
        var tenantService = Substitute.For<ITenantService>();
        _service = new TestAzureResourceService(subscriptionService, tenantService);
    }

    // ---- EscapeKqlIdentifier via KqlFilter assembly ----

    [Theory]
    [InlineData("name", "`name`")]
    [InlineData("id", "`id`")]
    [InlineData("resource`name", "`resource``name`")] // embedded backtick is doubled
    public void BuildKqlFilterClause_EscapesIdentifierCorrectly(string field, string expectedIdentifierPart)
    {
        var clause = _service.BuildKqlFilterClausePublic(new KqlFilter(field, "=~", "value"));
        Assert.StartsWith(expectedIdentifierPart, clause);
    }

    [Fact]
    public void BuildKqlFilterClause_EscapesValueCorrectly()
    {
        // Value contains single quotes — must be doubled
        var clause = _service.BuildKqlFilterClausePublic(new KqlFilter("name", "=~", "it's"));
        Assert.Contains("'it''s'", clause);
    }

    [Fact]
    public void BuildKqlFilterClause_EmitsOperatorBetweenIdentifierAndValue()
    {
        var clause = _service.BuildKqlFilterClausePublic(new KqlFilter("name", "=~", "myresource"));
        Assert.Equal("`name` =~ 'myresource'", clause);
    }

    [Fact]
    public void BuildKqlFilterClause_ContainsOperator()
    {
        var clause = _service.BuildKqlFilterClausePublic(new KqlFilter("id", "contains", "/subscriptions/abc"));
        Assert.Equal("`id` contains '/subscriptions/abc'", clause);
    }

    // ---- ValidateKqlOperator ----

    [Theory]
    [InlineData("=~")]
    [InlineData("contains")]
    [InlineData("==")]
    [InlineData("!=")]
    [InlineData("startswith")]
    [InlineData("endswith")]
    [InlineData("contains_cs")]
    public void BuildKqlFilterClause_AcceptsAllowlistedOperators(string op)
    {
        // Should not throw
        _service.BuildKqlFilterClausePublic(new KqlFilter("name", op, "value"));
    }

    [Theory]
    [InlineData("|")]
    [InlineData("union")]
    [InlineData("project")]
    [InlineData("; drop")]
    public void BuildKqlFilterClause_RejectsDisallowedOperators(string op)
    {
        Assert.Throws<ArgumentException>(() =>
            _service.BuildKqlFilterClausePublic(new KqlFilter("name", op, "value")));
    }

    // ---- Caller patterns (mirrors real services) ----

    [Theory]
    [InlineData("name", "=~", "my-registry")]          // AcrService
    [InlineData("name", "=~", "my-appconfig")]         // AppConfigService
    [InlineData("id", "contains", "/subscriptions/abc/resourceGroups/rg")]  // AuthorizationService
    [InlineData("name", "=~", "my-cluster")]           // KustoService
    [InlineData("name", "=~", "mystorageaccount")]     // StorageService
    [InlineData("name", "=~", "mydatabase")]           // SqlService
    public void BuildKqlFilterClause_AcceptsAllExistingCallerPatterns(string field, string op, string value)
    {
        var clause = _service.BuildKqlFilterClausePublic(new KqlFilter(field, op, value));
        Assert.NotEmpty(clause);
    }

    private sealed class TestAzureResourceService(
        ISubscriptionService subscriptionService,
        ITenantService tenantService)
        : BaseAzureResourceService(subscriptionService, tenantService)
    {
        public string BuildKqlFilterClausePublic(KqlFilter filter) =>
            $"{EscapeKqlIdentifier(filter.Field)} {ValidateKqlOperator(filter.Operator)} '{EscapeKqlString(filter.Value)}'";
    }
}
