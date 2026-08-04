namespace Masterdom.Core.Validation;

/// <summary>
/// Provides common argument validation helpers.
/// </summary>
public static class Guard
{
    public static class Against
    {
        public static T Null<T>(T? value, string paramName)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(value, paramName);
            return value;
        }

        public static string NullOrWhiteSpace(string? value, string paramName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
            return value;
        }

        public static Guid Empty(Guid value, string paramName)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("Value cannot be empty.", paramName);

            return value;
        }

        public static T Default<T>(T value, string paramName)
            where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(value, default))
                throw new ArgumentException("Value cannot be the default value.", paramName);

            return value;
        }

        public static int Negative(int value, string paramName)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(paramName);

            return value;
        }

        public static int NegativeOrZero(int value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(paramName);

            return value;
        }

        public static int OutOfRange(
            int value,
            int minimum,
            int maximum,
            string paramName)
        {
            if (value < minimum || value > maximum)
                throw new ArgumentOutOfRangeException(paramName);

            return value;
        }
    }
}
