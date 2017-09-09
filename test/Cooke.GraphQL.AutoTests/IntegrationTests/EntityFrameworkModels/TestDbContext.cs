using Microsoft.EntityFrameworkCore;

namespace Cooke.GraphQL.AutoTests.IntegrationTests.EntityFrameworkModels
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<TestUser> Users { get; set; }
    }
}
