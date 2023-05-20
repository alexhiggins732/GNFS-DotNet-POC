using System.IO;


namespace GNFSPoly
{
    public partial class GnfsPolynomial
    {
        public static GnfsPolynomial LoadFromJson(string json)
        {
            return Serialization.Load.Generic<GnfsPolynomial>(json);
        }

        public static GnfsPolynomial LoadFromJsonFile(string path)
        {
            return Serialization.Load.Generic<GnfsPolynomial>
                (File.ReadAllText(path));
        }

        public static GnfsPolynomial LoadGnfsPolyFromString(string contents)
        {
            return Serialization.Load.GnsfPolyFromString(contents);
        }

        public static GnfsPolynomial LoadGnfsPolyFromFile(string path)
        {
            return Serialization.Load.GnsfPolyFromFile(path);
        }

        public static string ToGnfsPolyString(GnfsPolynomial poly)
        {
            return Serialization.Save.GnfsPolyToString(poly);
        }

        public static void SaveToGnfsPoly(string path, GnfsPolynomial poly)
            => Serialization.Save.ToGnfsPolyFile(path, poly);

        public static void SaveToGnfsJson(string path, GnfsPolynomial poly)
            => Serialization.Save.ToGnfsJson(path, poly);
    }
}
