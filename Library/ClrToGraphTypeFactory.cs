using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Tests
{
    public class ClrToGraphTypeFactory
    {
        private readonly SchemaBuilderOptions _options;

        public ClrToGraphTypeFactory(SchemaBuilderOptions options)
        {
            _options = options;
        }

        public GraphType CreateType(Type clrType)
        {
            clrType = TypeHelper.UnwrapTask(clrType);

            if (TypeHelper.IsList(clrType))
            {
                var itemTYpe = clrType.GetTypeInfo().ImplementedInterfaces
                    .First(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                return new ListGraphType(clrType, CreateType(itemTYpe.GenericTypeArguments.Single()));
            }
            else if (clrType.GetTypeInfo().IsPrimitive || clrType == typeof(string))
            {
                return new PrimitiveGraphType(clrType);
            }
            else
            {
                var fields = CreateFields(clrType);
                return new ObjectGraphType(clrType, fields);
            }
        }

        private Dictionary<string, GraphFieldInfo> CreateFields(Type clrType)
        {
            var memberInfos = clrType.GetTypeInfo().DeclaredProperties.Where(x => x.GetMethod.IsPublic);
            var fields = memberInfos.Select(CreateFieldInfo).ToDictionary(x => x.Name);
            return fields;
        }

        private GraphFieldInfo CreateFieldInfo(MemberInfo memberInfo)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                var type = CreateType(propertyInfo.PropertyType);

                Func<object, Task<object>> resolver = async x =>
                {
                    var result = propertyInfo.GetValue(x);
                    var task = result as Task;
                    if (task != null)
                    {
                        await task;
                        // TODO investigate if using expressions directly would be more performant
                        dynamic dynamicTask = task;
                        return dynamicTask.Result;
                    }

                    return result;
                };

                return new GraphFieldInfo(_options.NamingStrategy.ResolveFieldName(propertyInfo), type, resolver);
            }

            throw new NotSupportedException();
        }
    }
}