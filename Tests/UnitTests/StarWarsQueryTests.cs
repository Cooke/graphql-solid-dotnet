using System;
using System.Collections.Generic;
using System.Text;
using Cooke.GraphQL;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Tests.UnitTests
{
    public class StarWarsQueryTests
    {
        private QueryExecutor _queryExecutor;

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


        private JObject ExecuteQuery(string query)
        {
            return _queryExecutor.ExecuteAsync(query).Result.Data;
        }

        private static void AssertResponse(string expected, JObject response)
        {
            var normExpected = JObject.Parse(expected).ToString();
            var normResponse = response.ToString();
            Assert.Equal(normExpected, normResponse);
        }
    }
}
