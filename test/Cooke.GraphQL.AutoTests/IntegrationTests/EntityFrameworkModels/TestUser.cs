using System.Collections.Generic;

namespace Tests.IntegrationTests.EntityFrameworkModels
{
    public class TestUser
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public List<TestEmailAddress> Emails { get; set; }
    }
}