namespace IdentityDirectory.Scim.Expressions
{
    using System;
    using IdentityDirectory.Scim.Query;

    public class ScimStringExpression : ScimExpression
    {
        public ScimStringExpression(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Value { get; }

        public override string ToString()
        {
            return Value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}