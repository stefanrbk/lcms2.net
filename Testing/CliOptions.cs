using CommandLine;

namespace lcms2.testbed;
public class CliOptions
{
    [Option(
        's',
        "speed",
        Required = false,
        HelpText = "Whether or not to run speed tests",
        Default = true)]
    public bool DoSpeed { get; set; }

    [Option(
        'e',
        "exhaustive",
        Required = false,
        HelpText = "Whether or not to run exhaustive interpolation tests",
        Default = false)]
    public bool DoExhaustive { get; set; }
    
    [Option(
        'c',
        "checks",
        Required = false,
        HelpText = "Whether or not to run regular tests",
        Default = true)]
    public bool DoChecks { get; set; }
    
    [Option(
        'p',
        "plugins",
        Required = false,
        HelpText = "Whether or not to run plugin tests",
        Default = true)]
    public bool DoPlugins { get; set; }
    
    [Option(
        'z',
        "zoo",
        Required = false,
        HelpText = "Whether or not to run zoo tests",
        Default = false)]
    public bool DoZoo { get; set; }
}
