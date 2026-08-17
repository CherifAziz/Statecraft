using System;
using System.Globalization;

namespace Statecraft.UI.Formatting
{
    public static class StatecraftValueFormatter
    {
        private static readonly CultureInfo FrenchCulture = CultureInfo.GetCultureInfo("fr-FR");

        public static string FormatPopulation(long population)
        {
            if (population >= 1_000_000L)
            {
                return $"{(population / 1_000_000d).ToString("0.#", FrenchCulture)} M";
            }

            if (population >= 1_000L)
            {
                return $"{(population / 1_000d).ToString("0.#", FrenchCulture)} K";
            }

            return population.ToString("N0", FrenchCulture);
        }

        public static string FormatUsd(double valueUsd)
        {
            var absoluteValue = Math.Abs(valueUsd);
            if (absoluteValue >= 1_000_000_000_000d)
            {
                return $"{(valueUsd / 1_000_000_000_000d).ToString("0.##", FrenchCulture)} T$";
            }

            if (absoluteValue >= 1_000_000_000d)
            {
                return $"{(valueUsd / 1_000_000_000d).ToString("0.#", FrenchCulture)} Md$";
            }

            if (absoluteValue >= 1_000_000d)
            {
                return $"{(valueUsd / 1_000_000d).ToString("0.#", FrenchCulture)} M$";
            }

            return $"{valueUsd.ToString("N0", FrenchCulture)} $";
        }

        public static string FormatPercentage(float value)
        {
            return $"{value.ToString("0.#", FrenchCulture)} %";
        }

        public static string FormatGameDate(DateTime date)
        {
            return date.ToString("d MMMM yyyy", FrenchCulture).ToUpper(FrenchCulture);
        }
    }
}
