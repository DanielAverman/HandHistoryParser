using HandHistoryParser.exception;
using System.ComponentModel.Design;
using System.Globalization;

namespace HandHistoryParser.utils
{
    internal class Converter
    {
        internal static long ParseLong(string text, int startIndex) => ParseLong(text, startIndex, GetDigitsSequenceLastIndex(text, startIndex));

        internal static long ParseLong(string text, int startIndex, int endIndex)
        {
            string numberStringValue = text[startIndex..endIndex];
            return long.TryParse(numberStringValue, out long result)
                ? result 
                : throw new DataConvertionException("Could not convert " + numberStringValue + " into long type.");
        }

        internal static int ParseInt(string text)
        {
            return ParseInt(text, 0);
        }

        internal static int ParseInt(string text, int startIndex) => ParseInt(text, startIndex, GetDigitsSequenceLastIndex(text, startIndex));

        internal static int ParseInt(string text, int startIndex, int endIndex)
        {
            string numberStringValue = text[startIndex..endIndex];
            return int.TryParse(numberStringValue, out int result) 
                ? result 
                : throw new DataConvertionException("Could not convert " + numberStringValue + " into int type.");
        }

        internal static decimal ParseDecimal(string text)
        {
            return ParseDecimal(text, 0);
        }

        internal static decimal ParseDecimal(string text, int startIndex)
        {
            return ParseDecimal(text, startIndex, GetDecimalNumberLastIndex(text, startIndex));
        }

        internal static decimal ParseDecimal(string text, int startIndex, int endIndex)
        {
            string numberStringValue = text[startIndex..endIndex];
            return decimal.TryParse(numberStringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result)
                ? result 
                : throw new DataConvertionException("Could not convert " + numberStringValue + " into decimal type.");
        }

        private static int GetDigitsSequenceLastIndex(string text, int startIndex)
        {
            if (!char.IsDigit(text[startIndex]))
            {
                throw new ArgumentException("Could not convert as char at startIndex is not a digit['" + text[startIndex] + "'].");
            }

            int endIndex = startIndex + 1;
            for (; endIndex < text.Length && char.IsDigit(text[endIndex]); endIndex++) ;

            return endIndex;
        }

        private static int GetDecimalNumberLastIndex(string text, int startIndex)
        {
            if (!char.IsDigit(text[startIndex]))
            {
                throw new ArgumentException("Could not convert as char at startIndex is not a digit['" + text[startIndex] + "'].");
            }

            int endIndex = startIndex + 1;
            for (; endIndex < text.Length && 
                (char.IsDigit(text[endIndex]) || endIndex == text.IndexOf('.', startIndex)); 
                endIndex++);

            return endIndex;
        }
    }
}
