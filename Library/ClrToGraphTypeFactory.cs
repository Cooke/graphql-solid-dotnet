using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cooke.GraphQL.Types;
using Tests;

namespace Cooke.GraphQL
{
    public class ClrToGraphTypeFactory
    {
        private readonly SchemaBuilderOptions _options;
        private readonly IList<FieldEnhancer> _fieldEnhancers;
        private readonly Dictionary<TypeCacheKey, GraphType> _typeCache = new Dictionary<TypeCacheKey, GraphType>();

        private struct TypeCacheKey
        {
            public Type ClrType { get; set; }

            public bool IsInput { get; set; }

            private bool Equals(TypeCacheKey other)
            {
                return ClrType.Equals(other.ClrType) && IsInput == other.IsInput;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is TypeCacheKey && Equals((TypeCacheKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (ClrType.GetHashCode() * 397) ^ IsInput.GetHashCode();
                }
            }
        }

        public ClrToGraphTypeFactory(SchemaBuilderOptions options, IList<FieldEnhancer> fieldEnhancers)
        {
            _options = options;
            _fieldEnhancers = fieldEnhancers;
        }

        public GraphType CreateType(Type clrType, bool inputType = false)
        {
            clrType = TypeHelper.UnwrapTask(clrType);

            var typeCacheKey = new TypeCacheKey { ClrType = clrType, IsInput = inputType };
            if (_typeCache.ContainsKey(typeCacheKey))
            {
                return _typeCache[typeCacheKey];
            }

            if (TypeHelper.IsList(clrType))
            {
                var itemTYpe = clrType.GetTypeInfo().ImplementedInterfaces
                    .First(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                var listGraphType = new ListGraphType();
                _typeCache[typeCacheKey] = listGraphType;
                listGraphType.ItemType = CreateType(itemTYpe.GenericTypeArguments.Single(), inputType);
                return listGraphType;
            }

            if (clrType == typeof(string))
            {
                return StringGraphType.Instance;
            }

            if (clrType == typeof(int))
            {
                return new IntGraphType();
            }

            if (clrType.GetTypeInfo().IsEnum)
            {
                var enumType = new EnumGraphType(Enum.GetNames(clrType).Select(x => new EnumValue(x)), clrType);
                _typeCache[typeCacheKey] = enumType;
                return enumType;
            }

            if (clrType.GetTypeInfo().IsClass)
            {
                if (!inputType)
                {
                    var objectGraphType = new ObjectGraphType(clrType);
                    _typeCache[typeCacheKey] = objectGraphType;
                    objectGraphType.Fields = CreateFields(clrType);
                    return objectGraphType;
                }
                else
                {
                    var inputObjectGraphType = new InputObjectGraphType(clrType);
                    _typeCache[typeCacheKey] = inputObjectGraphType;
                    inputObjectGraphType.Fields = CreateInputFields(clrType);
                    return inputObjectGraphType;
                }
            }

            throw new NotSupportedException($"The given CLR type '{clrType}' is currently not supported.");
        }

        private Dictionary<string, GraphFieldInfo> CreateFields(Type clrType)
        {
            var propertyInfos = clrType.GetTypeInfo().DeclaredProperties.Where(x => x.GetMethod.IsPublic && !x.GetMethod.IsStatic);
            var fields = propertyInfos.Select(x => _fieldEnhancers.Aggregate(CreateFieldInfo(x), (f, e) => e(f)));

            var methodInfos = clrType.GetTypeInfo().DeclaredMethods.Where(x => x.IsPublic && !x.IsSpecialName && !x.IsStatic);
            fields = fields.Concat(methodInfos.Select(x => _fieldEnhancers.Aggregate(CreateFieldInfo(x, null), (f, e) => e(f))));
            return fields.ToDictionary(x => x.Name);
        }

        private Dictionary<string, GraphInputFieldInfo> CreateInputFields(Type clrType)
        {
            var propertyInfos = clrType.GetTypeInfo().DeclaredProperties.Where(x => x.GetMethod.IsPublic && x.SetMethod.IsPublic && !x.GetMethod.IsStatic);
            // TODO replace reflection set property with expression
            var fields = propertyInfos.Select(x => new GraphInputFieldInfo(_options.FieldNamingStrategy.ResolveFieldName(x), CreateType(x.PropertyType, true), x.SetValue));
            return fields.ToDictionary(x => x.Name);
        }

        private GraphFieldInfo CreateFieldInfo(PropertyInfo propertyInfo)
        {
            var type = CreateType(propertyInfo.PropertyType);

            async Task<object> Resolver(FieldResolveContext context)
            {
                // TODO replace with a compiled expression that gets the property value
                var result = propertyInfo.GetValue(context.Instance);
                return await UnwrapResult(result);
            }

            var graphFieldInfo = new GraphFieldInfo(_options.FieldNamingStrategy.ResolveFieldName(propertyInfo), type, Resolver, new GraphFieldArgumentInfo[0]);
            return graphFieldInfo
                .WithMetadataField(propertyInfo)
                .WithMetadataField((MemberInfo)propertyInfo);
        }

        public GraphFieldInfo CreateFieldInfo(MethodInfo methodInfo, FieldResolver next)
        {
            var type = CreateType(methodInfo.ReturnType);
            var skipParameterTypes = new[] { typeof(FieldResolveContext), typeof(FieldResolver) };
            var resolverParameters = methodInfo.GetParameters();
            var fieldParameters = resolverParameters.Where(x => !skipParameterTypes.Contains(x.ParameterType)).Select(CreateFieldArgument).ToArray();
            var fieldName = _options.FieldNamingStrategy.ResolveFieldName(methodInfo);

            async Task<object> Resolver(FieldResolveContext context)
            {
                // NOTE arguments have already been coerced outside of the resolve function
                var paramteters = resolverParameters.Select(arg => GetResolverParameterValue(context, next, arg)).ToArray();

                // TODO replace with a compiled expression that invokes the method
                var result = methodInfo.Invoke(context.Instance, paramteters);
                return await UnwrapResult(result);
            }

            return new GraphFieldInfo(fieldName, type, Resolver, fieldParameters)
                .WithMetadataField(methodInfo)
                .WithMetadataField((MemberInfo)methodInfo);
        }

        private static object GetResolverParameterValue(FieldResolveContext context, FieldResolver next, ParameterInfo arg)
        {
            if (arg.ParameterType == typeof(FieldResolveContext))
            {
                return context;
            }

            if (arg.ParameterType == typeof(FieldResolver))
            {
                return next;
            }

            return context.Args.ContainsKey(arg.Name) ? context.Args[arg.Name] : null;
        }

        public GraphFieldArgumentInfo CreateFieldArgument(ParameterInfo arg)
        {
            var argType = CreateType(arg.ParameterType, true);
            
            // TODO use a naming strategy 
            return new GraphFieldArgumentInfo(argType, arg.Name, arg.HasDefaultValue, arg.DefaultValue);
        }

        // TODO this is crazy, should NOT sync wait on results. Better wrap "untasked" results
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
}