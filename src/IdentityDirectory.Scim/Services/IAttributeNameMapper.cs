namespace IdentityDirectory.Scim.Services
{
	public interface IAttributeNameMapper
	{
		string MapToInternal(string attr);

        string[] MapToInternal(string[] attr);

        string MapFromInternal(string attr);

		string[] MapFromInternal(string[] attr);
    }
    public interface IAttributeNameMapper2
    {
        dynamic MapToInternal(dynamic attr);

        dynamic[] MapToInternal(dynamic[] attr);

        dynamic MapFromInternal(dynamic attr);

        dynamic[] MapFromInternal(dynamic[] attr);
    }
}