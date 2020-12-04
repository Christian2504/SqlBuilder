# SqlBuilder

SqlBuilder is a C# framework that creates and executes database commands without using string literals.

It is a pure C# implementation without reflection or linq expression trees.

Downside: 'where' expressions cannot use the '&&' and '||' operators. Instead the '&' and '|' operators must be used to create conditional expressions.

See SqlBuilderFramework/Readme.md for more details.
