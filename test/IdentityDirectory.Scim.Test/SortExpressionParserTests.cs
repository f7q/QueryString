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

        [Fact]
        public void CanParseSimpleFilter2()
        {
            var rootNode = SortExpressionParser.ParseExpression("Item1 asc, Item2 desc, Item3 asc");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("Delimiter(Delimiter(Ascending(Item1),Descending(Item2)),Ascending(Item3))", rootNode.ToString());
        }

        [Fact]
        public void CanParseSimpleFilter3()
        {
            var rootNode = SortExpressionParser.ParseExpression("Item1 asc, Item2 desc");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("Delimiter(Ascending(Item1),Descending(Item2))", rootNode.ToString());
        }

        [Fact]
        public void CanParseSimpleFilter4()
        {
            var rootNode = SortExpressionParser.ParseExpression("Item1 asc");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("Ascending(Item1)", rootNode.ToString());
        }

        [Fact]
        public void CanParseSimpleFilter5()
        {
            var rootNode = SortExpressionParser.ParseExpression("Item1 desc");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("Descending(Item1)", rootNode.ToString());
        }
    }
}
