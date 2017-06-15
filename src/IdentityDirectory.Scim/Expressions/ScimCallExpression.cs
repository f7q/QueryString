namespace IdentityDirectory.Scim.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityDirectory.Scim.Query;

    public class ScimCallExpression : ScimExpression
    {
        public ScimCallExpression(string opName, params ScimExpression[] operands)
        {
            OperatorName = opName ?? throw new ArgumentNullException("opName");
            Operands = operands ?? throw new ArgumentNullException("operands");
        }

        public ScimExpression[] Operands { get; }

        public string OperatorName { get; }
   
        public override string ToString()
        {
            return OperatorName + "(" + string.Join(",", Operands.Select(o => o.ToString())) + ")";
        }
    }
}
