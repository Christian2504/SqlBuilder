using Kendo.DynamicLinqCore;
using Newtonsoft.Json;
using SqlBuilderFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlBuilderSamplesAndTests
{
    public static class DataSourceRequestExtensions
    {

        private static DbConstraint ToConstraint(Dictionary<string, DbColumn> columnMap, Filter filter)
        {
            DbConstraint result = null;

            if (filter.Filters != null && filter.Filters.Any())
            {
                foreach (var child in filter.Filters)
                {
                    if (result == null)
                        result = ToConstraint(columnMap, child);
                    else if (filter.Logic == "and")
                        result &= ToConstraint(columnMap, child);
                    else
                        result |= ToConstraint(columnMap, child);
                }
            }
            else if (filter.Operator != null)
            {
                var column = columnMap[filter.Field];

                if (column is object)
                {
                    object value = null;

                    if (filter.Value != null)
                        value = JsonConvert.DeserializeObject(((System.Text.Json.JsonElement)filter.Value).GetRawText());

                    var isDateTimeColumn = column.Type == typeof(DateTime);

                    if (value != null && isDateTimeColumn)
                        value = ((DateTime)value).ToUniversalTime();

                    var colValue = new DbColumn(new DbValueColumn<object>(value));

                    switch (filter.Operator)
                    {
                        case "eq":
                            if (value != null && isDateTimeColumn)
                                result = column >= colValue & column < ((DateTime)value).AddDays(1);
                            else
                                result = column == colValue;
                            break;
                        case "neq":
                            if (value != null && isDateTimeColumn)
                                result = column < colValue | column >= ((DateTime)value).AddDays(1);
                            else
                                result = column != colValue;
                            break;
                        case "lt":
                            result = column < colValue;
                            break;
                        case "lte":
                            result = column <= colValue;
                            break;
                        case "gt":
                            result = column > colValue;
                            break;
                        case "gte":
                            result = column >= colValue;
                            break;
                        case "startswith":
                            result = column.Like(AbstractDatabase.EscapeForLike(value?.ToString()) + "%");
                            break;
                        case "endswith":
                            result = column.Like("%" + AbstractDatabase.EscapeForLike(value?.ToString()));
                            break;
                        case "contains":
                            result = column.Like("%" + AbstractDatabase.EscapeForLike(value?.ToString()) + "%");
                            break;
                        case "doesnotcontain":
                            result = column.NotLike("%" + AbstractDatabase.EscapeForLike(value?.ToString()) + "%");
                            break;
                        case "isnull":
                            result = column.IsNull();
                            break;
                        case "isnotnull":
                            result = column.IsNotNull();
                            break;
                        case "isempty":
                            result = column == new DbColumn(new DbValueColumn<string>(string.Empty));
                            break;
                        case "isnotempty":
                            result = column != new DbColumn(new DbValueColumn<string>(string.Empty));
                            break;
                        case "isnullorempty":
                            result = column.IsNull() | column == new DbColumn(new DbValueColumn<string>(string.Empty));
                            break;
                        case "isnotnullorempty":
                            result = column.IsNotNull() & column != new DbColumn(new DbValueColumn<string>(string.Empty));
                            break;
                        default:
                            throw new NotImplementedException("Unknown operator");
                    }
                }
            }

            return result;
        }

        public static void ApplyToQuery(SqlBuilder query, Dictionary<string, DbColumn> columnMap, DataSourceRequest request)
        {
            query.Where(query.Constraint & ToConstraint(columnMap, request.Filter));

            if (request.Sort != null)
            {
                foreach (var sort in request.Sort)
                {
                    if (sort.Dir != null)
                    {
                        if (sort.Dir == "desc")
                            query.OrderDescending(columnMap[sort.Field]);
                        else
                            query.OrderAscending(columnMap[sort.Field]);
                    }
                }
            }

            query.Offset(request.Skip);
            query.Limit(request.Take);
        }
    }
}
