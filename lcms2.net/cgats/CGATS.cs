//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022-2023 Stefan Kewatt
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//---------------------------------------------------------------------------------
//

namespace lcms2.cgats;
internal static class CGATS
{
    public const ushort MAXID = 128;
    public const ushort MAXSTR = 1024;
    public const ushort MAXTABLES = 255;
    public const ushort MAXINCLUDE = 20;
    public static readonly byte[] DEFAULT_DBL_FORMAT = "{0:D}"u8.ToArray();
    public static readonly byte[] DEFAULT_NUM_FORMAT = "{0:D}"u8.ToArray();
    public static readonly byte[] DEFAULT_HEX_FORMAT = "{0:x}"u8.ToArray();
    public static readonly byte[] COMMENT_DELIMITER = "# "u8.ToArray();
    public static readonly byte DIR_CHAR = (byte)Path.DirectorySeparatorChar;

    public static readonly byte[] BinaryConversionBuffer = new byte[33];

    public static readonly KEYWORD[] TabKeys =
    {
        new() { id = "$INCLUDE"u8.ToArray(), sy = SYMBOL.SINCLUDE },
        new() { id = ".INCLUDE"u8.ToArray(), sy = SYMBOL.SINCLUDE },

        new() { id = "BEGIN_DATA"u8.ToArray(), sy = SYMBOL.SBEGIN_DATA },
        new() { id = "BEGIN_DATA_FORMAT"u8.ToArray(), sy = SYMBOL.SBEGIN_DATA_FORMAT },
        new() { id = "DATA_FORMAT_IDENTIFIER"u8.ToArray(), sy = SYMBOL.SDATA_FORMAT_ID },
        new() { id = "END_DATA"u8.ToArray(), sy = SYMBOL.SEND_DATA },
        new() { id = "END_DATA_FORMAT"u8.ToArray(), sy = SYMBOL.SEND_DATA_FORMAT },
        new() { id = "KEYWORD"u8.ToArray(), sy = SYMBOL.SKEYWORD },
    };
    public static int NUMKEYS => TabKeys.Length;

