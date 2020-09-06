using System;

namespace SqlBuilderFramework
{
    public struct ParameterEntity
    {
        public string Name { get; private set; }
        public Type Type { get; private set; }
        public object Value { get; private set; }

        public ParameterEntity(string name, Type type, object value) : this()
        {
            Name = name;
            Type = type;
            Value = value;
        }
    }
}
