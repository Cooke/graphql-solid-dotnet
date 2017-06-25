using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cooke.GraphQL;
using Cooke.GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Tests
{
    public class AuthorizationFieldMiddleware : QueryExecutor.IMiddleware
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthorizationFieldMiddleware(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public Task<object> Resolve(FieldResolveContext context, FieldResolver next)
        {
            // TODO Use AspNetCore authorization here

            var authorizeAttribute = context.FieldInfo.GetMetadata<List<Attribute>>()?.OfType<AuthorizeAttribute>().FirstOrDefault();
            if (authorizeAttribute != null && !_contextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                throw new FieldErrorException("Access denied");
            }

            return next(context);
        }
    }
}