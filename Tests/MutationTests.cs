using System;
using Cooke.GraphQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Tests
{
    public class MutationTests
    {
        private readonly Schema _schema;
        private readonly IServiceProvider _serviceProvider;

        public MutationTests()
        {
            _schema = new SchemaBuilder()
                .UseQuery<Query>()
                .UseMutation<Mutation>()
                .Build();

            var services = new ServiceCollection();
            services.AddDbContext<TestContext>(x => x.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            services.AddTransient<Query>();
            services.AddTransient<Mutation>();
            _serviceProvider = services.BuildServiceProvider();

            var testContext = _serviceProvider.GetService<TestContext>();
            testContext.Users.Add(new ApplicationUser { Username = "henrik" });
            testContext.SaveChanges();
        }
        
        [Fact]
        public void MutationInputObjectArgument()
        {
            var result = Exec(@"mutation { createUser(user: { username: ""henrik"" }) { username } }");
            var expected = "{ data: { createUser: { username: 'henrik' } } }";
            AssertResult(expected, result);
        }

        private static void AssertResult(string expectedResult, ExecutionResult result)
        {
            var expectedData = JObject.Parse(expectedResult);
            Assert.Equal(expectedData, result.Data);
        }

        private ExecutionResult Exec(string query)
        {
            var graphqlExecutor = new QueryExecutor(_schema, new QueryExecutorOptions { Resolver = _serviceProvider.GetRequiredService });
            var result = graphqlExecutor.ExecuteAsync(query).Result;
            return result;
        }
    }
}