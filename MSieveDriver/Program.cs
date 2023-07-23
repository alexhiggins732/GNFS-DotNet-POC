using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MSieveDriver
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            if (args.Length == 0)
            {
                return;
            }
            if (args.Length == 2)
            {
                var script = args[0];
                var n = args[1];
                Console.WriteLine($"Running {n}");
                var current = Directory.GetCurrentDirectory();
                //var currentDir = @"C:\Source\Repos\NumTheory\gnfs\GFNS-DotNet-POC\MSieveDriver\bin\Debug\net6.0\";
                //var dataDir = @"C:\Source\Repos\NumTheory\gnfs\GFNS-DotNet-POC\UnitTestData\Polynomials\";
                //var rel = Path.GetRelativePath(currentDir, dataDir);

                var rel = @"..\..\..\..\UnitTestData\Polynomials\";
                Directory.SetCurrentDirectory(rel);
                var src = Path.GetFullPath(n);
                if (!File.Exists(src))
                {
                    var fullPath = Path.GetFullPath(n);
                    Console.WriteLine($"Job file not found: {src}");
                }
                else
                {

                    Directory.SetCurrentDirectory(current);
                    var name = Path.GetFileNameWithoutExtension(n);
                    var destDir = Directory.CreateDirectory(name);
                    var destFile = Path.Combine(destDir.FullName, n);
                    File.Copy(src, destFile, true);
                    var jobName = $"{name}/{n}";
                    Directory.SetCurrentDirectory(destDir.FullName);
                    var p = new FactMSieve.Program();
                    p.Run(script, jobName);
                }
            }

        }
    }
}


namespace FactMSieve
{
    using static MsieveStatic;
    class Program
    {

        const string VERSION = "0.86";
        string GGNFS_PATH = "../ggnfs-min-depends";
        string MSIEVE_PATH = "../msieve-min-depends";
        string CWD = ".";

        const int NUM_CORES = 4;
        const int THREADS_PER_CORE = 2;
        bool USE_CUDA = true;
        const int GPU_NUM = 0;
        int MSIEVE_POLY_TIME_LIMIT = 0;
        //const int MIN_NFS_BITS = 264;
        const int MIN_NFS_BITS = 65;
        const int POLY_MIN_LC = 0;
        const int POLY_MAX_LC = 10000;
        const bool CHECK_BINARIES = true;
        const bool CHECK_POLY = true;
        const bool CLEANUP = false;
        const bool DOCLASSICAL = false;
        const bool NO_DEF_NM_PARAM = false;
        const bool PROMPTS = false;
        const bool SAVEPAIRS = true;
        int USE_KLEINJUNG_FRANKE_PS = 0;
        int USE_MSIEVE_POLY = 1;
        const bool VERBOSE = true;
        const int MS_THREADS = NUM_CORES * THREADS_PER_CORE;
        const int SV_THREADS = NUM_CORES * THREADS_PER_CORE;
        const int LA_THREADS = NUM_CORES * THREADS_PER_CORE;
        string MSIEVE = "msieve";
        const string MAKEFB = "makefb";
        const string PROCRELS = "procrels";
        const string POL51M0 = "pol51m0b";
        const string POL51OPT = "pol51opt";
        const string POLYSELECT = "polyselect";
        const string PLOT = "autogplot.sh";
        const string DEFAULT_PAR_FILE = "def-par.txt";
        const string DEFAULT_POLSEL_PAR_FILE = "def-nm-params.txt";
        const string PARAMFILE = ".params";
        const string RELSBIN = "rels.bin";
        static string EXE_SUFFIX = "";
        static string NICE_PATH = "";
        static int PNUM = 0;
        static int LARGEP = 3;
        static string LARGEPRIMES = "-" + LARGEP + "p";
        static int nonPrefDegAdjust = 12;
        static double polySelTimeMultiplier = 1.0;
        static Dictionary<string, int> pol5_p = new Dictionary<string, int> {
            { "max_pst_time", 0 }, { "search_a5step", 0 }, { "npr", 0 }, { "normmax", 0 },
            { "normmax1", 0 }, { "normmax2", 0 }, { "murphymax", 0 }
        };
        static Dictionary<string, int> pols_p = new Dictionary<string, int> {
            { "degree", 0 }, { "maxs1", 0 }, { "maxskew", 0 }, { "goodscore", 0 },
            { "examinefrac", 0 }, { "j0", 0 }, { "j1", 0 }, { "estepsize", 0 }, { "maxtime", 0 }
        };
        static Dictionary<string, dynamic> lats_p = new Dictionary<string, dynamic> {
            { "rlim", 0 }, { "alim", 0 }, { "lpbr", 0 }, { "lpba", 0 }, { "mfbr", 0 }, { "mfba", 0 },
            { "rlambda", 0.0 }, { "alambda", 0.0 }, { "qintsize", 0 }, { "lss", 1 },
            { "siever", "gnfs-lasieve4I10e" }, { "minrels", 0 }, { "currels", 0 }
        };

