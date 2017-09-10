# GraphQL for .NET 
This is a convention based .NET library for building GraphQL endpoints. It is delivered with a middleware for ASP.NET Core and supports authorization out of the box.

This library is proof of concept and its existance is motivated below.  

## Installation

```Install-Package Cooke.GraphQL.AspNetCore```

or if no ASP.NET Core support is needed:

```Install-Package Cooke.GraphQL```

## Usage with ASP.NET Core

See samples for more in-depth examples or how to use the library without ASP.NET Core.

Define query class:
```C#
public class Query {
    // Dependency injection in query constructor
    public Query(MyDbContext context) {
        this.context = context;
    }

    public IEnumerable<MyApiModel> Models => context.Models.Select(x => new MyApiModel(x)).ToArrayAsync();

    // Support for AspNetCore Authroization Attribute
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

Enable a GraphQL endpoint and service in StartUp:
```C#
public class TestStartup
        {
            public virtual void ConfigureServices(IServiceCollection services)
            {
                // Required for the Query to be resolved
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

## Motivation (for another .NET GraphQL Library)
The creation of this library is motivated in the context of a comparison with the https://github.com/graphql-dotnet/graphql-dotnet library (the rival). The biggest motivation is to trigger a discussion within the rival and hopefully make it pivot according to the bullets below.  

A great GraphQL Library should:

* Use a convention based approach as the primary technique to define api:s
* Should be ready to go with ASP.NET Core (delivered with middleware)
* Should be delivered with an available/suggested authorization technique
* Should return good error messages

## Dependencies

This library depends on the GraphQL query parser written by Marek Magdziak and released with a MIT license. Thank You!