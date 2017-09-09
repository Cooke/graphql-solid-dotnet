using System;
using System.Linq;
using Cooke.GraphQL.Annotations;

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

    public abstract class Character
    {
        private readonly DataCharacter _data;

        protected Character(DataCharacter data)
        {
            _data = data;
        }

        [NotNull]
        public string Id => _data.Id;

        public string Name => _data.Name;

        public Character[] Friends => _data.Friends.Select(Create).ToArray();

        public static Character Create(DataCharacter arg)
        {
            if (arg is DataHuman)
            {
                return new Human((DataHuman)arg);
            }

            if (arg is DataDroid)
            {
                return new Droid((DataDroid)arg);
            }

            throw new NotImplementedException();
        }

        public EpisodeEnum[] AppearsIn => _data.AppearsIn;

        public string SecretBackstory => null;
    }

    public class Human : Character
    {
        private readonly DataHuman _dataHuman;

        public Human(DataHuman dataHuman) : base(dataHuman)
        {
            _dataHuman = dataHuman;
        }

        public string HomePlanet => _dataHuman.HomePlanet;
    }

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