        static Dictionary<string, int> clas_p = new Dictionary<string, int> {
            { "a0", 0 }, { "a1", 0 }, { "b0", 0 }, { "b1", 0 }, { "cl_a", 0 }, { "cl_b", 0 }
};
        static Dictionary<string, dynamic> poly_p = new Dictionary<string, dynamic> {
            { "n", 0 }, { "degree", 0 }, { "m", 0 }, { "skew", 0.0 }, { "coeff", new Dictionary<string, dynamic>() }
        };
        static Dictionary<string, dynamic> fact_p = new Dictionary<string, dynamic> {
            { "n", 0 }, { "dec_digits", 0 }, { "type", "" }, { "knowndiv", 0 }, { "snfs_difficulty", 0 },
            { "digs", 0 }, { "qstart", 0 }, { "qstep", 0 }, { "q0", 0 }, { "dd", 0 },
            { "primes", new List<int>() }, { "comps", new List<int>() }, { "divisors", new List<int>() },
            { "q_dq", new Queue<(int, int, int)>() }
        };


        //The parameter degree, if nonzero, will let this function adjust
        //parameters for SNFS factorizations with a predetermined polynomial
        //of degree that is not the optimal degree.


        string pname = "";
        ProcResult[] procs = new ProcResult[] { };

        //string SummaryName;
        string LOGNAME;
        string NAME;
        public void Run(params string[] arg)
        {
            string EXE_SUFFIX;
            string NICE_PATH;


            if (Environment.OSVersion.Platform.ToString().StartsWith("Win"))
            {
                EXE_SUFFIX = "exe";
                MSIEVE = $"{MSIEVE}.{EXE_SUFFIX}";
            }
            else
            {
                EXE_SUFFIX = "";
                NICE_PATH = "";
                MSIEVE = "./" + MSIEVE;
            }

            Output("-> ________________________________________________________________");
            Output("-> | Running factmsieve.py, a Python driver for MSIEVE with GGNFS |");
            Output("-> | sieving support. It is Copyright, 2010, Brian Gladman and is |");
            Output("-> | a conversion of factmsieve.pl that is Copyright, 2004, Chris |");
            Output("-> | Monico.   Version {0:s} (Python 2.6 or later) 20th June 2011. |", VERSION);
            Output("-> |______________________________________________________________|");

            var args = Environment.GetCommandLineArgs().Skip(1).ToArray();

            if (args.Length != 2 && args.Length != 4)
            {
                Output("USAGE: {0:s} <number file | poly file | msieve poly file> [ id  num]", args[0]);
                Output("  where <polynomial file> is a file with the poly info to use");
                Output("  or <number file> is a file containing the number to factor.");
                Output("  Optional: id/num specifies that this is client <id> of <num>");
                Output("            clients total (clients are numbered 1,2,3,...).");
                Output(" (Multi-client mode is still very experimental - it should only");
                Output("  be used for testing, and is only intended as a hack for running");
                Output("  conveniently on a very small number of machines - perhaps 5 - 10).");
                Environment.Exit(-1);
            }

            GGNFS_PATH = Path.GetFullPath(GGNFS_PATH);
            MSIEVE_PATH = Path.GetFullPath(MSIEVE_PATH);
            CWD = Path.GetFullPath(Directory.GetCurrentDirectory() + "/");
            NAME = args[1];
            NAME = Regex.Replace(NAME, @"\.(poly|fb|n)", "");

            // NOTE: These names are global


            LOGNAME = NAME + ".log";
            string JOBNAME = NAME + ".job";
            string ININAME = NAME + ".ini";
            string DATNAME = NAME + ".dat";
            string FBNAME = NAME + ".fb";
            string OUTNAME = NAME + ".msp";
            string DEPFNAME = DATNAME + ".dep";
            string COLSNAME = DATNAME + ".cyc";
            string SIEVER_OUTPUTNAME = "spairs.out";
            string SIEVER_ADDNAME = "spairs.add";
            int POLY_SELECT_ONLY = 1;


            Console.CancelKeyPress += ConsoleCancelKeyPress;

            int client_id, num_clients;
            if (args.Length == 4)
            {
                client_id = int.Parse(args[2]);
                num_clients = int.Parse(args[3]);
                if (client_id < 1 || client_id > num_clients)
                {
                    Die($"-> Error: client id should be between 1 and the number of clients {num_clients}");
                }
                PNUM += client_id;
            }
            else
            {
                num_clients = 1;
                client_id = 1;
            }

            Output($"-> factmsieve.py (v{VERSION})", false);
            var t = Environment.Version;
            Output($"-> Running Python {t.Major}.{t.Minor}");
            Output($"-> This is client {client_id} of {num_clients}");
            Output($"-> Running on {NUM_CORES} Core{(NUM_CORES == 1 ? "" : "s")} with {THREADS_PER_CORE} hyper-thread{(THREADS_PER_CORE == 1 ? "" : "s")} per Core");
            Output($"-> Working with NAME = {NAME}");

            if (client_id > 1)
            {
                JOBNAME += $".{client_id}";
                SIEVER_OUTPUTNAME += $".{client_id}";
                SIEVER_ADDNAME += $".{client_id}";
            }

            CheckBinary(MSIEVE);

            //TODO refactor to check job directory for the poly file.
            if (!File.Exists(NAME + ".poly"))
            {
                if (File.Exists(NAME + ".fb"))
                {
                    Output("-> Converting msieve polynomial (*.fb) to ggnfs (*.poly) format.");
                    FbToPoly();
                }
                else if (!File.Exists(NAME + ".n"))
                {
                    var nFullName = Path.GetFullPath(NAME + ".n");
                    DieF("-> Could not find the file {0:s}.n", nFullName);
                }
                else
                {
                    DeleteFile(PARAMFILE);
                    var content = File.ReadAllText(NAME + ".n");
                    var norm = content.Replace(":", "");
                    var parts = norm.Split(" ");
                    var value = parts[1];
                    var n = BigInteger.Parse(value);
                    fact_p = new Dictionary<string, dynamic>
                    {
                        {"n", n},
                        {"dec_digits", n.ToString().Length }
                    };

                    // change code to only accept n: {value} 

                    //using (StreamReader in_f = new StreamReader(NAME + ".n"))
                    //{
                    //    List<string> numberinf = new List<string>(in_f.ReadToEnd().Split('\n'));
                    //    List<string> t1 = GrepL("^n:", numberinf);
                    //    if (t1.Count > 0)
                    //    {
                    //        Match m = Regex.Match(t1[0], "n:\\s*(\\d+)");
                    //        if (m.Success && m.Groups.Count > 1)
                    //        {
                    //            fact_p["n"] = int.Parse(m.Groups[1].Value);
                    //            fact_p["dec_digits"] = m.Groups[1].Value.Length;
                    //            Output("-> Found n = {0:d}.", fact_p["n"]);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        Output("-> Could not find a number in the file {0:s}.n", NAME);
                    //        Die("-> Did you forget the 'n:' tag?");
                    //    }
                    //}
                    //if (ProbablePrimeP(fact_p["n"], 10))
                    //{
                    //    Die("-> Error: n is probably prime!");
                    //}

                    var minNfs = BigInteger.One << MIN_NFS_BITS;
                    //   if (fact_p["n"] >= Math.Pow(2, MIN_NFS_BITS))
                    if (fact_p["n"] >= minNfs)
                    {
                        double poly_start = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                        Output("-> Polynomial file {0:s}.poly does not exist!", NAME);
                        if (num_clients > 1)
                        {
                            Die("-> Script does not support polynomial search across multiple clients!");
                        }
                        Output("-> Running polynomial selection ...");
                        if (fact_p["dec_digits"] < 98)
                        {
                            USE_KLEINJUNG_FRANKE_PS = 0;
                        }
                        if (USE_KLEINJUNG_FRANKE_PS != 0)
                        {
                            RunPol5(fact_p, pol5_p, lats_p, clas_p);
                        }
                        else if (USE_MSIEVE_POLY != 0)
                        {
                            if (!RunMsievePoly(fact_p, POLY_MIN_LC, POLY_MAX_LC))
                            {
                                Environment.Exit(0);
                            }
                        }
                        else
                        {
                            RunPolySelect(fact_p, pols_p, lats_p, clas_p);
                        }
                        if (!File.Exists(NAME + ".poly"))
                        {
                            Die("Polynomial selection failed.");
                        }
                        double poly_time = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds - poly_start;
                    }
                    else
                    {
                        Output("-> Running mpqs");
                        fact_p["type"] = "mpqs";
                        if (MsieveMpqs(fact_p))
                        {
                            OutputSummary(NAME, fact_p, pols_p, poly_p, lats_p);
                        }
                        Environment.Exit(0);
                    }
                }
            }



            void OutputSummary(string name,
               Dictionary<string, dynamic> fact_p,
               Dictionary<string, int> pols_p,
               Dictionary<string, dynamic> poly_p,
               Dictionary<string, dynamic> lats_p)
            {

                // Set the summary file name
                string sum_name = GetSummaryName(name, fact_p);

                // Figure the time scale for this machine.
                Console.WriteLine("-> Computing time scale for this machine...");

                //(int ret, string res) = RunExe(PROCRELS, "-speedtest", out_file: null);
                var procResult = RunExe(PROCRELS, "-speedtest", out_file: null);
                (int ret, string[] res) = (procResult.ExitCode.Value, procResult.Output);
                double timescale;
                if (res != null)
                {
                    //string tmp = res.Split('\n').FirstOrDefault(line => line.Contains("timeunit:"));
                    var tmp = res.FirstOrDefault(line => line.Contains("timeunit:"));
                    timescale = double.Parse(Regex.Replace(tmp, "timeunit:\\s*", ""));
                }
                else
                {
                    timescale = 0.0;
                }

                // And gather up some stats.
                double poly_time = 0;
                double total_time = 0;
                double sieve_time = 0.0;
                double relproc_time = 0.0;
                double matrix_time = 0;
                double sqrt_time = 0;
                int min_q = 0, max_q = 0;
                string prunedmat = "", rels = "";
                int rprimes = 0, aprimes = 0;
                string version = "Msieve-1.40";

                using (StreamReader in_f = new StreamReader(LOGNAME))
                {
                    string line;
                    while ((line = in_f.ReadLine()) != null)
                    {
                        line = line.TrimEnd();
                        line = Regex.Replace(line, "\\s+$", "");
                        string[] tu = line.Split(' ');

                        Match m = Regex.Match(line, "for q from (\\d+) to (\\d+) as file");
                        if (m.Success)
                        {
                            int t = int.Parse(m.Groups[1].Value);
                            min_q = t < min_q || min_q == 0 ? t : min_q;
                            t = int.Parse(m.Groups[2].Value);
                            max_q = t > max_q ? t : max_q;
                        }

                        m = Regex.Match(line, "LatSieveTime:\\s*(\\d+)");
                        if (m.Success)
                        {
                            sieve_time += double.Parse(m.Groups[1].Value);
                        }

                        m = Regex.Match(line, "(Msieve.*)$");
                        if (m.Success)
                        {
                            version = m.Groups[1].Value;
                        }

                        m = Regex.Match(line, "RelProcTime: (\\S+)");
                        if (m.Success)
                        {
                            relproc_time += double.Parse(m.Groups[1].Value);
                        }

                        m = Regex.Match(line, "BLanczosTime: (\\S+)");
                        if (m.Success)
                        {
                            matrix_time += int.Parse(m.Groups[1].Value);
                        }

                        m = Regex.Match(line, "sqrtTime: (\\S+)");
                        if (m.Success)
                        {
                            sqrt_time += int.Parse(m.Groups[1].Value);
                        }

                        m = Regex.Match(line, "rational ideals");
                        if (m.Success)
                        {
                            rprimes = int.Parse(tu[6]) + int.Parse(tu[7]) + int.Parse(tu[5]);
                        }

                        m = Regex.Match(line, "algebraic ideals");
                        if (m.Success)
                        {
                            aprimes = int.Parse(tu[6] + tu[7] + tu[5]);
                        }

                        m = Regex.Match(line, "unique relations");
                        if (m.Success)
                        {
                            rels = $"{tu[tu.Length - 3]} {tu[tu.Length - 1]}";
                        }

                        m = Regex.Match(line, "matrix is");
                        if (m.Success)
                        {
                            prunedmat = $"{tu[7]} x {tu[9]}";
                        }

                        m = Regex.Match(line, @"p[0-9]+\s+factor");
                        if (m.Success)
                        {
                            string tmpStr = Regex.Replace(line, @".*factor: ", "");
                            int tmp = int.Parse(tmpStr);
                            if (1 < tmp.ToString().Length && tmp.ToString().Length < fact_p["dec_digits"])
                            {
                                fact_p["divisors"].Add(tmp);
                            }
                        }

                    }

                }

                using (StreamWriter out_f = new StreamWriter("ggnfs.log", true))
                {
                    out_f.WriteLine("Number: {0:s}", NAME);
                    out_f.WriteLine("N = {0:d}", fact_p["n"]);

                    foreach (int dd in new SortedSet<int>((List<int>)fact_p["divisors"]))
                    {
                        out_f.WriteLine("factor: {0:d}", dd);
                    }
                    // Convert times from seconds to hours
                    poly_time = poly_time / 3600.0;
                    sieve_time = sieve_time / 3600.0;
                    relproc_time = relproc_time / 3600.0;
                    matrix_time = matrix_time / 3600.0;
                    sqrt_time = sqrt_time / 3600.0;
                    total_time = poly_time + sieve_time + relproc_time + matrix_time + sqrt_time;

                    out_f.WriteLine("Number: {0:s}", NAME);
                    out_f.WriteLine("N = {0:d} ({1:d} digits)", fact_p["n"], fact_p["dec_digits"]);

                    if (fact_p["type"].ToString() == "snfs")
                    {
                        out_f.WriteLine("SNFS difficulty: {0:d} digits.", fact_p["snfs_difficulty"]);
                    }

                    out_f.WriteLine("Divisors found:");
                    if (fact_p.ContainsKey("knowndiv"))
                    {
                        out_f.WriteLine(" knowndiv: {0:d}", fact_p["knowndiv"]);
                    }

                    int r = 1;
                    foreach (int dd in new SortedSet<int>((List<int>)fact_p["divisors"]))
                    {
                        out_f.WriteLine("r{0:d}={1:d} (pp{2:d})", r, dd, dd.ToString().Length);
                        r++;
                    }

                    out_f.WriteLine("Version: {0:s}", version);

                    if (fact_p["type"].ToString() == "mpqs")
                    {
                        out_f.WriteLine("Completed using mpqs mode");
                    }
                    else
                    {
                        out_f.WriteLine("Total time: {0:1.2f} hours.", total_time);
                        out_f.WriteLine("Scaled time: {0:1.2f} units (timescale= {1:1.3f}).", total_time * timescale, timescale);

                        try
                        {
                            using (StreamReader in_f = new StreamReader(NAME + ".poly"))
                            {
                                out_f.WriteLine("Factorization parameters were as follows:");
                                string line;
                                while ((line = in_f.ReadLine()) != null)
                                {
                                    out_f.WriteLine(line);
                                }
                            }
                        }
                        catch (IOException)
                        {
                            // Handle file not found exception
                        }

                        string siever_side = lats_p["lss"] ? "rational" : "algebraic";
                        out_f.WriteLine("Factor base limits: {0:d}/{1:d}", lats_p["rlim"], lats_p["alim"]);
                        out_f.WriteLine("Large primes per side: {0:d}", LARGEP);
                        out_f.WriteLine("Large prime bits: {0:d}/{1:d}", lats_p["lpbr"], lats_p["lpba"]);
                        out_f.WriteLine("Sieved{0:s} special-q in [{1:d}, {2:d})", siever_side, min_q, max_q);
                        out_f.WriteLine("Total raw relations: {0:d}", lats_p["currels"]);
                        out_f.WriteLine("Relations: {0:s}", rels);
                        out_f.WriteLine("Pruned matrix : {0:s}", prunedmat);

                        if (poly_time != 0)
                        {
                            out_f.WriteLine("Polynomial selection time: {0:1.2f} hours.", poly_time);
                        }

                        out_f.WriteLine("Total sieving time: {0:1.2f} hours.", sieve_time);
                        out_f.WriteLine("Total relation processing time: {0:1.2f} hours.", relproc_time);
                        out_f.WriteLine("Matrix solve time: {0:1.2f} hours.", matrix_time);
                        out_f.WriteLine("Time per square root: {0:1.2f} hours.", sqrt_time);

                        if (fact_p["type"].ToString() == "snfs")
                        {
                            fact_p["digs"] = fact_p["snfs_difficulty"];
                            string DFL = String.Format("{0[type]:s},{0[digs]:d},{1[degree]:d},{3:d},{4:d},{5:g},{6:g},{7:d},{8:d},{9:d},{10:d},{2[rlim]:d},{2[alim]:d},{2[lpbr]:d},{2[lpba]:d},{2[mfbr]:d},{2[mfba]:d},{2[rlambda]:g},{2[alambda]:g},{2[qintsize]:d}",
                                fact_p, poly_p, lats_p, 0, 0, 0, 0, 0, 0, 0, 0);
                            out_f.WriteLine("Prototype def-par.txt line would be: {0:s}", DFL);
                        }
                        else if (fact_p["type"].ToString() == "gnfs")
                        {
                            fact_p["digs"] = fact_p["dec_digits"] - 1;
                            string DFL = String.Format("{0[type]:s},{0[digs]:d},{1[degree]:d},{2[maxs1]:d},{2[maxskew]:d},{2[goodscore]:g},{2[examinefrac]:g},{2[j0]:d},{2[j1]:d},{2[estepsize]:d},{2[maxtime]:d},{3[rlim]:d},{3[alim]:d},{3[lpbr]:d},{3[lpba]:d},{3[mfbr]:d},{3[mfba]:d},{3[rlambda]:g},{3[alambda]:g},{3[qintsize]:d}",
                                fact_p, poly_p, pols_p, lats_p);
                            out_f.WriteLine("Prototype def-par.txt line would be: {0:s}", DFL);
                        }

                        out_f.WriteLine("Total time: {0:1.2f} hours.", total_time);
                    }

                    //out_f.WriteLine($"{platform.processor()}");
                    //out_f.WriteLine("processors: {0:d}, speed: {1:.2f}GHz", Environment.ProcessorCount, Environment.ProcessorSpeed());
                    //string platformInfo = platform.platform();
                    //if (platformInfo.Length > 0)
                    //{
                    //    out_f.WriteLine(platformInfo);
                    //}
                    //Tuple<string, string> pythonVersion = PythonVersion();
                    //out_f.WriteLine($"Running Python {pythonVersion.Item1}.{pythonVersion.Item2}");


                }
                Output("-> Factorization summary written to {0:s}", sum_name);

            }



            string GetSummaryName(string name, Dictionary<string, dynamic> fact_p)
            {
                string s = null!;
                if (fact_p["type"].ToString() == "gnfs")
                {
                    s = "g" + fact_p["dec_digits"].ToString();
                }
                else
                {
                    s = "s" + fact_p["snfs_difficulty"].ToString();
                }
                string[] t = name.Split(Path.PathSeparator);

                return Path.Combine(t[0], s + "-" + t[1] + ".txt");
            }

            bool MsieveMpqs(Dictionary<string, dynamic> fact_p)
            {
                //def msieve_mpqs(fact_p):

                //  with open(ININAME, 'w') as out_f:
                //    out_f.write('{0:d}'.format(fact_p['n']))
                //  ret = run_msieve('-v')
                //  if ret:
                //    die('Msieve Error: return value {0:d}... terminating...'.format(ret))
                //  return True
                var line = string.Format("{0:d}", fact_p["n"]);
                File.WriteAllText(ININAME, line);
                var ret = RunMsieve("-v");
                if (ret.ExitCode != 0)
                {
                    DieF("Msieve Error: return value {0:d}... terminating...", ret);
                }
                return true;
            }

            void RunPolySelect(Dictionary<string, dynamic> fact_p, Dictionary<string, int> pols_p, Dictionary<string, dynamic> lats_p, Dictionary<string, int> clas_p)
            {
                throw new NotImplementedException();
            }

            bool RunMsievePoly(Dictionary<string, dynamic> fact_p, int minv, int maxv)
            {


                //string[] poly = new string[0];
                List<string> poly = new();
                double bestscore = 0;

                if (USE_CUDA || MS_THREADS == 1)
                {
                    using (StreamWriter out_f = new StreamWriter(ININAME))
                    {
                        out_f.Write($"{fact_p["n"]}");
                    }

                    string ap, bp;
                    if (USE_CUDA)
                    {
                        ap = $"-g {GPU_NUM} -v -np ";
                        bp = " Is CUDA enabled?";
                    }
                    else
                    {
                        ap = "-v -np";
                        bp = "";
                    }

                    if (MSIEVE_POLY_TIME_LIMIT != 0)
                    {
                        ap = $"-d {MSIEVE_POLY_TIME_LIMIT} " + ap;
                    }
                    else
                    {
                        ap += $" {((minv != 0) ? minv : 1)},{maxv} -t {NUM_CORES}";
                    }
                    ap += $" -np poly_deadline=84000";
                    var pwd = Directory.GetCurrentDirectory();
                    var ret = RunMsieve(ap);
                    if (ret.ExitCode != 0)
                    {
                        Die($"Msieve Error: return value {ret}.{bp} Terminating...");
                    }
                    Directory.SetCurrentDirectory(pwd);
                    (bestscore, poly) = FindBest($"{NAME}.dat.p", bestscore, poly);
                }
                else
                {
                    //SignalHandler old_handler = Signal.Signal(SIGINT, TerminateThreads);
                    Console.CancelKeyPress += (sender, e) => TerminateThreads();
                    for (int j = 0; j < MS_THREADS; j++)
                    {
                        string extn = ".T" + j;
                        using (StreamWriter out_f = new StreamWriter(ININAME + extn))
                        {
                            out_f.Write("{0:d}", fact_p["n"]);
                        }
                    }

                    procs = new ProcResult[MS_THREADS];
                    int ret = 0;
                    int @base = minv;
                    if (File.Exists(NAME + ".lcf"))
                    {
                        try
                        {
                            using (StreamReader in_f = new StreamReader(NAME + ".lcf"))
                            {
                                int t = int.Parse(in_f.ReadLine().Trim());
                                if (minv < t && t < maxv)
                                {
                                    @base = t;
                                }
                                else if (t >= maxv)
                                {
                                    Die(string.Format("previous run conflict (delete {0:s}.lcf to run again)", NAME));
                                }
                            }
                        }
                        catch (IOException)
                        {
                            Die("Can't read " + NAME + ".lcf");
                        }
                    }

                    bool term = false;
                    while (@base < maxv && ret == 0)
                    {
                        for (int j = 0; j < MS_THREADS; j++)
                        {
                            string extn = ".T" + j;
                            int? retc = procs[j] == null ? 0 : procs[j].ExitCode;
                            if (retc != null)
                            {
                                ret |= retc.Value;
                                if (File.Exists(NAME + ".lcf"))
                                {
                                    try
                                    {
                                        using (StreamReader in_f = new StreamReader(NAME + ".lcf"))
                                        {
                                            @base = int.Parse(in_f.ReadLine().Trim());
                                        }
                                    }
                                    catch (IOException)
                                    {
                                        Die("Can't read " + NAME + ".lcf");
                                    }
                                }

                                try
                                {
                                    using (StreamWriter out_f = new StreamWriter(NAME + ".lcf"))
                                    {
                                        out_f.Write(@base + 100);
                                    }
                                }
                                catch (IOException)
                                {
                                    Die("Can't write to " + NAME + ".lcf");
                                }

                                procs[j] = RunMsieve(string.Format("-np {0:d},{1:d} -v ", @base + 1, @base + 100), extn, parallel: true, dieIfJobExists: true);
                                @base += 100;
                            }
                        }

                        try
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                        catch
                        {
                            term = true;
                            break;
                        }
                    }

                    //wait for last set of processes to complete.
                    for (int j = 0; j < MS_THREADS; j++)
                    {
                        procs[j].WaitForExit();
                        int retc = procs[j] == null ? 0 : procs[j].ExitCode ?? 0;
                        ret |= retc;
                    }

                    if (ret != 0)
                    {
                        Die("an error occurred during polynomial search");
                    }

                    if (term)
                    {
                        TerminateThreads();
                        Die("msieve terminated");
                    }

                    for (int j = 0; j < MS_THREADS; j++)
                    {
                        string nm = NAME + ".fb.T" + j;
                        if (File.Exists(nm))
                        {
                            File.Delete(nm);
                            Console.WriteLine("deleted " + nm);
                        }
                        nm = NAME + ".dat.T" + j + ".p";
                        (bestscore, poly) = FindBest(NAME + ".dat.T" + j + ".p", bestscore, poly);
                    }
                }


                // Your existing code...

                if (bestscore == 0)
                {
                    Output("could not find any polynomials");
                    Die("could not find any polynomials");

                }

                using (StreamWriter out_f = new StreamWriter($"{NAME}.poly"))
                {
                    using (StreamReader in_f = new StreamReader($"{NAME}.n"))
                    {
                        string line;
                        while ((line = in_f.ReadLine()) != null)
                        {
                            out_f.WriteLine(line);
                        }
                    }

                    foreach (string line in poly)
                    {
                        out_f.WriteLine(line);
                    }
                    out_f.WriteLine("type: gnfs");
                }

                return true;
            }



            void RunPol5(Dictionary<string, dynamic> fact_p, Dictionary<string, int> pol5_p, Dictionary<string, dynamic> lats_p, Dictionary<string, int> clas_p)
            {
                throw new NotImplementedException();
            }

            bool ProbablePrimeP(dynamic dynamic, int v)
            {
                throw new NotImplementedException();
            }

            List<string> GrepL(string v, List<string> numberinf)
            {
                throw new NotImplementedException();
            }

            void DeleteFile(string filePath)
            {
                File.Delete(filePath);
            }

            void FbToPoly()
            {
                string inputFileName = NAME + ".fb";
                string outputFileName = NAME + ".poly";
                double skew = 0.0;
                BigInteger n = 0, y0 = 0, r1 = 0;
                Dictionary<string, BigInteger> coeffs = new();

                using (StreamReader inputFile = new StreamReader(inputFileName))
                {
                    string line;
                    while ((line = inputFile.ReadLine()) != null)
                    {
                        Match match;

                        match = Regex.Match(line, @"N\s+(\d+)");
                        if (match.Success)
                        {
                            n = BigInteger.Parse(match.Groups[1].Value);

                        }

                        match = Regex.Match(line, @"SKEW\s+([\d.]+)");
                        if (match.Success)
                        {
                            skew = double.Parse(match.Groups[1].Value);
                        }

                        match = Regex.Match(line, @"R(\d+)\s+([+-]?\d+)");
                        if (match.Success)
                        {
                            if (match.Groups[1].Value == "0")
                                y0 = BigInteger.Parse(match.Groups[2].Value);
                            else
                                r1 = BigInteger.Parse(match.Groups[2].Value);

                        }

                        match = Regex.Match(line, @"A(\d+)\s+([+-]?\d+)");
                        if (match.Success)
                        {
                            var key = match.Groups[1].Value;
                            var value = BigInteger.Parse(match.Groups[2].Value);
                            coeffs[key] = value;
                        }
                    }
                }

                if (skew == 0 || y0 == 0 || r1 == 0 || coeffs.Count < 1)
                {
                    throw new Exception(inputFileName + " failed validation.");
                }

                string datFileName = NAME + ".dat.p";
                var c = coeffs.Select(x => $"c{x.Key}: {x.Value}").ToArray();
                string murphy = null;
                if (File.Exists(datFileName))
                {

                    using (var reader = new StreamReader(datFileName))
                    {
                        string line;
                        string last = string.Empty;
                        var polySkew = 0.0;
                        while (murphy is null && (line = reader.ReadLine()) != null)
                        {
                            if (line[0] == 's')
                            {
                                polySkew = double.Parse(line.Substring(5));
                                if (skew == polySkew)
                                {
                                    bool matched = true;
                                    for (var i = 0; i < c.Length && ((line = reader.ReadLine()) != null); i++)
                                    {
                                        matched = line == c[i];
                                    }
                                    if (matched && ((line = reader.ReadLine()) != null))
                                    {
                                        matched = line == $"Y0: {y0}";
                                    }
                                    if (matched && ((line = reader.ReadLine()) != null))
                                    {
                                        matched = line == $"Y1: {r1}";
                                    }
                                    if (matched)
                                    {
                                        murphy = last;
                                    }
                                    //previousl line
                                    //# norm 8.509823e-14 alpha -4.727986 e 9.981e-09 rroots 4
                                }

                            }
                            last = line;
                        }
                    }
                }


                using (StreamWriter outputFile = new StreamWriter(outputFileName))
                {
                    try
                    {
                        outputFile.WriteLine($"n: {n}{murphy}");
                        outputFile.WriteLine("skew: {0,-10:0.00}", skew);
                        for (var i = 0; i < c.Length; i++)
                        {
                            outputFile.WriteLine(c[i]);
                        }
                        outputFile.WriteLine("Y0: " + y0);
                        outputFile.WriteLine("Y1: " + r1);
                        outputFile.WriteLine("type: gnfs");
                    }
                    catch (Exception)
                    {
                        throw new Exception(inputFileName + " is not in the correct format");
                    }
                }
            }


            void CheckBinary(string exe)
            {
                var pth = exe == MSIEVE ? MSIEVE_PATH : GGNFS_PATH;
                string binaryPath = Path.Combine(pth, exe);
                binaryPath = Path.GetFullPath(binaryPath);
                if (!File.Exists(binaryPath))
                    Die($"-> Error: Binary '{exe}' not found at '{binaryPath}'");
            }

            void Die(string errorMessage)
            {
                Output(errorMessage);
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                Environment.Exit(-1);
            }
            void DieF(string errorFormat, params object[] args)
            {
                Die(string.Format(errorFormat, args));
            }

            void ConsoleCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
            {
                throw new NotImplementedException();
            }


            void SigExit()
            {
                // Perform cleanup and exit
            }

            ProcResult RunMsieve(string ap, string extn = "", bool parallel = false, bool dieIfJobExists = false)
            {
                string msd = Path.GetFullPath(Path.Combine(CWD, MSIEVE_PATH));
                Directory.SetCurrentDirectory(msd);
                string rel = Path.GetRelativePath(msd, CWD);
                string dp = Path.Combine(rel, DATNAME + extn);
                string lp = Path.Combine(rel, LOGNAME + extn);
                string ip = Path.Combine(rel, ININAME + extn);
                string fp = Path.Combine(rel, FBNAME + extn);
                string args = $"-s {dp} -l {lp} -i {ip} -nf {fp} ";
                if (dieIfJobExists && File.Exists(ip))
                {
                    Die($"Job already exists: '{ip}' - Please clean the directory if you wish to rerun the job.");
                }
                if (parallel)
                {
                    string op = Path.Combine(rel, OUTNAME + extn);
                    return RunExe(MSIEVE, args + ap, out_file: op, wait: false);
                }
                else
                {
                    return RunExe(MSIEVE, args + ap);
                }
            }
        }



