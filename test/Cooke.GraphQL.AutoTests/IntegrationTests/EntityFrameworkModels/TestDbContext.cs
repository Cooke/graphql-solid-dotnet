using Microsoft.EntityFrameworkCore;

namespace Tests.IntegrationTests.EntityFrameworkModels
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<TestUser> Users { get; set; }
    }
}
