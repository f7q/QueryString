namespace IdentityDirectory.Scim.Test
{
    using System;
    using IdentityDirectory.Scim.Query;
    using Xunit;

    public class SortExpressionParserTests
    {
        [Fact]
        public void CanParseSimpleFilter()
        {
            var rootNode = SortExpressionParser.ParseExpression("Item1 asc, Item2 desc, Item3 asc, Item4 desc");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("Delimiter(Delimiter(Delimiter(Ascending(Item1),Descending(Item2)),Ascending(Item3)),Descending(Item4))", rootNode.ToString());
        }
    }
}
