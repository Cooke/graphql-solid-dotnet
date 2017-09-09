using Cooke.GraphQL.AutoTests.IntegrationTests.EntityFrameworkModels;

namespace Cooke.GraphQL.AutoTests.IntegrationTests.Schema
{
    public class User
    {
        private readonly TestUser _backingUser;

        public User(TestUser backingUser)
        {
            _backingUser = backingUser;
        }

        public int Id => _backingUser.Id;

        public string Username => _backingUser.Username;
    }
}