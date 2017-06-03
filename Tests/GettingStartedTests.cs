using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Xunit;
using Newtonsoft.Json.Linq;

namespace Tests
{
    public class AspNetIntegrationTests
    {
        [Fact]
        public async Task Test()
        {
            var appBuilder = new WebHostBuilder()
                .UseStartup<TestStartup>();
            var testServer = new TestServer(appBuilder) { BaseAddress = new Uri("http://localhost:40000/") };

            var httpResponseMessage = await testServer.CreateClient().PostAsync("/graphql",
                new StringContent(JsonConvert.SerializeObject(new QueryContent {Query = @"{ users { username } }"})));

            httpResponseMessage.EnsureSuccessStatusCode();
            var s = await httpResponseMessage.Content.ReadAsStringAsync();
        }

        public class QueryContent
        {
            public string Query { get; set; }
        }

        public class TestStartup
        {
            public TestStartup(IHostingEnvironment env)
            {
                Configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            }

            public IConfigurationRoot Configuration { get; }

            public virtual void ConfigureServices(IServiceCollection services)
            {
                services.AddDbContext<TestContext>(x => x.UseInMemoryDatabase("test"));
                services.AddTransient<Query>();   
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
            {
                app.UseGraph<Query>();

                var testContext = app.ApplicationServices.GetService<TestContext>();
                testContext.Users.Add(new ApplicationUser { Username = "henrik" });
                testContext.SaveChanges();
            }
        }
    }

    public static class GraphMiddlewareExtentions
    {
        public static IApplicationBuilder UseGraph<TQuery>(this IApplicationBuilder builder)
        {
            var schema = new SchemaBuilder()
                .UseQuery<TQuery>()
                .Build();
            return builder.UseMiddleware<GraphMiddleware>(schema);
        }
    }

    public class GraphMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly QueryExecutor _queryExecutor;

        public GraphMiddleware(RequestDelegate next, Schema schema, IServiceProvider sp)
        {
            _next = next;
            _queryExecutor = new QueryExecutor(schema, new QueryExecutorOptions {Resolver = sp.GetRequiredService});
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/graphql"))
            {
                var requestBody = context.Request.Body;
                var streamReader = new StreamReader(requestBody);
                var bodyString = await streamReader.ReadToEndAsync();

                var query = JsonConvert.DeserializeObject<IDictionary<string, string>>(bodyString);
                var executionResult = await _queryExecutor.ExecuteAsync(query["Query"]);
                var jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(new JsonTextWriter(new StreamWriter(context.Response.Body)), executionResult.Data);
                var serializeObject = JsonConvert.SerializeObject(executionResult.Data);
                await context.Response.WriteAsync(serializeObject);
            }
            else
            {
                await _next(context);
            }
        }
    }

    public class GettingStartedTests
    {
        private readonly Schema _schema;
        private readonly IServiceProvider _serviceProvider;

        public GettingStartedTests()
        {
            _schema = new SchemaBuilder()
                .UseQuery<Query>()
                .Build();

            var services = new ServiceCollection();
            services.AddDbContext<TestContext>(x => x.UseInMemoryDatabase("test"));
            services.AddTransient<Query>();
            _serviceProvider = services.BuildServiceProvider();

            var testContext = _serviceProvider.GetService<TestContext>();
            testContext.Users.Add(new ApplicationUser { Username = "henrik" });
            testContext.SaveChanges();
        }

        [Fact]
        public void Test1()
        {
            var result = Exec(@"{ users { username } }");
            var expected = "{ data: { users: [ { username: 'henrik' } ] } }";
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