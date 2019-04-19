﻿using System;
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
            builder.Type<TestUser>(x =>
            {

            });
        }

        private class Query
        {
            public TestUser Me { get; } = new TestUser { Id = "123" };
        }

        private class TestUser
        {
            public string Id { get; set; }

            public List<TestUser> Friends { get; set; }
        }
    }
}
