using System;
using System.Collections.Generic;
using System.Linq;

namespace DomainChecker
{
    class Program
    {                
        static void Main(string[] args)
        {            
            string query;
            List<string> tlds = Whois.GetAllTLDs();

            Console.WriteLine("{0} TLDs loaded.", tlds.Count);
            Console.WriteLine();
            Console.Write("Query> ");

            while ((query = Console.ReadLine()) != string.Empty)
            {
                if (query != null)
                {
                    DomainSearchParameters parameters = ParseQuery(query, tlds);
                    List<string> availableTLDs = CheckForAvailableTLDs(parameters);

                    Console.WriteLine();
                    Console.WriteLine();

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
            
            var total = parameters.TLDs.Count;
            var progress = 0;

            foreach (var tld in parameters.TLDs)
            {
                progress++;

                try
                {
                    bool available = Whois.IsAvailable(parameters.Keyword, tld);
                    if (available) ret.Add(tld);
                }
                catch
                {
                    Console.WriteLine();
                    Console.WriteLine("{0} - Error!", tld);
                }

                ProgressDisplay.Update(progress, total);
            }

            return ret;
        }        
    }
}
