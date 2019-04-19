namespace Cooke.GraphQL.AutoTests.UnitTests
{
    public abstract class DataCharacter
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public DataCharacter[] Friends { get; set; }

        public EpisodeEnum[] AppearsIn { get; set; }
    }
}