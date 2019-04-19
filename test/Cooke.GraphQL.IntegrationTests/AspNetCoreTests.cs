using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Cooke.GraphQL.AspNetCore;
using Cooke.GraphQL.AutoTests.IntegrationTests.EntityFrameworkModels;
using Cooke.GraphQL.AutoTests.IntegrationTests.Schema;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Cooke.GraphQL.AutoTests.IntegrationTests
{
    public class AspNetCoreTests
    {
        private readonly HttpClient _httpClient;

        public AspNetCoreTests()
        {
            var appBuilder = new WebHostBuilder()
                .UseStartup<TestStartup>();
            var testServer = new TestServer(appBuilder);

            var testContext = testServer.Host.Services.GetService<TestDbContext>();
            testContext.Users.Add(new TestUser { Username = "henrik" });
            testContext.SaveChanges();

            _httpClient = testServer.CreateClient();
        }

        [Fact]
        public async Task BasicQueryShallWork()
        {
            var queryContent = new QueryContent {Query = @"{ users { username } }"};
            var response = await PostAsync(queryContent);
            
            var expected = "{ data: { users: [ { username: 'henrik' } ] } }";

            AssertResponse(expected, response);
        }

        [Fact]
        public async Task AuthorizeAttributeShallPreventUnwantedAccess()
        {
            var queryContent = new QueryContent { Query = @"{ usersProtected { username } }" };
            var response = await PostAsync(queryContent);

            var expected = "{ data: { usersProtected: null }, errors: [ { message: \"Access denied\" }] }";

            AssertResponse(expected, response);
        }

        [Fact]
        public async Task AuthorizeAttributeShallGrantAuthorizedUsersAccess()
        {
            var queryContent = new QueryContent { Query = @"{ usersProtected { username } }" };
            var response = await PostAsync(queryContent, true);

            var expected = "{ data: { users: [ { username: 'henrik' } ] } }";

            AssertResponse(expected, response);
        }

        private static void AssertResponse(string expected, string response)
        {
            Assert.Equal(JObject.Parse(expected), JObject.Parse(response));
        }

        private async Task<string> PostAsync(QueryContent queryContent, bool authenticate = false)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(queryContent), Encoding.UTF8, "application/json");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/graphql");

            if (authenticate)
            {
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("basic", "something");
            }

            httpRequestMessage.Content = stringContent;
            var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);
            return await httpResponseMessage.Content.ReadAsStringAsync();
        }

        public class QueryContent
        {
            [JsonProperty("query")]
            public string Query { get; set; }
        }

        public class TestStartup
        {
            public virtual void ConfigureServices(IServiceCollection services)
            {
                services.AddDbContext<TestDbContext>(x => x.UseInMemoryDatabase(Guid.NewGuid().ToString()));
                services.AddTransient<Query>();
                services.AddGraphQL();
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
            {
                app.Use(async (context, next) =>
                {
                    if (context.Request.Headers.ContainsKey(HeaderNames.Authorization))
                    {
                        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, "something"), 
                            new Claim(ClaimTypes.Role, "TestRole")
                        }, "fake"));
                        context.User = claimsPrincipal;
                    }

                    await next();
                });
                app.UseGraphQL<Query>();
            }
        }
    }
}