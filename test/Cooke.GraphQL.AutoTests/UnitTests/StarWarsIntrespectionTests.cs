using Newtonsoft.Json.Linq;
using Xunit;

namespace Cooke.GraphQL.AutoTests.UnitTests
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
        public void Allows_querying_the_schema_for_query_type()
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

        [Fact]
        public void Allows_querying_the_schema_object_fields()
        {
            const string query = @"query IntrospectionDroidFieldsQuery {
              __type(name: ""Droid"") {
                        name
                        fields {
                            name
                            type {
                                name
                                    kind
                            }
                        }
                    }
                }";

            var response = ExecuteQuery(query);

            AssertResponse(@"{ data: {
        __type: {
          name: 'Droid',
          fields: [
{
              name: 'primaryFunction',
              type: {
                name: 'String',
                kind: 'SCALAR'
              }
            },
            {
              name: 'id',
              type: {
                name: null,
                kind: 'NON_NULL'
              }
            },
            {
              name: 'name',
              type: {
                name: 'String',
                kind: 'SCALAR'
              }
            },
            {
              name: 'friends',
              type: {
                name: null,
                kind: 'LIST'
              }
            },
            {
              name: 'appearsIn',
              type: {
                name: null,
                kind: 'LIST'
              }
            },
            {
              name: 'secretBackstory',
              type: {
                name: 'String',
                kind: 'SCALAR'
              }
            }
          ]
        }
      }}", response);
        }

        [Fact]
        public void Allows_querying_the_schema_for_nested_object_fields()
        {
            const string query = @"query IntrospectionDroidNestedFieldsQuery {
                  __type(name: ""Droid"") {
                    name
                    fields {
                        name
                        type {
                            name
                                kind
                            ofType {
                                name
                                    kind
                            }
                        }
                    }
                }
            }";

            var response = ExecuteQuery(query);

            AssertResponse(@"{ data: {
        __type: {
          name: 'Droid',
          fields: [
            {
              name: 'primaryFunction',
              type: {
                name: 'String',
                kind: 'SCALAR',
                ofType: null
              }
            },
            {
              name: 'id',
              type: {
                name: null,
                kind: 'NON_NULL',
                ofType: {
                  name: 'String',
                  kind: 'SCALAR'
                }
              }
            },
            {
              name: 'name',
              type: {
                name: 'String',
                kind: 'SCALAR',
                ofType: null
              }
            },
            {
              name: 'friends',
              type: {
                name: null,
                kind: 'LIST',
                ofType: {
                  name: 'Character',
                  kind: 'INTERFACE'
                }
              }
            },
            {
              name: 'appearsIn',
              type: {
                name: null,
                kind: 'LIST',
                ofType: {
                  name: 'Episode',
                  kind: 'ENUM'
                }
              }
            },
            {
              name: 'secretBackstory',
              type: {
                name: 'String',
                kind: 'SCALAR',
                ofType: null
              }
            }
          ]
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