using System.Threading.Tasks;
using Tests.IntegrationTests.EntityFrameworkModels;

namespace Tests.IntegrationTests.Schema
{
    public class Mutation
    {
        private readonly TestDbContext _dbContext;

        public Mutation(TestDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> CreateUser(UserInput user)
        {
            var applicationUser = new TestUser
            {
                Username = user.Username
            };

            _dbContext.Users.Add(applicationUser);
            await _dbContext.SaveChangesAsync();
            return new User(applicationUser);
        }
    }
}