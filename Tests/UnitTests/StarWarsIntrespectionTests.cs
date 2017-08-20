using Cooke.GraphQL;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Tests.UnitTests
{
    public class StarWarsIntrespectionTests
    {
        private readonly QueryExecutor _queryExecutor;

        public StarWarsIntrespectionTests()
        {
            var schema = new SchemaBuilder()
                .UseQuery<Query>()
                .Build();

            _queryExecutor = new QueryExecutorBuilder()
                .WithSchema(schema)
                .Build();
        }

        [Fact]
        public void Allows_querying_the_schema_for_types()
        {
            const string query = @"
                query IntrospectionTypeQuery {
                  __schema {
                    types {
                      name
                    }
                  }
            }";

            var result = ExecuteQuery(query);

            AssertResponse(@"{
                data: {
                    __schema: {
                      types: [
                        {
                          name: 'Query'
                        },
                        {
                          name: 'Character'
                        },
                        {
                          name: 'String'
                        },
                        {
                          name: 'Episode'
                        },
                        {
                          name: 'Human'
                        },
                        {
                          name: 'Droid'
                        },
                        {
                          name: '__Schema'
                        },
                        {
                          name: '__Type'
                        },
                        {
                          name: '__TypeKind'
                        },
                        {
                          name: '__Field'
                        },
                        {
                          name: '__EnumValue'
                        },
                        {
                          name: 'Boolean'
                        },
                        {
                          name: '__Directive'
                        },
                        {
                          name: '__DirectiveLocation'
                        },

                        {
                          name: '__InputValue'
                        },
                      ]
                    }
                  }
                }", result);
        }

        [Fact]
        public void Allows_Querying_The_Schema_For_Query_Type()
        {
            const string query = @"
                query IntrospectionQueryTypeQuery {
                  __schema {
                    queryType {
                      name
                    }
                  }
                }";

            var response = ExecuteQuery(query);

            AssertResponse(@"{ data: {
                __schema: {
                  queryType: {
                    name: 'Query'
                  },
                }
              }}", response);
        }

        [Fact]
        public void Allows_querying_for_a_specific_type()
        {
            const string query = @"
                query IntrospectionDroidTypeQuery {
                  __type(name: ""Droid"") {
                    name
                }
            }";

            var response = ExecuteQuery(query);

            AssertResponse(@"{ data: {
                __type: {
                  name: 'Droid'
                }
              }}", response);
        }

        [Fact]
        public void Allows_querying_the_schema_for_an_object_kind()
        {
            const string query = @"
                query IntrospectionDroidKindQuery {
                  __type(name: ""Droid"") {
                    name
                    kind
                }
            }";

            var response = ExecuteQuery(query);

            AssertResponse(@"{ data: {
                __type: {
                  name: 'Droid',
                  kind: 'OBJECT'
                }
              }}", response);
        }

        [Fact]
        public void Allows_querying_the_schema_for_an_interface_kind()
        {
            const string query = @"
                query IntrospectionDroidKindQuery {
                  __type(name: ""Character"") {
                    name
                    kind
                }
            }";

            var response = ExecuteQuery(query);

            AssertResponse(@"{ data: {
                __type: {
                  name: 'Character',
                  kind: 'INTERFACE'
                }
              }}", response);
        }

        private JObject ExecuteQuery(string query)
        {
            return _queryExecutor.ExecuteAsync(query).Result;
        }

        private static void AssertResponse(string expected, JObject response)
        {
            var normExpected = JObject.Parse(expected).ToString();
            var normResponse = response.ToString();
            Assert.Equal(normExpected, normResponse);
        }
    }
}