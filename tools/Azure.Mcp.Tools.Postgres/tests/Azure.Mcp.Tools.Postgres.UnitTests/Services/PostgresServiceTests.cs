// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Data.Common;
using Azure.Mcp.Core.Exceptions;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Postgres.Auth;
using Azure.Mcp.Tools.Postgres.Providers;
using Azure.Mcp.Tools.Postgres.Services;
using Azure.Mcp.Tools.Postgres.UnitTests.Services.Support;
using Npgsql;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.UnitTests.Services
{
    public class PostgresServiceTests
    {
        private readonly IResourceGroupService _resourceGroupService;
        private readonly ITenantService _tenantService;
        private readonly IEntraTokenProvider _entraTokenAuth;
        private readonly IDbProvider _dbProvider;
        private readonly PostgresService _postgresService;

        private string subscriptionId;
        private string resourceGroup;
        private string user;
        private string server;
        private string database;
        private string query;
        private string authType;

        public PostgresServiceTests()
        {
            _resourceGroupService = Substitute.For<IResourceGroupService>();

            _tenantService = Substitute.For<ITenantService>();

            _entraTokenAuth = Substitute.For<IEntraTokenProvider>();
            _entraTokenAuth.GetEntraToken(Arg.Any<Azure.Core.TokenCredential>(), Arg.Any<CancellationToken>())
                .Returns(new Azure.Core.AccessToken("fake-token", DateTime.UtcNow.AddHours(1)));

            _dbProvider = Substitute.For<IDbProvider>();
            _dbProvider.GetPostgresResource(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Substitute.For<IPostgresResource>());
            _dbProvider.GetCommand(Arg.Any<string>(), Arg.Any<IPostgresResource>())
                .Returns(Substitute.For<NpgsqlCommand>());
            _dbProvider.ExecuteReaderAsync(Arg.Any<NpgsqlCommand>(), Arg.Any<CancellationToken>())
                .Returns(Substitute.For<DbDataReader>());

            _postgresService = new PostgresService(_resourceGroupService, _tenantService, _entraTokenAuth, _dbProvider);

            this.subscriptionId = "test-sub";
            this.resourceGroup = "test-rg";
            this.user = "test-user";
            this.server = "test-server";
            this.database = "test-db";
            this.query = "SELECT * FROM test-table;";
            this.authType = "MicrosoftEntra";
        }

        [Fact]
        public async Task ExecuteQueryAsync_InvalidCastException_Test()
        {
            // This test verifies that queries that returns unsupported data types return an exception
            // message that helps AI to understand the issue and fix the query.

            // Arrange
            this._dbProvider.ExecuteReaderAsync(Arg.Any<NpgsqlCommand>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<DbDataReader>(new FakeDbDataReader(
                    new object[][] {
                    new object[] { "row1", 1, new InvalidCastItem() },
                    new object[] { "row2", 2, new InvalidCastItem() },
                    new object[] { "row3", 3, new InvalidCastItem() }
                    },
                    new[] { "string", "integer", "unsupported" },
                    new[] { typeof(string), typeof(int), typeof(InvalidCastItem) })));

            // Act
            CommandValidationException exception = await Assert.ThrowsAsync<CommandValidationException>(async () =>
            {
                await _postgresService.ExecuteQueryAsync(subscriptionId, resourceGroup, authType, user, null, server, database, query, TestContext.Current.CancellationToken);
            });

            // Assert
            Assert.Contains("The PostgreSQL query failed because it returned one or more columns with non-standard data types (extension or user-defined) unsupported by the MCP agent", exception.Message);
        }

        [Fact]
        public async Task ExecuteQueryAsync_MixedDataTypes_Test()
        {
            // This test verifies that queries that return supported data types work as expected.

            // Arrange
            this._dbProvider.ExecuteReaderAsync(Arg.Any<NpgsqlCommand>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<DbDataReader>(new FakeDbDataReader(
                    new object[][] {
                        new object[] { "row1", 1, },
                        new object[] { "row2", 2, },
                        new object[] { "row3", 3, }
                    },
                    new[] { "string", "integer" },
                    new[] { typeof(string), typeof(int), typeof(InvalidCastItem) })));

            // Act
            List<string> rows = await _postgresService.ExecuteQueryAsync(subscriptionId, resourceGroup, authType, user, null, server, database, query, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(4, rows.Count);
            Assert.Contains("string, integer", rows.ElementAt(0));
            Assert.Contains("row1, 1", rows.ElementAt(1));
            Assert.Contains("row2, 2", rows.ElementAt(2));
            Assert.Contains("row3, 3", rows.ElementAt(3));
        }

        [Fact]
        public async Task ExecuteQueryAsync_NoRows_Test()
        {
            // This test verifies that if no elements are found, only the header row is returned.

            // Arrange
            this._dbProvider.ExecuteReaderAsync(Arg.Any<NpgsqlCommand>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<DbDataReader>(new FakeDbDataReader(
                    new object[][] { },
                    new[] { "string", "integer" },
                    new[] { typeof(string), typeof(int), typeof(InvalidCastItem) })));

            // Act
            List<string> rows = await _postgresService.ExecuteQueryAsync(subscriptionId, resourceGroup, authType, user, null, server, database, query, TestContext.Current.CancellationToken);

            // Assert
            Assert.Single(rows);
            Assert.Contains("string, integer", rows.ElementAt(0));
        }
    }
}
