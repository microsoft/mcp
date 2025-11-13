namespace Azure.Mcp.Tools.Postgres.UnitTests.Services.Support
{
    internal class InvalidCastItem
    {
        public override string ToString()
        {
            throw new InvalidCastException("This is an invalid cast item.");
        }
    }
}
