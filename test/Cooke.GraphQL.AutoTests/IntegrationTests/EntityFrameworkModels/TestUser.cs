using System.Collections.Generic;

namespace Cooke.GraphQL.AutoTests.IntegrationTests.EntityFrameworkModels
{
    public class TestUser
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public List<TestEmailAddress> Emails { get; set; }
    }
}