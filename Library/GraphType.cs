using System;

namespace Tests
{
    public abstract class GraphType
    {
        private readonly Type _clrType;
        
        public GraphType(Type clrType)
        {
            _clrType = clrType;
        }
    }
}