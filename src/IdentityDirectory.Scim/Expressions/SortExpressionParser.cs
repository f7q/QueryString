namespace IdentityDirectory.Scim.Query
{
    using System;
    using System.Linq;
    using Sprache;

    public class SortExpressionParser
    {
        private static readonly Parser<string> Descending = Parse.String("desc").Token().Return("Descending");
        private static readonly Parser<string> Ascending = Parse.String("asc").Token().Return("Ascending");
        private static readonly Parser<string> Delimiter = Parse.Char(',').Token().Return("Delimiter");


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

            // compareOp = "-" / "d" valuePath "desc" /
            //        valuePath" / "" / "asc" /
            Presence = Operand.SelectMany(operand => Descending.Or(Ascending), (operand, pr) => ScimExpression.Unary(pr, operand));
            
            // logExp    = FILTER SP (",") SP FILTER
            Filter = Parse.XChainOperator(Delimiter, Presence, ScimExpression.Binary);
        }

        public static ScimExpression ParseExpression(string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            return Filter.End().Parse(expression);
        }
    }
}