namespace IdentityDirectory.Scim.Expressions
{
    using IdentityDirectory.Scim.Query;

    public class ScimConstantExpression : ScimExpression
    {
        public ScimConstantExpression(object constantValue)
        {
            ConstantValue = constantValue;
        }

        public object ConstantValue { get; }

        public override string ToString()
        {
            return (ConstantValue ?? "null").ToString();
        }
    }
}