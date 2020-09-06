using System;

namespace SqlBuilderFramework
{
    /// <summary>
    /// Schnittstelle für die Bestandteile einer SQL-Bedingung für den QueryCommander.
    /// </summary>
    interface IConditionPart { }

    class ColumnValue : IConditionPart
    {
        public string Name { get; set; }
        public Comparison Comparison { get; set; }
        public object Value { get; set; }
        public bool IsNull { get; set; }
        public bool IsNotNull { get; set; }
        public Type Type { get; set; }
    }

    class Operator : IConditionPart
    {
        public string Value { get; private set; }

        public static readonly Operator And = new Operator { Value = "AND" };
        public static readonly Operator Or = new Operator { Value = "OR" };
    }

    class Paranthesis : IConditionPart
    {
        public static readonly Paranthesis Left = new Paranthesis();
        public static readonly Paranthesis Right = new Paranthesis();
    }

    class JoinPart : IConditionPart
    {
        public string LeftColumn { get; set; }
        public string RightColumn { get; set; }
    }

    class InPart : IConditionPart
    {
        public string Column { get; set; }
        public string Sql { get; set; }
    }

    class SqlPart : IConditionPart
    {
        public string Sql { get; set; }
    }
}
