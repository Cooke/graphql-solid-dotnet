using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tests;
using Xunit;

namespace Tests
{
    public class AspNetIntegrationTests
    {
        private readonly HttpClient _httpClient;

        public AspNetIntegrationTests()
        {
            var appBuilder = new WebHostBuilder()
                .UseStartup<TestStartup>();
            var testServer = new TestServer(appBuilder);

            var testContext = testServer.Host.Services.GetService<TestContext>();
            testContext.Users.Add(new ApplicationUser { Username = "henrik" });
            testContext.SaveChanges();

            _httpClient = testServer.CreateClient();
        }

        [Fact]
        public async Task BasicQueryShallWork()
        {
            var queryContent = new QueryContent {Query = @"{ users { username } }"};
            var response = await PostAsync(queryContent);
            
            var expected = "{ data: { users: [ { username: 'henrik' } ] } }";

            Assert.Equal(JObject.Parse(expected), JObject.Parse(response));
        }

        private async Task<string> PostAsync(QueryContent queryContent)
        {
            var httpResponseMessage = await _httpClient.PostAsync("/graphql",
                new StringContent(JsonConvert.SerializeObject(queryContent)));
            return await httpResponseMessage.Content.ReadAsStringAsync();
        }

        public class QueryContent
        {
            public string Query { get; set; }
        }

        public class TestStartup
        {
            public virtual void ConfigureServices(IServiceCollection services)
            {
                services.AddDbContext<TestContext>(x => x.UseInMemoryDatabase("test"));
                services.AddTransient<Query>();   
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
            {
                app.UseGraphQL<Query>();
            }
        }
    }
}