﻿namespace IdentityDirectory.Scim.Test.Models
{
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

        // public DbSet<CommonName> CommonNames { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ScimUser>()
                .HasKey(c => c.UserName);

            modelBuilder.Entity<CommonName>()
                .HasKey(c => c.FamilyName);
        }
    }
}