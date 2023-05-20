using System.Linq;
using System.Numerics;


namespace GNFSPoly
{
    /// <summary>
    /// A model class representing the .poly file format widely used in Gnfs implementations such as GGNFS and msieve.
    /// </summary>
    public partial class GnfsPolynomial
    {
        public BigInteger N { get; set; }
        public double Norm { get; set; }
        public double Alpha { get; set; }
        public double E { get; set; }
        public int RRoots { get; set; }
        public double Skew { get; set; }
        public BigInteger C0 { get; set; }
        public BigInteger? C1 { get; set; }
        public BigInteger? C2 { get; set; }
        public BigInteger? C3 { get; set; }
        public BigInteger? C4 { get; set; }
        public BigInteger? C5 { get; set; }
        public BigInteger? C6 { get; set; }
        public BigInteger? C7 { get; set; }
        public BigInteger? C8 { get; set; }
        public BigInteger? C9 { get; set; }
        public BigInteger Y0 { get; set; }
        public BigInteger Y1 { get; set; }
        public string Type { get; set; }


        /// <summary>
        /// A convenience property to retrieve each coefficient set from <see cref="C0"/> to CN that is not null.
        /// </summary>
        public BigInteger[] Coefficients
            => (new[] { C0, C1, C2, C3, C4, C5, C6, C7, C8, C9 })
                .Where(x => x != null)
                .Select(x => x.Value)
            .ToArray();



        /// <summary>
        /// Returns the <see cref="GnfsPolynomial"/> formatted as f(x) = c0 + c1x + c2x^2 + c3x^3 + c4x^4 + c5x^5 + c6x^6 ... c6n^n
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = "f(x) = {}";
            var coeffs = Coefficients;
            if (coeffs.Length > 0)
            {
                var tail = Enumerable.Range(0, Coefficients.Length)
               .Select(i => Coefficients[i].ToCoeffString(i));
                result = $"f(x) = {string.Join(" ", tail)}";
            }
            return result;
        }
    }
}
