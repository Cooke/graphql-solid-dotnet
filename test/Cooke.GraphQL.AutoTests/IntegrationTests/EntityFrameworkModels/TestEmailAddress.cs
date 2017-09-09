namespace Cooke.GraphQL.AutoTests.IntegrationTests.EntityFrameworkModels
{
    public class TestEmailAddress
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public TestEmailAddressType Type { get; set; }
    }
}