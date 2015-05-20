using System.Collections.Generic;

namespace DomainChecker
{
    public class DomainSearchParameters
    {
        public string Keyword { get; set; }
        public List<string> TLDs { get; set; }

        public DomainSearchParameters()
        {
            TLDs = new List<string>();
        }
    }
}