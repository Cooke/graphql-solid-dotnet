using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Newtonsoft.Json.Linq;

namespace Tests
{
    public class UnitTest1
    {
        private readonly object _schemaInstance;
        private readonly SchemaGraphType _schemaType;

        public UnitTest1()
        {
            _schemaType = new SchemaTypeBuilder()
                .UseQuery<Query>()
                .Build();

            var services = new ServiceCollection();
            services.AddDbContext<TestContext>(x => x.UseInMemoryDatabase("test"));
            services.AddTransient<Query>();
            var serviceProvider = services.BuildServiceProvider();

            var testContext = serviceProvider.GetService<TestContext>();
            testContext.Users.Add(new ApplicationUser { Username = "henrik" });
            testContext.SaveChanges();

            _schemaInstance = _schemaType.Create(serviceProvider.GetRequiredService);
        }

        [Fact]
        public void Test1()
        {
            var result = Exec(@"{ Users { Username } }");
            var expected = "{ data: { Users: [ { Username: 'henrik' } ] } }";
            AssertResult(expected, result);
        }

        private static void AssertResult(string expectedResult, ExecutionResult result)
        {
            var expectedData = JObject.Parse(expectedResult);
            Assert.Equal(expectedData, result.Data);
        }

        private ExecutionResult Exec(string query)
        {
            var graphqlExecutor = new GraphqlExecutor();
            var result = graphqlExecutor.ExecuteAsync(query, _schemaType, _schemaInstance).Result;
            return result;
        }
    }
}