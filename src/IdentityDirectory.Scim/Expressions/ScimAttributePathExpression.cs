namespace IdentityDirectory.Scim.Expressions
{
    using System;
    using System.Linq;
    using Query;

    public class ScimAttributePathExpression : ScimExpression
    {
        public string AttributePath { get; set; }

        public ScimAttributePathExpression(string attrPath)
        {
            AttributePath = attrPath ?? throw new ArgumentNullException("attrPath");
        }

        public override string ToString()
        {
            return AttributePath;
        }
    }
}