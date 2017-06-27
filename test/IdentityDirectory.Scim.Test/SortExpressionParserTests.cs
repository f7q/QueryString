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
            Assert.Equal("Delimiter(Delimiter(Delimiter(OrderBy(Item1),ThenByDescending(Item2)),ThenBy(Item3)),ThenByDescending(Item4))", rootNode.ToString());
        }

        [Fact]
        public void CanParseSimpleFilter2()
        {
            var rootNode = SortExpressionParser.ParseExpression("Item1 asc, Item2 desc, Item3 asc");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("Delimiter(Delimiter(OrderBy(Item1),ThenByDescending(Item2)),ThenBy(Item3))", rootNode.ToString());
        }

        [Fact]
        public void CanParseSimpleFilter3()
        {
            var rootNode = SortExpressionParser.ParseExpression("Item1 asc, Item2 desc");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("Delimiter(OrderBy(Item1),ThenByDescending(Item2))", rootNode.ToString());
        }

        [Fact]
        public void CanParseSimpleFilter4()
        {
            var rootNode = SortExpressionParser.ParseExpression("Item1 asc");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("OrderBy(Item1)", rootNode.ToString());
        }

        [Fact]
        public void CanParseSimpleFilter5()
        {
            var rootNode = SortExpressionParser.ParseExpression("Item1 desc");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("OrderByDescending(Item1)", rootNode.ToString());
        }
        
        /*
        /// <summary>
        /// https://gist.github.com/carlhoerberg/549690
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
        {
            string[] props = property.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (string prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            object result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), type)
                    .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }*/
    }
}