    public static readonly PROPERTY[] PredefinedProperties =
    {
        new() { id = "NUMBER_OF_FIELDS"u8.ToArray(), @as = WRITEMODE.WRITE_UNCOOKED},    // Required - NUMBER OF FIELDS
        new() { id = "NUMBER_OF_SETS"u8.ToArray(),   @as = WRITEMODE.WRITE_UNCOOKED},    // Required - NUMBER OF SETS
        new() { id = "ORIGINATOR"u8.ToArray(),       @as = WRITEMODE.WRITE_STRINGIFY},   // Required - Identifies the specific system, organization or individual that created the data file.
        new() { id = "FILE_DESCRIPTOR"u8.ToArray(),  @as = WRITEMODE.WRITE_STRINGIFY},   // Required - Describes the purpose or contents of the data file.
        new() { id = "CREATED"u8.ToArray(),          @as = WRITEMODE.WRITE_STRINGIFY},   // Required - Indicates date of creation of the data file.
        new() { id = "DESCRIPTOR"u8.ToArray(),       @as = WRITEMODE.WRITE_STRINGIFY},   // Required  - Describes the purpose or contents of the data file.
        new() { id = "DIFFUSE_GEOMETRY"u8.ToArray(), @as = WRITEMODE.WRITE_STRINGIFY},   // The diffuse geometry used. Allowed values are "sphere" or "opal".
        new() { id = "MANUFACTURER"u8.ToArray(),     @as = WRITEMODE.WRITE_STRINGIFY},
        new() { id = "MANUFACTURE"u8.ToArray(),      @as = WRITEMODE.WRITE_STRINGIFY},   // Some broken Fuji targets does store this value
        new() { id = "PROD_DATE"u8.ToArray(),        @as = WRITEMODE.WRITE_STRINGIFY},   // Identifies year and month of production of the target in the form yyyy:mm.
        new() { id = "SERIAL"u8.ToArray(),           @as = WRITEMODE.WRITE_STRINGIFY},   // Uniquely identifies individual physical target.

        new() { id = "MATERIAL"u8.ToArray(),         @as = WRITEMODE.WRITE_STRINGIFY},    // Identifies the material on which the target was produced using a code
                                                  // uniquely identifying th e material. This is intend ed to be used for IT8.7
                                                  // physical targets only (i.e . IT8.7/1 and IT8.7/2).

        new() { id = "INSTRUMENTATION"u8.ToArray(),  @as = WRITEMODE.WRITE_STRINGIFY},    // Used to report the specific instrumentation used (manufacturer and
                                                  // model number) to generate the data reported. This data will often
                                                  // provide more information about the particular data collected than an
                                                  // extensive list of specific details. This is particularly important for
                                                  // spectral data or data derived from spectrophotometry.

        new() { id = "MEASUREMENT_SOURCE"u8.ToArray(), @as = WRITEMODE.WRITE_STRINGIFY},  // Illumination used for spectral measurements. This data helps provide
                                                  // a guide to the potential for issues of paper fluorescence, etc.

        new() { id = "PRINT_CONDITIONS"u8.ToArray(), @as = WRITEMODE.WRITE_STRINGIFY},     // Used to define the characteristics of the printed sheet being reported.
                                                   // Where standard conditions have been defined (e.g., SWOP at nominal)
                                                   // named conditions may suffice. Otherwise, detailed information is
                                                   // needed.

        new() { id = "SAMPLE_BACKING"u8.ToArray(),   @as = WRITEMODE.WRITE_STRINGIFY},     // Identifies the backing material used behind the sample during
                                                   // measurement. Allowed values are "black", "white", or {"na".
                                                  
        new() { id = "CHISQ_DOF"u8.ToArray(),        @as = WRITEMODE.WRITE_STRINGIFY},     // Degrees of freedom associated with the Chi squared statistic
                                                   // below properties are new in recent specs:

        new() { id = "MEASUREMENT_GEOMETRY"u8.ToArray(), @as = WRITEMODE.WRITE_STRINGIFY}, // The type of measurement, either reflection or transmission, should be indicated
                                                   // along with details of the geometry and the aperture size and shape. For example,
                                                   // for transmission measurements it is important to identify 0/diffuse, diffuse/0,
                                                   // opal or integrating sphere, etc. For reflection it is important to identify 0/45,
                                                   // 45/0, sphere (specular included or excluded), etc.

       new() { id = "FILTER"u8.ToArray(),            @as = WRITEMODE.WRITE_STRINGIFY},     // Identifies the use of physical filter(s) during measurement. Typically used to
                                                   // denote the use of filters such as none, D65, Red, Green or Blue.
                                                  
       new() { id = "POLARIZATION"u8.ToArray(),      @as = WRITEMODE.WRITE_STRINGIFY},     // Identifies the use of a physical polarization filter during measurement. Allowed
                                                   // values are {"yes", "white", "none" or "na".

       new() { id = "WEIGHTING_FUNCTION"u8.ToArray(), @as = WRITEMODE.WRITE_PAIR},         // Indicates such functions as: the CIE standard observer functions used in the
                                                   // calculation of various data parameters (2 degree and 10 degree), CIE standard
                                                   // illuminant functions used in the calculation of various data parameters (e.g., D50,
                                                   // D65, etc.), density status response, etc. If used there shall be at least one
                                                   // name-value pair following the WEIGHTING_FUNCTION tag/keyword. The first attribute
                                                   // in the set shall be {"name" and shall identify the particular parameter used.
                                                   // The second shall be {"value" and shall provide the value associated with that name.
                                                   // For ASCII data, a string containing the Name and Value attribute pairs shall follow
                                                   // the weighting function keyword. A semi-colon separates attribute pairs from each
                                                   // other and within the attribute the name and value are separated by a comma.

       new() { id = "COMPUTATIONAL_PARAMETER"u8.ToArray(), @as = WRITEMODE.WRITE_PAIR},    // Parameter that is used in computing a value from measured data. Name is the name
                                                   // of the calculation, parameter is the name of the parameter used in the calculation
                                                   // and value is the value of the parameter.
                                                   
       new() { id = "TARGET_TYPE"u8.ToArray(),        @as = WRITEMODE.WRITE_STRINGIFY},    // The type of target being measured, e.g. IT8.7/1, IT8.7/3, user defined, etc.
                                                  
       new() { id = "COLORANT"u8.ToArray(),           @as = WRITEMODE.WRITE_STRINGIFY},    // Identifies the colorant(s) used in creating the target.
                                                  
       new() { id = "TABLE_DESCRIPTOR"u8.ToArray(),   @as = WRITEMODE.WRITE_STRINGIFY},    // Describes the purpose or contents of a data table.
                                                  
       new() { id = "TABLE_NAME"u8.ToArray(),         @as = WRITEMODE.WRITE_STRINGIFY}     // Provides a short name for a data table.
    };
    public static int NUMPREDEFINEDPROPS => PredefinedProperties.Length;

