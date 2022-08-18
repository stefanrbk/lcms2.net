namespace lcms2.it8_template;
public struct Property
{
    public string Id;
    public WriteMode As;

    public Property(string id, WriteMode @as) =>
        (Id, As) = (id, @as);

    public static readonly Property[] PredefinedProperties = new Property[]
    {
        // Required - NUMBER OF FIELDS
        new("NUMBER_OF_FIELDS", WriteMode.Uncooked),

        // Required - NUMBER OF SETS
        new("NUMBER_OF_SETS",   WriteMode.Uncooked),

        // Required - Identifies the specific system, organization or individual that created the data file.
        new("ORIGINATOR",       WriteMode.Stringify),

        // Required - Describes the purpose or contents of the data file.
        new("FILE_DESCRIPTOR",  WriteMode.Stringify),

        // Required - Indicates date of creation of the data file.
        new("CREATED",          WriteMode.Stringify),

        // Required  - Describes the purpose or contents of the data file.
        new("DESCRIPTOR",       WriteMode.Stringify),

        // The diffuse geometry used. Allowed values are "sphere" or "opal".
        new("DIFFUSE_GEOMETRY", WriteMode.Stringify),

        new("MANUFACTURER",     WriteMode.Stringify),

        // Some broken Fuji targets does store this value
        new("MANUFACTURE",      WriteMode.Stringify),

        // Identifies year and month of production of the target in the form yyyy:mm.
        new("PROD_DATE",        WriteMode.Stringify),

        // Uniquely identifies individual physical target.
        new("SERIAL",           WriteMode.Stringify),

        // Identifies the material on which the target was produced using a code
        // uniquely identifying th e material. This is intend ed to be used for IT8.7
        // physical targets only (i.e . IT8.7/1 and IT8.7/2).
        new("MATERIAL",         WriteMode.Stringify),

        // Used to report the specific instrumentation used (manufacturer and
        // model number) to generate the data reported. This data will often
        // provide more information about the particular data collected than an
        // extensive list of specific details. This is particularly important for
        // spectral data or data derived from spectrophotometry.
        new("INSTRUMENTATION",  WriteMode.Stringify),

        // Illumination used for spectral measurements. This data helps provide
        // a guide to the potential for issues of paper fluorescence, etc.
        new("MEASUREMENT_SOURCE", WriteMode.Stringify),

        // Used to define the characteristics of the printed sheet being reported.
        // Where standard conditions have been defined (e.g., SWOP at nominal)
        // named conditions may suffice. Otherwise, detailed information is
        // needed.
        new("PRINT_CONDITIONS", WriteMode.Stringify),

        // Identifies the backing material used behind the sample during
        // measurement. Allowed values are "black", "white", or {"na".
        new("SAMPLE_BACKING",   WriteMode.Stringify),

        // Degrees of freedom associated with the Chi squared statistic
        // below properties are new in recent specs:
        new("CHISQ_DOF",        WriteMode.Stringify),

        // The type of measurement, either reflection or transmission, should be indicated
        // along with details of the geometry and the aperture size and shape. For example,
        // for transmission measurements it is important to identify 0/diffuse, diffuse/0,
        // opal or integrating sphere, etc. For reflection it is important to identify 0/45,
        // 45/0, sphere (specular included or excluded), etc.
        new("MEASUREMENT_GEOMETRY", WriteMode.Stringify),

        // Identifies the use of physical filter(s) during measurement. Typically used to
        // denote the use of filters such as none, D65, Red, Green or Blue.
        new("FILTER",            WriteMode.Stringify),

        // Identifies the use of a physical polarization filter during measurement. Allowed
        // values are {"yes", "white", "none" or "na".
        new("POLARIZATION",      WriteMode.Stringify),

        // Indicates such functions as: the CIE standard observer functions used in the
        // calculation of various data parameters (2 degree and 10 degree), CIE standard
        // illuminant functions used in the calculation of various data parameters (e.g., D50,
        // D65, etc.), density status response, etc. If used there shall be at least one
        // name-value pair following the WEIGHTING_FUNCTION tag/keyword. The first attribute
        // in the set shall be {"name" and shall identify the particular parameter used.
        // The second shall be {"value" and shall provide the value associated with that name.
        // For ASCII data, a string containing the Name and Value attribute pairs shall follow
        // the weighting function keyword. A semi-colon separates attribute pairs from each
        // other and within the attribute the name and value are separated by a comma.
        new("WEIGHTING_FUNCTION", WriteMode.Pair),

        // Parameter that is used in computing a value from measured data. Name is the name
        // of the calculation, parameter is the name of the parameter used in the calculation
        // and value is the value of the parameter.
        new("COMPUTATIONAL_PARAMETER", WriteMode.Pair),

        // The type of target being measured, e.g. IT8.7/1, IT8.7/3, user defined, etc.                                            
        new("TARGET_TYPE",        WriteMode.Stringify),

        // Identifies the colorant(s) used in creating the target.                                           
        new("COLORANT",           WriteMode.Stringify),

        // Describes the purpose or contents of a data table.                                           
        new("TABLE_DESCRIPTOR",   WriteMode.Stringify),

        // Provides a short name for a data table.
        new("TABLE_NAME",         WriteMode.Stringify)
    };

    public static int NumPredefinedProperties =>
        PredefinedProperties.Length;
}