        private class ProcResult
        {
            internal bool Wait;
            internal Process? Process;
            internal string[] Output;
            internal bool HasExited;
            internal int? ExitCode;

            internal void WaitForExit() => Process?.WaitForExit();
        }

        ProcResult RunExe(string exe, string args, string inp = "", string in_file = null, string out_file = null,
          bool log = true, bool display = VERBOSE, bool wait = true)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = exe;
            psi.Arguments = args;
            psi.UseShellExecute = false;
            psi.RedirectStandardInput = in_file != null && File.Exists(in_file);
            psi.RedirectStandardOutput = out_file != null;
            psi.CreateNoWindow = !VERBOSE;




            if (out_file != null)
            {
                psi.StandardOutputEncoding = Encoding.UTF8;
                psi.RedirectStandardOutput = true;
            }

            Output($"-> {exe} {args}" + (in_file != null ? $"< {in_file}" : "") + (out_file != null ? $"> {out_file}" : ""),
                   console: display, log: log);
            var procResult = new ProcResult();
            procResult.Wait = wait;


            var process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo = psi;
            procResult.Process = process;
            process.Exited += (sender, e) =>
            {
                procResult.HasExited = true;
                procResult.ExitCode = process.ExitCode;
            };

            process.Start();
            if (inp != null && psi.RedirectStandardInput)
            {
                using (var sw = process.StandardInput)
                {
                    sw.Write(inp);
                }
            }

