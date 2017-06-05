using Microsoft.AspNetCore.Builder;

namespace Cooke.GraphQL.AspNetCore
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

        public static IApplicationBuilder UseGraphQL<TQuery, TMutation>(this IApplicationBuilder builder)
        {
            var schema = new SchemaBuilder()
                .UseQuery<TQuery>()
                .UseMutation<TMutation>()
                .Build();
            return builder.UseMiddleware<GraphQLMiddleware>(schema);
        }
    }
}