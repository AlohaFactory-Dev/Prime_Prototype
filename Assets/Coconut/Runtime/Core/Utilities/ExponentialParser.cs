using System.Numerics;
using System.Text.RegularExpressions;

public static class ExponentialParser
{
    public static bool TryParse(string exponentialString, out BigInteger result)
    {
        if (Regex.IsMatch(exponentialString, "[0-9]\\.[0-9]+E\\+[0-9]+"))
        {
            var split = exponentialString.Split('E');
            var fractionalPartLength = split[0].Length - 2;
            var exponential = -fractionalPartLength + int.Parse(split[1]);

            result = (BigInteger)(int.Parse(split[0].Remove(1, 1)));
            while (exponential > 0)
            {
                result *= 10000000000;
                exponential -= 10;
            }

            while (exponential < 0)
            {
                result /= 10;
                exponential++;
            }

            return true;
        }

        return false;
    }

    public static string ToExponential(BigInteger bigInteger)
    {
        var bigIntegerString = bigInteger.ToString();
        if (bigIntegerString.Length <= 4) return bigIntegerString;

        var integerPart = bigIntegerString.Substring(0, 1);
        var fractionalPart = bigIntegerString.Substring(1, 3);
        var fractionalPartLength = bigIntegerString.Length - 1;
        var exponential = fractionalPartLength;

        return $"{integerPart}.{fractionalPart}E+{exponential}";
    }
}