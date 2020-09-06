using System;
using System.Collections.Generic;
using System.Data;

namespace SqlBuilderFramework
{
    public class DbResultSet : IDisposable
    {
        static readonly HashSet<string> _booleanTrueStrings = new HashSet<string> { "true", "t", "yes", "y", "1" };
        static readonly HashSet<string> _booleanFalseStrings = new HashSet<string> { "false", "f", "no", "n", "0" };

        public static bool? ToBool(object value)
        {
            if (value == null)
                return null;

            if (value is bool boolean)
            {
                return boolean;
            }

            if (value is int number)
            {
                return number != 0;
            }

            if (value is string text)
            {
                return _booleanTrueStrings.Contains(text.Trim().ToLower());
            }

            if (value is long largeNumber)
            {
                return largeNumber != 0;
            }

            return Convert.ToBoolean(value);
        }

        private readonly IDataReader _reader;

        public DbResultSet(IDataReader reader)
        {
            _reader = reader;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _reader.Dispose();
            }
        }

        public bool Next()
        {
            return _reader.Read();
        }

        public bool IsDbNull(int i)
        {
            return _reader.IsDBNull(i);
        }

        public bool IsDbNull(string column)
        {
            return IsDbNull(GetOrdinal(column));
        }

        public object GetValue(int i)
        {
            return _reader.GetValue(i);
        }

        public object GetValue(string column)
        {
            return GetValue(GetOrdinal(column));
        }

        public bool GetBool(int i)
        {
            if (IsDbNull(i))
                return false;

            return ToBool(_reader.GetValue(i)) ?? false;
        }

        public bool GetBool(string column)
        {
            return GetBool(GetOrdinal(column));
        }

        public bool? GetNullableBool(int i)
        {
            if (IsDbNull(i))
                return null;

            return GetBool(i);
        }

        public bool? GetNullableBool(string column)
        {
            return GetNullableBool(GetOrdinal(column));
        }

        public string GetString(int i)
        {
            if (_reader.IsDBNull(i))
            {
                return null;
            }

            return _reader.GetString(i);
        }

        public string GetString(string column)
        {
            return GetString(GetOrdinal(column));
        }

        public char GetChar(int i)
        {
            if (_reader.IsDBNull(i))
            {
                return default;
            }

            var stringValue = _reader.GetString(i);
            return stringValue == string.Empty ? '\0' : stringValue[0];
        }

        public char GetChar(string column)
        {
            return GetChar(GetOrdinal(column));
        }

        public int GetInt(int i)
        {
            if (_reader.IsDBNull(i))
                return 0;

            return _reader.GetInt32(i);
        }

        public int GetInt(string column)
        {
            return GetInt(GetOrdinal(column));
        }

        public int? GetNullableInt(int i)
        {
            if (_reader.IsDBNull(i))
                return null;

            return GetInt(i);
        }

        public int? GetNullableInt(string column)
        {
            return GetNullableInt(GetOrdinal(column));
        }

        public long GetLong(int i)
        {
            if (_reader.IsDBNull(i))
                return 0;

            return _reader.GetInt64(i);
        }

        public long GetLong(string column)
        {
            return GetLong(GetOrdinal(column));
        }

        public long? GetNullableLong(int i)
        {
            if (_reader.IsDBNull(i))
                return null;

            return _reader.GetInt64(i);
        }

        public long? GetNullableLong(string column)
        {
            return GetNullableLong(GetOrdinal(column));
        }

        public decimal GetDecimal(int i)
        {
            if (_reader.IsDBNull(i))
                return 0;

            return _reader.GetDecimal(i);
        }

        public decimal GetDecimal(string column)
        {
            return GetDecimal(GetOrdinal(column));
        }

        public DateTime GetDateTime(int i)
        {
            if (_reader.IsDBNull(i))
                return DateTime.MinValue;

            return _reader.GetDateTime(i);
        }

        public DateTime GetDateTime(string column)
        {
            return GetDateTime(GetOrdinal(column));
        }

        public DateTime? GetNullableDateTime(int i)
        {
            if (_reader.IsDBNull(i))
                return null;

            return _reader.GetDateTime(i);
        }

        public DateTime? GetNullableDateTime(string column)
        {
            return GetNullableDateTime(GetOrdinal(column));
        }

        public byte[] GetData(int i)
        {
            return _reader.GetValue(i) as byte[];
        }

        public byte[] GetData(string column)
        {
            return GetData(GetOrdinal(column));
        }

        public int ColumnCount
        {
            get { return _reader.FieldCount; }
        }

        public IList<string> ColumnNames
        {
            get
            {
                var columnList = new List<string>();

                for (int i = 0; i < _reader.FieldCount; i++)
                    columnList.Add(_reader.GetName(i));

                return columnList;
            }
        }

        public T GetEnum<T>(int i) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("Type is not an enumeration");
            }

            var value = _reader.GetValue(i);

            if (value is DBNull)
            {
                return default(T);
            }
            if (value is string)
            {
                return (T)Enum.Parse(typeof(T), value.ToString());
            }

            return (T)Enum.ToObject(typeof(T), value);
        }

        public T GetEnum<T>(string column) where T : struct
        {
            return GetEnum<T>(GetOrdinal(column));
        }

        public bool TryGetEnum<T>(int i, out T enumType) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("Type is not an enumeration");
            }

            var value = _reader.GetValue(i);

            if (value is DBNull)
            {
                enumType = default(T);
                return true;
            }
            return Enum.TryParse(value.ToString(), out enumType);
        }

        public bool TryGetEnum<T>(string column, out T enumType) where T : struct
        {
            return TryGetEnum(GetOrdinal(column), out enumType);
        }

        /// <summary>
        /// Zurückgeben des Indexes des benannten Felds.
        /// </summary>
        /// <param name="name">Der Name des gesuchten Felds.</param>
        /// <returns>Der Index des benannten Felds.</returns>
        /// <exception cref="ColumnNotFoundException">Triff auf, der Feldname nicht gefunden wird</exception>
        private int GetOrdinal(string name)
        {
            var index = _reader.GetOrdinal(name);
            if (index == -1)
            {
                // Wir werfen eine Exception, da bei -1 man dann die hier in der Klasse definieren "IsDBNull"-Werte zurückbekommt
                throw new ColumnNotFoundException(name);
            }
            return index;
        }
    }
}
