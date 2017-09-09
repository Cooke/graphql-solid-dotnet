namespace Cooke.GraphQL.AutoTests.UnitTests
{
    public class Droid : Character
    {
        private readonly DataDroid _dataDroid;

        public Droid(DataDroid dataDroid) : base(dataDroid)
        {
            _dataDroid = dataDroid;
        }

        public string PrimaryFunction => _dataDroid.PrimaryFunction;
    }
}