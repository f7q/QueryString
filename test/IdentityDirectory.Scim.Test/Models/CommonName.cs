using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityDirectory.Scim.Test.Models
{
    public class CommonName
    {
        public CommonName(string givenName, string familyName)
        {
            this.GivenName = givenName;
            this.FamilyName = familyName;
            this.Formatted = givenName + " " + familyName;
        }

        public CommonName()
        {
            this.GivenName = "";
            this.FamilyName = "";
            this.Formatted = "";
        }

        public string FamilyName { get; set; }

        public string Formatted { get; set; }

        public string GivenName { get; set; }

        public string HonorificPrefix { get; set; }

        public string HonorificSuffix { get; set; }

        public string MiddleName { get; set; }
    }
    public class Item2
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool Flg { get; set; }
    }
}