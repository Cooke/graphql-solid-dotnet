using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Cooke.GraphQL.Annotations;
using Cooke.GraphQL.Introspection;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL
{
    public delegate GqlFieldInfo FieldEnhancer(GqlFieldInfo fieldInfo);

    public class SchemaBuilder
    {
        private readonly SchemaBuilderOptions _options;
        private readonly IList<FieldEnhancer> _fieldEnhancers = new List<FieldEnhancer>();
        private readonly Dictionary<string, GqlTypeBuilder> _types;
        private GqlTypeBuilder _queryType;
        private GqlTypeBuilder _mutationType;

        public SchemaBuilder(SchemaBuilderOptions options)
        {
            _options = options;

            _types = StandardScalars.All.ToDictionary(x => x.Name, x => (GqlTypeBuilder) new StandardGqlTypeBuilder(x));
        }

        public SchemaBuilder() : this(new SchemaBuilderOptions())
        {
        }

        public SchemaBuilderOptions Options => _options;

        public SchemaBuilder Query<T>()
        {
            _queryType = ObjectType<T>("Query");
            return this;
        }

        public SchemaBuilder UseMutation<T>()
        {
            _mutationType = ObjectType<T>("Mutation");
            return this;
        }

        public SchemaBuilder UseFieldEnhancer(FieldEnhancer fieldEnhancer)
        {
            _fieldEnhancers.Add(fieldEnhancer);
            return this;
        }

        public Schema Build()
        {
            var graphQueryType = _queryType.Build();
            var graphMutationType = _mutationType?.Build();
            return new Schema((GqlObjectType) graphQueryType, (GqlObjectType)graphMutationType, _types.Values);
        }

        // TODO change to data driven type creation and add possibility to register custom type factories
        private GqlObjectTypeBuilder<T> CreateType<T>(bool withinInputType = false)
        {
            var clrType = typeof(T);
            var name = _options.TypeNamingStrategy.ResolveTypeName(clrType);
            clrType = TypeHelper.UnwrapTask(clrType);

            if (_types.ContainsKey(typeCacheKey))
            {
                return _types[typeCacheKey];
            }

            if (clrType == typeof(bool))
            {
                _types[typeCacheKey] = BooleanType.Instance;
                return StringType.Instance;
            }

            if (clrType == typeof(string))
            {
                _types[typeCacheKey] = StringType.Instance;
                return StringType.Instance;
            }

            if (clrType == typeof(int))
            {
                _types[typeCacheKey] = IntType.Instance;
                return IntType.Instance;
            }

            if (clrType.GetTypeInfo().IsEnum)
            {
                var enumType = new GqlEnumType(Enum.GetNames(clrType).Select(x => new EnumValue(x)), clrType);
                _types[typeCacheKey] = enumType;
                return enumType;
            }

            if (TypeHelper.IsList(clrType))
            {
                var listEnumerableType = clrType.GetTypeInfo().ImplementedInterfaces.Concat(new[] { clrType })
                    .First(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                var listGraphType = new ListType();
                _types[typeCacheKey] = listGraphType;
                listGraphType.ItemType = CreateType(listEnumerableType.GenericTypeArguments.Single(), withinInputType);
                return listGraphType;
            }

            if (clrType.GetTypeInfo().IsClass)
            {
                var baseType = clrType.GetTypeInfo().BaseType;
                GqlType baseGraphType = null;
                if (baseType != typeof(object))
                {
                    baseGraphType = CreateType(baseType, withinInputType);
                }

                if (!withinInputType)
                {
                    GqlType resultGraphType;
                    if (clrType.GetTypeInfo().IsAbstract)
                    {
                        var interfaceGraphType = new InterfaceType(clrType);
                        _types[typeCacheKey] = interfaceGraphType;
                        interfaceGraphType.Fields = CreateFields(clrType);
                        resultGraphType = interfaceGraphType;
                    }
                    else
                    {
                        var objectGraphType = new GqlObjectType(clrType);
                        _types[typeCacheKey] = objectGraphType;
                        objectGraphType.Fields = CreateFields(clrType);

                        // TODO better support for interface types
                        objectGraphType.Interfaces = baseGraphType?.Kind == __TypeKind.Interface
                            ? new List<InterfaceType> {(InterfaceType) baseGraphType}.ToArray()
                            : new InterfaceType[0]; 
                        resultGraphType = objectGraphType;
                    }

                    foreach (var subType in clrType.GetTypeInfo().Assembly.DefinedTypes.Where(x => x.BaseType == clrType))
                    {
                        CreateType(subType.AsType());
                    }

                    return resultGraphType;
                }
                else
                {
                    var inputObjectGraphType = new InputObjectType(clrType);
                    _types[typeCacheKey] = inputObjectGraphType;
                    inputObjectGraphType.Fields = CreateInputFields(clrType);
                    return inputObjectGraphType;
                }
            }

            throw new NotSupportedException($"The given CLR type '{clrType}' is currently not supported.");
        }

        private Dictionary<string, GqlFieldInfo> CreateFields(Type clrType)
        {
            var propertyInfos = clrType.GetRuntimeProperties().Where(x => x.GetMethod.IsPublic && !x.GetMethod.IsStatic);
            var fields = propertyInfos.Select(x => _fieldEnhancers.Aggregate(CreateFieldInfo(x), (f, e) => e(f)));

            var methodInfos = clrType.GetTypeInfo().DeclaredMethods.Where(x => x.IsPublic && !x.IsSpecialName && !x.IsStatic);
            fields = fields.Concat(methodInfos.Select(x => _fieldEnhancers.Aggregate(CreateFieldInfo(x, null), (f, e) => e(f))));
            return fields.ToDictionary(x => x.Name);
        }

        private Dictionary<string, InputFieldDescriptor> CreateInputFields(Type clrType)
        {
            var propertyInfos = clrType.GetTypeInfo().DeclaredProperties.Where(x => x.GetMethod.IsPublic && x.SetMethod.IsPublic && !x.GetMethod.IsStatic);
            // TODO replace reflection set property with expression
            var fields = propertyInfos.Select(x => new InputFieldDescriptor(_options.FieldNamingStrategy.ResolveFieldName(x), CreateType(x.PropertyType, true), x.SetValue));
            return fields.ToDictionary(x => x.Name);
        }

        private GqlFieldInfo CreateFieldInfo(PropertyInfo propertyInfo)
        {
            var type = CreateType(propertyInfo.PropertyType);
            FieldResolver resolver = async context =>
            {
                // TODO replace with a compiled expression that gets the property value
                var result = propertyInfo.GetValue(context.Instance);
                return await UnwrapResult(result);
            };

            if (propertyInfo.GetCustomAttribute<NotNull>() != null)
            {
                type = new NonNullType {ItemType = type};
                var localResolver = resolver;
                resolver = async context =>
                {
                    var resolvedValue = await localResolver(context);
                    if (resolvedValue == null)
                    {
                        // TODO better exception
                        throw new NullReferenceException();
                    }

                    return resolvedValue;
                };
            }

            var graphFieldInfo = new GqlFieldInfo(_options.FieldNamingStrategy.ResolveFieldName(propertyInfo), type, resolver, new FieldArgumentDescriptor[0]);
            return graphFieldInfo
                .WithMetadataField(propertyInfo)
                .WithMetadataField((MemberInfo)propertyInfo);
        }

        private GqlFieldInfo CreateFieldInfo(MethodInfo methodInfo, FieldResolver next)
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

            return new GqlFieldInfo(fieldName, type, Resolver, fieldParameters)
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

            return context.Arguments.ContainsKey(arg.Name) ? context.Arguments[arg.Name] : null;
        }

        private FieldArgumentDescriptor CreateFieldArgument(ParameterInfo arg)
        {
            var argType = CreateType(arg.ParameterType, true);
            
            // TODO use a naming strategy 
            return new FieldArgumentDescriptor(argType, arg.Name, arg.HasDefaultValue, arg.DefaultValue);
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

        public GqlTypeBuilder ObjectType(string name, Action<GqlTypeBuilder> typeBuilderAction)
        {
            if (_types.TryGetValue(name, out var typeBuilder))
            {
                return typeBuilder;
            }

            typeBuilder = new GqlTypeBuilder(name);
            _types[name] = typeBuilder;
            typeBuilderAction?.Invoke(typeBuilder);
            return typeBuilder;
        }

        public GqlObjectTypeBuilder<T> ObjectType<T>(string name, Action<GqlObjectTypeBuilder<T>> typeBuilderAction)
        {
            if (_types.TryGetValue(name, out var builder))
            {
                if (!(builder is GqlObjectTypeBuilder<T> typedBuilder))
                {
                    throw new InvalidOperationException("The given type name has already been associated with another CLR type");
                }

                return typedBuilder;
            }

            var typeBuilder = new GqlObjectTypeBuilder<T>(name, this);
            _types[name] = typeBuilder;
            typeBuilderAction?.Invoke(typeBuilder);
            return typeBuilder;
        }

        public GqlObjectTypeBuilder<T> ObjectType<T>(Action<GqlObjectTypeBuilder<T>> typeBuilderAction)
        {
            var name = _options.TypeNamingStrategy.ResolveTypeName(typeof(T));
            return ObjectType<T>(name, null);
        }

        public GqlObjectTypeBuilder<T> ObjectType<T>(string name)
        {
            return ObjectType<T>(name, null);
        }

        public GqlObjectTypeBuilder<T> ObjectType<T>()
        {
            var name = _options.TypeNamingStrategy.ResolveTypeName(typeof(T));
            return ObjectType<T>(name, null);
        }
    }

    public class StandardGqlTypeBuilder : GqlTypeBuilder
    {
        private readonly GqlType _type;

        public StandardGqlTypeBuilder(GqlType type) : base(type.Name)
        {
            _type = type;
        }

        public override GqlType Build() => _type;
    }

    public class GqlObjectTypeBuilder<T> : GqlTypeBuilder
    {
        private readonly SchemaBuilder _schemaBuilder;

        public GqlObjectTypeBuilder(string name, SchemaBuilder schemaBuilder) : base(name)
        {
            _schemaBuilder = schemaBuilder;
        }

        public void Field<TReturn>(Expression<Func<T, TReturn>> expression)
        {
            if (!(expression.Body is MemberExpression memberExpression))
            {
                throw new ArgumentException("Only expressions starting with a member expression is valid.");
            }

            var fieldName = _schemaBuilder.Options.FieldNamingStrategy.ResolveFieldName(memberExpression.Member);
        }

        public override GqlType Build()
        {
            Func<Dictionary<string, GqlFieldInfo>> fieldsFunc = null;
            return new GqlObjectType(Name, fieldsFunc, new InterfaceType[0]);
        }
    }

    public class GqlObjectTypeBuilder : GqlTypeBuilder
    {
        private readonly SchemaBuilder _schemaBuilder;

        public GqlObjectTypeBuilder(string name, SchemaBuilder schemaBuilder) : base(name)
        {
            _schemaBuilder = schemaBuilder;
        }

        public void Field<TReturn>(string name)
        {
            var fieldBuilder = new FieldBuilder(name);
            _fields[name] = fieldBuilder;
            return fieldBuilder;
        }

        public override GqlType Build()
        {
            Func<Dictionary<string, GqlFieldInfo>> fieldsFunc = null;
            return new GqlObjectType(Name, fieldsFunc, new InterfaceType[0]);
        }
    }

    public class FieldBuilder
    {
        private readonly string _name;
        private readonly SchemaBuilder _schemaBuilder;
        private GqlTypeBuilder _returns;

        public FieldBuilder(string name, SchemaBuilder schemaBuilder)
        {
            _name = name;
            _schemaBuilder = schemaBuilder;
        }

        public FieldBuilder Returns(GqlTypeBuilder typeBuilder)
        {
            _returns = typeBuilder;
            return this;
        }

        public FieldBuilder Returns<T>()
        {
            _returns = _schemaBuilder.Type<T>();
        }
    }

    public abstract class GqlTypeBuilder
    {
        public GqlTypeBuilder(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public abstract GqlType Build();
    }
}