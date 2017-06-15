namespace IdentityDirectory.Scim.Query
{
    using System;
    using System.Linq;
    using Sprache;

    public class SortExpressionParser
    {
        private static readonly Parser<string> Descending = Parse.String("desc").Or(Parse.String("-")).Or(Parse.String("d")).Token().Return("Descending");
        private static readonly Parser<string> Ascending = Parse.String("asc").Token().Return("Ascending");
        private static readonly Parser<string> Delimiter = Parse.String(",").Token().Return("Delimiter");
        
        private static readonly Parser<ScimExpression> Filter;
        private static readonly Parser<ScimExpression> ExpressionInParentheses = from lparen in Parse.Char('-').Or(Parse.Char('d')).Optional()
                                                                                 from expr in Parse.Ref(() => Filter)
                                                                                 from rparen in Parse.String("desc").Or(Parse.String("asc")).Optional()
                                                                                 select expr;
        
        private static readonly Parser<ScimExpression> Comparison;
        private static readonly Parser<ScimExpression> Presence;

        /// <summary>
        /// 仮想コンストラクタ
        /// </summary>
        static SortExpressionParser()
        {

            // compareOp = "-" / "d" valuePath "desc" /
            //        valuePath" / "" / "asc" /
            Comparison = Parse.XChainOperator(
                Descending
                .XOr(Ascending)
                , ExpressionInParentheses.Token(), ScimExpression.Binary);

            // logExp    = FILTER SP (",") SP FILTER
            Filter = Parse.XChainOperator(Delimiter, Comparison, ScimExpression.Binary);
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