using System.Linq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Cooke.GraphQL.AutoTests.UnitTests
{
    // Port of https://github.com/graphql/graphql-js/blob/master/src/__tests__/starWarsQuery-test.js
    public class StarWarsQueryTests
    {
        private readonly QueryExecutor _queryExecutor;

        public StarWarsQueryTests()
        {
            var schema = new SchemaBuilder()
                .UseQuery<Query>()
                .Build();

            _queryExecutor = new QueryExecutorBuilder()
                .WithSchema(schema)
                .Build();
        }

        [Fact]
        public void Correctly_Identifies_R2_D2_As_The_Hero_of_The_Star_Wars_Saga()
        {
            const string query = @"
                query HeroNameQuery {
                  hero {
                    name
                  }
                }";

            var result = ExecuteQuery(query);

            AssertResponse(@"{
                data: {
                  hero: {
                    name: 'R2-D2'
                  }
                }
              }", result);
        }

        [Fact]
        public void Allows_us_to_query_for_id_and_friends_of_R2_D2()
        {
            const string query = @"
                query HeroNameAndFriendsQuery {
                  hero {
                    id
                    name
                    friends {
                      name
                    }
                  }
                }";

            var result = ExecuteQuery(query);

            AssertResponse(@"{
                data: {
                  hero: {
                    id: '2001',
                    name: 'R2-D2',
                    friends: [
                      {
                        name: 'Luke Skywalker',
                      },
                      {
                        name: 'Han Solo',
                      },
                      {
                        name: 'Leia Organa',
                      },
                    ]
                  }
                }
              }", result);
        }

        [Fact]
        public void Allows_us_to_query_for_friends_of_friends_of_R2_D2()
        {
            const string query = @"
                query NestedQuery {
                  hero {
                    name
                    friends {
                      name
                      appearsIn
                      friends {
                        name
                      }
                    }
                  }
                }";

            var result = ExecuteQuery(query);

            AssertResponse(@"{
                data: {
                  hero: {
                    name: 'R2-D2',
                    friends: [
                      {
                        name: 'Luke Skywalker',
                        appearsIn: [ 'NEWHOPE', 'EMPIRE', 'JEDI' ],
                        friends: [
                          {
                            name: 'Han Solo',
                          },
                          {
                            name: 'Leia Organa',
                          },
                          {
                            name: 'C-3PO',
                          },
                          {
                            name: 'R2-D2',
                          },
                        ]
                      },
                      {
                        name: 'Han Solo',
                        appearsIn: [ 'NEWHOPE', 'EMPIRE', 'JEDI' ],
                        friends: [
                          {
                            name: 'Luke Skywalker',
                          },
                          {
                            name: 'Leia Organa',
                          },
                          {
                            name: 'R2-D2',
                          },
                        ]
                      },
                      {
                        name: 'Leia Organa',
                        appearsIn: [ 'NEWHOPE', 'EMPIRE', 'JEDI' ],
                        friends: [
                          {
                            name: 'Luke Skywalker',
                          },
                          {
                            name: 'Han Solo',
                          },
                          {
                            name: 'C-3PO',
                          },
                          {
                            name: 'R2-D2',
                          },
                        ]
                      },
                    ]
                  }
                }
              }", result);
        }

        [Fact]
        public void Allows_us_to_query_for_Luke_Skywalker_directly_using_his_ID()
        {
            const string query = @"
                query FetchLukeQuery {
                      human(id: ""1000"") {
                        name
                    }
                }";

            var result = ExecuteQuery(query);

            AssertResponse(@"{
                data: {
                  human: {
                    name: 'Luke Skywalker'
                  }
                }
              }", result);
        }

        [Fact]
        public void Allows_us_to_create_a_generic_query_then_use_it_to_fetch_Luke_Skywalker_using_his_ID()
        {
            const string query = @"
                query FetchSomeIDQuery($someId: String!) {
                  human(id: $someId) {
                    name
                  }
                }";

            var result = ExecuteQuery(query, ("someId", "1000"));

            AssertResponse(@"{
                data: {
                  human: {
                    name: 'Luke Skywalker'
                  }
                }
              }", result);
        }

        [Fact]
        public void Allows_us_to_create_a_generic_query_then_use_it_to_fetch_Han_Solo_using_his_ID()
        {
            const string query = @"
                query FetchSomeIDQuery($someId: String!) {
                  human(id: $someId) {
                    name
                  }
                }";

            var result = ExecuteQuery(query, ("someId", "1002"));

            AssertResponse(@"{
                data: {
                  human: {
                    name: 'Han Solo'
                  }
                }
              }", result);
        }

        [Fact]
        public void Allows_us_to_create_a_generic_query_then_pass_invalid_id_to_get_null_back()
        {
            const string query = @"
                query FetchSomeIDQuery($someId: String!) {
                  human(id: $someId) {
                    name
                  }
                }";

            var result = ExecuteQuery(query, ("someId", "invalid id"));

            AssertResponse(@"{
                data: {
                  human: null
                }
              }", result);
        }

        [Fact]
        public void Allows_us_to_query_for_Luke_chaning_his_key_with_an_alias()
        {
            const string query = @"
                query FetchLukeAliased {
                    luke: human(id: ""1000"") {
                        name
                    }
                }";

            var result = ExecuteQuery(query);

            AssertResponse(@"{
                data: {
                  luke: {
                    name: 'Luke Skywalker'
                  }
                }
              }", result);
        }

        [Fact]
        public void Allows_us_to_query_for_both_Luke_and_Leia_using_two_root_fields_with_an_alias()
        {
            const string query = @"
                query FetchLukeAndLeiaAliased {
                    luke: human(id: ""1000"") {
                        name
                    }
                    leia: human(id: ""1003"")
                    {
                        name
                    }
                }";

            var result = ExecuteQuery(query);

            AssertResponse(@"{
                data: {
                  luke: {
                    name: 'Luke Skywalker',
                  },
                  leia: {
                    name: 'Leia Organa',
                  },
                }}", result);
        }

        [Fact]
        public void Allows_us_to_query_using_duplicate_content()
        {
            const string query = @"
                query DuplicateFields {
                    luke: human(id: ""1000"") {
                        name
                        homePlanet
                    }
                    leia: human(id: ""1003"")
                    {
                        name
                        homePlanet
                    }
                }";

            var result = ExecuteQuery(query);

            AssertResponse(@"{
                data: {
                  luke: {
                    name: 'Luke Skywalker',
                    homePlanet: 'Tatooine',
                  },
                  leia: {
                    name: 'Leia Organa',
                    homePlanet: 'Alderaan',
                  },
                },
              }", result);
        }

        [Fact]
        public void Allows_us_to_use_a_fragment_to_avoid_duplicate_content()
        {
            const string query = @"
                query UseFragment {
                    luke: human(id: ""1000"") {
                        ...HumanFragment
                    }
                    leia: human(id: ""1003"")
                    {
                        ...HumanFragment
                    }
                }

                fragment HumanFragment on Human {
                          name
                          homePlanet
                        }
                ";

            var result = ExecuteQuery(query);

            AssertResponse(@"{
                data: {
                  luke: {
                    name: 'Luke Skywalker',
                    homePlanet: 'Tatooine',
                  },
                  leia: {
                    name: 'Leia Organa',
                    homePlanet: 'Alderaan',
                  },
                },
              }", result);
        }

        // Using__typename_to_find_the_type_of_an_object
        [Fact(Skip = "Not yet implemented")]
        public void Allows_us_to_verify_that_R2_D2_is_a_droid()
        {
            const string query = @"
                query CheckTypeOfR2 {
                    hero {
                    __typename
                    name
                    }
                }";

            var result = ExecuteQuery(query);

            AssertResponse(@"{
             data: {
                  hero: {
                    __typename: 'Droid',
                    name: 'R2-D2',
                  },
                },
              }", result);
        }

        private JObject ExecuteQuery(string query, params (string, object)[] variables)
        {
            return _queryExecutor.ExecuteRequestAsync(query, null, JObject.FromObject(variables.ToDictionary(x => x.Item1, x => x.Item2))).GetAwaiter().GetResult();
        }

        private static void AssertResponse(string expected, JObject response)
        {
            var normExpected = JObject.Parse(expected).ToString();
            var normResponse = response.ToString();
            Assert.Equal(normExpected, normResponse);
        }
    }
}
