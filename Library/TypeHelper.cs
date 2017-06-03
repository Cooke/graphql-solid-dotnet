using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Tests
{
    public class TypeHelper
    {
        public static bool IsList(Type clrType)
        {
            return typeof(IEnumerable<object>).GetTypeInfo().IsAssignableFrom(clrType.GetTypeInfo());
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