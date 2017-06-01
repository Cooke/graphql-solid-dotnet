using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public class Schema
    {
        public Query Query
        {
            get
            {
                var dbContextOptions = new DbContextOptionsBuilder<TestContext>();
                dbContextOptions.UseInMemoryDatabase("test");
                var testContext = new TestContext(dbContextOptions.Options);
                testContext.Users.Add(new ApplicationUser {Username = "henrik"});

                return new Query(testContext);
            }
        }
    }

    public class Query
    {
        private readonly TestContext _context;

        public Query(TestContext context)
        {
            _context = context;
        }

        //public Task<User[]> UsersWithRole(string role) => _context.Users.Where(x => x.Roles.Contains(role)).Select(x => new User(x)).ToArrayAsync();

        public Task<User[]> Users => _context.Users.Select(x => new User(x)).ToArrayAsync();

        //public async Task<User> User(int id) => new User(await _context.Users.FindAsync(id));

        //[Authorize]
        //public async Task<User> ProtectedUser(int id) => new User(await _context.Users.FindAsync(id));
    }

    public class AuthorizeAttribute : Attribute
    {
    }

    public class User
    {
        private readonly ApplicationUser _backingUser;

        public User(ApplicationUser backingUser)
        {
            _backingUser = backingUser;
        }

        public int Id => _backingUser.Id;

        public string Username => _backingUser.Username;
    }
}
