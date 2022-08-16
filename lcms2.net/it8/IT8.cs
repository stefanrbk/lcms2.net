using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcms2.state;

namespace lcms2.it8;
public class IT8
{
    public const int MaxId = 128;
    public const int MaxStr = 1024;
    public const int MaxTables = 255;
    public const int MaxInclude = 20;

    /// <summary>
    /// How many tables in this stream
    /// </summary>
    public int TablesCount;

    public int NumTable;

    public Table[] Tables = new Table[MaxTables];

    // Parser state machine
    public Symbol Sy;
    public int Ch;

    public int Inum;
    public double Dnum;

    public String Id;
    public String Str;

    // Allowed keywords & datasets. They have visibility on whole stream
    public KeyValue[] ValidKeywords;
    public KeyValue[] ValidSampleId;

    public byte[] Source;
    public int LineNo;

    public FileContext[] FileStack = new FileContext[MaxInclude];
    public int IncludeStackPointer;

    public byte[]? MemoryBlock;

    public byte[] DoubleFormatter = new byte[MaxId];

    public Context? Context;

    public static readonly string[] PredefinedSampleId = new string[]
    {
        "SAMPLE_ID",      // Identifies sample that data represents
        "STRING",         // Identifies label, or other non-machine readable value.
                          // Value must begin and end with a " symbol

        "CMYK_C",         // Cyan component of CMYK data expressed as a percentage
        "CMYK_M",         // Magenta component of CMYK data expressed as a percentage
        "CMYK_Y",         // Yellow component of CMYK data expressed as a percentage
        "CMYK_K",         // Black component of CMYK data expressed as a percentage
        "D_RED",          // Red filter density
        "D_GREEN",        // Green filter density
        "D_BLUE",         // Blue filter density
        "D_VIS",          // Visual filter density
        "D_MAJOR_FILTER", // Major filter d ensity
        "RGB_R",          // Red component of RGB data
        "RGB_G",          // Green component of RGB data
        "RGB_B",          // Blue com ponent of RGB data
        "SPECTRAL_NM",    // Wavelength of measurement expressed in nanometers
        "SPECTRAL_PCT",   // Percentage reflectance/transmittance
        "SPECTRAL_DEC",   // Reflectance/transmittance
        "XYZ_X",          // X component of tristimulus data
        "XYZ_Y",          // Y component of tristimulus data
        "XYZ_Z",          // Z component of tristimulus data
        "XYY_X",          // x component of chromaticity data
        "XYY_Y",          // y component of chromaticity data
        "XYY_CAPY",       // Y component of tristimulus data
        "LAB_L",          // L* component of Lab data
        "LAB_A",          // a* component of Lab data
        "LAB_B",          // b* component of Lab data
        "LAB_C",          // C*ab component of Lab data
        "LAB_H",          // hab component of Lab data
        "LAB_DE",         // CIE dE
        "LAB_DE_94",      // CIE dE using CIE 94
        "LAB_DE_CMC",     // dE using CMC
        "LAB_DE_2000",    // CIE dE using CIE DE 2000
        "MEAN_DE",        // Mean Delta E (LAB_DE) of samples compared to batch average
                          // (Used for data files for ANSI IT8.7/1 and IT8.7/2 targets)
        "STDEV_X",        // Standard deviation of X (tristimulus data)
        "STDEV_Y",        // Standard deviation of Y (tristimulus data)
        "STDEV_Z",        // Standard deviation of Z (tristimulus data)
        "STDEV_L",        // Standard deviation of L*
        "STDEV_A",        // Standard deviation of a*
        "STDEV_B",        // Standard deviation of b*
        "STDEV_DE",       // Standard deviation of CIE dE
        "CHI_SQD_PAR"};   // The average of the standard deviations of L*, a* and b*. It is
                          // used to derive an estimate of the chi-squared parameter which is
                          // recommended as the predictor of the variability of dE

    public static int NumPredefinedSampleId =>
        PredefinedSampleId.Length;
}

public enum WriteMode
{
    Uncooked,
    Stringify,
    Hexadecimal,
    Binary,
    Pair,
}