using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Tests;

namespace Cooke.GraphQL.AspNetCore
{
    public static class GraphQLExtentions
    {
        public static IApplicationBuilder UseGraphQL<TQuery>(this IApplicationBuilder builder)
        {
            var schemaBuilder = new SchemaBuilder()
                .UseQuery<TQuery>();
            var schema = schemaBuilder.Build();

            var queryExecBuilder = new QueryExecutorBuilder()
                .WithSchema(schema)
                .WithResolver(builder.ApplicationServices.GetService)
                .UseAuthorization();
            var queryExecutor = queryExecBuilder.Build();

            return builder.UseMiddleware<GraphQLMiddleware>(queryExecutor);
        }

        public static IServiceCollection AddGraphQL(this IServiceCollection services)
        {
            if (services.Any(x => x.ServiceType != typeof(IHttpContextAccessor)))
            {
                services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
            }

            services.AddTransient<AuthorizationFieldMiddleware>();
            return services;
        }

        public static QueryExecutorBuilder UseAuthorization(this QueryExecutorBuilder builder)
        {
            return builder.UseMiddleware<AuthorizationFieldMiddleware>();
        }
    }
}