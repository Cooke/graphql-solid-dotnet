using System;
using System.Collections.Generic;
using System.Text;
using Cooke.GraphQL.Types;
using Xunit.Sdk;

namespace Cooke.GraphQL.AutoTests
{
    public class ScenarioTests
    {
        public ScenarioTests()
        {
            var builder = new SchemaBuilder(new SchemaBuilderOptions { NonNullDefault = true });
            builder.Query<Query>();
            builder.DefineType<TestUser>(type =>
            {
                type.DefineField(x => x.Friends).Type(c => c.NonNull.ListOf.NonNull.Type<TestUser>());
                type.DefineField(x => x.Friends).Type(c => c.Nullable.ListOf.Nullable.Type<TestUser>());
                type.DefineField(x => x.Friends).Type(c => c.NonNull.String);
                type.DefineField("MyFriends", ctx => ctx.Instance.Friends).Type(c => c.NonNull.Type("Query"));
                type.DefineField("ListOfInts", ctx => new[] {1, 2, 3});
                type.DefineField("Mirror", ctx => ctx.Arguments["value"]).Arguments(("value", c => c.String));
                type.IncludeResolverType<TestUserResolver>();
            });

            

//            builder.AddSchema(@"
//                type Post {
//                    name: String
//                    tags: [Tag!]!
//                }

//                type Tag {
//                    name: String!
//                }
//");
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