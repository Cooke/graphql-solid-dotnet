using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL
{
    public static class AttributeMetadataFieldEnhancer
    {
        public static SchemaBuilder UseAttributeMetadata(this SchemaBuilder builder)
        {
            return builder.UseFieldEnhancer(AddAttributeMetadata);
        }

        private static GraphFieldInfo AddAttributeMetadata(GraphFieldInfo fieldInfo)
        {
            List<Attribute> customAttributes = fieldInfo.GetMetadata<MemberInfo>().GetCustomAttributes().ToList();
            return fieldInfo.WithMetadataField(customAttributes);
        }
    }
}