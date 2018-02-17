# GraphQL for .NET 
This is a convention based .NET standard library for building [GraphQL](http://graphql.org) endpoints. It is delivered with a middleware for ASP.NET Core and supports ASP.NET Core authorization.

This library is a work in progress and is not yet production ready. Key missing features are:

 * Validation support
 * Directive support
 * Union support
 * A few introspection fields are still missing (GraphiQL works though)
 * Support for documentation attributes (description, deprecated, ...)
 * Support for execution context data in field resolvers together with resolver injection.

## Installation

With ASP.NET Core integration:

```Install-Package Cooke.GraphQL.AspNetCore```

Without ASP.NET Core integration:

```Install-Package Cooke.GraphQL```

## Usage with ASP.NET Core

See the [integration tests](https://github.com/Cooke/graphql-plain-dotnet/tree/master/test/Cooke.GraphQL.AutoTests/IntegrationTests) for more in-depth examples on how to use the library with and without ASP.NET Core.

Define a query class:
```C#
public class Query {
    // Dependency injection in query constructor
    public Query(MyDbContext context) {
        this.context = context;
    }

    // Properties and methods corresponds to fields in GraphQL
    public Task<IEnumerable<MyApiModel>> Models => context.Models.Select(x => new MyApiModel(x)).ToArrayAsync();

    // Support for the Authorization attribute
    [Authorize]
    public MyApiModel Model(int id) => {
        var model = await context.Models.FindAsync(id);
        if (model != null) {
            return new MyApiModel(model);
        }

        return null;
    }
}
```

Enable a GraphQL endpoint and add required services in StartUp:
```C#
public class Startup
{
    public virtual void ConfigureServices(IServiceCollection services)
    {
        // Required for the Query class to be resolved
        services.AddTransient<Query>();

        // Adds services required by GraphQL
        services.AddGraphQL();
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        // Adds a graphql endpoint at /graphql
        app.UseGraphQL<Query>();
    }
}
```

## Motivation (for another .NET GraphQL Library) <a name="motivation"></a>
The creation of this library is motivated in the context of a comparison with the https://github.com/graphql-dotnet/graphql-dotnet library. The key driving bullets are as follows. 

A great GraphQL Library should:

* Use a convention based approach as the primary technique to define api:s
* Should be ready to go with ASP.NET Core (delivered with middleware)
* Should be delivered with an available/suggested authorization technique
* The library interface should be intuitive/easy to understand

## Dependencies

This library depends on the GraphQL query parser written by Marek Magdziak and released with a MIT license. Thank You!
