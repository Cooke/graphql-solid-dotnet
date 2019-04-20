using System;
using System.Collections.Generic;
using System.Text;

namespace Cooke.GraphQL.AutoTests
{
    public class ScenarioTests
    {
        public ScenarioTests()
        {
            var builder = new SchemaBuilder();
            builder.Query<Query>();
            builder.ObjectType<TestUser>(type =>
            {
                type.Field(x => x.Friends);
                type.Field("ListOfInts", x => new[] {1, 2, 3});
                type.Field("ListOfStrings");
                type.AddResolverType<TestUserResolver>();
            });

            builder.AddSchema(@"
                type Post {
                    name: String
                    tags: [Tag!]!
                }

                type Tag {
                    name: String!
                }
");

            builder.ObjectType("Tag", type => type.AddResolverType<TagResolverType>());
            builder.ObjectType("Post", type =>
            {
                type.Field("tags", builder.Type("Tag"));
            });

            builder.ObjectType("Post", type => { type.AddResolverType<PostResolver>(); });
        }

        private class PostResolver
        {
            private readonly Db _db;

            public PostResolver(Db db)
            {
                _db = db;
            }

            public string Name(FieldResolveContext ctx) => "name";
        }

        private class Query
        {
            public TestUser Me { get; } = new TestUser {Id = "123"};
        }

        private class TestUser
        {
            public string Id { get; set; }

            public List<TestUser> Friends { get; set; }

            public string BestFriendId { get; set; }
        }

        private class TestUserResolver
        {
            private readonly Db _db;

            public TestUserResolver(Db db)
            {
                _db = db;
            }

            public List<string> ListOfStrings(FieldResolveContext context) => new List<string> {"one", "two"};

            public TestUser BestFriend(FieldResolveContext<TestUser> context) =>
                _db.Find(context.Instance.BestFriendId);
        }

        private class Db
        {
            public TestUser Find(string bestFriendId)
            {
                return new TestUser {BestFriendId = bestFriendId};
            }
        }
    }

    public class TagResolverType
    {
    }
}