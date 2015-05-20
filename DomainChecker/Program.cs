using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DomainChecker
{
    class Program
    {
        private const string RootTLDServerURL = @"whois.iana.org";
        private const string MasterTLDListURL = @"http://data.iana.org/TLD/tlds-alpha-by-domain.txt";
        
        static void Main(string[] args)
        {            
            string query;
            List<string> tlds = GetAllTLDs();

            Console.WriteLine("{0} TLDs loaded.", tlds.Count);

            Console.Write("Query> ");
            while ((query = Console.ReadLine()) != string.Empty)
            {
                if (query != null)
                {
                    string keywordPart = query.Substring(0, query.LastIndexOf('.'));
                    string tldPart = query.Substring(query.LastIndexOf('.') + 1);

                    if (tldPart.Contains('*'))
                    {
                        var tldQuery = tldPart.Substring(0, tldPart.IndexOf('*'));
                        foreach (var tld in tlds.Where(t => t.StartsWith(tldQuery, StringComparison.OrdinalIgnoreCase)))
                        {
                            bool available = IsAvailable(keywordPart, tld);
                            Console.WriteLine("{0} -- {1}", tld, available);
                        }
                    }
                    else if (tlds.Any(t => t.Equals(tldPart, StringComparison.OrdinalIgnoreCase)))
                    {
                        bool available = IsAvailable(keywordPart, tldPart);
                        Console.WriteLine(available);
                    }
                    else
                        Console.WriteLine("TLD not valid.");                    
                }

                Console.Write("Query> ");
            }
        }

        static List<string> GetAllTLDs()
        {
            List<string> ret = new List<string>();
            WebRequest webRequest = WebRequest.Create(MasterTLDListURL);

            using (WebResponse response = webRequest.GetResponse())
            using (Stream content = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(content))
            {
                while (!reader.EndOfStream)
                {
                    string tld = reader.ReadLine();
                    if (tld == null || tld.Contains("#") || tld.Contains("--")) continue;

                    ret.Add(tld);
                }
            }

            return ret;
        }

        /// <summary>
        /// http://stackoverflow.com/a/12701846
        /// </summary>
        static bool IsAvailable(string keyword, string tld)
        {            
            string whoisForRoot = GetWhoisInformation(RootTLDServerURL, tld);
            whoisForRoot = whoisForRoot.Remove(0, whoisForRoot.IndexOf("whois:", StringComparison.Ordinal) + 6).TrimStart();

            string tldServer = whoisForRoot.Substring(0, whoisForRoot.IndexOf('\r'));
            string domain = string.Format("{0}.{1}", keyword, tld);
            string whois = GetWhoisInformation(tldServer, domain);

            return whois.Contains("Domain not found") || whois.Contains("No match for");
        }

        static string GetWhoisInformation(string whoisServer, string url)
        {
            try
            {
                StringBuilder ret = new StringBuilder();

                using (TcpClient whoisClient = new TcpClient(whoisServer, 43))
                using (NetworkStream networkStream = whoisClient.GetStream())
                using (BufferedStream bufferedStream = new BufferedStream(networkStream))
                {
                    StreamWriter streamWriter = new StreamWriter(bufferedStream);
                    streamWriter.WriteLine(url);
                    streamWriter.Flush();

                    StreamReader streamReader = new StreamReader(bufferedStream);

                    while (!streamReader.EndOfStream)
                        ret.AppendLine(streamReader.ReadLine());
                }

                return ret.ToString();
            }
            catch
            {
                return "Query failed";
            }
        }
    }
}
