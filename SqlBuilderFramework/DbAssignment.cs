namespace SqlBuilderFramework
{
    public class DbAssignment
    {
        public DbColumn Left { get; }
        public DbColumn Right { get; }

        public DbAssignment(DbColumn left, DbColumn right)
        {
            Left = left;
            Right = right;
        }
    }
}
