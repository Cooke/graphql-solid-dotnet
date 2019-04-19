using System.Threading.Tasks;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL.AutoTests.IntegrationTests
{
    public class CasingFieldMiddleware
    {
        public async Task<object> Execute(FieldResolveContext context, FieldResolver next, string casing = "upper")
        {
            var result = (string)await next(context);
            
            if (casing == "upper")
            {
                return result.ToUpperInvariant();
            }
            else
            {
                return result.ToLowerInvariant();
            }
        }
    }
}