using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBuilderFramework
{
    public enum JoinMode
    {
        Inner,
        Left,
        Right,
        Outer
    }

    public abstract class DbTable
    {
        protected DbTable(string @alias = null)
        {
            Alias = alias;
        }

        public virtual string Name
        {
            get { return Alias ?? ""; }
            set { Alias = value; }
        }

        public abstract string Sql(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database);

        public abstract DbTable Find(ISqlBuilder query);

        public DbJoinedTable InnerJoin(DbTable rightTable)
        {
            return new DbJoinedTable(this, rightTable, JoinMode.Inner);
        }

        public DbJoinedTable InnerJoin(DbTable rightTable, DbConstraint constraint)
        {
            return new DbJoinedTable(this, rightTable, JoinMode.Inner, constraint);
        }

        public DbJoinedTable LeftJoin(DbTable rightTable, DbConstraint constraint)
        {
            return new DbJoinedTable(this, rightTable, JoinMode.Left, constraint);
        }

        protected string Alias;
    }

    public class DbPlainTable : DbTable
    {
        public DbPlainTable(string name)
        {
            _name = name;
        }

        public override string Name
        {
            get { return _name; }
        }

        public override string Sql(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            return Name;
        }

        public override DbTable Find(ISqlBuilder query)
        {
            return null;
        }

        private readonly string _name;
    }

    public class DbJoinedTable : DbTable
    {
        private readonly DbTable _leftTable;
        private readonly DbTable _rightTable;
        private readonly JoinMode _joinMode;
        private DbConstraint _constraint;

        public DbJoinedTable(DbTable leftTable, DbTable rightTable, JoinMode joinMode, string @alias = null)
            : base(alias)
        {
            _leftTable = leftTable;
            _rightTable = rightTable;
            _joinMode = joinMode;
        }

        public DbJoinedTable(DbTable leftTable, DbTable rightTable, JoinMode joinMode, DbConstraint constraint, string @alias = null)
            : base(alias)
        {
            _leftTable = leftTable;
            _rightTable = rightTable;
            _joinMode = joinMode;
            _constraint = constraint;
        }

        public DbJoinedTable On(DbConstraint constraint)
        {
            _constraint = constraint;
            return this;
        }

        public override string Sql(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            var result = new StringBuilder();

            // Left table
            result.Append(_leftTable.Sql(parameterList, query, database));

            // Join mode
            result.Append(" ");
            switch (_joinMode)
            {
                case JoinMode.Inner:
                    result.Append("INNER JOIN");
                    break;
                case JoinMode.Left:
                    result.Append("LEFT OUTER JOIN");
                    break;
                case JoinMode.Right:
                    if (database.Provider == DatabaseProvider.Sqlite)
                    {
                        throw new NotImplementedException("Right outer join not supported");
                    }
                    else
                    {
                        result.Append("RIGHT OUTER JOIN");
                    }
                    break;
                case JoinMode.Outer:
                    if (database.Provider == DatabaseProvider.Sqlite)
                    {
                        throw new NotImplementedException("Full outer join not supported");
                    }
                    else
                    {
                        result.Append("FULL OUTER JOIN");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Right table
            result.Append(" ");
            result.Append(_rightTable.Sql(parameterList, query, database));

            // On clause
            result.Append(" ON ");
            result.Append(_constraint.Sql(parameterList, query, database));

            // Alias
            if (!string.IsNullOrEmpty(Alias))
            {
                result.Append(" ");
                result.Append(Alias);
            }

            return result.ToString();
        }

        public override DbTable Find(ISqlBuilder query)
        {
            DbTable table = null;

            if (_leftTable != null)
                table = _leftTable.Find(query);
            if (table == null && _rightTable != null)
                table = _rightTable.Find(query);
            return table;
        }
    }

    public class DbInlineTable : DbTable
    {
        public DbInlineTable(ISqlBuilder query, string @alias = null)
            : base(alias)
        {
            _query = query;
        }

        public override string Sql(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            var result = "(" + _query.Sql(parameterList, database) + ")";

            if (!string.IsNullOrEmpty(Alias))
                result += " " + Alias;

            return result;
        }

        public override DbTable Find(ISqlBuilder query)
        {
            return _query == query ? this : null;
        }

        private readonly ISqlBuilder _query;
    }
}
