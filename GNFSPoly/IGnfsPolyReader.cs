namespace GNFSPoly
{
    public interface IGnfsPolyReader
    {
        GnfsPolynomial ReadFile(string path);
        void WriteToFile(string path, GnfsPolynomial polynomial);
        GnfsPolynomial ReadString(string value);
        string WriteToString(GnfsPolynomial polynomial);
    }
}
