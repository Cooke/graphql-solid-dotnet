namespace Cooke.GraphQL.AutoTests.UnitTests
{
    public class Query
    {
        private readonly StarWarsData _starWarsData;

        public Query()
        {
            _starWarsData = new StarWarsData();
        }

        public Character Hero(EpisodeEnum episode)
        {
            return Character.Create(_starWarsData.GetHero(episode));
        }
    }
}
