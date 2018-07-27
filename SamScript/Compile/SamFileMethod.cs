using System;
using System.Collections.Generic;
using System.Text;

namespace SamScript.Compile
{
    public class SamFileMethod : SamFileField
    {
        public List<Token> Tokens = new List<Token>();
        public List<SamFileField> Arguments = new List<SamFileField>();
    }
}
