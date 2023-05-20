using Newtonsoft.Json;


using System.IO;


namespace GNFSPoly
{
    public static partial class Serialization
    {
        static IGnfsPolyReader gnfsPolyReader = GnfsPolyReader.Default;

        public static class Load
        {
            public static T Generic<T>(string path)
            {
                var result = JsonConvert.DeserializeObject<T>(path);
                return result;
            }

            public static GnfsPolynomial GnfsJson(string path)
                => Generic<GnfsPolynomial>(path);


            public static GnfsPolynomial GnsfPolyFromFile(string path)
            {
                return gnfsPolyReader.ReadFile(path);
            }

            public static GnfsPolynomial GnsfPolyFromString(string contents)
            {
                return gnfsPolyReader.ReadString(contents);
            }
        }

        public static class Save
        {
            public static string GnfsPolyToString(GnfsPolynomial value)
            {
                return gnfsPolyReader.WriteToString(value);
            }

            public static void ToGnfsPolyFile(string path, GnfsPolynomial value)
            {
                gnfsPolyReader.WriteToFile(path, value);
            }

            public static void ToGnfsJson(string path, GnfsPolynomial value)
            {
                var json = JsonConvert.SerializeObject(value);
                File.WriteAllText(path, json);
            }

            public static void Generic<T>(string path, T value)
            {
                var json = JsonConvert.SerializeObject(value);
                File.WriteAllText(path, json);
            }
        }

    }
}
