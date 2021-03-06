﻿namespace IdentityDirectory.Scim.Tests
{
    using System;
    using IdentityDirectory.Scim.Query;
    using Xunit;

    public class ScimFilterParserTests
    {
        [Fact]
        public void CanParseSimpleFilter()
        {
            var rootNode = ScimExpressionParser.ParseExpression("title pr and userType eq 'Employee'");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("And(Present(title),Equal(userType,Employee))", rootNode.ToString());
        }

        [Fact]
        public void CanParseFilterWithPrecedence()
        {
            var rootNode = ScimExpressionParser.ParseExpression("title pr and (userType eq'Employee' or userType eq 'ParttimeEmployee')");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("And(Present(title),Or(Equal(userType,Employee),Equal(userType,ParttimeEmployee)))", rootNode.ToString());
        }

        [Fact]
        public void CanParsePathPrecedence()
        {
            var rootNode = ScimExpressionParser.ParseExpression("name.familyName");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("name.familyName", rootNode.ToString());
        }

        [Fact]
        public void CanParseCollectionFilter()
        {
            var rootNode = ScimExpressionParser.ParseExpression("addresses[type eq 'work'].streetAddress");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("Where(addresses,Equal(type,work)).streetAddress", rootNode.ToString());
        }

        [Fact]
        public void CanParseCollectionSlashFilter()
        {
            var rootNode = ScimExpressionParser.ParseExpression("(User_Type Eq'労働者')OR(UserType LT'2017/12/31 23:59:59.999')");
            Assert.NotNull(rootNode);
            Console.WriteLine(rootNode);
            Assert.Equal("Or(Equal(User_Type,労働者),LessThan(UserType,2017/12/31 23:59:59.999))", rootNode.ToString());
        }
    }
}
