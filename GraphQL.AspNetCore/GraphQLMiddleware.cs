using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Cooke.GraphQL.AspNetCore
{
    public class GraphQLMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly QueryExecutor _queryExecutor;

        public GraphQLMiddleware(RequestDelegate next, QueryExecutor queryExecutor)
        {
            _next = next;
            _queryExecutor = queryExecutor;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/graphql"))
            {
                // TODO this impl was thrown together just to make it work. Go over it and think it through.
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
}