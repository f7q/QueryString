namespace IdentityDirectory.Scim.Query
{
    using System;
    using System.Linq;
    using Sprache;

    public class SelectExpressionParser
    {
        private static readonly Parser<string> Delimiter = Parse.Char(',').Token().Return("Delimiter");
        
        private static readonly Parser<ScimExpression> Filter;
        private static readonly Parser<ScimExpression> ExpressionInParentheses;
        private static readonly Parser<ScimExpression> CaseInsensitiveString;
        private static readonly Parser<ScimExpression> Operand;
        private static readonly Parser<ScimExpression> Presence;

        /// <summary>
        /// 仮想コンストラクタ
        /// </summary>
        static SelectExpressionParser()
        {
            CaseInsensitiveString = from space in Parse.WhiteSpace.Many().Optional()
                                    from content in Parse.LetterOrDigit.Or(Parse.Chars("_")).Many().Text()
                                    from space2 in Parse.WhiteSpace.Many().Optional()
                                    select ScimExpression.String(content);
            ExpressionInParentheses = from content in Parse.Ref(() => Filter)
                                      select content;
            Operand = (CaseInsensitiveString
                ).Token();
            
            // logExp    = FILTER SP (",") SP FILTER
            //Filter = ExpressionInParentheses.SelectMany(operand => Delimiter, (operand, pr) => ScimExpression.Unary(pr, operand));
            Filter = Parse.XChainOperator(Delimiter, CaseInsensitiveString, ScimExpression.Binary);
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