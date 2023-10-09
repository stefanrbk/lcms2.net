//---------------------------------------------------------------------------------
//
//  Little Color Management System, multithreaded extensions
//  Copyright (c) 1998-2022 Marti Maria Saguer, all rights reserved
//                     2023 Stefan Kewatt, all rights reserved
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

using lcms2.state;

namespace lcms2.ThreadedPlugin;
public static partial class Threaded
{
    private record ThreadAdaptorParam(Transform2Fn worker, WorkSlice param);

    private static void thread_adaptor(object p)
    {
        var ap = (ThreadAdaptorParam)p;
        var s = ap.param;

        ap.worker(s.CMMcargo, s.InputBuffer.Span, s.OutputBuffer.Span, s.PixelsPerLine, s.LineCount, s.Stride);
    }

    internal static int _cmsThrCreateWorker(Context _1, Transform2Fn worker, WorkSlice param)
    {
        var p = new ThreadAdaptorParam(worker, param);

        var err = Task.Run(() => thread_adaptor(p));
        return err.Id;
    }

    internal static void _cmsThrJoinWorker(Context _1, Task hWorker) =>
        hWorker.Wait();

    internal static int _cmsThrIdealThreadCount() =>
        Environment.ProcessorCount;
}
