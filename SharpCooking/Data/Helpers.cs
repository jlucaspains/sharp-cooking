using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SharpCooking.Data
{
    public static class Helpers
    {
        public static (decimal Numerator, decimal Denomimator) GetFraction(decimal num, decimal epsilon = 0.0001m, int maxIterations = 20)
        {
            decimal[] d = new decimal[maxIterations + 2];
            d[1] = 1;
            decimal z = num;
            decimal n = 1;
            int t = 1;

            decimal decimalNumberPart = num;

            while (t < maxIterations && Math.Abs((n / d[t]) - num) > epsilon)
            {
                t++;
                z = 1 / (z - (int)z);
                d[t] = (d[t - 1] * (int)z) + d[t - 2];
                n = (int)((decimalNumberPart * d[t]) + 0.5m);
            }

            return (n, d[t]);
        }

        public static string ApplyMultiplier(string input, decimal multiplier, bool useFractionsOverDecimal, string regex)
        {
            multiplier = multiplier > 0 ? multiplier : 1;

            var regexResult = Regex.Replace(input, regex, (match) =>
            {
                var compositeFractionGroup = match.Groups["CompositeFraction"];
                var fractionGroup = match.Groups["Fraction"];
                var regularGroup = match.Groups["Regular"];
                var originalFraction = string.Empty;
                decimal parsedMatch = 0;

                if (compositeFractionGroup.Success)
                {
                    originalFraction = compositeFractionGroup.Value;
                    var parts = compositeFractionGroup.Value.Split(' ');
                    var first = parts[0];
                    var second = parts[1];

                    var fractionParts = second.Split('/');

                    var wholeResult = decimal.TryParse(first, out var firstNumber);
                    var numeratorResult = decimal.TryParse(fractionParts[0], out var fracNumerator);
                    var fracResult = decimal.TryParse(fractionParts[1], out var fracDecimal);

                    if (!numeratorResult || !fracResult || !wholeResult)
                        return first;

                    parsedMatch = firstNumber + (fracNumerator / fracDecimal);
                }
                else if (fractionGroup.Success)
                {
                    originalFraction = fractionGroup.Value;

                    var parts = fractionGroup.Value.Split('/');
                    var numeratorResult = decimal.TryParse(parts[0], out var fracNumerator);
                    var fracResult = decimal.TryParse(parts[1], out var fracDecimal);

                    if (!numeratorResult || !fracResult)
                        return "0";

                    parsedMatch = fracNumerator / fracDecimal;
                }
                else
                {
                    var parseResult = decimal.TryParse(regularGroup.Value, out parsedMatch);

                    if (!parseResult)
                        return "0";
                }

                var newIngredientValue = Math.Round(parsedMatch * multiplier, 2);

                if (!useFractionsOverDecimal)
                    return newIngredientValue.ToString("G29", CultureInfo.CurrentCulture);

                // HACK: we don't want to convert a fraction to number and back if it is not necessary
                if(multiplier == 1 && !string.IsNullOrEmpty(originalFraction))
                    return originalFraction;

                var whole = decimal.Floor(newIngredientValue);

                if (whole == newIngredientValue)
                {
                    return newIngredientValue.ToString("0", CultureInfo.CurrentCulture);
                }
                else
                {
                    (var numerator, var denominator) = Helpers.GetFraction(newIngredientValue - whole);
                    return whole == 0 ? $"{numerator}/{denominator}" : $"{whole:0} {numerator}/{denominator}";
                }
            }, RegexOptions.Multiline);

            return regexResult;
        }

        public static TimeSpan GetImpliedTimeFromString(string input, string timeIdentifierRegex)
        {
            var match = Regex.Match(input, timeIdentifierRegex);

            if (!match.Success)
                return TimeSpan.Zero;

            var minutesResult = int.TryParse(match.Groups["Minutes"]?.Value, out var minutes);
            var hoursResult = int.TryParse(match.Groups["Hours"]?.Value, out var hours);
            var daysResult = int.TryParse(match.Groups["Days"]?.Value, out var days);

            if (minutesResult || hoursResult || daysResult)
                return new TimeSpan(days, hours, minutes, 0);

            return TimeSpan.Zero;
        }

        public static IEnumerable<string> BreakTextIntoList(string input)
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(input))
                return result;

            return input.Split(new string[] { "\r\n", "\n\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)?
                .Where(Item => !string.IsNullOrEmpty(Item?.Trim()))?
                .ToList();
        }
    }
}
