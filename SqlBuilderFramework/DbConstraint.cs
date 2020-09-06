using System.Collections.Generic;
using System.Text;

namespace SqlBuilderFramework
{
    public class DbConstraint
    {
        public DbConstraint()
        {
        }

        public DbConstraint(DbColumn left, string op, DbColumn right)
        {
            _leftColumn = left;
            _operator = op;
            _rightColumn = right;
        }

        public static DbConstraint operator &(DbConstraint left, DbConstraint right)
        {
            if (left?._operator == null)
                return right;
            if (right?._operator == null)
                return left;
            return new DbConstraint { _operator = "AND", _leftConstraint = left, _rightConstraint = right };
        }

        public static DbConstraint operator |(DbConstraint left, DbConstraint right)
        {
            if (left?._operator == null)
                return right;
            if (right?._operator == null)
                return left;
            return new DbConstraint { _operator = "OR", _leftConstraint = left, _rightConstraint = right };
        }

        public string Sql(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)
        {
            var sql = new StringBuilder();

            sql.Append("(");

            if ((object)_leftColumn == null && (object)_rightColumn == null)
            {
                if (_leftConstraint != null)
                {
                    sql.Append(_leftConstraint.Sql(parameterList, query, database));
                    sql.Append(" ");
                }
                sql.Append(_operator);
                if (_rightConstraint != null)
                {
                    sql.Append(" ");
                    sql.Append(_rightConstraint.Sql(parameterList, query, database));
                }
            }
            else
            {
                string lValue = null;
                string rValue = null;
                string op = _operator;

                if ((object)_leftColumn != null)
                    lValue = _leftColumn.Sql(parameterList, query, database);
                if ((object)_rightColumn != null)
                {
                    rValue = _rightColumn.Sql(parameterList, query, database);
                    if (rValue == "NULL")
                        op = (op == "=" ? "IS" : "IS NOT");
                }

                if (!string.IsNullOrEmpty(lValue))
                {
                    sql.Append(lValue);
                    sql.Append(" ");
                }

                sql.Append(op);

                if (!string.IsNullOrEmpty(rValue))
                {
                    sql.Append(" ");
                    sql.Append(rValue);
                }
            }

            sql.Append(")");

            return sql.ToString();
        }

        private string _operator;
        private readonly DbColumn _leftColumn;
        private readonly DbColumn _rightColumn;
        private DbConstraint _leftConstraint;
        private DbConstraint _rightConstraint;
    }
}
