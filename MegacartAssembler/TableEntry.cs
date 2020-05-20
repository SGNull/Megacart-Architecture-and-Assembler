using System;
using System.Text.RegularExpressions;

namespace MegacartAssembler
{
    class TableEntry
    {
        //Big shoutout to https://stackoverflow.com/questions/1271562/binary-string-to-integer
        private static readonly Regex Binary = new Regex("^[01]{1,32}$", RegexOptions.Compiled);

        public bool? IsLine { get; set; }

        public string Keyword { get; set; }

        private string _value;
        public string Value
        {
            get {return _value;}
            set
            {
                if (!Binary.IsMatch(value))
                    throw new FormatException("Not binary input value for Table Entry: Keyword-" + Keyword);

                if (value.Length > 6)
                    throw new OverflowException("Input value too large for Table Entry: Keyword-" + Keyword);

                int zeros;
                if (IsLine == null)
                {
                    zeros = 0;
                }
                else if ((bool) IsLine)
                {
                    zeros = 6 - value.Length;
                }
                else
                {
                    if (value.Length > 3)
                        throw new OverflowException("Input value too large for tribble (3-bit) Table Entry: Keyword-" + Keyword);
                    zeros = 3 - value.Length;
                }

                string newValue = value;
                while (zeros > 0)
                {
                    newValue = 0 + newValue;
                    zeros--;
                }

                _value = newValue;
            }
        }

        public TableEntry(string keyword, string value)
        {
            this.IsLine = null;
            this.Keyword = keyword;
            this.Value = value;
        }

        public TableEntry(string keyword, int value)
        {
            string valueString = Convert.ToString(value, 2);

            this.IsLine = null;
            this.Keyword = keyword;
            this.Value = valueString;
        }

        public TableEntry(string keyword, int value, bool? isLine)
        {
            string valueString = Convert.ToString(value, 2);

            this.IsLine = isLine;
            this.Keyword = keyword;
            this.Value = valueString;
        }

        public TableEntry(string keyword, string value, bool? isLine)
        {
            this.IsLine = isLine;
            this.Keyword = keyword;
            this.Value = value;
        }

        public int GetValueAsInt()
        {
            int output = Convert.ToInt32(Value);
            return output;
        }

        public bool HasKeyword(string keyword)
        {
            return Keyword == keyword;
        }

        public bool HasValue(string value)
        {
            return Value == value;
        }
    }
}
