using System;
using System.Text.RegularExpressions;

namespace Megacart_Assembler
{
    class TableEntry
    {
        public string Keyword { get; set; }

        private string value;
        public string Value
        {
            get {return value;}
            set
            {
                if (!binary.IsMatch(value))
                {
                    Console.WriteLine("Improper input value for Table Entry: Keyword-" + Keyword);
                    throw new FormatException();
                }
                else if (value.Length > 6)
                {
                    Console.WriteLine("Input value too large for Table Entry: Keyword-" + Keyword);
                    throw new OverflowException();
                }
                else
                {
                    this.value = value;
                }
            }
        }

        //Big shoutout to https://stackoverflow.com/questions/1271562/binary-string-to-integer
        private static readonly Regex binary = new Regex("^[01]{1,32}$", RegexOptions.Compiled);
        public TableEntry(string keyword, string value)
        {
            this.Keyword = keyword;
            this.Value = value;
        }

        public int GetValueAsInt()
        {
            int output = Convert.ToInt32(Value);
            return output;
        }
    }
}
