using Microsoft.AspNetCore.Builder;

namespace Tests
{
    public static class GraphQLMiddlewareExtentions
    {
        public static IApplicationBuilder UseGraphQL<TQuery>(this IApplicationBuilder builder)
        {
            var schema = new SchemaBuilder()
                .UseQuery<TQuery>()
                .Build();
            return builder.UseMiddleware<GraphQLMiddleware>(schema);
        }
    }
}