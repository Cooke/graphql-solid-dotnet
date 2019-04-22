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
        private readonly Dictionary<string, IGqlTypeBuilder> _typeBuilders = new Dictionary<string, IGqlTypeBuilder>();
        private IGqlTypeBuilder _queryType;
        private IGqlTypeBuilder _mutationType;

        public SchemaBuilder(SchemaBuilderOptions options)
        {
            _options = options;

            // _types = StandardScalars.All.ToDictionary(x => x.Name, x => (GqlTypeReference) new BuiltFutureType(x));
        }

        public SchemaBuilder() : this(new SchemaBuilderOptions())
        {
        }

        public SchemaBuilderOptions Options => _options;

        public SchemaBuilder Query<T>()
        {
            _queryType = DefineType<object>("Query", type => type.IncludeResolverType<T>());
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
            return new Schema((GqlObjectType) graphQueryType, (GqlObjectType) graphMutationType);
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

        public ObjectTypeBuilder<T> DefineType<T>(string name, Action<ObjectTypeBuilder<T>> typeBuilderAction)
        {
            if (_typeBuilders.ContainsKey(name))
            {
                throw new ArgumentException($"A type with name '{name}' has already been defined");
            }

            var typeBuilder = new ObjectTypeBuilder<T>(name, this);
            _typeBuilders[name] = typeBuilder;
            typeBuilderAction?.Invoke(typeBuilder);
            return typeBuilder;
        }

        public ObjectTypeBuilder<T> DefineType<T>(Action<ObjectTypeBuilder<T>> typeBuilderAction)
        {
            var name = _options.TypeNamingStrategy.ResolveTypeName(typeof(T));
            return DefineType<T>(name, null);
        }

        public ObjectTypeBuilder<T> DefineType<T>(string name)
        {
            return DefineType<T>(name, null);
        }

        public ObjectTypeBuilder<T> DefineType<T>()
        {
            var name = _options.TypeNamingStrategy.ResolveTypeName(typeof(T));
            return DefineType<T>(name, null);
        }

        public IGqlTypeReference TypeRef<TReturn>()
        {
            return new TypeReferenceBuilder(_options.NonNullDefault).Type<TReturn>();
        }
    }

    public interface IGqlTypeBuilder
    {
        GqlType Build();
    }

    public class ObjectTypeBuilder<TObject> : IGqlTypeBuilder
    {
        private readonly SchemaBuilder _schemaBuilder;
        private readonly Dictionary<string, IFieldBuilder> _fields = new Dictionary<string, IFieldBuilder>();

        public ObjectTypeBuilder(string name, SchemaBuilder schemaBuilder)
        {
            Name = name;
            _schemaBuilder = schemaBuilder;
        }

        public string Name { get; }

        public FieldBuilder DefineField<TReturn>(Expression<Func<TObject, TReturn>> expression)
        {
            if (!(expression.Body is MemberExpression memberExpression))
            {
                throw new ArgumentException("Only expressions starting with a member expression is valid.");
            }

            var fieldName = _schemaBuilder.Options.FieldNamingStrategy.ResolveFieldName(memberExpression.Member);
            var resolver = expression.Compile();
            return DefineField(fieldName, ctx => resolver(ctx.Instance));
        }

        public FieldBuilder DefineField<TReturn>(string name, Func<FieldResolveContext<TObject>, TReturn> resolver)
        {
            if (_fields.ContainsKey(name))
            {
                throw new ArgumentException($"A field with name '{name}' has already been defined");
            }

            return new FieldBuilder(
                name,
                (FieldResolveContext context) => Task.FromResult((object)resolver((FieldResolveContext<TObject>) context)),
                _schemaBuilder.TypeRef<TReturn>());
        }

        public GqlType Build(BuildContext buildContext)
        {
            return new GqlObjectType(Name, () => _fields.ToDictionary(x => x.Key, x => x.Value.Build(buildContext)),
                new InterfaceType[0]);
        }

        private ObjectTypeBuilder<T> CreateType<T>()
        {
            var clrType = TypeHelper.UnwrapTask(typeof(T));

            if (TypeHelper.IsList(clrType))
            {
                var listEnumerableType = clrType.GetTypeInfo().ImplementedInterfaces.Concat(new[] {clrType})
                    .First(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                var listGraphType = new GqlListType
                {
                    ItemType = CreateType<T>(listEnumerableType.GenericTypeArguments.Single(), withinInputType)
                };
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

                    foreach (var subType in clrType.GetTypeInfo().Assembly.DefinedTypes
                        .Where(x => x.BaseType == clrType))
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
            var propertyInfos =
                clrType.GetRuntimeProperties().Where(x => x.GetMethod.IsPublic && !x.GetMethod.IsStatic);
            var fields = propertyInfos.Select(x => _fieldEnhancers.Aggregate(CreateFieldInfo(x), (f, e) => e(f)));

            var methodInfos = clrType.GetTypeInfo().DeclaredMethods
                .Where(x => x.IsPublic && !x.IsSpecialName && !x.IsStatic);
            fields = fields.Concat(methodInfos.Select(x =>
                _fieldEnhancers.Aggregate(CreateFieldInfo(x, null), (f, e) => e(f))));
            return fields.ToDictionary(x => x.Name);
        }

        private Dictionary<string, InputFieldDescriptor> CreateInputFields(Type clrType)
        {
            var propertyInfos = clrType.GetTypeInfo().DeclaredProperties.Where(x =>
                x.GetMethod.IsPublic && x.SetMethod.IsPublic && !x.GetMethod.IsStatic);
            // TODO replace reflection set property with expression
            var fields = propertyInfos.Select(x =>
                new InputFieldDescriptor(_options.FieldNamingStrategy.ResolveFieldName(x),
                    CreateType(x.PropertyType, true), x.SetValue));
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

            var graphFieldInfo = new GqlFieldInfo(_options.FieldNamingStrategy.ResolveFieldName(propertyInfo), type,
                resolver, new FieldArgumentDescriptor[0]);
            return graphFieldInfo
                .WithMetadataField(propertyInfo)
                .WithMetadataField((MemberInfo) propertyInfo);
        }

        private GqlFieldInfo CreateFieldInfo(MethodInfo methodInfo, FieldResolver next)
        {
            var type = CreateType(methodInfo.ReturnType);
            var skipParameterTypes = new[] {typeof(FieldResolveContext), typeof(FieldResolver)};
            var resolverParameters = methodInfo.GetParameters();
            var fieldParameters = resolverParameters.Where(x => !skipParameterTypes.Contains(x.ParameterType))
                .Select(CreateFieldArgument).ToArray();
            var fieldName = _options.FieldNamingStrategy.ResolveFieldName(methodInfo);

            async Task<object> Resolver(FieldResolveContext context)
            {
                // NOTE arguments have already been coerced outside of the resolve function
                var paramteters = resolverParameters.Select(arg => GetResolverParameterValue(context, next, arg))
                    .ToArray();

                // TODO replace with a compiled expression that invokes the method
                var result = methodInfo.Invoke(context.Instance, paramteters);
                return await UnwrapResult(result);
            }

            return new GqlFieldInfo(fieldName, type, Resolver, fieldParameters)
                .WithMetadataField(methodInfo)
                .WithMetadataField((MemberInfo) methodInfo);
        }

        private static object GetResolverParameterValue(FieldResolveContext context, FieldResolver next,
            ParameterInfo arg)
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

        public void IncludeResolverType<T1>()
        {
            throw new NotImplementedException();
        }
    }

    public interface IFieldBuilder
    {
        GqlFieldInfo Build(BuildContext context);
    }

    public class FieldBuilder : IFieldBuilder
    {
        private readonly string _name;
        private readonly FieldResolver _resolver;
        private IGqlTypeReference _type;
        private readonly List<(string, IGqlTypeReference)> _arguments = new List<(string, IGqlTypeReference)>();

        public FieldBuilder(string name, FieldResolver resolver, IGqlTypeReference type)
        {
            _name = name;
            _resolver = resolver;
            _type = type;
        }

        public GqlFieldInfo Build(BuildContext buildContext)
        {
            return new GqlFieldInfo(_name, _type.Resolve(buildContext),
                _resolver, _arguments.Select(x => new FieldArgumentDescriptor(x.Item2.Resolve(buildContext))));
        }

        public void Type(Func<TypeReferenceBuilder, IGqlTypeReference> builderFunc)
        {
            _type = builderFunc(new TypeReferenceBuilder(false));
        }

        public void Arguments(params (string name, Func<TypeReferenceBuilder, IGqlTypeReference> config)[] args)
        {
            _arguments.AddRange(args.Select(x => (x.name, x.config(new TypeReferenceBuilder(false)))));   
        }
    }

    public class TypeReferenceBuilder
    {
        private readonly Stack<WrapType> _wrapTypeStack = new Stack<WrapType>();

        private enum WrapType
        {
            NonNull,
            Nullable,
            List
        }

        public TypeReferenceBuilder(bool defaultNonNull)
        {
        }

        public TypeReferenceBuilder NonNull
        {
            get
            {
                _wrapTypeStack.Push(WrapType.NonNull);
                return this;
            }
        }

        public TypeReferenceBuilder ListOf
        {
            get
            {
                _wrapTypeStack.Push(WrapType.List);
                return this;
            }
        }

        public TypeReferenceBuilder Nullable
        {
            get
            {
                _wrapTypeStack.Push(WrapType.Nullable);
                return this;
            }
        }

        public IGqlTypeReference Type<T>()
        {
            IGqlTypeReference innerTypeRef = new NamedGqlTypeReference(typeof(T));
            return Wrap(innerTypeRef);
        }

        public IGqlTypeReference String => Wrap(new NamedGqlTypeReference(StandardScalars.String));

        public IGqlTypeReference Type(string gqlTypeName) => Wrap(new NamedGqlTypeReference(gqlTypeName));

        private IGqlTypeReference Wrap(IGqlTypeReference innerTypeRef)
        {
            return _wrapTypeStack.Reverse().Aggregate(innerTypeRef, (inner, type) =>
            {
                switch (type)
                {
                    case WrapType.NonNull:
                        return new NonNullGqlTypeReference(innerTypeRef);

                    case WrapType.Nullable:
                        return innerTypeRef;

                    case WrapType.List:
                        return new ListGqlTypeReference(innerTypeRef);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            });
        }
    }

    public class BuildContext
    {
        public IReadOnlyCollection<GqlType> Types { get; set; }
    }

    public interface IGqlTypeReference
    {
        GqlType Resolve(BuildContext context);
    }

    public class NonNullGqlTypeReference : IGqlTypeReference
    {
        private readonly IGqlTypeReference _typeReference;

        public NonNullGqlTypeReference(IGqlTypeReference typeReference)
        {
            _typeReference = typeReference;
        }

        public GqlType Resolve(BuildContext context)
        {
            return new NonNullType(_typeReference.Resolve(context));
        }
    }

    public class ListGqlTypeReference : IGqlTypeReference
    {
        private readonly IGqlTypeReference _typeReference;

        public ListGqlTypeReference(IGqlTypeReference typeReference)
        {
            _typeReference = typeReference;
        }

        public GqlType Resolve(BuildContext context)
        {
            return new GqlListType(_typeReference.Resolve(context));
        }
    }

    public class NamedGqlTypeReference : IGqlTypeReference
    {
        public NamedGqlTypeReference(string name)
        {
            Name = name;
        }

        public NamedGqlTypeReference(GqlType gqlType)
        {
            GqlType = gqlType;
        }

        public NamedGqlTypeReference(Type clrType)
        {
            ClrType = clrType;
        }

        public Type ClrType { get; set; }

        public string Name { get; }

        public GqlType GqlType { get; }

        public GqlType Resolve(BuildContext context)
        {
            if (ClrType != null)
            {
                var matchingTypes = context.Types
                    .Where(x => x.ClrType.GetTypeInfo().IsAssignableFrom(ClrType.GetTypeInfo())).ToArray();
                if (matchingTypes.Length == 0)
                {
                    throw new GqlTypeException(
                        $"Could not find any matching GraphQL type for the CLR type '{ClrType}'");
                }
                else if (matchingTypes.Length > 1)
                {
                    throw new GqlTypeException(
                        $"Found more than one matching GraphQL type for the CLR type '{ClrType}'");
                }

                return matchingTypes.First();
            }
            else if (Name != null)
            {
                var matchingTypes = context.Types.Where(x => x.Name == Name).ToArray();
                if (matchingTypes.Length == 0)
                {
                    throw new GqlTypeException(
                        $"Could not find any matching GraphQL type with the name '{Name}'");
                }

                return matchingTypes.First();
            }
            else
            {
                return GqlType;
            }
        }

        //private GqlType ResolveClrType(BuildContext context)
        //{
        //    if (TypeHelper.IsList(ClrType))
        //    {
        //        var listEnumerableType = ClrType.GetTypeInfo().ImplementedInterfaces.Concat(new[] { ClrType })
        //            .First(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        //        var listGraphType = new GqlListType
        //        {
        //            ItemType = CreateType<T>(listEnumerableType.GenericTypeArguments.Single(), withinInputType)
        //        };
        //        return listGraphType;
        //    }
        //}
    }

    public class GqlTypeException : Exception
    {
        public GqlTypeException(string message)
        {
        }
    }
}