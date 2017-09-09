using System.Reflection;

namespace Cooke.GraphQL
{
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
}