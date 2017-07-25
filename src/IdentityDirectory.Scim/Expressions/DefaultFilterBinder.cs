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
                return this.BindAttributeExpression<TResource>(callExpression, mapper);
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
                try
                {
                    // Workaround for missing coersion between String and Guid types.
                    var propertyValue = property.Type == typeof(Guid) ? (object)Guid.Parse(expression.Operands[1].ToString()) : expression.Operands[1].ToString();
                    binaryExpression = Expression.Equal(property, Expression.Convert(Expression.Constant(propertyValue), property.Type));
                }
                catch (System.InvalidOperationException ex)
                {
                    System.Console.WriteLine(ex.ToString());
                    try
                    {
                        // If value cannot be null, then it is always present.
                        if (IsNullable(property.Type))
                        {
                            binaryExpression = Expression.Equal(property, Expression.Convert(Expression.Constant(null), property.Type));
                        }
                        else
                        {
                            binaryExpression = Expression.Equal(property, Expression.Convert(Expression.Constant(true), property.Type));
                        }
                    }
                    catch (System.InvalidOperationException edate)
                    {
                        binaryExpression = Expression.Equal(property, Expression.Convert(Expression.Constant(DateTime.Parse(expression.Operands[1].ToString())), property.Type));
                    }
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
                binaryExpression = Expression.GreaterThan(property, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
            }
            else if (expression.OperatorName.Equals("GreaterThanOrEqual"))
            {
                binaryExpression = Expression.GreaterThanOrEqual(property, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
            }
            else if (expression.OperatorName.Equals("LessThan"))
            {
                binaryExpression = Expression.LessThan(property, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
            }
            else if (expression.OperatorName.Equals("LessThanOrEqual"))
            {
                binaryExpression = Expression.LessThanOrEqual(property, Expression.Convert(Expression.Constant(expression.Operands[1].ToString()), property.Type));
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

        // TODO: Move to extensions
        private static bool IsNullable(Type type)
        {
            if (!type.GetTypeInfo().IsValueType) return true; // ref-type
            return Nullable.GetUnderlyingType(type) != null;
        }
    }
}