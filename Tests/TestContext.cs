using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public class TestContext : DbContext
    {
        public TestContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }
    }

    public class ApplicationUser
    {
        public int Id { get; set; }

        public string Username { get; set; }

        //public List<string> Roles { get; set; }

        public List<EmailAddress> Emails { get; set; }
    }

    public class EmailAddress
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public EmailAddressType Type { get; set; }
    }

    public enum EmailAddressType
    {
        Home,
        Work,
        Mobile
    }
}
