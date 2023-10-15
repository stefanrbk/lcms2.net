//---------------------------------------------------------------------------------
//
//  Little Color Management System, multithreaded extensions
//  Copyright (c) 1998-2023 Marti Maria Saguer, all rights reserved
//                2022-2023 Stefan Kewatt, all rights reserved
//
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//---------------------------------------------------------------------------------
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
