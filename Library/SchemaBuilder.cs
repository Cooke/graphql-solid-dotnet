using System;
using System.Reflection;

namespace Tests
{
    public class SchemaBuilderOptions
    {
        public IFieldNamingStrategy NamingStrategy { get; set; } = new CamelCaseFieldNamingStrategy();
    }

    public interface IFieldNamingStrategy
    {
        string ResolveFieldName(MemberInfo memberInfo);
    }

    class CamelCaseFieldNamingStrategy : IFieldNamingStrategy
    {
        public string ResolveFieldName(MemberInfo memberInfo)
        {
            var fieldName = memberInfo.Name;
            if (string.IsNullOrEmpty(fieldName) || !char.IsUpper(fieldName[0]))
            {
                return fieldName;
            }

            char[] charArray = fieldName.ToCharArray();
            charArray[0] = char.ToLowerInvariant(charArray[0]);
            return new string(charArray);
        }
    }

    public class SchemaBuilder
    {
        private readonly SchemaBuilderOptions _options;
        private Type _queryType;

        public SchemaBuilder(SchemaBuilderOptions options)
        {
            _options = options;
        }

        public SchemaBuilder() : this(new SchemaBuilderOptions())
        {
        }

        public SchemaBuilder UseQuery<T>()
        {
            _queryType = typeof(T);
            return this;
        }

        public Schema Build()
        {
            var factory = new ClrToGraphTypeFactory(_options);
            var graphType = factory.CreateType(_queryType);
            return new Schema((ObjectGraphType) graphType);
        }
    }
}