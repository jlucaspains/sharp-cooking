using System;

namespace SharpCooking.Data
{
    public static class Fraction
    {
        public static (decimal Numerator, decimal Denomimator) Get(decimal num, decimal epsilon = 0.0001m, int maxIterations = 20)
        {
            decimal[] d = new decimal[maxIterations + 2];
            d[1] = 1;
            decimal z = num;
            decimal n = 1;
            int t = 1;

            decimal decimalNumberPart = num;

            while (t < maxIterations && Math.Abs(n / d[t] - num) > epsilon)
            {
                t++;
                z = 1 / (z - (int)z);
                d[t] = d[t - 1] * (int)z + d[t - 2];
                n = (int)(decimalNumberPart * d[t] + 0.5m);
            }

            return (n, d[t]);
        }
    }
}
