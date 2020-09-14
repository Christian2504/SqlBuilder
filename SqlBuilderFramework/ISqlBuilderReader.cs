using System;

namespace SqlBuilderFramework
{
    public interface ISqlBuilderReader : IDisposable
    {
        bool Next();
    }
}