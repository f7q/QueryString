namespace IdentityDirectory.Scim.Query
{
    using System;
    using System.Linq;
    using Sprache;

    public class SortExpressionParser
    {
        private static readonly Parser<string> ThenByDescending = Parse.String("desc").Token().Return("ThenByDescending");
        private static readonly Parser<string> ThenByAscending = Parse.String("asc").Token().Return("ThenBy");
        private static readonly Parser<string> OrderByDescending = Parse.String("desc").Token().Return("OrderByDescending");
        private static readonly Parser<string> OrderByAscending = Parse.String("asc").Token().Return("OrderBy");
        private static readonly Parser<string> Descending = Parse.String("desc").Token().Return("Descending");
        private static readonly Parser<string> Ascending = Parse.String("asc").Token().Return("Ascending");
        private static readonly Parser<string> Delimiter = Parse.Char(',').Token().Return("Delimiter");

        private static readonly Parser<string> Sort;
        private static readonly Parser<string> Sort2;

        private static readonly Parser<ScimExpression> Filter;
        private static readonly Parser<ScimExpression> ExpressionInParentheses;
        private static readonly Parser<ScimExpression> CaseInsensitiveString;
        private static readonly Parser<ScimExpression> Operand;
        private static readonly Parser<ScimExpression> Presence;

        /// <summary>
        /// 仮想コンストラクタ
        /// </summary>
        static SortExpressionParser()
        {
            CaseInsensitiveString = from space in Parse.WhiteSpace.Optional()
                                    from lparen in Parse.Char('-').Or(Parse.Char('d')).Optional()
                                    from content in Parse.LetterOrDigit.Or(Parse.Chars("_")).Many().Text()
                                    select ScimExpression.String(content);

            ExpressionInParentheses = from expr in CaseInsensitiveString
                                      from space in Parse.WhiteSpace
                                      from rparen in Parse.Ref(() => Filter)
                                      select rparen;

            Operand = (CaseInsensitiveString
                ).Token();

            Sort = OrderByAscending.Or(ThenByAscending).XOr(OrderByDescending.Or(ThenByDescending));

            // compareOp = "-" / "d" valuePath "desc" /
            //        valuePath" / "" / "asc" /
            Presence = Operand.SelectMany(operand => Sort,
            (operand, pr) => ScimExpression.Unary(pr, operand));
            
            // logExp    = FILTER SP (",") SP FILTER
            Filter = Parse.XChainOperator(Delimiter, Presence, ScimExpression.Binary);
        }

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
            //return Filter.End().Parse(expression);
        }
    }
}