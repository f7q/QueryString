namespace IdentityDirectory.Scim.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IdentityDirectory.Scim.Query;
    using IdentityDirectory.Scim.Services;
    using Moq;
    using Xunit;
    using Microsoft.EntityFrameworkCore;

    public class ScimUserContext : DbContext
    {
        public ScimUserContext()
        {
        }

        public ScimUserContext(DbContextOptions<ScimUserContext> options)
            : base(options)
        {
        }

        public DbSet<ScimUser> ScimUsers { get; set; }

        //public DbSet<CommonName> CommonNames { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ScimUser>()
                .HasKey(c => c.UserName);

            modelBuilder.Entity<CommonName>()
                .HasKey(c => c.FamilyName);
        }
    }

    public class ScimUser
    {
        public CommonName Name { get; set; }

        public string UserName { get; set; }

        /// <summary>
        /// Entityは空のコンストラクタでなくてはいけない。
        /// </summary>
        public ScimUser() : this(string.Empty, string.Empty, string.Empty, string.Empty)
        {
            //this.UserName = string.Empty;
            //this.Name = new CommonName(string.Empty, string.Empty);
        }

        public ScimUser(string id, string userName, string givenName, string familyName)
        {
            this.UserName = userName;
            this.Name = new CommonName(givenName, familyName);
        }
    }

    //public sealed class CommonName
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

    public class UserAccount
    {
        public string ProfileUrl { get; set; }

        public string DisplayName { get; set; }
    }

    public class ScimQueryModelTests
    {
        private readonly List<UserAccount> testUsers = new List<UserAccount>
                                                    {
                                                        new UserAccount { ProfileUrl = "http://myprofile.com", DisplayName = "BestEmployee" },
                                                        new UserAccount { ProfileUrl = "http://myprofile.com", DisplayName = "SomeEmployee" },
                                                        new UserAccount { DisplayName = "BestEmployee" },
                                                    };

        private readonly List<ScimUser> testResources = new List<ScimUser>
                                                    {
                                                        new ScimUser("2", "user2", "user2gn", "user2fn"),
                                                        new ScimUser("3", "user3", "user3gn", "user3fn")
                                                    };

        [Fact]
        public void CanConvertBasicFilter()
        {
            var mapperMoq = new Mock<IAttributeNameMapper>();
            mapperMoq.Setup(m => m.MapToInternal(It.IsAny<string>())).Returns((string s) => char.ToUpper(s[0]) + s.Substring(1));

            var filterNode = ScimExpressionParser.ParseExpression("profileUrl pr and displayName co 'Employee'");
            var converter = new DefaultFilterBinder();
            var predicate = converter.Bind<UserAccount>(filterNode, string.Empty, false, mapperMoq.Object);
            Assert.NotNull(predicate);
            Console.WriteLine(predicate);
            var usersCount = this.testUsers.AsQueryable().Count(predicate);
            Assert.Equal(2, usersCount);
        }

        [Fact]
        public void CanConvertPathFilter()
        {
            var converter = new DefaultFilterBinder();
            var nameMapper = new DefaultAttributeNameMapper();
            var filterNode = ScimExpressionParser.ParseExpression("name.familyName co 'user3'");
            var predicate = converter.Bind<ScimUser>(filterNode, string.Empty, false, nameMapper);
            Assert.NotNull(predicate);
            Console.WriteLine(predicate);
            var usersCount = this.testResources.AsQueryable().Count(predicate);
            Assert.Equal(1, usersCount);
            var linq = this.testResources.AsQueryable().Where(predicate); //検索条件で集約
            Assert.Equal("user3fn", linq.FirstOrDefault().Name.FamilyName);
        }

        [Fact]
        public void CanConvertPath2Filter()
        {
            var converter = new DefaultFilterBinder();
            var nameMapper = new DefaultAttributeNameMapper();
            var filterNode = ScimExpressionParser.ParseExpression("name.familyName co 'user'");
            var predicate = converter.Bind<ScimUser>(filterNode, string.Empty, false, nameMapper);
            Assert.NotNull(predicate);
            Console.WriteLine(predicate);
            var usersCount = this.testResources.AsQueryable().Count(predicate);
            Assert.Equal(2, usersCount);
            var linq = this.testResources.AsQueryable().Where(predicate); //検索条件で集約
            var users = linq.Where(x => x.UserName == "user2").FirstOrDefault(); //複数件だとさらに条件で集約
            Assert.Equal("user2fn", users.Name.FamilyName);
            users = linq.Where(x => x.UserName == "user3").FirstOrDefault();
            Assert.Equal("user3fn", users.Name.FamilyName);
        }

        [Fact]
        public void Add_writes_to_database()
        {
            var options = new DbContextOptionsBuilder<ScimUserContext>()
                .UseInMemoryDatabase(databaseName: "Add_writes_to_database")
                .Options;

            // Run the test against one instance of the context
            using (var context = new ScimUserContext(options))
            {
                // var service = new BlogService(context);
                // service.Add("http://sample.com");

                foreach (var val in this.testResources)
                {
                    context.ScimUsers.Add(val);
                    context.SaveChanges();
                }

                context.ScimUsers.Add(new ScimUser("1", "user1", "user1gn", "user1fn"));
                context.SaveChanges();
                context.ScimUsers.Add(new ScimUser("4", "user4", "user4gn", "user4fn"));
                context.SaveChanges();
            }

            // Use a separate instance of the context to verify correct data was saved to database
            using (var context = new ScimUserContext(options))
            {
                Assert.Equal(4, context.ScimUsers.Count());
                Assert.Throws<InvalidOperationException>(() => context.ScimUsers.Single().UserName);
                Assert.Equal("user1", context.ScimUsers.OrderBy(x => x.UserName).FirstOrDefault().UserName);
            }
        }

        [Fact]
        public void Add_writes_to_database2()
        {
            var options = new DbContextOptionsBuilder<ScimUserContext>()
                .UseInMemoryDatabase(databaseName: "Add_writes_to_database2")
                .Options;

            // Run the test against one instance of the context
            using (var context = new ScimUserContext(options))
            {
                // var service = new BlogService(context);
                // service.Add("http://sample.com");

                foreach (var val in this.testResources)
                {
                    context.ScimUsers.Add(val);
                    context.SaveChanges();
                }

                context.ScimUsers.Add(new ScimUser("1", "user1", "user1gn", "user1fn"));
                context.SaveChanges();
                var scimuser = new ScimUser("4", "user4", "user4gn", "user4fn");
                scimuser.Name = new CommonName("user4gn", "user4fn");
                context.ScimUsers.Add(scimuser);

                context.SaveChanges();
            }

            // Use a separate instance of the context to verify correct data was saved to database
            using (var context = new ScimUserContext(options))
            {
                Assert.Equal(4, context.ScimUsers.Count());
                var scimuser = context.ScimUsers.OrderByDescending(x => x.UserName).FirstOrDefault();
                Assert.Equal("user4", scimuser.UserName);
                Assert.Equal("", scimuser.Name.FamilyName);
            }
        }

        [Fact]
        public void CanConvertPath3Filter()
        {
            var options = new DbContextOptionsBuilder<ScimUserContext>()
                .UseInMemoryDatabase(databaseName: "CanConvertPath3Filter")
                .Options;

            // Run the test against one instance of the context
            using (var context = new ScimUserContext(options))
            {
                // var service = new BlogService(context);
                // service.Add("http://sample.com");

                foreach (var val in this.testResources)
                {
                    context.ScimUsers.Add(val);
                    context.SaveChanges();
                }

                context.ScimUsers.Add(new ScimUser("1", "user1", "user1gn", "user1fn"));
                context.SaveChanges();
                context.ScimUsers.Add(new ScimUser("4", "user4", "user4gn", "user4fn"));
                context.SaveChanges();
            }

            // Use a separate instance of the context to verify correct data was saved to database
            using (var context = new ScimUserContext(options))
            {
                var converter = new DefaultFilterBinder();
                var nameMapper = new DefaultAttributeNameMapper();
                var filterNode = ScimExpressionParser.ParseExpression("UserName eq 'user3'");
                var predicate = converter.Bind<ScimUser>(filterNode, string.Empty, false, nameMapper);
                Assert.NotNull(predicate);
                Console.WriteLine(predicate);

                Assert.Equal(1, context.ScimUsers.Count(predicate));
                Assert.Throws<InvalidOperationException>(() => context.ScimUsers.Single().UserName);
                Assert.Equal("user3", context.ScimUsers.Where(predicate).FirstOrDefault().UserName);
                Assert.Equal("", context.ScimUsers.FirstOrDefault().Name.FamilyName); // ICollection化しないとリレーション張れない？
            }
        }
    }
}