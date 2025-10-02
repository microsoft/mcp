using Azure.Mcp.Core.Exceptions;
using Azure.Mcp.Tools.Cosmos.Validation;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.UnitTests;

public class CosmosQueryValidatorTests
{
    [Theory]
    [InlineData("SELECT * FROM c")]
    [InlineData("SELECT c.id, c.name FROM c WHERE c.type = 'test'")]
    [InlineData("SELECT VALUE c.id FROM c ORDER BY c.id DESC")]
    [InlineData("SELECT TOP 10 * FROM c ORDER BY c._ts DESC")]
    [InlineData("SELECT COUNT(1) FROM c WHERE c.status = 'active'")]
    [InlineData("SELECT * FROM c WHERE c.age BETWEEN 18 AND 65")]
    [InlineData("SELECT c.name FROM c WHERE c.id IN ('1', '2', '3')")]
    [InlineData("SELECT * FROM c WHERE c.description LIKE '%test%'")]
    [InlineData("SELECT DISTINCT c.category FROM c")]
    [InlineData("SELECT c.id AS identifier, c.name AS fullName FROM c")]
    [InlineData("SELECT CASE WHEN c.age > 18 THEN 'adult' ELSE 'minor' END FROM c")]
    [InlineData("SELECT * FROM c WHERE c.flag IS NULL")]
    [InlineData("SELECT * FROM c WHERE c.active IS NOT NULL")]
    [InlineData("SELECT SUM(c.amount), AVG(c.score), MIN(c.date), MAX(c.value) FROM c")]
    [InlineData("SELECT * FROM c JOIN d ON c.id = d.parentId")]
    [InlineData("SELECT * FROM c ORDER BY c.name ASC OFFSET 10 LIMIT 20")]
    [InlineData("select * from c where c.name = 'test'")] // lowercase
    public void EnsureReadOnlySelect_ValidQueries_ShouldPass(string query)
    {
        CosmosQueryValidator.EnsureReadOnlySelect(query);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureReadOnlySelect_EmptyQuery_ShouldThrow(string query)
    {
        var ex = Assert.Throws<CommandValidationException>(() => CosmosQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("empty", ex.Message.ToLowerInvariant());
    }

    [Theory]
    [InlineData("INSERT INTO c VALUES(1)")]
    [InlineData("UPDATE c SET x = 1")]
    [InlineData("DELETE FROM c WHERE 1=1")]
    [InlineData("DROP TABLE c")]
    public void EnsureReadOnlySelect_NonSelectQuery_ShouldThrow(string query)
    {
        // These don't start with SELECT, so they're rejected
        var ex = Assert.Throws<CommandValidationException>(() => CosmosQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("select", ex.Message.ToLowerInvariant());
    }

    [Theory]
    [InlineData("SELECT * FROM c UNION SELECT * FROM d")]  // UNION is now allowed (Cosmos will reject at execution)
    public void EnsureReadOnlySelect_UnionQueries_ShouldPass(string query)
    {
        // UNION is not blocked since Cosmos SQL doesn't support it - Cosmos DB will reject at execution
        CosmosQueryValidator.EnsureReadOnlySelect(query);
    }

    [Theory]
    [InlineData("SELECT id FROM c; INSERT INTO c VALUES(1)")]
    public void EnsureReadOnlySelect_StackedStatementsWithDML_ShouldThrow(string query)
    {
        // This fails because of multiple statements (semicolon)
        var ex = Assert.Throws<CommandValidationException>(() => CosmosQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("multiple", ex.Message.ToLowerInvariant());
    }

    [Theory]
    [InlineData("SELECT * FROM c; SELECT * FROM c")]
    [InlineData("SELECT * FROM c;DROP TABLE x")]
    public void EnsureReadOnlySelect_StackedStatements_ShouldThrow(string query)
    {
        var ex = Assert.Throws<CommandValidationException>(() => CosmosQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("multiple", ex.Message.ToLowerInvariant());
    }

    [Theory]
    [InlineData("-- comment only")] // doesn't start with SELECT
    [InlineData("/* block comment */ SELECT * FROM c")] // has comments which are blocked
    public void EnsureReadOnlySelect_CommentsPresent_ShouldThrow(string query)
    {
        var ex = Assert.Throws<CommandValidationException>(() => CosmosQueryValidator.EnsureReadOnlySelect(query));
        Assert.True(ex.Message.Contains("Comments", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("select", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void EnsureReadOnlySelect_LongQuery_ShouldThrow()
    {
        var longQuery = "SELECT * FROM c WHERE c.x = '" + new string('x', 6000) + "'";
        var ex = Assert.Throws<CommandValidationException>(() => CosmosQueryValidator.EnsureReadOnlySelect(longQuery));
        Assert.Contains("exceeds", ex.Message.ToLowerInvariant());
    }

    [Theory]
    [InlineData(null)]
    public void EnsureReadOnlySelect_NullQuery_ShouldThrow(string? query)
    {
        var ex = Assert.Throws<CommandValidationException>(() => CosmosQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("empty", ex.Message.ToLowerInvariant());
    }

    [Theory]
    [InlineData("SELECT * FROM c WHERE c.name = 'test' OR 1=1")]
    [InlineData("SELECT * FROM c WHERE c.name = 'x' OR '1'='1'")]
    public void EnsureReadOnlySelect_SqlInjectionTautologies_ShouldThrow(string query)
    {
        var ex = Assert.Throws<CommandValidationException>(() => CosmosQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("tautology", ex.Message.ToLowerInvariant());
    }

    [Theory]
    [InlineData("SELECT * FROM c; SELECT * FROM d")]
    [InlineData("SELECT * FROM c;DROP TABLE users")]
    [InlineData("SELECT * FROM c; INSERT INTO d VALUES(1)")]
    public void EnsureReadOnlySelect_MultipleStatements_ShouldThrow(string query)
    {
        var ex = Assert.Throws<CommandValidationException>(() => CosmosQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("multiple", ex.Message.ToLowerInvariant());
    }

    [Theory]
    [InlineData("SELECT * FROM c;")]  // trailing semicolon should be allowed
    public void EnsureReadOnlySelect_TrailingSemicolon_ShouldPass(string query)
    {
        CosmosQueryValidator.EnsureReadOnlySelect(query);
    }

    [Theory]
    [InlineData("SELECT * FROM c WHERE c.description = 'This contains INSERT keyword'")]
    [InlineData("SELECT * FROM c WHERE c.note = 'DELETE this later'")]
    [InlineData("SELECT * FROM c WHERE c.title = 'How to UPDATE records'")]
    public void EnsureReadOnlySelect_KeywordsInStrings_ShouldPass(string query)
    {
        // Keywords inside quoted strings should not trigger validation errors
        CosmosQueryValidator.EnsureReadOnlySelect(query);
    }

    [Theory]
    [InlineData("SELECT my_table.my_column FROM my_table WHERE custom_field = 'value'")]
    [InlineData("SELECT user_data.firstName FROM user_data")]
    [InlineData("SELECT ProductCatalog.ItemId FROM ProductCatalog WHERE Category123 = 'electronics'")]
    public void EnsureReadOnlySelect_CustomIdentifiers_ShouldPass(string query)
    {
        // Custom table names, column names, and identifiers should be allowed
        CosmosQueryValidator.EnsureReadOnlySelect(query);
    }

    [Theory]
    [InlineData("SELECT * FROM c WHERE c.price > 100 AND c.inStock = true")]
    [InlineData("SELECT * FROM c WHERE (c.category = 'A' OR c.category = 'B') AND c.active = true")]
    [InlineData("SELECT * FROM c WHERE c.age NOT BETWEEN 25 AND 35")]
    public void EnsureReadOnlySelect_ComplexWhereConditions_ShouldPass(string query)
    {
        CosmosQueryValidator.EnsureReadOnlySelect(query);
    }

    [Theory]
    [InlineData("EXEC sp_procedure")]
    [InlineData("EXECUTE dynamic_query")]
    public void EnsureReadOnlySelect_StoredProcedureExecution_ShouldThrow(string query)
    {
        // These don't start with SELECT or attempt to execute stored procedures
        var ex = Assert.Throws<CommandValidationException>(() => CosmosQueryValidator.EnsureReadOnlySelect(query));
        Assert.True(ex.Message.Contains("select", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("SELECT * FROM c EXCEPT SELECT * FROM d")]
    [InlineData("SELECT * FROM c INTERSECT SELECT * FROM d")]
    public void EnsureReadOnlySelect_SetOperations_ShouldPass(string query)
    {
        // EXCEPT and INTERSECT are not blocked - Cosmos DB will reject them at execution if unsupported
        CosmosQueryValidator.EnsureReadOnlySelect(query);
    }

    [Theory]
    [InlineData("   SELECT * FROM c   ")]  // leading/trailing whitespace
    [InlineData("\nSELECT * FROM c\n")]    // newlines
    [InlineData("\tSELECT * FROM c\t")]    // tabs
    public void EnsureReadOnlySelect_WhitespaceVariations_ShouldPass(string query)
    {
        CosmosQueryValidator.EnsureReadOnlySelect(query);
    }

    [Theory]
    [InlineData("SELECT c['nested.property'] FROM c")]
    [InlineData("SELECT c.properties[0].value FROM c")]
    [InlineData("SELECT * FROM c WHERE ARRAY_CONTAINS(c.tags, 'important')")]
    [InlineData("SELECT * FROM c WHERE IS_DEFINED(c.optionalField)")]
    public void EnsureReadOnlySelect_CosmosSpecificSyntax_ShouldPassWithCustomIdentifiers(string query)
    {
        // Cosmos-specific functions and syntax should work when they're treated as identifiers
        CosmosQueryValidator.EnsureReadOnlySelect(query);
    }

    [Theory]
    [InlineData("SELECT * FROM c WHERE c.name = 'O''Reilly'")] // escaped single quote
    [InlineData("SELECT * FROM c WHERE c.path = 'C:\\Program Files\\App'")] // backslashes in string
    [InlineData("SELECT * FROM c WHERE c.json = '{\"key\": \"DROP TABLE\"}'")] // JSON with keywords
    public void EnsureReadOnlySelect_StringLiteralsWithSpecialContent_ShouldPass(string query)
    {
        CosmosQueryValidator.EnsureReadOnlySelect(query);
    }

    [Theory]
    [InlineData("CREATE TABLE test (id int)")]
    [InlineData("ALTER TABLE test ADD COLUMN name varchar(50)")]
    [InlineData("GRANT SELECT ON test TO user")]
    [InlineData("REVOKE ALL ON test FROM user")]
    [InlineData("TRUNCATE TABLE test")]
    [InlineData("REPLACE INTO test VALUES(1)")]
    [InlineData("UPSERT INTO test VALUES(1)")]
    public void EnsureReadOnlySelect_DDLStatements_ShouldThrow(string query)
    {
        // These don't start with SELECT
        var ex = Assert.Throws<CommandValidationException>(() => CosmosQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("select", ex.Message.ToLowerInvariant());
    }

    [Theory]
    [InlineData("SELECT COUNT(*) FROM c GROUP BY c.category HAVING COUNT(*) > 5")]  // GROUP BY with HAVING - these are treated as identifiers since they're not in KnownCosmosKeywords
    [InlineData("SELECT * FROM c WHERE c.name REGEXP '^[A-Z]'")] // REGEXP - treated as identifier
    public void EnsureReadOnlySelect_UnrecognizedKeywords_ShouldPassAsIdentifiers(string query)
    {
        // Keywords not in KnownCosmosKeywords are treated as identifiers and allowed
        CosmosQueryValidator.EnsureReadOnlySelect(query);
    }

    [Theory]
    [InlineData("SELECT * FROM c WHERE c.amount = 123.45")]
    [InlineData("SELECT * FROM c WHERE c.id = 0x1A2B")]  // hex number
    [InlineData("SELECT * FROM c WHERE c.scientific = 1.23E-4")] // scientific notation
    public void EnsureReadOnlySelect_NumericLiterals_ShouldPass(string query)
    {
        CosmosQueryValidator.EnsureReadOnlySelect(query);
    }

    [Fact]
    public void EnsureReadOnlySelect_EdgeCaseLengthQuery_ShouldPass()
    {
        // Query that's just under the length limit
        var nearLimitQuery = "SELECT * FROM c WHERE c.description = '" + new string('x', 4900) + "'";
        CosmosQueryValidator.EnsureReadOnlySelect(nearLimitQuery);
    }

    [Theory]
    [InlineData("SELECT")]  // incomplete query
    [InlineData("SELECT *")] // missing FROM
    public void EnsureReadOnlySelect_IncompleteButValidSelectQueries_ShouldPass(string query)
    {
        // These start with SELECT so they pass basic validation (Cosmos DB will reject them at execution)
        CosmosQueryValidator.EnsureReadOnlySelect(query);
    }

    [Theory]
    [InlineData("FROM c")]   // missing SELECT
    public void EnsureReadOnlySelect_NonSelectQueries_ShouldThrow(string query)
    {
        var ex = Assert.Throws<CommandValidationException>(() => CosmosQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("select", ex.Message.ToLowerInvariant());
    }

    [Fact]
    public void EnsureReadOnlySelect_RegexTimeoutProtection_ShouldNotHang()
    {
        // Test with a potentially problematic pattern that could cause catastrophic backtracking
        // This shouldn't take more than a few milliseconds due to the timeout
        var potentialReDoSPattern = "SELECT * FROM c WHERE c.field = '" + new string('a', 1000) + new string('b', 1000) + "'";

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        CosmosQueryValidator.EnsureReadOnlySelect(potentialReDoSPattern);
        stopwatch.Stop();

        // Should complete quickly (well under 5 seconds) due to regex timeout protection
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Validation took too long: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Theory]
    [InlineData("SELECT * FROM c WHERE EXEC('malicious')")]
    [InlineData("SELECT * FROM c WHERE EXECUTE sp_adduser")]
    [InlineData("SELECT sp_executesql(@sql) FROM c")]
    [InlineData("SELECT * FROM c WHERE trigger = 1")]
    public void EnsureReadOnlySelect_StoredProcedureKeywords_ShouldThrow(string query)
    {
        // Attempts to use stored procedure execution keywords should be blocked
        var ex = Assert.Throws<CommandValidationException>(() => CosmosQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("stored procedure", ex.Message.ToLowerInvariant());
    }

    [Theory]
    [InlineData("SELECT * FROM c WHERE c.description = 'EXEC command here'")]  // exec in string literal
    [InlineData("SELECT * FROM c WHERE c.note = 'Call trigger later'")]  // trigger in string literal
    [InlineData("SELECT call_sproc() FROM c")]  // UDF name that happens to contain 'call'
    [InlineData("SELECT my_sproc_result FROM c")]  // Column name that happens to contain 'sproc'
    [InlineData("SELECT * FROM c WHERE c.comment = 'Run EXECUTE on server'")]  // execute in string
    [InlineData("SELECT * FROM c WHERE c.code = 'sp_adduser example'")]  // sp_ in string
    [InlineData("SELECT * FROM c WHERE c.note = '''EXEC'' escaped quotes'")]  // escaped quotes with EXEC
    public void EnsureReadOnlySelect_StoredProcedureKeywordsInStringsOrIdentifiers_ShouldPass(string query)
    {
        // Keywords inside quoted strings or as part of legitimate identifiers should not trigger validation errors
        CosmosQueryValidator.EnsureReadOnlySelect(query);
    }

    [Theory]
    [InlineData("SELECT * FROM c WHERE EXEC = 1")]  // EXEC as identifier (not in string)
    [InlineData("SELECT * FROM c, EXECUTE AS e")]  // EXECUTE as alias
    [InlineData("SELECT trigger.field FROM c JOIN trigger ON c.id = trigger.id")]  // trigger as table name
    public void EnsureReadOnlySelect_StoredProcedureKeywordsAsIdentifiers_ShouldThrow(string query)
    {
        // Keywords used as identifiers (outside of strings) should be blocked
        var ex = Assert.Throws<CommandValidationException>(() => CosmosQueryValidator.EnsureReadOnlySelect(query));
        Assert.Contains("stored procedure", ex.Message.ToLowerInvariant());
    }
}
