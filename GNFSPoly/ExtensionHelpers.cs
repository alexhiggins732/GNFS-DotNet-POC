using System.Numerics;


namespace GNFSPoly
{
    public static class ExtensionHelpers
    {

        public static string ToCoeffString(this BigInteger value, int degree)
        {
            var sign = degree == 0 ? "" : (value.Sign < 0 ? "-" : "+");
            var result = $"{(degree == 0 ? "" : $"{sign} ")}{BigInteger.Abs(value)}";
            var d = degree == 0 ? "" : (degree == 1 ? "x" : $"x^{degree}");
            return $"{result}{d}";
        }

        public static string ToCoefficientString(this BigInteger? value, int degree)
            => value is null ? "" : value.Value.ToCoeffString(degree);

        public static string ToLowerE(this double value) => value.ToString().Replace('E', 'e');
    }
}
