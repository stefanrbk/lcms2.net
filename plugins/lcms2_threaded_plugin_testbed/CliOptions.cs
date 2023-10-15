using CommandLine;

namespace lcms2.ThreadedPlugin.testbed;

public class CliOptions
{
    #region Properties

    [Option(
        'c',
        "no-checks",
        Required = false,
        HelpText = "Whether or not to run regular tests",
        Default = false)]
    public bool NoChecks { get; set; }

    [Option(
                    's',
        "speed",
        Required = false,
        HelpText = "Whether or not to run speed tests",
        Default = false)]
    public bool DoSpeed { get; set; }

    #endregion Properties
}
