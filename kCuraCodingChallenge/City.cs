using System.Collections.Generic;
using System.Diagnostics;

namespace kCuraCodingChallenge
{
    [DebuggerDisplay( "{Name}, {State}" )]
    public class City
    {
        public int Population { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public List<string> Interstates { get; private set; }
        public int? DegreesRemoved { get; set; }

        public City()
        {
            Interstates = new List<string>();
        }
    }
}
