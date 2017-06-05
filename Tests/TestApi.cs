using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public class Query
    {
        private readonly TestContext _context;

        public Query(TestContext context)
        {
            _context = context;
        }

        //public Task<User[]> UsersWithRole(string role) => _context.Users.Where(x => x.Roles.Contains(role)).Select(x => new User(x)).ToArrayAsync();

        public Task<User[]> Users => _context.Users.ToArrayAsync().ContinueWith(t => t.Result.Select(x => new User(x)).ToArray());

        public Task<User[]> UsersFunc() => _context.Users.ToArrayAsync().ContinueWith(t => t.Result.Select(x => new User(x)).ToArray());

        public Task<User> User(string username) => _context.Users.FirstOrDefaultAsync(x => x.Username == username).ContinueWith(x => x.Result != null ? new User(x.Result) : null);

        

        //[Authorize]
        //public async Task<User> ProtectedUser(int id) => new User(await _context.Users.FindAsync(id));
    }

    public class Mutation
    {
        private readonly TestContext _context;

        public Mutation(TestContext context)
        {
            _context = context;
        }

        public async Task<User> CreateUser(UserInput user)
        {
            var applicationUser = new ApplicationUser
            {
                Username = user.Username
            };

            _context.Users.Add(applicationUser);
            await _context.SaveChangesAsync();
            return new User(applicationUser);
        }
    }

    public class AuthorizeAttribute : Attribute
    {
    }

    public class UserInput
    {
        public string Username { get; set; }
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
