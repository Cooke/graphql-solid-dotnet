using System;
using System.Linq;
using Cooke.GraphQL.Annotations;

namespace Cooke.GraphQL.AutoTests.UnitTests
{
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

        public Character[] Friends => Enumerable.ToArray(_data.Friends.Select(Create));

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
}