using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Cooke.GraphQL;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL.AspNetCore
{
    public class AuthorizationFieldMiddleware : QueryExecutor.IMiddleware
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAuthorizationPolicyProvider _policyProvider;
        private readonly IAuthorizationService _authorizationService;

        public AuthorizationFieldMiddleware(IHttpContextAccessor contextAccessor, IAuthorizationPolicyProvider policyProvider, IAuthorizationService authorizationService)
        {
            _contextAccessor = contextAccessor;
            _policyProvider = policyProvider;
            _authorizationService = authorizationService;
        }

        public async Task<object> Resolve(FieldResolveContext context, FieldResolver next)
        {
            // TODO add support for authorize attribute on type 
            var authorizeAttribute = context.FieldInfo.GetMetadata<List<Attribute>>()?.OfType<AuthorizeAttribute>().FirstOrDefault();

            if (authorizeAttribute != null)
            {
                var policy = await AuthorizationPolicy.CombineAsync(_policyProvider, new [] { authorizeAttribute });
                var authResult = await _authorizationService.AuthorizeAsync(_contextAccessor.HttpContext.User, null, policy);
                if (!authResult.Succeeded)
                {
                    throw new FieldErrorException("Access denied");
                }
            }

            return await next(context);
        }
    }
}