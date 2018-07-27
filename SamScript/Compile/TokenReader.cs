using System;
using System.Collections.Generic;
using System.Text;

namespace SamScript.Compile
{
    public class TokenReader : IDisposable
    {
        public string[] Words;
        private int _pos;
        public TokenReader(string source)
        {
            var wordList = new List<string>();
            bool inQuote = false;
            var builder = new StringBuilder();

            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == '"')
                {
                    if(inQuote)
                    {
                        builder.Append(source[i]);

                        wordList.Add(builder.ToString());
                        builder = new StringBuilder();

                        inQuote = true;
                    }
                    else
                    {
                        if (builder.Length > 0)
                        {
                            wordList.Add(builder.ToString());
                            builder = new StringBuilder();
                        }
                        builder.Append(source[i]);

                        inQuote = false;
                    }
                    inQuote = !inQuote;
                }
                else
                {
                    if(inQuote)
                    {
                        builder.Append(source[i]);
                    }
                    else
                    {
                        if (char.IsWhiteSpace(source[i]))
                        {
                            if (builder.Length > 0)
                            {
                                wordList.Add(builder.ToString());
                                builder = new StringBuilder();
                            }
                            continue;
                        }
                        else if (!char.IsLetterOrDigit(source[i]))
                        {
                            if (builder.Length > 0)
                            {
                                wordList.Add(builder.ToString());
                                builder = new StringBuilder();
                            }
                            wordList.Add(source[i].ToString());
                        }
                        else
                        {
                            builder.Append(source[i]);
                        }                            
                    }
                }
            }

            if (builder.Length > 0)
                wordList.Add(builder.ToString());

            Words = wordList.ToArray();

        }

        /// <summary>
        /// func- return false to break
        /// </summary>
        /// <param name="interateAction"></param>
        public void Do(Func<bool> interateAction)
        {
            if (interateAction == null)
                return;

            do
            {
                if (string.IsNullOrWhiteSpace(Current))
                    continue;
                if (!interateAction())
                    break;

            } while (MoveNext());
        }

        public bool MoveNext(int count = 1)
        {
            _pos += count;
            return _pos < Words.Length;
        }

        public string Current { get { return Words[_pos]; } }
        public bool CanMoveNext { get { return _pos < Words.Length; } }
        public string Next { get { return Words[_pos + 1]; } }

        public decimal GetValue()
        {
            return decimal.Parse(Words[_pos]);
        }

        public bool IsNumberLiteral()
        {
            string x = Words[_pos];
            
            if (x.Length == 0)
                return false;
            int TotalDots = 0;
            for (int i = 0; i < x.Length; i++)
            {
                if (!char.IsNumber(x[i]))
                {
                    if (x[i] == '.')
                    {
                        TotalDots++;
                        if (TotalDots == 1)
                            continue;
                    }
                    return false;
                }
            }

            return true;
        }

        public bool EqualTo(params string[] words)
        {
            int Total = 0;
            int index = 0;
            for (int i = _pos; i < _pos + words.Length && i < Words.Length; i++)
            {
                if (words[index].ToLower() == Words[i].ToLower())
                    Total++;
                else
                    return false;
                index++;
            }
            return Total == words.Length;
        }

        public void Dispose()
        {
        }
    }
}
