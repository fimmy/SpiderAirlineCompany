using System;
using System.Collections.Generic;
using System.Text;

namespace SpiderAirlineCompany
{
    public class AirlineCompany
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Url { get; set; }
        public string PageUrl { get; set; }
        public string IATA { get; set; }
        public string ICAO { get; set; }
        public string Country { get; set; }
    }
}
