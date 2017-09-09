namespace Cooke.GraphQL.AutoTests.UnitTests
{
    public class Human : Character
    {
        private readonly DataHuman _dataHuman;

        public Human(DataHuman dataHuman) : base(dataHuman)
        {
            _dataHuman = dataHuman;
        }

        public string HomePlanet => _dataHuman.HomePlanet;
    }
}