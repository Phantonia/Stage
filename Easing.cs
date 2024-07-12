using System;

namespace Stage
{
    internal static class Easing
    {
        public static double EaseInOutCubic(double x)
        {
            return x < 0.5
                ? 4 * x * x * x
                : 1 - Math.Pow(-2 * x + 2, 3) / 2;
        }

        public static double EaseIntoIndentiyCubic(double x)
        {
            return -x * x * x + 2 * x * x;
        }
    }
}
