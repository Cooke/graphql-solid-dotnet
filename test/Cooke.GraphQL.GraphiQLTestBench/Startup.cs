using System.Threading.Tasks;
using Cooke.GraphQL.AspNetCore;
using Cooke.GraphQL.AutoTests.UnitTests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cooke.GraphQL.GraphiQLTestBench
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphQL();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseGraphiQl("/graphiql");
            app.UseGraphQL<Query>();

            app.Run(context =>
            {
                context.Response.Redirect("/graphiql");
                return Task.CompletedTask;
            });
        }
    }
}
