using System;

namespace TableGenerator
{
    /// <summary>
    /// Setellt ein Token in einem Ausdruck dar.
    /// </summary>
    public struct Token
    {
        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public Token(string text, TokenType type, object value = null) : this()
        {
            Text = text;
            Type = type;
            Value = value ?? text;
        }

        /// <summary>
        /// Der für das Token geparste Text (mit allen Leerzeichen etc.).
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Der für Value relevante Bereich in Text
        /// </summary>
        public IntegralRange SignificantPart { get; set; }

        /// <summary>
        /// Der Typ Tokens.
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// Wert des Tokens (je nach Typ).
        /// </summary>
        public object Value { get; set; }

        public string Identifier => Type == TokenType.Identifier ? Value as string : null;

        public string Separator => Type == TokenType.Separator ? Value as string : null;

        /// <summary>
        /// Vereinfachte Weise einen Identifier zu überprüfen.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        public bool IsIdentifier(string name, StringComparison comparison = StringComparison.Ordinal)
        {
            return Type == TokenType.Identifier && string.Compare(Value as string, name, comparison) == 0;
        }

        /// <summary>
        /// Vereinfachte Weise einen Identifier zu überprüfen.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsSeparator(string name)
        {
            return Type == TokenType.Separator && Value as string == name;
        }

        public static bool operator ==(Token first, Token second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(Token first, Token second)
        {
            return !(first == second);
        }

        public override bool Equals(System.Object obj)
        {
            if (!(obj is Token))
                return false;

            return Equals((Token)obj);
        }

        private bool Equals(Token token)
        {
            return Type == token.Type && Value.Equals(token.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode() ^ Type.GetHashCode();
        }
    }
}