            if (wait)
            {
                if (out_file != null)
                {
                    using (var sr = process.StandardOutput)
                    {
                        string output = sr.ReadToEnd();
                        if (!string.IsNullOrEmpty(output))
                        {
                            procResult.Output = Regex.Split(output, "(?:\\r|\\n)*");
                        }
                    }
                }

                process.WaitForExit();
                procResult.HasExited = true;
                procResult.ExitCode = process.ExitCode;
            }

            return procResult;

        }

        private void WriteStringToLog(string message)
        {
            if (LOGNAME is not null)
            {
                string logFilePath = Path.GetFullPath(LOGNAME);
                File.AppendAllText(logFilePath, $"[{DateTime.Now}] {message}\n");
            }
        }

        void TerminateThreads()
        {
            try
            {
                foreach (ProcResult p in procs)
                {
                    if (p.Process is not null && !p.Process.HasExited)
                        p.Process.Kill();
                }

                foreach (ProcResult p in procs)
                {
                    if (p.Process is not null && !p.Process.HasExited)
                        p.Process.WaitForExit();
                }
            }
            catch
            {
                // Handle any exceptions here
            }
        }

        void Output(string message, params object[] args)
        {
            Output(string.Format(message, args));
        }
        void Output(string message, bool console = true, bool log = false)
        {

            if (console)
                Console.WriteLine(message);
            if (log)
                WriteStringToLog(message);
        }
        //void OutputF(string format, params object[] args)
        //   => Output2F(format, false, args);


