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
            else if (clrType == typeof(string))
            {
                return new StringGraphType(clrType);
            }
            else if (clrType == typeof(int))
            {
                return new IntGraphType(clrType);
            }
            else if (clrType.GetTypeInfo().IsClass)
            {
                var fields = CreateFields(clrType);
                return new ObjectGraphType(clrType, fields);
            }
            else
            {
                throw new NotSupportedException($"The given CLR type '{clrType}' is currently not supported.");
            }
        }

        private Dictionary<string, GraphFieldInfo> CreateFields(Type clrType)
        {
            var propertyInfos = clrType.GetTypeInfo().DeclaredProperties.Where(x => x.GetMethod.IsPublic && !x.GetMethod.IsStatic);
            var fields = propertyInfos.Select(CreateFieldInfo);

            var methodInfos = clrType.GetTypeInfo().DeclaredMethods.Where(x => x.IsPublic && !x.IsSpecialName && !x.IsStatic);
            fields = fields.Concat(methodInfos.Select(CreateFieldInfo));
            return fields.ToDictionary(x => x.Name);
        }

        private GraphFieldInfo CreateFieldInfo(PropertyInfo propertyInfo)
        {
            var type = CreateType(propertyInfo.PropertyType);

            Func<object, IDictionary<string, object>, Task<object>> resolver = async (x, args) =>
            {
                // TODO replace with a compiled expression that gets the property value
                var result = propertyInfo.GetValue(x);
                return await UnwrapResult(result);
            };

            return new GraphFieldInfo(_options.NamingStrategy.ResolveFieldName(propertyInfo), type, resolver, new FieldArgumentInfo[0]);
        }

        private GraphFieldInfo CreateFieldInfo(MethodInfo methodInfo)
        {
            var type = CreateType(methodInfo.ReturnType);
            var arguments = methodInfo.GetParameters().Select(CreateFieldArgument).ToArray();

            Func<object, IDictionary<string, object>, Task<object>> resolver = async (obj, args) =>
            {
                // NOTE arguments have already been coerced outside of the resolve function
                var paramteters = arguments.Select(arg => args.ContainsKey(arg.Name) ? args[arg.Name] : null).ToArray();

                // TODO replace with a compiled expression that invokes the method
                var result = methodInfo.Invoke(obj, paramteters);
                return await UnwrapResult(result);
            };

            return new GraphFieldInfo(_options.NamingStrategy.ResolveFieldName(methodInfo), type, resolver, arguments);
        }

        private FieldArgumentInfo CreateFieldArgument(ParameterInfo arg)
        {
            var argType = CreateType(arg.ParameterType);
            
            // TODO use a naming strategy 
            return new FieldArgumentInfo(argType, arg.Name, arg.HasDefaultValue, arg.DefaultValue);
        }

        private static async Task<object> UnwrapResult(object result)
        {
            var task = result as Task;
            if (task != null)
            {
                await task;
                // TODO investigate if using expressions directly would be more performant
                dynamic dynamicTask = task;
                return dynamicTask.Result;
            }

            return result;
        }
    }

    public class FieldArgumentInfo
    {
        public string Name { get; }

        public bool HasDefaultValue { get; }

        public object DefaultValue { get; }

        public GraphType Type { get; }

        public FieldArgumentInfo(GraphType type, string name, bool hasDefaultValue, object defaultValue)
        {
            Type = type;
            Name = name;
            HasDefaultValue = hasDefaultValue;
            DefaultValue = defaultValue;
        }
    }
}