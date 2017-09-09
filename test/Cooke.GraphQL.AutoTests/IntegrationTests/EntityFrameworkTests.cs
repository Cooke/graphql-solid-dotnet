using System;
using System.Threading.Tasks;
using Cooke.GraphQL;
using Cooke.GraphQL.AspNetCore;
using Cooke.GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Tests.IntegrationTests.EntityFrameworkModels;
using Tests.IntegrationTests.Schema;
using Xunit;

namespace Tests.IntegrationTests
{
    public class EntityFrameworkTests
    {
        private readonly Cooke.GraphQL.Schema _schema;
        private readonly IServiceProvider _serviceProvider;

        public EntityFrameworkTests()
        {
            _schema = new SchemaBuilder()
                .UseQuery<Query>()
                .UseMutation<Mutation>()
                .UseAttributeMetadata()
                .Build();

            var services = new ServiceCollection();
            services.AddDbContext<TestDbContext>(x => x.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            services.AddTransient<Query>();
            services.AddTransient<Mutation>();
            services.AddTransient<AuthorizationFieldMiddleware>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            _serviceProvider = services.BuildServiceProvider();

            var testContext = _serviceProvider.GetService<TestDbContext>();
            testContext.Users.Add(new TestUser { Username = "henrik" });
            testContext.SaveChanges();
        }

        [Fact]
        public void QueryListNoArguments()
        {
            var result = Exec(@"{ users { username } }");
            var expected = "{ data: { users: [ { username: 'henrik' } ] } }";
            AssertResult(expected, result);
        }

        [Fact]
        public void QueryFuncListNoArguments()
        {
            var result = Exec(@"{ usersFunc { username } }");
            var expected = "{ data: { usersFunc: [ { username: 'henrik' } ] } }";
            AssertResult(expected, result);
        }

        [Fact]
        public void QueryObjectOneArgument()
        {
            var result = Exec(@"{ user(username: ""henrik"") { username } }");
            var expected = "{ data: { user: { username: 'henrik' } } }";
            AssertResult(expected, result);
        }

        [Fact]
        public void MutationInputObjectArgument()
        {
            var result = Exec(@"mutation { createUser(user: { username: ""henrik"" }) { username } }");
            var expected = "{ data: { createUser: { username: 'henrik' } } }";
            AssertResult(expected, result);
        }

        private static void AssertResult(string expectedResult, JObject result)
        {
            var expectedData = JObject.Parse(expectedResult);
            Assert.Equal(expectedData, result);
        }

        private JObject Exec(string query)
        {
            var queryExecutor = new QueryExecutorBuilder()
                .WithSchema(_schema)
                .WithResolver(_serviceProvider)
                .Build();
            
            var result = queryExecutor.ExecuteAsync(query).Result;
            return result;
        }
    }

    public class CasingFieldMiddleware
    {
        public async Task<object> Execute(FieldResolveContext context, FieldResolver next, string casing = "upper")
        {
            var result = (string)await next(context);
            
            if (casing == "upper")
            {
                return result.ToUpperInvariant();
            }
            else
            {
                return result.ToLowerInvariant();
            }
        }
    }
}