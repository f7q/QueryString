namespace IdentityDirectory.Scim.Query
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Expressions;
    using Klaims.Framework.Utility;
    using Services;

    public class DefaultFilterBinder : IFilterBinder
    {
        public Expression<Func<TResource, bool>> Bind<TResource>(ScimExpression filter, string sortBy, bool ascending, IAttributeNameMapper mapper = null)
        {
            return this.BuildExpression<TResource>(filter, mapper, null);
        }

        private Expression<Func<TResource, bool>> BuildExpression<TResource>(ScimExpression filter, IAttributeNameMapper mapper, string prefix)
        {
            var callExpression = filter as ScimCallExpression;
            if (callExpression != null && (callExpression.OperatorName == "Delimiter"))
            {
                return this.BindDelimiterExpression<TResource>(callExpression, mapper, prefix);
            }

            if (callExpression!=null && (callExpression.OperatorName == "And" || callExpression.OperatorName == "Or"))
            {
                return this.BindLogicalExpression<TResource>(callExpression, mapper, prefix);
            }
          
            if (callExpression != null && (callExpression.OperatorName != "And" || callExpression.OperatorName != "Or"))
            {
                return this.BindAttributeExpression<TResource>(callExpression, mapper);
            }

            throw new InvalidOperationException("Unknown node type");
        }

        protected virtual Expression<Func<TResource, bool>> BindAttributeExpression<TResource>(ScimCallExpression expression, IAttributeNameMapper mapper)
        {
            //var attributePathExpression = expression.Operands[0];
            //return null;
            var parameter = Expression.Parameter(typeof(TResource));
            var property = mapper.MapToInternal(expression.Operands[0].ToString())
                .Split('.')
                .Aggregate<string, Expression>(parameter,Expression.PropertyOrField);

            Expression binaryExpression = null;
            if (expression.OperatorName.Equals("Equal"))
            {
                if (property.Type == typeof(System.String))
                {
                    // Workaround for missing coersion between String and Guid types.
                    var propertyValue = property.Type == typeof(Guid) ? (object)Guid.Parse(expression.Operands[1].ToString()) : expression.Operands[1].ToString();
                    binaryExpression = Expression.Equal(property, Expression.Convert(Expression.Constant(propertyValue), property.Type));
                }
                else if (IsNullable(property.Type))
                {
                    binaryExpression = Expression.Equal(property, Expression.Convert(Expression.Constant(null), property.Type));
                }
                else if (property.Type == typeof(bool))
                {
                    binaryExpression = Expression.Equal(property, Expression.Convert(Expression.Constant(true), property.Type));
                }
                else if (property.Type == typeof(System.DateTime))
                {
                    binaryExpression = Expression.Equal(property, Expression.Convert(Expression.Constant(DateTime.Parse(expression.Operands[1].ToString())), property.Type));
                }
            }
            else if (expression.OperatorName.Equals("StartsWith"))
            {
                var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                binaryExpression = Expression.Call(property, method, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
            }
            else if (expression.OperatorName.Equals("EndsWith"))
            {
                var method = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
                binaryExpression = Expression.Call(property, method, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
            }
            else if (expression.OperatorName.Equals("Contains"))
            {
                var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                binaryExpression = Expression.Call(property, method, Expression.Constant(expression.Operands[1].ToString(), typeof(string)));
            }
            else if (expression.OperatorName.Equals("GreaterThan"))
            {
                if (property.Type == typeof(System.String))
                {
                    binaryExpression = Expression.GreaterThan(property, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
                }
                else if (property.Type == typeof(System.DateTime))
                {
                    binaryExpression = Expression.GreaterThan(property, Expression.Convert(Expression.Constant(expression.Operands[1]), property.Type));
                }
            }
            else if (expression.OperatorName.Equals("GreaterThanOrEqual"))
            {
                if (property.Type == typeof(System.String))
                {
                    binaryExpression = Expression.GreaterThanOrEqual(property, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
                }
                else if (property.Type == typeof(System.DateTime))
                {
                    binaryExpression = Expression.GreaterThanOrEqual(property, Expression.Convert(Expression.Constant(expression.Operands[1]), property.Type));
                }
            }
            else if (expression.OperatorName.Equals("LessThan"))
            {
                if (property.Type == typeof(System.String))
                {
                    binaryExpression = Expression.LessThan(property, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
                }
                else if (property.Type == typeof(System.DateTime))
                {
                    binaryExpression = Expression.LessThan(property, Expression.Convert(Expression.Constant(expression.Operands[1]), property.Type));
                }
            }
            else if (expression.OperatorName.Equals("LessThanOrEqual"))
            {
                if (property.Type == typeof(System.String))
                {
                    binaryExpression = Expression.LessThanOrEqual(property, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
                }
                else if (property.Type == typeof(System.DateTime))
                {
                    binaryExpression = Expression.LessThanOrEqual(property, Expression.Convert(Expression.Constant(expression.Operands[1]), property.Type));
                }
            }
            else if (expression.OperatorName.Equals("Present"))
            {
                // If value cannot be null, then it is always present.
                if (IsNullable(property.Type))
                {
                    binaryExpression = Expression.NotEqual(property, Expression.Constant(null, property.Type));
                }
                else
                {
                    binaryExpression = Expression.Constant(true);
                }
            }
            else if (expression.OperatorName.Equals("OrderBy") || expression.OperatorName.Equals("OrderByDescending"))
            {
                /*
                var propertyValue = property.Type == typeof(bool) ? (object)Boolean.Parse(expression.Operands[0].ToString()) : expression.Operands[0].ToString();
                var method = typeof(string).GetMethod(expression.OperatorName, new[] { typeof(string) });
                //binaryExpression = Expression.Call(property, method, Expression.Convert(Expression.Constant(null), property.Type));
                var sortby = expression.Operands[0].ToString();
                var param = parameter; // Expression.Parameter(property.Type);// typeof(T));
                binaryExpression = Expression.Convert(Expression.Property(param, sortby), property.Type);
                */
                //OrderBy
                var ope = expression.Operands[0].ToString();
                var method = typeof(string).GetMethod(ope, new[] { typeof(string) });
                binaryExpression = Expression.Call(property, method, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
            }
            else if (expression.OperatorName.Equals("Delimiter"))
            {
                var ope = expression.Operands[0].ToString();
                if (ope.Equals("OrderByDescending") || ope.Equals("ThenByDescending"))
                {
                    //ThenByDescending
                    var method = typeof(string).GetMethod(ope, new[] { typeof(string) });
                    binaryExpression = Expression.Call(property, method, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
                }
                else if (ope.Equals("OrderBy") || ope.Equals("ThenBy"))
                {
                    //ThenBy
                    var method = typeof(string).GetMethod(ope, new[] { typeof(string) });
                    binaryExpression = Expression.Call(property, method, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
                }
                else
                {
                    binaryExpression = this.BindAttributeExpression<TResource>(expression, mapper);
                }
            }
            if (binaryExpression == null)
            {
                throw new InvalidOperationException("Unsupported node operator");
            }
            try
            {
                return Expression.Lambda<Func<TResource, bool>>(binaryExpression, parameter);

            }
            catch (System.ArgumentException ex)
            {
                System.Console.WriteLine(ex.ToString());
                throw new InvalidOperationException("Expression.Lambda ArgumentException");
            }
        }

        protected virtual Expression<Func<TResource, bool>> BindLogicalExpression<TResource>(ScimCallExpression expression, IAttributeNameMapper mapper, string prefix)
        {
            var leftNodeExpression = this.BuildExpression<TResource>(expression.Operands[0], mapper, prefix);
            var rightNodeExpression = this.BuildExpression<TResource>(expression.Operands[1], mapper, prefix);

            if (expression.OperatorName.Equals("Or"))
            {
                return leftNodeExpression.Or(rightNodeExpression);
            }
            if (expression.OperatorName.Equals("And"))
            {
                return leftNodeExpression.And(rightNodeExpression);
            }
            if ((expression.OperatorName != "And" || expression.OperatorName != "Or"))
            {
                return this.BindAttributeExpression<TResource>(expression, mapper);
            }

            throw new InvalidOperationException("Unsupported branch operator");
        }

        protected virtual Expression<Func<TResource, bool>> BindDelimiterExpression<TResource>(ScimCallExpression expression, IAttributeNameMapper mapper, string prefix)
        {
            var leftNodeExpression = this.BuildExpression<TResource>(expression.Operands[0], mapper, prefix);
            var rightNodeExpression = this.BuildExpression<TResource>(expression.Operands[1], mapper, prefix);

            if (expression.OperatorName.Equals("Delimiter"))
            {
#if false
                string[] companies = { "Consolidated Messenger", "Alpine Ski House", "Southridge Video", "City Power & Light",
                                               "Coho Winery", "Wide World Importers", "Graphic Design Institute", "Adventure Works",
                                               "Humongous Insurance", "Woodgrove Bank", "Margie's Travel", "Northwind Traders",
                                               "Blue Yonder Airlines", "Trey Research", "The Phone Company",
                                               "Wingtip Toys", "Lucerne Publishing", "Fourth Coffee" };

                            // The IQueryable data to query.
                            IQueryable<String> queryableData = companies.AsQueryable<string>();

                            // Compose the expression tree that represents the parameter to the predicate.
                            ParameterExpression pe = Expression.Parameter(typeof(string), "company");

                            // ***** Where(company => (company.ToLower() == "coho winery" || company.Length > 16)) *****
                            // Create an expression tree that represents the expression 'company.ToLower() == "coho winery"'.
                            Expression left = Expression.Call(pe, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
                            Expression right = Expression.Constant("coho winery");
                            Expression e1 = Expression.Equal(left, right);

                            // Create an expression tree that represents the expression 'company.Length > 16'.
                            left = Expression.Property(pe, typeof(string).GetProperty("Length"));
                            right = Expression.Constant(16, typeof(int));
                            Expression e2 = Expression.GreaterThan(left, right);

                            // Combine the expression trees to create an expression tree that represents the
                            // expression '(company.ToLower() == "coho winery" || company.Length > 16)'.
                            Expression predicateBody = Expression.OrElse(e1, e2);

                            // Create an expression tree that represents the expression
                            // 'queryableData.Where(company => (company.ToLower() == "coho winery" || company.Length > 16))'
                            MethodCallExpression whereCallExpression = Expression.Call(
                                typeof(Queryable),
                                "Where",
                                new Type[] { queryableData.ElementType },
                                queryableData.Expression,
                                Expression.Lambda<Func<string, bool>>(predicateBody, new ParameterExpression[] { pe }));
                            // ***** End Where *****

                            // ***** OrderBy(company => company) *****
                            // Create an expression tree that represents the expression
                            // 'whereCallExpression.OrderBy(company => company)'
                            MethodCallExpression orderByCallExpression = Expression.Call(
                                typeof(Queryable),
                                "OrderBy",
                                new Type[] { queryableData.ElementType, queryableData.ElementType },
                                whereCallExpression,
                                Expression.Lambda<Func<string, string>>(pe, new ParameterExpression[] { pe }));
                            // ***** End OrderBy *****

                            // Create an executable query from the expression tree.
                            IQueryable<string> results = queryableData.Provider.CreateQuery<string>(orderByCallExpression);

                            // Enumerate the results.
                            foreach (string company in results)
                                Console.WriteLine(company);

                            /*  This code produces the following output:

                                Blue Yonder Airlines
                                City Power & Light
                                Coho Winery
                                Consolidated Messenger
                                Graphic Design Institute
                                Humongous Insurance
                                Lucerne Publishing
                                Northwind Traders
                                The Phone Company
                                Wide World Importers
                            */
#endif
            }
            else 
            {
                return this.BindAttributeExpression<TResource>(expression, mapper);
            }

            throw new InvalidOperationException("Unsupported branch operator");
        }

        // TODO: Move to extensions
        private static bool IsNullable(Type type)
        {
            if (!type.GetTypeInfo().IsValueType) return true; // ref-type
            return Nullable.GetUnderlyingType(type) != null;
        }
    }
}