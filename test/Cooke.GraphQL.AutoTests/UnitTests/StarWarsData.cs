using System.Collections.Generic;
using Cooke.GraphQL.Annotations;

namespace Tests.UnitTests
{
    [TypeName("Episode")]
    public enum EpisodeEnum
    {
        NewHope = 4,
        Empire = 5,
        Jedi = 6
    }

    public abstract class DataCharacter
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public DataCharacter[] Friends { get; set; }

        public EpisodeEnum[] AppearsIn { get; set; }
    }

    public class DataHuman : DataCharacter
    {
        public string HomePlanet { get; set; }
    }

    public class DataDroid : DataCharacter
    {
        public string PrimaryFunction { get; set; }
    }

    public class StarWarsData
    {
        private readonly DataHuman _luke;
        private readonly DataDroid _artoo;
        private readonly DataHuman _han;
        private readonly DataHuman _vader;
        private readonly DataHuman _leia;
        private readonly DataHuman _tarkin;
        private readonly DataDroid _threepio;

        public StarWarsData()
        {
            _luke = new DataHuman
            {
                Id = "1000",
                Name = "Luke Skywalker",
                AppearsIn = new[] { EpisodeEnum.NewHope, EpisodeEnum.Empire, EpisodeEnum.Jedi, },
                HomePlanet = "Tatooine"
            };

            _vader = new DataHuman
            {
                Id = "1001",
                Name = "Darth Vader",
                AppearsIn = new[] { EpisodeEnum.NewHope, EpisodeEnum.Empire, EpisodeEnum.Jedi, },
                HomePlanet = "Tatooine",
            };

            _han = new DataHuman
            {
                Id = "1002",
                Name = "Han Solo",
                AppearsIn = new[] { EpisodeEnum.NewHope, EpisodeEnum.Empire, EpisodeEnum.Jedi, }
            };

            _leia = new DataHuman
            {
                Id = "1003",
                Name = "Leia Organa",
                AppearsIn = new[] { EpisodeEnum.NewHope, EpisodeEnum.Empire, EpisodeEnum.Jedi, },
                HomePlanet = "Alderaan"
            };

            _tarkin = new DataHuman
            {
                Id = "1004",
                Name = "Wilhuff Tarkin",
                AppearsIn = new[] { EpisodeEnum.NewHope, }
            };
            
            _threepio = new DataDroid
            {
                Id = "2000",
                Name = "C-3PO",
                AppearsIn = new[] {EpisodeEnum.NewHope, EpisodeEnum.Empire, EpisodeEnum.Jedi,},
                PrimaryFunction = "Protocol",
            };

            _artoo = new DataDroid
            {
                Name = "R2-D2",
                Id = "2001",
                Friends = new[] { _luke },
                AppearsIn = new[] { EpisodeEnum.NewHope, EpisodeEnum.Empire, EpisodeEnum.Jedi, },
                PrimaryFunction = "Astromech",
            };

            _luke.Friends = new DataCharacter[] { _han, _leia, _threepio, _artoo };
            _vader.Friends = new DataCharacter[] { _tarkin };
            _han.Friends = new DataCharacter[] { _luke, _leia, _artoo };
            _leia.Friends = new DataCharacter[] { _luke, _han, _threepio, _artoo };
            _tarkin.Friends = new DataCharacter[] { _vader };
            _threepio.Friends = new DataCharacter[] { _luke, _han, _leia, _artoo };
            _artoo.Friends = new DataCharacter[] { _luke, _han, _leia };

            var humans = new Dictionary<string, DataCharacter>
            {
                { _luke.Id, _luke },
                { _vader.Id, _vader},
                { _leia.Id, _leia },
                { _han.Id, _han },
                { _tarkin.Id, _tarkin },
            };

            var droids = new Dictionary<string, DataDroid>
            {
                { _threepio.Id, _threepio },
                { _artoo.Id, _artoo }
            };
        }

        public DataCharacter GetHero(EpisodeEnum episode)
        {
            if (episode == EpisodeEnum.Empire)
            {
                return _luke;
            }

            return _artoo;
        }
    }

}
