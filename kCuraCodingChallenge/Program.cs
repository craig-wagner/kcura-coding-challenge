using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommandLine;

namespace kCuraCodingChallenge
{
    class Program
    {
        private const string _citiesByPopulation = "Cities_By_Population.txt";
        private const string _interstatesByCity = "Interstates_By_City.txt";
        private static string _degreesFromBaseCity = "Degrees_From_{0}.txt";

        private static Options _options;

        static void Main( string[] args )
        {
            _options = new Options();

            if( Parser.Default.ParseArguments( args, _options ) )
            {
                if( !File.Exists( _options.InputFile ) )
                {
                    Console.WriteLine();
                    Console.WriteLine( "\"{0}\" not found.", _options.InputFile );
                }
                else
                {
                    _degreesFromBaseCity = String.Format( _degreesFromBaseCity, _options.BaseCity );

                    IList<City> cities = LoadCities();

                    Console.WriteLine();
                    Console.WriteLine( "Writing files..." );
                    Console.WriteLine();

                    WriteCitiesByPopulation( cities );

                    WriteInterstatesByCity( cities );

                    WriteDegreesFromBaseCity( cities );
                }

                if( Debugger.IsAttached )
                {
                    Console.WriteLine();
                    Console.Write( "Press any key to continue..." );
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Load all the cities into objects in memory so we don't have to keep going back to
        /// disk every time.
        /// </summary>
        private static IList<City> LoadCities()
        {
            IList<City> cities = new List<City>();

            using( var reader = new StreamReader( "Cities.txt" ) )
            {
                string line;

                while( (line = reader.ReadLine()) != null )
                {
                    string[] parts = line.Split( '|' );

                    var city = new City();
                    city.Population = Convert.ToInt32( parts[0] );
                    city.Name = parts[1];
                    city.State = parts[2];
                    city.Interstates.AddRange( parts[3].Split( ';' ) );

                    cities.Add( city );
                }
            }

            return cities;
        }

        private static void WriteCitiesByPopulation( IList<City> cities )
        {
            var orderedCities = cities.OrderByDescending( c => c.Population )
                                    .ThenBy( c => c.State )
                                    .ThenBy( c => c.Name )
                                    .ToList();

            using( var writer = new StreamWriter( _citiesByPopulation ) )
            {
                int previousPopulation = -1;

                foreach( var city in orderedCities )
                {
                    if( previousPopulation != city.Population )
                    {
                        writer.WriteLine( city.Population );
                        writer.WriteLine();

                        previousPopulation = city.Population;
                    }

                    writer.WriteLine( "{0}, {1}", city.Name, city.State );
                    writer.Write( "Interstates: " );
                    writer.WriteLine( String.Join( ", ", city.Interstates ) );
                    writer.WriteLine();
                }
            }

            Console.WriteLine( "  {0} has been written.", _citiesByPopulation );
        }

        private static void WriteInterstatesByCity( IList<City> cities )
        {
            // The OrderBy is a little funky because the requirement is to sort by the 
            // interstate number. As stored (e.g. I-5) the interstate is a string so it
            // doesn't use numeric sorting (e.g. I-5 comes after I-10), so we need to extract
            // the number of the interstate, turn it into an actual number, and sort on that.
            var interstates = cities.SelectMany( c => c.Interstates )
                                .Distinct()
                                .OrderBy( i => Convert.ToInt32( i.Substring( 2 ) ) )
                                .ToList();

            using( var writer = new StreamWriter( _interstatesByCity ) )
            {
                foreach( var interstate in interstates )
                {
                    int count = cities.Count( c => c.Interstates.Contains( interstate ) );

                    writer.Write( interstate );
                    writer.Write( " " );
                    writer.WriteLine( count );
                }
            }

            Console.WriteLine( "  {0} has been written.", _interstatesByCity );
        }

        private static void WriteDegreesFromBaseCity( IList<City> cities )
        {
            // Start by finding the base city and initializing it as our starting point.
            var baseCity = cities.Where( c => c.Name.Equals( _options.BaseCity, StringComparison.OrdinalIgnoreCase ) ).SingleOrDefault();

            if( baseCity == null )
            {
                Console.WriteLine();
                Console.WriteLine( "  Provided base city \"{0}\" not found in dataset.", _options.BaseCity );
                Console.WriteLine( "  The file \"{0}\" will not be produced.", _degreesFromBaseCity );
                Console.WriteLine();
            }
            else
            {
                baseCity.DegreesRemoved = 0;

                bool cityFound = false;
                int degreesRemoved = 1;

                do
                {
                    cityFound = false;

                    int previousDegreesRemoved = degreesRemoved - 1;

                    // Get the cities that have one degree less separation than what we're
                    // currently looking for
                    var previousCitiesInterstates =
                        cities.Where( c => c.DegreesRemoved == previousDegreesRemoved )
                            .SelectMany( c => c.Interstates ).ToList();

                    // Get the cities for which we haven't already figured out the degrees of separation
                    var citiesToEvaluate = cities.Where( c => !c.DegreesRemoved.HasValue ).ToList();

                    foreach( var city in citiesToEvaluate )
                    {
                        // Determine if any of the interstates for the current city have a 
                        // connection to any of the interstates for the previous degree of separation
                        if( previousCitiesInterstates.Intersect( city.Interstates ).Any() )
                        {
                            city.DegreesRemoved = degreesRemoved;
                            cityFound = true;
                        }
                    }

                    degreesRemoved++;
                } while( cityFound );

                // Take care of any cities that have no direct or indirect connection to base city
                foreach( var city in cities.Where( c => !c.DegreesRemoved.HasValue ) )
                {
                    city.DegreesRemoved = -1;
                }

                var orderedList = cities.OrderByDescending( c => c.DegreesRemoved )
                                        .ThenBy( c => c.Name )
                                        .ThenBy( c => c.State )
                                        .ToList();

                using( var writer = new StreamWriter( _degreesFromBaseCity ) )
                {
                    foreach( var city in orderedList )
                    {
                        writer.Write( city.DegreesRemoved );
                        writer.Write( " " );
                        writer.WriteLine( "{0}, {1}", city.Name, city.State );
                    }
                }

                Console.WriteLine( "  {0} has been written.", _degreesFromBaseCity );
            }
        }
    }
}
