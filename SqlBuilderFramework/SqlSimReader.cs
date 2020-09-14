namespace SqlBuilderFramework
{
    public class SqlSimReader : ISqlBuilderReader
    {
        private int _count = 1;

        public bool Next()
        {
            if (_count == 0)
                return false;
            _count--;
            return true;
        }

        public void Dispose()
        {
        }
    }
}
