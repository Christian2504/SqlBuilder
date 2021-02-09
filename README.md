# SqlBuilder

SqlBuilder is a C# framework with which you can create and execute database commands without writing string literals.

It is implemented in pure C# without using reflection or linq expression trees. This is also the reason why conditional expressions in where clauses use single '|' and '&' operators for OR and AND instead of their logical counterparts '||' and '&&'.

See SqlBuilderFramework/Readme.md for more details.
