using System;
using System.Text.RegularExpressions;

//I am unsure whether this class will actually prove to be useful or not.
//I want some way of going back and forth between binary and integers, and I figured this would be the best way to go about it.
//However, every number will require two fields: the actual value, and the length.

namespace Megacart_Assembler
{
    public struct CustomBinary
    {
        //Big shoutout to https://stackoverflow.com/questions/1271562/binary-string-to-integer and https://stackoverflow.com/questions/2954962/convert-integer-to-binary-in-c-sharp
        private static readonly Regex Binary = new Regex("^[01]{1,32}$", RegexOptions.Compiled);

        private int _length;
        public int Length
        {
            get { return this._length; }
            set { this._length = value; }
        }

        private string _value;
        public string Value
        {
            get { return this._value; }
            set
            {
                if (!Binary.IsMatch(value))
                    throw new FormatException();

                if (value.Length > Length)
                    throw new OverflowException();

                string newValue = value;
                int zeros = Length - value.Length;

                while (zeros > 0)
                {
                    newValue = "0" + newValue;
                    zeros--;
                }

                this._value = newValue;
            }
        }

        public CustomBinary(string value, int length) : this()
        {
            Length = length;
            Value = value;
        }

        public CustomBinary(int value, int length) : this()
        {
            Length = length;
            string stringValue = Convert.ToString(value, 2);
            Value = stringValue;
        }

        public int ToInt()
        {
            int integerValue = Convert.ToInt32(Value, 2);
            return integerValue;
        }
    }
}