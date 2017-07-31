using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Tests.IntegrationTests.EntityFrameworkModels;

namespace Tests.IntegrationTests.Schema
{
    public class Query
    {
        private readonly TestDbContext _dbContext;

        public Query(TestDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<User[]> Users => _dbContext.Users.ToArrayAsync().ContinueWith(t => t.Result.Select(x => new User(x)).ToArray());

        public Task<User[]> UsersFunc() => _dbContext.Users.ToArrayAsync().ContinueWith(t => t.Result.Select(x => new User(x)).ToArray());

        public Task<User> User(string username) => _dbContext.Users.FirstOrDefaultAsync(x => x.Username == username).ContinueWith(x => x.Result != null ? new User(x.Result) : null);

        [Authorize]
        public Task<User[]> UsersProtected() => _dbContext.Users.ToArrayAsync().ContinueWith(t => t.Result.Select(x => new User(x)).ToArray());
    }
}