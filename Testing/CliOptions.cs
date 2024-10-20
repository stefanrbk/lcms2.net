﻿//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright ©️ 1998-2024 Marti Maria Saguer
//              2022-2024 Stefan Kewatt
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

using CommandLine;

namespace lcms2.testbed;

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
        'e',
        "exhaustive",
        Required = false,
        HelpText = "Whether or not to run exhaustive interpolation tests",
        Default = false)]
    public bool DoExhaustive { get; set; }

    [Option(
        'p',
        "no-plugins",
        Required = false,
        HelpText = "Whether or not to run plugin tests",
        Default = false)]
    public bool NoPlugins { get; set; }

    [Option(
                    's',
        "speed",
        Required = false,
        HelpText = "Whether or not to run speed tests",
        Default = false)]
    public bool DoSpeed { get; set; }

    [Option(
        'z',
        "zoo",
        Required = false,
        HelpText = "Whether or not to run zoo tests",
        Default = false)]
    public bool DoZoo { get; set; }

    [Option(
        't',
        "time",
        Required = false,
        HelpText = "Whether or not to time each test",
        Default = false)]
    public bool TimeTests { get; set; }

    #endregion Properties
}