        //void Output2F(string format, bool console = true, params object[] args)
        //{
        //    if (console)
        //        Console.WriteLine(string.Format(format, args));
        //    else
        //        Console.Error.WriteLine(string.Format(format, args));
        //}


    }

    // host none local variable functions outside of runner to enable edit and continue
    public static class MsieveStatic
    {
        public static (double, List<string>) FindBest(string fn, double bestscore, List<string> poly)
        {
            /*
              # first input line (followed by polynomial lines) format:
              # norm 1.498232e-10 alpha -5.818790 e 9.344e-10 rroots 3
              #   1      2          3       4     5   6        7     8
             * */
            try
            {
                fn = Path.GetFullPath(fn);
                using (StreamReader in_f = new StreamReader(fn))
                {
                    string line;
                    bool better = false;

                    while ((line = in_f.ReadLine()) != null)
                    {
                        string[] words = line.Split();
                        if (words[1] == "norm")
                        {
                            better = false;
                            if (double.Parse(words[6]) > bestscore)
                            {
                                bestscore = double.Parse(words[6]);
                                Console.WriteLine("Best score so far: " + line.Trim());
                                better = true;
                                poly = new List<string>();
                            }
                        }
                        if (better)
                        {
                            poly.Add(line);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Can't read '{fn}' - {ex}");
            }

            return (bestscore, poly);
        }
    }

}

