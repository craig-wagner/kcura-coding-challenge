using CommandLine;
using CommandLine.Text;

namespace kCuraCodingChallenge
{
    class Options
    {
        [Option( 'f', "file", DefaultValue = "cities.txt", HelpText = "Name of the input file." )]
        public string InputFile { get; set; }

        [Option( 'c', "city", DefaultValue = "Chicago", HelpText = "Name of the city to use as the base for Degrees_From_Chicago report." )]
        public string BaseCity { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild( this, ( HelpText current ) => HelpText.DefaultParsingErrorsHandler( this, current ) );
        }
    }
}
