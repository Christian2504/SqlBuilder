using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqlBuilderFramework
{
    public interface IDbColumnImpl
    {
        string Alias { get; }
        Type Type { get; set; }
        DbTable Table { get; }
        bool IsAggregate { get; }
        bool IsGroupable { get; }
        string Sql(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database);
        string Definition(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database);
    }

    public class DbColumn
    {
        protected bool Equals(DbColumn other)
        {
            return Equals(DbColumnImpl, other.DbColumnImpl);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DbColumn) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (DbColumnImpl != null ? DbColumnImpl.GetHashCode() : 0) * 397;
            }
        }

        protected readonly IDbColumnImpl DbColumnImpl;

        public string Alias => DbColumnImpl?.Alias;
        public Type Type => DbColumnImpl?.Type;
        public bool IsAggregate => DbColumnImpl?.IsAggregate ?? false;
        public bool IsGroupable => DbColumnImpl?.IsGroupable ?? false;

        public DbBoundValue BoundValue { get; set; }

        public string TableColumnName
        {
            get
            {
                if (DbColumnImpl is DbTableColumn column)
                {
                    return column.Name;
                }

                return string.Empty;
            }
        }

        public DbColumn(IDbColumnImpl dbColumnImpl)
        {
            DbColumnImpl = dbColumnImpl;
        }

        //
        // Comparison operators
        //

        public static DbConstraint operator ==(DbColumn left, DbColumn right)
        {
            return new DbConstraint(left, "=", right);
        }

        public static DbConstraint operator !=(DbColumn left, DbColumn right)
        {
            return new DbConstraint(left, "<>", right);
        }

        public static DbConstraint operator ==(DbColumn left, bool right)
        {
            return new DbConstraint(left, "=", new DbColumn(new DbValueColumn<bool>(right)));
        }

        public static DbConstraint operator !=(DbColumn left, bool right)
        {
            return new DbConstraint(left, "<>", new DbColumn(new DbValueColumn<bool>(right)));
        }

        public static DbConstraint operator ==(DbColumn left, int right)
        {
            return new DbConstraint(left, "=", new DbColumn(new DbValueColumn<int>(right)));
        }

        public static DbConstraint operator !=(DbColumn left, int right)
        {
            return new DbConstraint(left, "<>", new DbColumn(new DbValueColumn<int>(right)));
        }

        public static DbConstraint operator ==(DbColumn left, long right)
        {
            return new DbConstraint(left, "=", new DbColumn(new DbValueColumn<long>(right)));
        }

        public static DbConstraint operator !=(DbColumn left, long right)
        {
            return new DbConstraint(left, "<>", new DbColumn(new DbValueColumn<long>(right)));
        }

        public static DbConstraint operator ==(DbColumn left, decimal right)
        {
            return new DbConstraint(left, "=", new DbColumn(new DbValueColumn<decimal>(right)));
        }

        public static DbConstraint operator !=(DbColumn left, decimal right)
        {
            return new DbConstraint(left, "<>", new DbColumn(new DbValueColumn<decimal>(right)));
        }

        public static DbConstraint operator ==(DbColumn left, DateTime right)
        {
            return new DbConstraint(left, "=", new DbColumn(new DbValueColumn<DateTime>(right)));
        }

        public static DbConstraint operator !=(DbColumn left, DateTime right)
        {
            return new DbConstraint(left, "<>", new DbColumn(new DbValueColumn<DateTime>(right)));
        }

        public static DbConstraint operator ==(DbColumn left, string right)
        {
            return new DbConstraint(left, "=", new DbColumn(new DbValueColumn<string>(right)));
        }

        public static DbConstraint operator !=(DbColumn left, string right)
        {
            return new DbConstraint(left, "<>", new DbColumn(new DbValueColumn<string>(right)));
        }

        public static DbConstraint operator ==(DbColumn left, byte[] right)
        {
            return new DbConstraint(left, "=", new DbColumn(new DbValueColumn<byte[]>(right)));
        }

        public static DbConstraint operator !=(DbColumn left, byte[] right)
        {
            return new DbConstraint(left, "<>", new DbColumn(new DbValueColumn<byte[]>(right)));
        }

        public static DbConstraint operator ==(DbColumn left, Guid right)
        {
            return new DbConstraint(left, "=", new DbColumn(new DbValueColumn<Guid>(right)));
        }

        public static DbConstraint operator !=(DbColumn left, Guid right)
        {
            return new DbConstraint(left, "<>", new DbColumn(new DbValueColumn<Guid>(right)));
        }

        public DbConstraint IsNull()
        {
            return new DbConstraint(this, "=", new DbColumn(new DbValueColumn<object>(null)));
        }

        public DbConstraint IsNotNull()
        {
            return new DbConstraint(this, "<>", new DbColumn(new DbValueColumn<object>(null)));
        }

        public static DbConstraint operator <(DbColumn left, DbColumn right)
        {
            return new DbConstraint(left, "<", right);
        }

        public static DbConstraint operator >(DbColumn left, DbColumn right)
        {
            return new DbConstraint(left, ">", right);
        }

        public static DbConstraint operator <(DbColumn left, bool right)
        {
            return new DbConstraint(left, "<", new DbColumn(new DbValueColumn<bool>(right)));
        }

        public static DbConstraint operator >(DbColumn left, bool right)
        {
            return new DbConstraint(left, ">", new DbColumn(new DbValueColumn<bool>(right)));
        }

        public static DbConstraint operator <(DbColumn left, int right)
        {
            return new DbConstraint(left, "<", new DbColumn(new DbValueColumn<int>(right)));
        }

        public static DbConstraint operator >(DbColumn left, int right)
        {
            return new DbConstraint(left, ">", new DbColumn(new DbValueColumn<int>(right)));
        }

        public static DbConstraint operator <(DbColumn left, long right)
        {
            return new DbConstraint(left, "<", new DbColumn(new DbValueColumn<long>(right)));
        }

        public static DbConstraint operator >(DbColumn left, long right)
        {
            return new DbConstraint(left, ">", new DbColumn(new DbValueColumn<long>(right)));
        }

        public static DbConstraint operator <(DbColumn left, decimal right)
        {
            return new DbConstraint(left, "<", new DbColumn(new DbValueColumn<decimal>(right)));
        }

        public static DbConstraint operator >(DbColumn left, decimal right)
        {
            return new DbConstraint(left, ">", new DbColumn(new DbValueColumn<decimal>(right)));
        }

        public static DbConstraint operator <(DbColumn left, DateTime right)
        {
            return new DbConstraint(left, "<", new DbColumn(new DbValueColumn<DateTime>(right)));
        }

        public static DbConstraint operator >(DbColumn left, DateTime right)
        {
            return new DbConstraint(left, ">", new DbColumn(new DbValueColumn<DateTime>(right)));
        }

        public static DbConstraint operator <(DbColumn left, string right)
        {
            return new DbConstraint(left, "<", new DbColumn(new DbValueColumn<string>(right)));
        }

        public static DbConstraint operator >(DbColumn left, string right)
        {
            return new DbConstraint(left, ">", new DbColumn(new DbValueColumn<string>(right)));
        }

        public static DbConstraint operator <(DbColumn left, byte[] right)
        {
            return new DbConstraint(left, "<", new DbColumn(new DbValueColumn<byte[]>(right)));
        }

        public static DbConstraint operator >(DbColumn left, byte[] right)
        {
            return new DbConstraint(left, ">", new DbColumn(new DbValueColumn<byte[]>(right)));
        }

        public static DbConstraint operator <=(DbColumn left, DbColumn right)
        {
            return new DbConstraint(left, "<=", right);
        }

        public static DbConstraint operator >=(DbColumn left, DbColumn right)
        {
            return new DbConstraint(left, ">=", right);
        }

        public static DbConstraint operator <=(DbColumn left, bool right)
        {
            return new DbConstraint(left, "<=", new DbColumn(new DbValueColumn<bool>(right)));
        }

        public static DbConstraint operator >=(DbColumn left, bool right)
        {
            return new DbConstraint(left, ">=", new DbColumn(new DbValueColumn<bool>(right)));
        }

        public static DbConstraint operator <=(DbColumn left, int right)
        {
            return new DbConstraint(left, "<=", new DbColumn(new DbValueColumn<int>(right)));
        }

        public static DbConstraint operator >=(DbColumn left, int right)
        {
            return new DbConstraint(left, ">=", new DbColumn(new DbValueColumn<int>(right)));
        }

        public static DbConstraint operator <=(DbColumn left, long right)
        {
            return new DbConstraint(left, "<=", new DbColumn(new DbValueColumn<long>(right)));
        }

        public static DbConstraint operator >=(DbColumn left, long right)
        {
            return new DbConstraint(left, ">=", new DbColumn(new DbValueColumn<long>(right)));
        }

        public static DbConstraint operator <=(DbColumn left, decimal right)
        {
            return new DbConstraint(left, "<=", new DbColumn(new DbValueColumn<decimal>(right)));
        }

        public static DbConstraint operator >=(DbColumn left, decimal right)
        {
            return new DbConstraint(left, ">=", new DbColumn(new DbValueColumn<decimal>(right)));
        }

        public static DbConstraint operator <=(DbColumn left, DateTime right)
        {
            return new DbConstraint(left, "<=", new DbColumn(new DbValueColumn<DateTime>(right)));
        }

        public static DbConstraint operator >=(DbColumn left, DateTime right)
        {
            return new DbConstraint(left, ">=", new DbColumn(new DbValueColumn<DateTime>(right)));
        }

        public static DbConstraint operator <=(DbColumn left, string right)
        {
            return new DbConstraint(left, "<=", new DbColumn(new DbValueColumn<string>(right)));
        }

        public static DbConstraint operator >=(DbColumn left, string right)
        {
            return new DbConstraint(left, ">=", new DbColumn(new DbValueColumn<string>(right)));
        }

        public static DbConstraint operator <=(DbColumn left, byte[] right)
        {
            return new DbConstraint(left, "<=", new DbColumn(new DbValueColumn<byte[]>(right)));
        }

        public static DbConstraint operator >=(DbColumn left, byte[] right)
        {
            return new DbConstraint(left, ">=", new DbColumn(new DbValueColumn<byte[]>(right)));
        }

        public DbConstraint Like(string expression)
        {
            return new DbConstraint(this, "LIKE", new DbColumn(new DbValueColumn<string>(expression)));
        }

        public DbConstraint NotLike(string expression)
        {
            return new DbConstraint(this, "NOT LIKE", new DbColumn(new DbValueColumn<string>(expression)));
        }

        public DbConstraint In<T>(IEnumerable<T> intList)
        {
            return new DbConstraint(this, "IN", new DbColumn(new DbListColumn(intList.Select(cmpValue => new DbValueColumn<T>(cmpValue)))));
        }

        public DbConstraint In(params object[] cmpList)
        {
            return new DbConstraint(this, "IN", new DbColumn(new DbListColumn(cmpList.Select(cmpValue => new DbValueColumn<object>(cmpValue)))));
        }

        public DbConstraint In(ISqlBuilder query)
        {
            return new DbConstraint(this, "IN", new DbColumn(new DbQueryColumn(query)));
        }

        public DbConstraint NotIn<T>(IEnumerable<T> intList)
        {
            return new DbConstraint(this, "NOT IN", new DbColumn(new DbListColumn(intList.Select(cmpValue => new DbValueColumn<T>(cmpValue)))));
        }

        public DbConstraint NotIn(params object[] cmpList)
        {
            return new DbConstraint(this, "NOT IN", new DbColumn(new DbListColumn(cmpList.Select(cmpValue => new DbValueColumn<object>(cmpValue)))));
        }

        public DbConstraint NotIn(ISqlBuilder query)
        {
            return new DbConstraint(this, "NOT IN", new DbColumn(new DbQueryColumn(query)));
        }

        //
        // Expressions
        //

        public DbColumn Max(string @alias = null)
        {
            return new DbColumn(new DbExprColumn("MAX({0})", true, new[] { DbColumnImpl }, alias));
        }

        public DbTypedColumn<int> Count(string @alias = null)
        {
            return new DbTypedColumn<int>(new DbExprColumn("COUNT({0})", true, new[] { DbColumnImpl }, alias));
        }

        public static DbColumn operator +(DbColumn left, int right)
        {
            return new DbColumn(new DbExprColumn("({0} + {1})", false, new[] { left.DbColumnImpl, new DbValueColumn<int>(right) }));
        }

        public static DbColumn operator -(DbColumn left, int right)
        {
            return new DbColumn(new DbExprColumn("({0} - {1})", false, new[] { left.DbColumnImpl, new DbValueColumn<int>(right) }));
        }

        public static DbColumn operator +(DbColumn left, long right)
        {
            return new DbColumn(new DbExprColumn("({0} + {1})", false, new[] { left.DbColumnImpl, new DbValueColumn<long>(right) }));
        }

        public static DbColumn operator -(DbColumn left, long right)
        {
            return new DbColumn(new DbExprColumn("({0} - {1})", false, new[] { left.DbColumnImpl, new DbValueColumn<long>(right) }));
        }

        public static DbColumn operator +(DbColumn left, DbColumn right)
        {
            return new DbColumn(new DbExprColumn("({0} + {1})", false, new[] { left.DbColumnImpl, right.DbColumnImpl }));
        }

        public static DbColumn operator -(DbColumn left, DbColumn right)
        {
            return new DbColumn(new DbExprColumn("({0} - {1})", false, new[] { left.DbColumnImpl, right.DbColumnImpl }));
        }

        public DbColumn ConcatWith(DbColumn column)
        {
            return new DbColumn(new DbExprColumn("CONCAT({0}, {1})", false, new[] { DbColumnImpl, column.DbColumnImpl }));
        }

        public DbTypedColumn<string> ConcatWith(string text)
        {
            return new DbTypedColumn<string>(new DbExprColumn("CONCAT({0}, {1})", false, new[] { DbColumnImpl, new DbValueColumn<string>(text) }));
        }

        public static DbTypedColumn<T> CaseWhen<T>(DbConstraint constraint, DbColumn truePart, DbColumn falsePart = null)
        {
            if ((object)falsePart == null)
                return new DbTypedColumn<T>(new DbExprColumn("CASE WHEN {0} THEN {1} END", false, new[] { new DbConditionColumn(constraint), truePart.DbColumnImpl }));
            return new DbTypedColumn<T>(new DbExprColumn("CASE WHEN {0} THEN {1} ELSE {2} END", false, new[] { new DbConditionColumn(constraint), truePart.DbColumnImpl, falsePart.DbColumnImpl }));
        }

        //
        // Assignemnt
        //

        public DbAssignment ToNull()
        {
            return new DbAssignment(this, new DbColumn(new DbValueColumn<object>(null)));
        }

        public DbAssignment To(DbColumn right)
        {
            return new DbAssignment(this, right);
        }

        public DbAssignment To(bool right)
        {
            return new DbAssignment(this, new DbColumn(new DbValueColumn<bool>(right)));
        }

        public DbAssignment To(bool? right)
        {
            if (right == null)
                return ToNull();
            return new DbAssignment(this, new DbColumn(new DbValueColumn<bool>((bool)right)));
        }

        public DbAssignment To(int right)
        {
            return new DbAssignment(this, new DbColumn(new DbValueColumn<int>(right)));
        }

        public DbAssignment To(int? right)
        {
            return new DbAssignment(this, new DbColumn(new DbValueColumn<int?>(right)));
        }

        public DbAssignment To(long right)
        {
            return new DbAssignment(this, new DbColumn(new DbValueColumn<long>(right)));
        }

        public DbAssignment To(long? right)
        {
            return new DbAssignment(this, new DbColumn(new DbValueColumn<long?>(right)));
        }

        public DbAssignment To(DateTime right)
        {
            return new DbAssignment(this, new DbColumn(new DbValueColumn<DateTime>(right)));
        }

        public DbAssignment To(DateTime? right)
        {
            return new DbAssignment(this, new DbColumn(new DbValueColumn<DateTime?>(right)));
        }

        public DbAssignment To(decimal right)
        {
            return new DbAssignment(this, new DbColumn(new DbValueColumn<decimal>(right)));
        }

        public DbAssignment To(decimal? right)
        {
            return new DbAssignment(this, new DbColumn(new DbValueColumn<decimal?>(right)));
        }

        public DbAssignment To(string right)
        {
            if (right == null)
                return ToNull();
            return new DbAssignment(this, new DbColumn(new DbValueColumn<string>(right)));
        }

        public DbAssignment To(byte[] right)
        {
            return new DbAssignment(this, new DbColumn(new DbValueColumn<byte[]>(right)));
        }

        public DbAssignment To(Guid? guid)
        {
            if (guid != null)
                return new DbAssignment(this, new DbColumn(new DbValueColumn<byte[]>(guid.Value.ToByteArray())));
            return new DbAssignment(this, new DbColumn(new DbValueColumn<object>(null)));
        }

        public DbAssignment To(Enum right)
        {
            return new DbAssignment(this, new DbColumn(new DbValueColumn<int>(Convert.ToInt32(right))));
        }

        public string Sql(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            return DbColumnImpl?.Sql(parameterList, query, database);
        }

        public string Definition(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            return DbColumnImpl?.Definition(parameterList, query, database);
        }
    }

    public class DbValueColumn<T> : IDbColumnImpl
    {
        private readonly T _value;

        public string Alias { get; }
        public Type Type { get; set; }
        public DbTable Table => null;
        public bool IsAggregate => false;
        public bool IsGroupable => false;

        public DbValueColumn(T value, string @alias = null)
        {
            _value = value;
            Alias = alias;
            Type = typeof(T);
        }

        public string Sql(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            return _value != null ? AddParameter(parameterList, _value, typeof(T), database) : "NULL";
        }
        public string Definition(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            var result = Sql(parameterList, query, database);

            if (!string.IsNullOrEmpty(Alias))
                result += " " + Alias;

            return result;
        }

        private static string AddParameter(List<ParameterEntity> parameterList, object value, Type type, IDatabase database)
        {
            if (parameterList == null || value == null || value is bool || value is int || value is long)
                return database.ToLiteral(value);

            var parId = "ph" + parameterList.Count.ToString("D2");

            parameterList.Add(new ParameterEntity(parId, type, value));

            return database.ToParameterName(parId);
        }
    }

    public class DbExprColumn : IDbColumnImpl
    {
        private readonly string _expression;
        private readonly IEnumerable<IDbColumnImpl> _columnList;

        public string Alias { get; private set; }
        public Type Type { get; set; }
        public DbTable Table => null;
        public bool IsAggregate { get; }
        public bool IsGroupable => !IsAggregate;

        public DbExprColumn(string expression, bool isAggregate, IEnumerable<IDbColumnImpl> columnList, string @alias = null)
        {
            _expression = expression;
            _columnList = columnList;
            Alias = alias;
            IsAggregate = isAggregate;
        }

        public string Sql(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            if (database.Provider == DatabaseProvider.Sqlite && _expression.StartsWith("CONCAT"))
            {
                return string.Join(" || ", _columnList.Select(column => column.Sql(parameterList, query, database)));
            }
            return string.Format(_expression, _columnList.Select(column => column.Sql(parameterList, query, database)).Cast<object>().ToArray());
        }

        public string Definition(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            var result = Sql(parameterList, query, database);

            // Expressions should always have an alias for sorting
            if (string.IsNullOrEmpty(Alias))
                Alias = query.NextColumnAlias();

            result += " " + Alias;

            return result;
        }
    }

    public class DbListColumn : IDbColumnImpl
    {
        private readonly IEnumerable<IDbColumnImpl> _columnList;

        public string Alias { get; }
        public Type Type { get; set; }
        public DbTable Table => null;
        public bool IsAggregate => false;
        public bool IsGroupable => false;

        public DbListColumn(IEnumerable<IDbColumnImpl> columnList, string @alias = null)
        {
            _columnList = columnList;
            Alias = alias;
        }

        public string Sql(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            return $"({string.Join(", ", _columnList.Select(column => column.Sql(null, query, database)))})"; // Do not use placeholder in an IN clause
        }

        public string Definition(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            var result = Sql(parameterList, query, database);

            if (!string.IsNullOrEmpty(Alias))
                result += " " + Alias;

            return result;
        }
    }

    public class DbTableColumn : IDbColumnImpl
    {
        private readonly int _size;
        private readonly int _scale;

        public string Alias { get; }
        public Type Type { get; set; }
        public bool IsAggregate => false;
        public bool IsGroupable => true;
        public DbTable Table { get; }

        public string Name { get; }

        public DbTableColumn(DbTable table, string name, int size, int scale, string @alias = null)
        {
            _size = size;
            _scale = scale;
            Alias = alias;
            Table = table;
            Name = name;
        }

        public string Sql(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            return Table != null ? string.Format("{0}.{1}", Table.Name, Name) : "";
        }

        public string Definition(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            var result = Sql(parameterList, query, database);

            if (!string.IsNullOrEmpty(Alias))
                result += " " + Alias;

            return result;
        }

        public string Adjust(string value)
        {
            if (value != null)
            {
                value = value.Trim();
                if (_size > 0 && value.Length > _size)
                    value = value.Substring(0, _size);
            }

            return value;
        }
    }

    public class DbConditionColumn : IDbColumnImpl
    {
        private readonly DbConstraint _constraint;

        public string Alias { get; }
        public Type Type { get; set; }
        public DbTable Table => null;
        public bool IsAggregate => false;
        public bool IsGroupable => false;

        public DbConditionColumn(DbConstraint constraint, string @alias = null)
        {
            _constraint = constraint;
            Alias = alias;
        }

        public string Sql(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            return _constraint.Sql(parameterList, query, database);
        }

        public string Definition(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            var result = Sql(parameterList, query, database);

            if (!string.IsNullOrEmpty(Alias))
                result += " " + Alias;

            return result;
        }
    }

    public class DbQueryColumn : IDbColumnImpl
    {
        private readonly ISqlBuilder _query;

        public string Alias { get; }
        public Type Type { get; set; }
        public DbTable Table => null;
        public bool IsAggregate => false;
        public bool IsGroupable => false;

        public DbQueryColumn(ISqlBuilder query, string @alias = null)
        {
            _query = query;
            Alias = alias;
        }

        public string Sql(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            return $"({_query.Sql(parameterList, database)})"; // Do not use placeholder in an IN clause
        }

        public string Definition(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            var result = Sql(parameterList, query, database);

            if (!string.IsNullOrEmpty(Alias))
                result += " " + Alias;

            return result;
        }
    }

    //
    // Typed columns
    //

    public class DbTypedColumn<T> : DbColumn
    {
        public DbTypedColumn(IDbColumnImpl dbColumnImpl) : base(dbColumnImpl)
        {
            dbColumnImpl.Type = typeof(T);
        }

        public new DbTypedColumn<T> Max(string @alias = null)
        {
            return new DbTypedColumn<T>(new DbExprColumn("MAX({0})", true, new[] { this.DbColumnImpl }, alias));
        }
    }

    //
    // Bound value types
    //

    public abstract class DbBoundValue
    {
        protected object DbValue { get; set; }

        public abstract void SetValue(DbResultSet resultSet, int index);
        public abstract void SetValue(object value);

        public bool IsNull => DbValue == null;
        public bool IsNotNull => DbValue != null;
    }

    public interface IDbConverter<out T>
    {
        T DbConvert(object value);
    }

    public class DbConverter<T> : IDbConverter<T>
    {
        public static readonly IDbConverter<T> P = DbConverter.P as IDbConverter<T> ?? new DbConverter<T>();

        //default implementation
        T IDbConverter<T>.DbConvert(object value)
        {
            return (T)value;
        }
    }

    class DbConverter : IDbConverter<bool>, IDbConverter<bool?>, IDbConverter<int>, IDbConverter<int?>, IDbConverter<long>, IDbConverter<long?>, IDbConverter<DateTime>, IDbConverter<DateTime?>
    {
        public static DbConverter P = new DbConverter();

        bool IDbConverter<bool>.DbConvert(object value)
        {
            return DbResultSet.ToBool(value) ?? false;
        }

        public bool? DbConvert(object value)
        {
            return DbResultSet.ToBool(value);
        }

        //specialized for int
        int IDbConverter<int>.DbConvert(object value)
        {
            return Convert.ToInt32(value);
        }

        int? IDbConverter<int?>.DbConvert(object value)
        {
            if (value == null)
                return null;
            return Convert.ToInt32(value);
        }

        long IDbConverter<long>.DbConvert(object value)
        {
            return Convert.ToInt64(value);
        }

        long? IDbConverter<long?>.DbConvert(object value)
        {
            if (value == null)
                return null;
            return Convert.ToInt64(value);
        }

        DateTime IDbConverter<DateTime>.DbConvert(object value)
        {
            return Convert.ToDateTime(value);
        }

        DateTime? IDbConverter<DateTime?>.DbConvert(object value)
        {
            if (value == null)
                return null;
            return Convert.ToDateTime(value);
        }
    }

    public class DbTypedBoundValue<T> : DbBoundValue
    {
        static T Convert(object value)
        {
            return DbConverter<T>.P.DbConvert(value);
        }

        public T Value
        {
            get => Convert(DbValue);
            set => DbValue = value;
        }

        public Action<T> Setter { get; set; }

        public override void SetValue(DbResultSet resultSet, int index)
        {
            SetValue(resultSet.GetValue(index));
        }

        public override void SetValue(object value)
        {
            DbValue = value is DBNull ? null : value;
            Setter?.Invoke(Value);
        }

        public static implicit operator T(DbTypedBoundValue<T> boundValue) => boundValue.Value;

        public override string ToString() => Value.ToString();
    }
}
