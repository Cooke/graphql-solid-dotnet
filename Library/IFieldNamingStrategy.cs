using System.Reflection;

namespace Tests
{
    public interface IFieldNamingStrategy
    {
        string ResolveFieldName(MemberInfo memberInfo);
    }
}