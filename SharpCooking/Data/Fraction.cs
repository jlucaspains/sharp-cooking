using System;

namespace SharpCooking.Data
{
    public static class Fraction
    {
        public static (long Numerator, long Denomimator) Get(decimal fraction, long maximumDenominator = 4096)
        {
            long a;
            var h = new long[3] { 0, 1, 0 };
            var k = new long[3] { 1, 0, 0 };
            long x, d, n = 1;
            int i, neg = 0;

            if (maximumDenominator <= 1)
            {
                return ((long)fraction, 1);
            }

            if (fraction < 0) { neg = 1; fraction = -fraction; }

            while (fraction != Math.Floor(fraction)) { n <<= 1; fraction *= 2; }
            d = (long)fraction;

            /* continued fraction and check denominator each step */
            for (i = 0; i < 64; i++)
            {
                a = (n != 0) ? d / n : 0;
                if ((i != 0) && (a == 0)) break;

                x = d; d = n; n = x % n;

                x = a;
                if (k[1] * a + k[0] >= maximumDenominator)
                {
                    x = (maximumDenominator - k[0]) / k[1];
                    if (x * 2 >= a || k[1] >= maximumDenominator)
                        i = 65;
                    else
                        break;
                }

                h[2] = x * h[1] + h[0]; h[0] = h[1]; h[1] = h[2];
                k[2] = x * k[1] + k[0]; k[0] = k[1]; k[1] = k[2];
            }

            return (neg != 0 ? -h[1] : h[1], k[1]);
        }
    }
}
