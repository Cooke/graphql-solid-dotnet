using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace Cooke.GraphQL
{
    public class TypeHelper
    {
        public static bool IsList(Type clrType)
        {
            return clrType.GetTypeInfo().ImplementedInterfaces.Concat(new[] { clrType })
                .Any(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        public static Type UnwrapTask(Type clrType)
        {
            if (typeof(Task).GetTypeInfo().IsAssignableFrom(clrType.GetTypeInfo()))
            {
                clrType = clrType.GenericTypeArguments.Single();
            }
            return clrType;
        }
    }
}