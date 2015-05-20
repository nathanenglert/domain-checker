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
            Console.WriteLine();
            Console.Write("Query> ");

            while ((query = Console.ReadLine()) != string.Empty)
            {
                if (query != null)
                {
                    DomainSearchParameters parameters = ParseQuery(query, tlds);
                    List<string> availableTLDs = CheckForAvailableTLDs(parameters);

                    if (!availableTLDs.Any())
                        Console.WriteLine("No domains available.");
                    else
                    {
                        foreach (var tld in availableTLDs)
                            Console.WriteLine(tld);
                    }
                }

                Console.WriteLine();
                Console.Write("Query> ");
            }
        }

        private static DomainSearchParameters ParseQuery(string query, List<string> tlds)
        {
            DomainSearchParameters ret = new DomainSearchParameters
            {
                Keyword = query.Substring(0, query.LastIndexOf('.'))
            };

            // Look for wildcards and invalid TLDs
            string tldPart = query.Substring(query.LastIndexOf('.') + 1);
            if (tldPart.Contains('*'))
            {
                var tldQuery = tldPart.Substring(0, tldPart.IndexOf('*'));
                ret.TLDs = tlds.Where(t => t.StartsWith(tldQuery, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else if (tlds.Any(t => t.Equals(tldPart, StringComparison.OrdinalIgnoreCase)))
                ret.TLDs.Add(tldPart);

            return ret;
        }

        static List<string> CheckForAvailableTLDs(DomainSearchParameters parameters)
        {
            List<string> ret = new List<string>();

            try
            {
                ret.AddRange(
                    from tld in parameters.TLDs
                    let available = IsAvailable(parameters.Keyword, tld)
                    where available
                    select tld);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex.Message);                
            }            

            return ret;
        } 

        static List<string> GetAllTLDs()
        {
            List<string> ret = new List<string>();
            WebRequest webRequest = WebRequest.Create(MasterTLDListURL);

            // Get the most recent list of TLDs to search on from IANA
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

        static bool IsAvailable(string keyword, string tld)
        {            
            // Search IANA to find the whois server for the given TLD
            string whoisForRoot = GetWhoisInformation(RootTLDServerURL, tld);
            whoisForRoot = whoisForRoot.Remove(0, whoisForRoot.IndexOf("whois:", StringComparison.Ordinal) + 6).TrimStart();

            // Search the resulting whois server to check domain availability
            string tldServer = whoisForRoot.Substring(0, whoisForRoot.IndexOf('\r'));
            string domain = string.Format("{0}.{1}", keyword, tld);
            string whois = GetWhoisInformation(tldServer, domain);

            // TODO: Some whois servers may be returning different results for non matching domains
            return whois.Contains("Domain not found") || whois.Contains("No match for");
        }

        static string GetWhoisInformation(string whoisServer, string url)
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
    }
}
