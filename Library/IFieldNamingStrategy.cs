using System.Reflection;

namespace Cooke.GraphQL
{
    public interface IFieldNamingStrategy
    {
        string ResolveFieldName(MemberInfo memberInfo);
    }
}