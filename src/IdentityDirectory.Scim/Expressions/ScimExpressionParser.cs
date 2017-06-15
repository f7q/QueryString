namespace IdentityDirectory.Scim.Query
{
    using System;
    using Sprache;
    using System.Linq;

    public class ScimExpressionParser
    {
        private static readonly Parser<string> And = Parse.String("and").Or(Parse.String("And")).Or(Parse.String("AND")).Token().Return("And");
        private static readonly Parser<string> Eq = Parse.String("eq").Or(Parse.String("Eq")).Or(Parse.String("EQ")).Token().Return("Equal");
        private static readonly Parser<string> Ne = Parse.String("ne").Or(Parse.String("Ne")).Or(Parse.String("NE")).Token().Return("NotEqual");
        private static readonly Parser<string> Gt = Parse.String("gt").Or(Parse.String("Gt")).Or(Parse.String("GT")).Token().Return("GreaterThan");
        private static readonly Parser<string> Ge = Parse.String("ge").Or(Parse.String("Ge")).Or(Parse.String("GE")).Token().Return("GreaterThanOrEqual");
        private static readonly Parser<string> Or = Parse.String("or").Or(Parse.String("Or")).Or(Parse.String("OR")).Token().Return("Or");
        private static readonly Parser<string> Lt = Parse.String("lt").Or(Parse.String("Lt")).Or(Parse.String("LT")).Token().Return("LessThan");
        private static readonly Parser<string> Le = Parse.String("le").Or(Parse.String("Le")).Or(Parse.String("LE")).Token().Return("LessThanOrEqual");
        private static readonly Parser<string> Co = Parse.String("co").Or(Parse.String("Co")).Or(Parse.String("CO")).Token().Return("Contains");
        private static readonly Parser<string> Sw = Parse.String("sw").Or(Parse.String("Sw")).Or(Parse.String("SW")).Token().Return("StartsWith");
        private static readonly Parser<string> Ew = Parse.String("ew").Or(Parse.String("Ew")).Or(Parse.String("EW")).Token().Return("EndsWith");
        private static readonly Parser<string> Pr = Parse.String("pr").Or(Parse.String("Pr")).Or(Parse.String("PR")).Token().Return("Present");

        private static readonly Parser<string> IdentifierName;
        private static readonly Parser<ScimExpression> AttrName;
        private static readonly Parser<ScimExpression> AttrPath;
        private static readonly Parser<Func<ScimExpression, ScimExpression>> SubAttr;
        private static readonly Parser<Func<ScimExpression, ScimExpression>> ValuePath;

        private static readonly Parser<ScimExpression> CaseInsensitiveString;
        private static readonly Parser<ScimExpression> Operand;
        private static readonly Parser<ScimExpression> Literal;
        private static readonly Parser<ScimExpression> Filter;
        private static readonly Parser<ScimExpression> ExpressionInParentheses = from lparen in Parse.Char('(')
                                                                                 from expr in Parse.Ref(() => Filter)
                                                                                 from rparen in Parse.Char(')')
                                                                                 select expr;
        
        private static readonly Parser<ScimExpression> LogicalExpression;
        
        private static readonly Parser<ScimExpression> AttributeExpression;

        private static readonly Parser<ScimExpression> Comparison;
        private static readonly Parser<ScimExpression> Presence;


        private static readonly Parser<char> StringContentChar = Parse.CharExcept("\\\"").Or(Parse.String("\\\\").Return('\\')).Or(Parse.String("\\\"").Return('\"'));
        private static readonly Parser<string> QuotedString = from open in Parse.Char('\'').Optional()
                                                              from content in Parse.LetterOrDigit.Or(Parse.Chars(",;:.-/ ")).Many().Text()
                                                              from close in Parse.Char('\'').Optional()
                                                              select content;
        
        /// <summary>
        /// 仮想コンストラクタ
        /// </summary>
        static ScimExpressionParser()
        {
            // Value値 [']?[a-zA-Z][a-zA-Z0-9]+['}?
            CaseInsensitiveString = from content in QuotedString
                                    select ScimExpression.String(content);
            // Key値 [a-zA-Z][_a-zA-Z0-9]+
            IdentifierName = Parse.Identifier(Parse.Letter, Parse.LetterOrDigit.Or(Parse.Char('_')));

            //compValue = false / null / true / number / string 
            //; rules from JSON(RFC 7159)
            Literal = Parse.String("true").Return(ScimExpression.Constant(true))
                .XOr(Parse.String("false").Return(ScimExpression.Constant(false)))
                .XOr(Parse.String("null").Return(ScimExpression.Constant(null)));

            //ATTRNAME = ALPHA * (nameChar)
            //nameChar = "-" / "_" / DIGIT / ALPHA
            // TODO : check - and _
            AttrName = IdentifierName.Select(ScimExpression.Attribute);

            //valuePath = attrPath "[" valFilter "]"
            //    ; FILTER uses sub - attributes of a parent attrPath
            ValuePath = from open in Parse.Char('[')
                        from expr in Parse.Ref(() => Filter)
                        from close in Parse.Char(']')
                        select new Func<ScimExpression, ScimExpression>(r => ScimExpression.Binary("Where", r, expr));

            //subAttr = "." ATTRNAME
            //; a sub-attribute of a complex attribute
            SubAttr = Parse.Char('.')
                .Then(_ => IdentifierName)
                .Then(n => Parse.Return(new Func<ScimExpression, ScimExpression>(r => ScimExpression.SubAttribute(n, r))));

            //attrPath = [URI ":"] ATTRNAME * 1subAttr
            //     ; SCIM attribute name
            //     ; URI is SCIM "schema" URI
            AttrPath = AttrName
                .SelectMany(root => SubAttr.XOr(ValuePath).XMany(), (name, path) => path.Aggregate(name, (o, f) => f(o)));

            Operand = (ExpressionInParentheses
                .XOr(Literal.Or(AttrPath.Token()))
                .XOr(CaseInsensitiveString)).Token();

            // compareOp = "eq" / "ne" / "co" /
            //        "sw" / "ew" /
            //        "gt" / "lt" /
            //        "ge" / "le"
            Comparison = Parse.XChainOperator(
                Le.Or(Lt)
                .XOr(Ge.Or(Gt))
                .XOr(Eq.Or(Ew))
                .XOr(Sw)
                .XOr(Ne)
                .XOr(Co)
                .XOr(Pr)
                , Operand, ScimExpression.Binary);

            // attrPath SP "pr"
            Presence = Operand.SelectMany(operand => Pr, (operand, pr) => ScimExpression.Unary(pr, operand));

            // attrExp = (attrPath SP "pr") /
            //   (attrPath SP compareOp SP compValue)
            AttributeExpression = Presence.Or(Comparison);

            // logExp    = FILTER SP ("and" / "or") SP FILTER
            LogicalExpression = Parse.XChainOperator(Or.Or(And), AttributeExpression, ScimExpression.Binary);
            Filter = LogicalExpression;
        }

        public static ScimExpression ParseExpression(string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            return Filter.End().Parse(expression);
        }

        private static Parser<string> Operator(string op, string opName)
        {
            return Parse.String(op).Token().Return(opName);
        }
    }
}