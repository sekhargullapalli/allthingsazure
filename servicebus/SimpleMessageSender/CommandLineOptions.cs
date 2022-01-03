
using CommandLine;

public class CommandLineOptions
{
    [Option('t', "topic", Required = false, HelpText = "Service bus topic")]
    public string Topic { get; set; } = "default";
}


