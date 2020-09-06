using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TableGenerator
{
    /// <summary>
    /// Zerlegt einen Stringausdruck in einzelne Token.
    /// Der Tokenizer ist sehr primitiv und verwendet nur 2 Zeichengruppen: Trennzeichen (einzelnes festgelegte Zeichen (Separators)) und alle Nicht-Trennzeichen.
    /// Den Nicht-Trennzeichen Token (den Namen) wird dann noch eine Bedeutung zugeordnet (Typ).
    /// Eine Ausnahme bilden Leerzeichen. Sie gelten als Trennzeichen werden aber nicht als Token ausgegeben.
    /// Zu den Leerzeichen gehören auch Kommentare, wenn IgnoreComments auf True steht!
    /// Kommentare gehen im SQL-Mode von -- bis Zeilenende und zwischen /**/. Im CSharp-Mode sind es // bis Zeilenende und /**/.
    /// Alle Kommentare enden auch beim Kommentar-Ende-Zeichen falls es definiert ist. Dies ist ein Sonderfall für List&Label.
    /// Eine andere Ausnahme bei Trennzeichen sind die Separatoren " und '. Sie kennzeichnen den Beginn (und das Ende) eines Strings und werden nicht as Separatoren ausgegeben.
    /// Alle folgenden Zeichen bis zum nächsten " bzw. ' gehören zum String-Token (auch Kommentar-Ende-Zeichen wie z.B. Chevrons).
    /// In einem String können einzelne " bzw. ' durch verdoppelung eingefügt werden. Beispiele:
    /// """" ergibt: "
    /// "Hallo ""Welt""" ergibt: Hallo "Welt"
    /// Identifier die als long (64-Bit Zahl) interpretierbar sind erhalten den Typ Integer.
    /// Beispiel:
    /// Der Ausdruck " Eine    Funktion( \"Hallo\" , 5 )" wird in folgende {Token.Text, Token.Type, Token.Value} zerlegt:
    ///  {{"Eine", Identifier, "Eine"},
    ///  {{"Funktion", Identifier, "Funktion"},
    ///  {"(", Separator, "("},
    ///  {"Hallo", String, "Hallo"},
    ///  {",", Separator, ","},
    ///  {"5", Integer, 5},
    ///  {")", Separator, ")"}}
    /// </summary>
    public class SimpleTokenizer
    {
        private enum CommentTypeEnd
        {
            None, // Kein Kommentar
            Line, // Kommentar geht bis zum Zeilenende
            CStyle, // Kommentar hört mit */
            NextChar // Kommentar hört mit */ auf aber erst beim nächsten Zeichen
        }

        public enum Mode { SQL, CSharp }

        /// <summary>
        /// Ausdruck der geparst werden soll
        /// </summary>
        protected string _expression;

        /// <summary>
        /// Aktuelle Position im Ausdruck (immer nach dem zuletzt geparsten Token)
        /// </summary>
        private int _pos;

        private Token _token;

        protected virtual bool FetchMore()
        {
            return false;
        }

        protected virtual string GetRawString()
        {
            return string.Empty;
        }

        public Mode ParseMode { get; set; }

        public bool IgnoreComments { get; set; }

        /// <summary>
        /// Liste mit Trennzeichen
        /// </summary>
        public static char[] Separators = { '!', '"', '#', '$', '%', '&', '\'', '*', '+', '-', '.', '/', ':', '<', '=', '>', '?', '@', '\\', '^', '|', '~' };

        public static char[] Delimiters = { '(', ')', ',', ';', '[', ']', '{', '}', '»', '«' };

        public char CommentEndChar { get; set; } // Must be one of the Delimiters

        public Token Token => _token;

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="expression"></param>
        public SimpleTokenizer(string expression = "")
        {
            _expression = expression;
            ParseMode = Mode.SQL;
            IgnoreComments = true;
        }

        public static TokenList Parse(string expression, Mode mode = Mode.SQL)
        {
            var tokenList = new TokenList();

            var tokenizer = new SimpleTokenizer(expression) {ParseMode = mode};

            while (tokenizer.Next())
                tokenList.Add(tokenizer.Token);

            return tokenList;
        }

        public string Expression
        {
            get { return _expression; }
            set
            {
                _pos = 0;
                _expression = value;
                _token = new Token();
            }
        }

        /// <summary>
        /// Aktuelle Position des Parsers im String
        /// </summary>
        public int Pos
        {
            get { return _pos; }
            set
            {
                _pos = value;
                _token = new Token();
            }
        }

        /// <summary>
        /// Parst das nächste Token.
        /// </summary>
        /// <returns></returns>
        public bool Next()
        {
            _token = new Token();

            if (_expression == null || _pos >= _expression.Length)
            {
                if (!FetchMore())
                    return false;
            }

            var stringVal = new StringBuilder();
            var stringChar = '\0'; // Zeichen mit dem eine Zeichenkette begonnen wurde
            var escapeCStyle = ParseMode == Mode.CSharp;
            var prev = '\0'; // Das vorhergehende Zeichen
            var tokenType = TokenType.Unknown;
            decimal numVal = 0;
            var magn = 0L;
            var commentType = CommentTypeEnd.None;
            var begin = _pos;
            var tokenStart = _pos; // Beginn des relevanten Teils für Value (nach Leerzeichen und Kommentaren)

            // Zeichen für Zeichen analysieren bis das nächste Token vollständig ist.
            while (_pos < _expression.Length)
            {
                var ch = _expression[_pos];

                if (commentType != CommentTypeEnd.None)
                {
                    // The CommentEndChar (for example a closing chevron) ends all comments
                    if (ch == CommentEndChar)
                    {
                        commentType = CommentTypeEnd.None;
                        if (IgnoreComments)
                        {
                            stringVal.Clear();
                            tokenType = TokenType.Separator;
                            tokenStart = _pos;
                            stringVal.Append(ch);
                            _pos++;
                        }
                        break;
                    }

                    // Check for normal comment terminator
                    switch (commentType)
                    {
                        case CommentTypeEnd.Line:
                            if (ch == '\n')
                                commentType = CommentTypeEnd.None;
                            else
                                stringVal.Append(ch);
                            break;
                        case CommentTypeEnd.CStyle:
                            if (prev == '*' && ch == '/')
                                commentType = CommentTypeEnd.None;
                            else
                                stringVal.Append(prev);
                            break;
                        default:
                            commentType = CommentTypeEnd.CStyle; // Auf das nächste Zeichen warten
                            break;
                    }
                    if (commentType == CommentTypeEnd.None && IgnoreComments)
                    {
                        tokenType = TokenType.Unknown;
                        stringVal.Clear();
                    }
                }
                else if (stringChar != '\0') // TokenType == TokenType.String
                {
                    if (escapeCStyle)
                    {
                        if (prev == '\\')
                        {
                            var escIndex = "ntvbrfa\\'\"0".IndexOf(ch);

                            if (escIndex != -1)
                            {
                                ch = "\n\t\v\b\r\f\a\\\'\"\0"[escIndex];
                            }
                            stringVal[stringVal.Length - 1] = ch;
                            ch = '\0'; // Das hinzugefügte Zeichen wird nicht mit Folgezeichen kombiniert
                        }
                        else if (ch == stringChar)
                        {
                            stringChar = '\0';
                        }
                        else
                        {
                            stringVal.Append(ch == '¶' ? '\n' : ch);
                        }
                    }
                    else if (ch == stringChar)
                    {
                        stringChar = '\0';
                    }
                    else
                    {
                        stringVal.Append(ch == '¶' ? '\n' : ch);
                    }
                }
                else if (tokenType == TokenType.Identifier)
                {
                    if (Delimiters.Contains(ch))
                        break;
                    if (Separators.Contains(ch))
                        break;
                    if (IsWhiteSpace(ch))
                        break;
                    stringVal.Append(ch);
                }
                else if (tokenType == TokenType.Integer || tokenType == TokenType.Decimal)
                {
                    if (ch == '.')
                    {
                        if (tokenType == TokenType.Decimal)
                            break;
                        tokenType = TokenType.Decimal;
                    }
                    else
                    {
                        if (!char.IsDigit(ch))
                            break;
                        numVal *= 10;
                        numVal += (int)char.GetNumericValue(ch);
                        if (tokenType == TokenType.Decimal)
                            magn *= 10L;
                    }
                }
                else if (tokenType == TokenType.Comment)
                {
                    break;
                }
                // TokenType == TokenType.Unknown || TokenType == TokenType.String || TokenType == TokenType.Separator
                else if (ch == '\'' || ch == '"')
                {
                    if (!escapeCStyle)
                    {
                        // Prüfen ob die Zeichenkette fortgesetzt wird (ob zwei ' oder " angegeben wurden)
                        if (tokenType == TokenType.String && prev == ch)
                        {
                            stringVal.Append(ch);
                        }
                        else if (tokenType != TokenType.Unknown)
                        {
                            break;
                        }
                    }
                    else if (prev == '@')
                    {
                        escapeCStyle = false;
                        stringVal.Clear();
                    }
                    else if (tokenType != TokenType.Unknown)
                    {
                        break;
                    }
                    tokenType = TokenType.String;
                    tokenStart = _pos;
                    stringChar = ch;
                }
                else if (tokenType == TokenType.String)
                {
                    break; // Hier ist der String auf jeden Fall zu Ende (kein Fortsetzungszeichen)
                }
                else if (tokenType == TokenType.Separator)
                {
                    if (prev == '$' && ch == ':')
                    {
                        // Spezialfall Raw-Strings

                        Debug.Assert(_pos + 1 == _expression.Length);

                        tokenType = TokenType.String;
                        tokenStart = _pos - 1;
                        stringVal.Append(GetRawString() ?? string.Empty);
                        _pos = _expression.Length - 1;
                    }
                    else
                    {
                        // Auf Kommentar prüfen!
                        // Bei allen anderen Separatoren abbrechen da zur Zeit nur einzeichige Separatoren erkannt werden.
                        if (ParseMode == Mode.SQL)
                        {
                            if (prev == '-' && ch == '-')
                                commentType = CommentTypeEnd.Line;
                        }
                        else if (ParseMode == Mode.CSharp)
                        {
                            if (prev == '/' && ch == '/')
                                commentType = CommentTypeEnd.Line;
                        }
                        if (prev == '/' && ch == '*')
                            commentType = CommentTypeEnd.NextChar; // Es muss mindestens ein Zeichen gewartet werden bevor auf das Ende geprüft werden darf.

                        if (commentType == CommentTypeEnd.None)
                        {
                            if (prev == '\\')
                            {
                                stringVal.Append(ch);
                                _pos++;
                            }
                            break;
                        }

                        tokenType = TokenType.Comment;
                        tokenStart = _pos - 1; // Jede Art von Kommentar startet mit dem vorhergehenden Zeichen
                        stringVal.Clear();
                    }
                }
                else if (Delimiters.Contains(ch))
                {
                    if (tokenType != TokenType.Unknown)
                        break;
                    tokenType = TokenType.Separator;
                    tokenStart = _pos;
                    stringVal.Append(ch);
                    _pos++;
                    break;
                }
                else if (Separators.Contains(ch))
                {
                    if (tokenType != TokenType.Unknown)
                        break;
                    tokenType = TokenType.Separator;
                    tokenStart = _pos;
                    stringVal.Append(ch);
                }
                else if (char.IsDigit(ch))
                {
                    numVal = (int) char.GetNumericValue(ch);
                    magn = 1L;
                    tokenType = TokenType.Integer;
                    tokenStart = _pos;
                }
                else if (!IsWhiteSpace(ch)) // Alle Leerzeichen ignorieren
                {
                    if (tokenType != TokenType.Unknown)
                        break;
                    tokenType = TokenType.Identifier;
                    tokenStart = _pos;
                    stringVal.Append(ch);
                }

                prev = ch;
                _pos++;

                if (_pos == _expression.Length)
                    FetchMore();
            }

            _token = new Token(_expression.Substring(begin, _pos - begin), tokenType);

            _token.SignificantPart = IntegralRange.RelativeRange(tokenStart - begin, _pos - tokenStart);

            if (tokenType == TokenType.Integer)
                _token.Value = (long)numVal;
            else if (tokenType == TokenType.Decimal)
                _token.Value = numVal / magn;
            else
                _token.Value = stringVal.ToString();

            return tokenType != TokenType.Unknown;
        }

        /// <summary>
        /// Prüft ob <paramref name="ch"/> WhiteSpace ist.
        /// Diese Methode prüft auch auf das '¶' und '¤' zeichen, welches Whitespace ist aber nicht mit <see cref="char.IsWhiteSpace(char)"/> gefunden wird.
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private static bool IsWhiteSpace(char ch)
        {
            return char.IsWhiteSpace(ch) || Equals('¶', ch) || Equals('¤', ch);
        }
    }
}