using System.Collections.Generic;
using System.Linq;

namespace TableGenerator
{
    public class TokenList : List<Token>
    {
        public bool IsEmpty { get { return Count == 0; } }

        public TokenList()
        {
        }

        public TokenList(IList<Token> tokenList)
            : base(tokenList)
        {
        }

        public override string ToString()
        {
            return string.Join("", this.Select(t => t.Text));
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
                return false;

            return Equals(obj as TokenList);
        }

        private bool Equals(TokenList tokenList)
        {
            if (tokenList == null)
                return false;

            if (Count != tokenList.Count)
                return false;

            for (var i = 0; i < tokenList.Count; ++i)
            {
                if (this[i] != tokenList[i])
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return this.Aggregate(0, (current, token) => current ^ token.GetHashCode());
        }
    }
}