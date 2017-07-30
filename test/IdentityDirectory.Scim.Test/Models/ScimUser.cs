namespace IdentityDirectory.Scim.Test.Models
{
    public class ScimUser
    {
        public CommonName Name { get; set; }

        public string UserName { get; set; }

        public string Id { get; set; }

        /// <summary>
        /// Entityは空のコンストラクタでなくてはいけない。
        /// </summary>
        public ScimUser() : this(string.Empty, string.Empty, string.Empty, string.Empty)
        {
            //this.UserName = string.Empty;
            //this.Name = new CommonName(string.Empty, string.Empty);
            // return new ScimUser(string.Empty, string.Empty, string.Empty, string.Empty);
        }

        public ScimUser(string id, string userName, string givenName, string familyName)
        {
            this.Id = id;
            this.UserName = userName;
            this.Name = new CommonName(givenName, familyName);
        }
    }
}