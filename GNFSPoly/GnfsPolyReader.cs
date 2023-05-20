using System;


using System.IO;
using System.Linq.Expressions;
using System.Numerics;


namespace GNFSPoly
{
    public partial class GnfsPolyReader
        : IGnfsPolyReader
    {
        public static IGnfsPolyReader Default { get; internal set; } = new GnfsPolyReader();

        public GnfsPolynomial ReadFile(string path)
        {
            var text = File.ReadAllText(path);
            return ReadString(text);
        }


        //TODO: Unit test me.
        public GnfsPolynomial ReadString(string text)
        {
            //nomalize test into string of space seperated values
            var normalized = (text + "\n")
                .Replace(": ", " ")
                .Replace("# ", " ")
                .Replace("\r\n", "\n")
                .Replace("\n", " ");


            var result = new GnfsPolynomial();

            //reads the next space seperated value after the first occurrence of the property name token.
            //method then calls a converter to parse the value as T.
            T ReadValue<T>(string propertyName, Func<string, T> parser, bool required = false)
            {
                string value = null;
                var token = $"{propertyName} ";
                var idx = normalized.IndexOf(token, StringComparison.OrdinalIgnoreCase);

                if (idx < 0 && !required)
                    return default(T);
                else if (idx < 0)
                    throw new Exception($"Required property {propertyName} missing");
                else
                {
                    var endIndex = normalized.IndexOf(' ', (idx = idx + token.Length));
                    value = normalized.Substring(idx, endIndex - idx);
                    T ret;
                    try
                    {
                        ret = parser(value);
                    }
                    catch (Exception ex)
                    {
                        var msg = $"Error parsing {propertyName} from value '{value}' - {ex}";
                        throw new Exception(msg, ex);
                    }
                    return ret;
                }
            }

            // convience function to retrive the property name specified in the lambda, parse the value, and set the property.
            bool ParseProperty<T>(
                Func<string, T> parser,
                Expression<Func<GnfsPolynomial, T>> setIt,
                bool required = true)
            {

                var propertyName = PropertyHelper.GetPropertyName(setIt);
                var value = ReadValue(propertyName, parser, required);
                if ((object)value != null)
                {
                    var setter = PropertyHelper.CreateSetter(setIt);
                    setter(result, value);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // Nullable overload for optional BigInteger Coefficents.
            BigInteger? OptionalParse(string value) => (value is null) ? (BigInteger?)null : BigInteger.Parse(value);


            ParseProperty(BigInteger.Parse, x => result.N);
            ParseProperty(Double.Parse, x => result.Norm);
            ParseProperty(Double.Parse, x => result.Alpha);
            ParseProperty(Double.Parse, x => result.E);
            ParseProperty(Int32.Parse, x => result.RRoots);
            ParseProperty(Double.Parse, x => result.Skew);
            ParseProperty(BigInteger.Parse, x => result.C0);



            //TODO: confirm minimum required number of coefficents and maximum optional coeffs
            //  For time being hard code to allow up to 9.

            //Parse optional coefficents, short-circuiting ater first not found coefficent
            var parsedOptional =
                ParseProperty(OptionalParse, x => result.C1, false)
                && ParseProperty(OptionalParse, x => result.C2, false)
                && ParseProperty(OptionalParse, x => result.C3, false)
                && ParseProperty(OptionalParse, x => result.C4, false)
                && ParseProperty(OptionalParse, x => result.C5, false)
                && ParseProperty(OptionalParse, x => result.C6, false)
                && ParseProperty(OptionalParse, x => result.C7, false)
                && ParseProperty(OptionalParse, x => result.C8, false)
                && ParseProperty(OptionalParse, x => result.C9, false);


            ParseProperty(BigInteger.Parse, x => result.Y0);
            ParseProperty(BigInteger.Parse, x => result.Y1);
            ParseProperty(s => s, x => result.Type);


            return result;

        }
        public void WriteToFile(string path, GnfsPolynomial polynomial)
        {
            var contents = WriteToString(polynomial);
            File.WriteAllText(path, contents);
        }

        public string WriteToString(GnfsPolynomial polynomial)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"n: {polynomial.N}# norm {polynomial.Norm.ToLowerE()} alpha {polynomial.Alpha.ToLowerE()} e {polynomial.E.ToLowerE()} rroots {polynomial.RRoots}\n");
            sb.Append($"skew: {polynomial.Skew.ToLowerE()}\n");
            sb.Append($"c0: {polynomial.C0}\n");
            var coeffs = polynomial.Coefficients;
            for (var i = 1; i < coeffs.Length; i++)
            {
                if (coeffs[i] != null)
                {
                    sb.Append($"c{i}: {coeffs[i]}\n");
                }
            }
            sb.Append($"Y0: {polynomial.Y0}\n");
            sb.Append($"Y1: {polynomial.Y1}\n");
            sb.Append($"type: {polynomial.Type}\n");
            var result = sb.ToString();
            return result;
        }
    }
}
