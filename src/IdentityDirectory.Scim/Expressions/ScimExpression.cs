namespace IdentityDirectory.Scim.Query
{
    using IdentityDirectory.Scim.Expressions;

    /// <summary>
    /// 関数ファクトリ
    /// </summary>
    public class ScimExpression
    {
        public static ScimExpression String(string value)
        {
            return new ScimStringExpression(value);
        }

        /// <summary>
        /// 枝が二つ
        /// </summary>
        /// <param name="opName">オペランド名</param>
        /// <param name="leftOperand">カラム名</param>
        /// <param name="rightOperand">値</param>
        /// <returns></returns>
        public static ScimExpression Binary(string opName, ScimExpression leftOperand, ScimExpression rightOperand)
        {
            return new ScimCallExpression(opName, leftOperand, rightOperand);
        }

        /// <summary>
        /// 枝がひとつ
        /// </summary>
        /// <param name="opName">オペランド名</param>
        /// <param name="operand">値</param>
        /// <returns></returns>
        public static ScimExpression Unary(string opName, ScimExpression operand)
        {
            return new ScimCallExpression(opName, operand);
        }

        public static ScimExpression Attribute(string name)
        {
            return new ScimAttributePathExpression(name);
        }

        public static ScimExpression SubAttribute(string name, ScimExpression parent)
        {
            return new ScimSubAttributeExpression(name, parent);
        }

        public static ScimExpression Constant(object constantValue)
        {
            return new ScimConstantExpression(constantValue);
        }
    }
}