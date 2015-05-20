using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace DomainChecker
{
    class Program
    {
        private const string RootTLDServer = "whois.iana.org";

        /// <summary>
        /// http://stackoverflow.com/a/12701846
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            
            string query;

            Console.Write("Query> ");
            while ((query = Console.ReadLine()) != string.Empty)
            {
                if (query != null)
                {
                    var available = IsAvailable(query);
                    Console.WriteLine(available);
                }

                Console.Write("Query> ");
            }
        }

        static bool IsAvailable(string url)
        {
            string tld = url.Substring(url.LastIndexOf('.') + 1);
            string whoisForRoot = GetWhoisInformation(RootTLDServer, tld);
            whoisForRoot = whoisForRoot.Remove(0, whoisForRoot.IndexOf("whois:", StringComparison.Ordinal) + 6).TrimStart();

            string tldServer = whoisForRoot.Substring(0, whoisForRoot.IndexOf('\r'));
            string whois = GetWhoisInformation(tldServer, url);

            return whois.Contains("Domain not found") || whois.Contains("No match for");
        }

        static string GetWhoisInformation(string whoisServer, string url)
        {
            try
            {
                StringBuilder ret = new StringBuilder();
                TcpClient whoisClient = new TcpClient(whoisServer, 43);
                NetworkStream networkStream = whoisClient.GetStream();
                BufferedStream bufferedStream = new BufferedStream(networkStream);
                StreamWriter streamWriter = new StreamWriter(bufferedStream);

                streamWriter.WriteLine(url);
                streamWriter.Flush();

                StreamReader streamReader = new StreamReader(bufferedStream);

                while (!streamReader.EndOfStream)
                    ret.AppendLine(streamReader.ReadLine());

                return ret.ToString();
            }
            catch
            {
                return "Query failed";
            }
        }
    }
}
