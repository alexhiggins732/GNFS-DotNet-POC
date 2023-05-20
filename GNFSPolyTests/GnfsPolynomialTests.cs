using Microsoft.VisualStudio.TestTools.UnitTesting;
using GNFSPoly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GNFSPoly.Tests
{
    [TestClass()]
    public class GnfsPolynomialTests
    {
        string UnitTestData = nameof(UnitTestData);
        string Polynomials = nameof(Polynomials);

        string dataDirectory = null;
        string polyDirectory = null;


        const string rsa100PolyJsonFile = "rsa100.poly.json";

        const string rsa100PolyFile = "rsa100.poly";

        const string rsa250PolyJsonFile = "rsa250.poly.json";
        const string rsa250PolyFile = "rsa250.poly";

        public GnfsPolynomialTests()
        {
            var d = Path.GetFullPath(".");
            var di = new DirectoryInfo(d);
            DirectoryInfo dataDirectory = null;

            while (di != null)
            {
                dataDirectory = di
                    .GetDirectories()
                    .FirstOrDefault(x => x.Name == UnitTestData);

                if (dataDirectory != null)
                    break;

                di = di.Parent;
            }

            if (dataDirectory is null)
                throw new Exception($"Folder {UnitTestData} not found in parent directory tree.");


            var polyDir = dataDirectory
                .GetDirectories()
                .FirstOrDefault(x => x.Name == Polynomials);

            if (polyDir is null)
                throw new Exception($"Folder {Polynomials} not found in folder {UnitTestData}.");

            this.dataDirectory = dataDirectory.FullName;
            this.polyDirectory = polyDir.FullName;
        }

        [TestMethod()]
        public void Load_Rsa100JsonPoly_Test()
        {
            var fileName = rsa100PolyJsonFile;
            var filePath = Path.Combine(polyDirectory, fileName);
            Assert.IsTrue(File.Exists(filePath),
                $"Test file '{fileName}' does not exist in directory '{filePath}'");

            var json = File.ReadAllText(filePath);
            var gnfsPoly = GnfsPolynomial.LoadFromJson(json);
            Assert.IsNotNull(gnfsPoly);

            var polyString = gnfsPoly.ToString();
            var expected = "f(x) = 58495626606615351512386040384 - 5772374246691267911604x - 8833629323103645x^2 + 595383944x^3 + 300x^4";
            //"f(x) = 3173523767811864034448660721125921529107975704636740160\r\n- 352715789847349253776461522519367952677500900736x\r\n- 67695972675816169807789389533909825279660x^2\r\n+ 1330431520915892806434003673850422x^3\r\n+ 316345805996221928737510484x^4\r\n- 1288745654147765x^5\r\n+ 2640x^6";
            Assert.AreEqual(expected, polyString);


            var polyPath = Path.Combine(polyDirectory, rsa100PolyFile);
            GnfsPolynomial.SaveToGnfsPoly(polyPath, gnfsPoly);
        }

        [TestMethod()]
        public void Load_Rsa100Poly_Test()
        {
            var fileName = rsa100PolyFile;
            var filePath = Path.Combine(polyDirectory, fileName);
            Assert.IsTrue(File.Exists(filePath),
                $"Test file '{fileName}' does not exist in directory '{filePath}'");

            var contents = File.ReadAllText(filePath);
            var gnfsPoly = GnfsPolynomial.LoadGnfsPolyFromString(contents);
            Assert.IsNotNull(gnfsPoly);

            var polyString = gnfsPoly.ToString();
            var expected = "f(x) = 58495626606615351512386040384 - 5772374246691267911604x - 8833629323103645x^2 + 595383944x^3 + 300x^4";
            Assert.AreEqual(expected, polyString);
        }



        [TestMethod()]
        public void Load_Rsa250GnfsPoly_Test()
        {
            var fileName = rsa250PolyFile;
            var filePath = Path.Combine(polyDirectory, fileName);
            Assert.IsTrue(File.Exists(filePath),
                $"Test file '{fileName}' does not exist in directory '{filePath}'");


            var gnfsPoly = GnfsPolynomial.LoadGnfsPolyFromFile(filePath);
            Assert.IsNotNull(gnfsPoly);

            var polyString = gnfsPoly.ToString();
            var expected = "f(x) = 3173523767811864034448660721125921529107975704636740160 - 352715789847349253776461522519367952677500900736x - 67695972675816169807789389533909825279660x^2 + 1330431520915892806434003673850422x^3 + 316345805996221928737510484x^4 - 1288745654147765x^5 + 2640x^6";
            Assert.AreEqual(expected, polyString);

            var actualContent = GnfsPolynomial.ToGnfsPolyString(gnfsPoly);

            var expectedContent = File.ReadAllText(filePath);
            Assert.AreEqual(expectedContent, actualContent);

            var jsonPath = Path.Combine(polyDirectory, rsa250PolyJsonFile);

            GnfsPolynomial.SaveToGnfsJson(jsonPath, gnfsPoly);
        }

        [TestMethod()]
        public void Load_Rsa250JsonPoly_Test()
        {
            var fileName = rsa250PolyJsonFile;
            var filePath = Path.Combine(polyDirectory, fileName);
            Assert.IsTrue(File.Exists(filePath),
                $"Test file '{fileName}' does not exist in directory '{filePath}'");

            var json = File.ReadAllText(filePath);
            var gnfsPoly = GnfsPolynomial.LoadFromJson(json);
            Assert.IsNotNull(gnfsPoly);

            var polyString = gnfsPoly.ToString();
            var expected = "f(x) = 3173523767811864034448660721125921529107975704636740160 - 352715789847349253776461522519367952677500900736x - 67695972675816169807789389533909825279660x^2 + 1330431520915892806434003673850422x^3 + 316345805996221928737510484x^4 - 1288745654147765x^5 + 2640x^6";
            Assert.AreEqual(expected, polyString);
        }
    }
}