namespace IdentityDirectory.Scim.Services
{
    using System.Linq;
    using System.Dynamic;

    public class DefaultAttributeNameMapper :IAttributeNameMapper
	{
		// Simple uppercase for now.
		public string MapToInternal(string attr)
		{
		    var namePathParts = attr.Split('.');
		    var mappedPathParts = namePathParts.Select(part => char.ToUpper(part[0]) + part.Substring(1));
            return string.Join(".", mappedPathParts);
		}

		public string[] MapToInternal(string[] attr)
		{
			throw new System.NotImplementedException();
		}

		public string MapFromInternal(string attr)
		{
			throw new System.NotImplementedException();
		}

		public string[] MapFromInternal(string[] attr)
		{
			throw new System.NotImplementedException();
		}
    }
    public class CusutomAttributeNameMapper : IAttributeNameMapper2
    {
        // Simple uppercase for now.
        public dynamic MapToInternal(dynamic attr)
        {
            DynamicObject obj = attr;
            if(obj.GetType() == typeof(string))
            {
                string str = attr;
                var namePathParts = str.Split('.');
                var mappedPathParts = namePathParts.Select(part => char.ToUpper(part[0]) + part.Substring(1));
                return string.Join(".", mappedPathParts);
            }
            return null;
        }

        public dynamic[] MapToInternal(dynamic[] attr)
        {
            throw new System.NotImplementedException();
        }

        public dynamic MapFromInternal(dynamic attr)
        {
            throw new System.NotImplementedException();
        }

        public dynamic[] MapFromInternal(dynamic[] attr)
        {
            throw new System.NotImplementedException();
        }
    }
}