namespace IdentityDirectory.Scim.Test
{
    using System;
    using IdentityDirectory.Scim.Query;
    using Xunit;

    public class SelectExpressionParserTests
    {
        [Fact]
        public void CanParseSimpleFilter()
        {
            var rootNode = SelectExpressionParser.ParseExpression("Item1, Item2, Item3, Item4");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("Delimiter(Delimiter(Delimiter(Item1,Item2),Item3),Item4)", rootNode.ToString());
        }

        [Fact]
        public void CanParseSimpleFilter2()
        {
            var rootNode = SelectExpressionParser.ParseExpression("   Item1   ,  Item2   ,   Item3     ");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("Delimiter(Delimiter(Item1,Item2),Item3)", rootNode.ToString());
        }

        [Fact]
        public void CanParseSimpleFilter3()
        {
            var rootNode = SelectExpressionParser.ParseExpression("Item2,Item3");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("Delimiter(Item2,Item3)", rootNode.ToString());
        }

        [Fact]
        public void CanParseSimpleFilter4()
        {
            var rootNode = SelectExpressionParser.ParseExpression("Item1");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("Item1", rootNode.ToString());
        }
    }
}
