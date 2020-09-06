namespace TableGenerator
{
    /// <summary>
    /// Der Typ eines Tokens.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// (Noch) Unbekannter Typ
        /// </summary>
        Unknown,

        /// <summary>
        /// Identifikator. Eine Variable oder Funktion
        /// </summary>
        Identifier,

        /// <summary>
        /// Seperator. Klammern, Kommata.
        /// </summary>
        Separator,

        /// <summary>
        /// Ganze zahl
        /// </summary>
        Integer,

        /// <summary>
        /// Kommazahl
        /// </summary>
        Decimal,

        /// <summary>
        /// Zeichenkette
        /// </summary>
        String,

        /// <summary>
        /// Kommentar
        /// </summary>
        Comment
    }
}