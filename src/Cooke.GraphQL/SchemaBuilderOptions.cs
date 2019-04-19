using System;
using System.Reflection;

namespace Cooke.GraphQL
{
    public class SchemaBuilderOptions
    {
        public IFieldNamingStrategy FieldNamingStrategy { get; set; } = new CamelCaseFieldNamingStrategy();
        public ITypeNamingStrategy TypeNamingStrategy { get; set; } = new SameNameTypeNameStrategy();
    }

    public class SameNameTypeNameStrategy : ITypeNamingStrategy
    {
        public string ResolveTypeName(Type typeInfo)
        {
            return typeInfo.Name;
        }
    }

    public interface ITypeNamingStrategy
    {
        string ResolveTypeName(Type typeInfo);
    }
}