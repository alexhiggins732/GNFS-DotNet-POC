using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Net;
using System.Numerics;
using System.Text;
using System.Xml.Linq;

namespace RsaChallengPolynomials
{
    public class RsaChallenge
    {
        public string Id;
        public BigInteger N;
        public BigInteger? P;
        public BigInteger? Q;
        public string Href = null!;
        public string ChallengeHtml = null!;
        public string ChallengeText = null!;
        public RsaChallenge(string id, string href, BigInteger n)
        {
            Id = id;
            N = n;
            Href = href;
        }
    }
    internal class Program
    {
        const string rsaChallengesUrl = "https://en.wikipedia.org/wiki/RSA_numbers";
        static void Main()
        {




            var doc = DownloadHtml(rsaChallengesUrl);

            var challengLinks = ExtractChallengeNumbers(doc);

            var challenges = new List<RsaChallenge>();
            foreach (var link in challengLinks)
            {
                var challenge = ExtractRsaChallenge(doc, link);
                challenges.Add(challenge);
            }

            var json = JsonConvert.SerializeObject(challenges, Formatting.Indented);
            File.WriteAllText("rsaChallenges.json", json);
            Console.WriteLine($"Save {challenges.Count} RSA challenge numbers successfully.");

            // create n files for polynomial search.
            var batchCommands = new List<string>();
            foreach (var challenge in challenges)
            {
                var content = $"n: {challenge.N}";
                var fileName = $"{challenge.Id}.n";
                batchCommands.Add($"factor-it {fileName}");
                SaveToWorkInputFile(fileName, content);
            }

            var polySelectCommand = string.Join(Environment.NewLine, batchCommands) + Environment.NewLine;
            SaveToWorkInputFile("PolySelectAll.bat", polySelectCommand);
        }

        private static RsaChallenge ExtractRsaChallenge(HtmlDocument doc, HtmlNode link)
        {

            //get the anchor for the challenge
            var linkHref = link.GetAttributeValue("href", null);
            if (linkHref == null)
                throw new Exception($"Couldn't find hrefId: {linkHref}");

            var href = $"{rsaChallengesUrl}{linkHref}";
            var id = linkHref.Substring(1);

            // go to the heading tag for the challenge
            var span = doc.GetElementbyId(id);
            var heading = span.ParentNode;

            var htmlBuilder = new StringBuilder();
            var textBuilder = new StringBuilder();

            htmlBuilder.Append(heading.OuterHtml);
            textBuilder.AppendLine($"* {heading.InnerText}");



            //find the rsa number by enumerating next siblings for a pre tag.
            var next = heading.NextSibling;
            while (next != null && next.Name != "pre")
            {
                htmlBuilder.Append(next.OuterHtml);
                if (!string.IsNullOrEmpty((next.InnerText ?? "").Trim()))
                    textBuilder.AppendLine(next.InnerText);
                next = next.NextSibling;
            }


            //Throw an error if the pre tag wasn't found.
            if (next is null)
                throw new Exception($"Couldn't find N for {id}");

            // Parse the RSA challenge number and create a result.
            BigInteger n = ExtractRsaNumber(next, id);
            var result = new RsaChallenge(id, href, n);



            // check to see if there is a factorization by enumerating next siblings for a pre tag.
            next = next.NextSibling;
            while (next != null && next.Name != "pre" && next.Name != "h2")
            {
                htmlBuilder.Append(next.OuterHtml);
                if (!string.IsNullOrEmpty((next.InnerText ?? "").Trim()))
                    textBuilder.AppendLine(next.InnerText);
                next = next.NextSibling;
            }


            // if we found a pre tag the factorization is known.
            if (next != null)
            {
                htmlBuilder.Append(next.OuterHtml);
                if (next.Name == "pre")
                    ExtractRsaFactors(next, result, id);

                next = next.NextSibling;
                while (next != null && next.Name != "h2")
                {
                    htmlBuilder.Append(next.OuterHtml);
                    if (!string.IsNullOrEmpty((next.InnerText ?? "").Trim()))
                        textBuilder.AppendLine(next.InnerText);
                    next = next.NextSibling;
                }
            }

            result.ChallengeHtml = htmlBuilder.ToString();
            result.ChallengeText = textBuilder.ToString();

            // return the result;
            return result;
        }


        private static BigInteger ExtractRsaNumber(HtmlNode node, string id)
        {
            // format RSA-{number} =  {digits}
            var text = (node.InnerText ?? "").Replace("\r", " ").Replace("\n", " ").Replace(" ", "").Trim();
            var idx = text.IndexOf('=');
            if (idx < 0)
                throw new Exception($"Failed to parse RSA number for {id} from {node}");

            var value = text.Substring(idx + 1).Trim();
            if (!BigInteger.TryParse(value, out BigInteger result))
                throw new Exception($"Failed to parse BigInteger N for {id} from value {value}");

            return result;
        }

        private static void ExtractRsaFactors(HtmlNode node, RsaChallenge challenge, string id)
        {
            // format RSA-{number} =  {p} × {q}" 
            var text = (node.InnerText ?? "").Replace("\r", " ").Replace("\n", " ").Trim();
            var idx = text.IndexOf('=');
            if (idx < 0)
                throw new Exception($"Failed to parse RSA {{p}} × {{q}} for {id} from {node}");

            var value = text.Substring(idx + 1).Trim();
            idx = value.IndexOf('×');
            if (idx < 0)
                throw new Exception($"Failed to parse RSA {{p}} × {{q}} for {id} from {node}");

            var left = value.Substring(0, idx).Replace(" ", "").Trim();
            var right = value.Substring(idx + 1).Replace(" ", "").Trim();



            if (!BigInteger.TryParse(left, out BigInteger p))
                throw new Exception($"Failed to parse BigInteger P for {id} from value {left}");

            if (!BigInteger.TryParse(right, out BigInteger q))
                throw new Exception($"Failed to parse BigInteger Q for {id} from value {right}");

            challenge.P = p;
            challenge.Q = q;

        }


        static HtmlDocument DownloadHtml(string url)
        {
            var html = Get(url);
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return document;

        }

        static string Get(string url)
            => new WebClient().DownloadString(url);


        static List<HtmlNode> ExtractChallengeNumbers(HtmlDocument doc)
        {
            var toc = doc.GetElementbyId("toc");
            var challenges = toc.QuerySelectorAll("tr")[1].QuerySelectorAll("a");
            return challenges.ToList();
        }


        static void SaveToWorkInputFile(string fileName, string content)
        {
            File.WriteAllText($"RsaChallengeInputFiles/{fileName}", content);
        }
    }
}