    public static readonly byte[][] PredefinedSampleID =
    {
        "SAMPLE_ID"u8.ToArray(),      // Identifies sample that data represents
        "STRING"u8.ToArray(),         // Identifies label, or other non-machine readable value.
                                      // Value must begin and end with a " symbol

        "CMYK_C"u8.ToArray(),         // Cyan component of CMYK data expressed as a percentage
        "CMYK_M"u8.ToArray(),         // Magenta component of CMYK data expressed as a percentage
        "CMYK_Y"u8.ToArray(),         // Yellow component of CMYK data expressed as a percentage
        "CMYK_K"u8.ToArray(),         // Black component of CMYK data expressed as a percentage
        "D_RED"u8.ToArray(),          // Red filter density
        "D_GREEN"u8.ToArray(),        // Green filter density
        "D_BLUE"u8.ToArray(),         // Blue filter density
        "D_VIS"u8.ToArray(),          // Visual filter density
        "D_MAJOR_FILTER"u8.ToArray(), // Major filter d ensity
        "RGB_R"u8.ToArray(),          // Red component of RGB data
        "RGB_G"u8.ToArray(),          // Green component of RGB data
        "RGB_B"u8.ToArray(),          // Blue com ponent of RGB data
        "SPECTRAL_NM"u8.ToArray(),    // Wavelength of measurement expressed in nanometers
        "SPECTRAL_PCT"u8.ToArray(),   // Percentage reflectance/transmittance
        "SPECTRAL_DEC"u8.ToArray(),   // Reflectance/transmittance
        "XYZ_X"u8.ToArray(),          // X component of tristimulus data
        "XYZ_Y"u8.ToArray(),          // Y component of tristimulus data
        "XYZ_Z"u8.ToArray(),          // Z component of tristimulus data
        "XYY_X"u8.ToArray(),          // x component of chromaticity data
        "XYY_Y"u8.ToArray(),          // y component of chromaticity data
        "XYY_CAPY"u8.ToArray(),       // Y component of tristimulus data
        "LAB_L"u8.ToArray(),          // L* component of Lab data
        "LAB_A"u8.ToArray(),          // a* component of Lab data
        "LAB_B"u8.ToArray(),          // b* component of Lab data
        "LAB_C"u8.ToArray(),          // C*ab component of Lab data
        "LAB_H"u8.ToArray(),          // hab component of Lab data
        "LAB_DE"u8.ToArray(),         // CIE dE
        "LAB_DE_94"u8.ToArray(),      // CIE dE using CIE 94
        "LAB_DE_CMC"u8.ToArray(),     // dE using CMC
        "LAB_DE_2000"u8.ToArray(),    // CIE dE using CIE DE 2000
        "MEAN_DE"u8.ToArray(),        // Mean Delta E (LAB_DE) of samples compared to batch average
                                      // (Used for data files for ANSI IT8.7/1 and IT8.7/2 targets)
        "STDEV_X"u8.ToArray(),        // Standard deviation of X (tristimulus data)
        "STDEV_Y"u8.ToArray(),        // Standard deviation of Y (tristimulus data)
        "STDEV_Z"u8.ToArray(),        // Standard deviation of Z (tristimulus data)
        "STDEV_L"u8.ToArray(),        // Standard deviation of L*
        "STDEV_A"u8.ToArray(),        // Standard deviation of a*
        "STDEV_B"u8.ToArray(),        // Standard deviation of b*
        "STDEV_DE"u8.ToArray(),       // Standard deviation of CIE dE
        "CHI_SQD_PAR"u8.ToArray()     // The average of the standard deviations of L*, a* and b*. It is
                                      // used to derive an estimate of the chi-squared parameter which is
                                      // recommended as the predictor of the variability of dE
    };
    public static int NUMPREDEFINEDSAMPLEID => PredefinedSampleID.Length;
}
