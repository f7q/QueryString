namespace IdentityDirectory.Scim.Query
{
    using System;

    public class SortExpressionParser
    {

        public static ScimExpression ParseExpression(string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            if (!expression.Contains(","))
            {
                if (expression.Contains("asc"))
                {
                    var value = expression.Split(' ')[0];
                    return ScimExpression.Unary("OrderBy", ScimExpression.String(value));
                }
                else
                {
                    var value = expression.Split(' ')[0];
                    return ScimExpression.Unary("OrderByDescending", ScimExpression.String(value));
                }
            }
            ScimExpression tree = null;
            var list = expression.Split(',');
            int end = list.Length;
            int count = 0;
            foreach (var sort in list)
            {
                if (count == 0)
                {
                    if (sort.Contains("asc"))
                    {
                        var value = sort.TrimStart(' ').Split(' ');
                        tree = ScimExpression.Unary("OrderBy", ScimExpression.String(value[0]));
                    }
                    else
                    {
                        var value = sort.TrimStart(' ').Split(' ');
                        tree = ScimExpression.Unary("OrderByDescending", ScimExpression.String(value[0]));
                    }
                }
                else if (end >= count)
                { 
                    if (sort.Contains("asc"))
                    {
                        var value = sort.TrimStart(' ').Split(' ');
                        tree = ScimExpression.Binary("Delimiter", tree, ScimExpression.Unary("ThenBy", ScimExpression.String(value[0])));
                    }
                    else
                    {
                        var value = sort.TrimStart(' ').Split(' ');
                        tree = ScimExpression.Binary("Delimiter", tree, ScimExpression.Unary("ThenByDescending", ScimExpression.String(value[0])));
                    }
                }
                else
                {
                    if (sort.Contains("asc"))
                    {
                        var value = sort.TrimStart(' ').Split(' ');
                        tree = ScimExpression.Binary("Delimiter", tree, ScimExpression.Unary("ThenBy", ScimExpression.String(value[0])));
                    }
                    else
                    {
                        var value = sort.TrimStart(' ').Split(' ');
                        tree = ScimExpression.Binary("Delimiter", tree, ScimExpression.Unary("ThenByDescending", ScimExpression.String(value[0])));
                    }
                }
                count++;
            }
            return tree;
        }
    }
}