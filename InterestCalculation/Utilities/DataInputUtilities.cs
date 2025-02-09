using System.Globalization;

namespace InterestCalculation.Utilities
{
    internal class DataInputUtilities
    {
        private static string format = "dd/MM/yyyy"; // Define the format of the input string
        private static readonly CultureInfo culture = new CultureInfo("el-GR");

        internal static DateOnly ConvertToDateOnly(string DateString)
        {
            bool dateSuccess = DateOnly.TryParseExact(DateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly resultDate);

            return resultDate;
        }

        internal static decimal ConvertToDecimal(string input)
        {
            var conversionSuccess = decimal.TryParse(input, NumberStyles.AllowDecimalPoint, culture, out decimal result);

            return result;
        }
    }
}