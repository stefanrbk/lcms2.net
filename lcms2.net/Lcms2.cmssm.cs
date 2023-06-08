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
using lcms2.state;
using lcms2.types;

using static System.Math;

namespace lcms2;

public static unsafe partial class Lcms2
{
    private const byte SECTORS = 16;

    private readonly record struct _spiral(int AdvX, int AdvY);
    private static readonly _spiral[] Spiral = new _spiral[]
    {
        new(0,  -1), new(+1, -1), new(+1,  0), new(+1, +1), new(0,  +1), new(-1, +1),
        new(-1,  0), new(-1, -1), new(-1, -2), new(0,  -2), new(+1, -2), new(+2, -2),
        new(+2, -1), new(+2,  0), new(+2, +1), new(+2, +2), new(+1, +2), new(0,  +2),
        new(-1, +2), new(-2, +2), new(-2, +1), new(-2, 0),  new(-2, -1), new(-2, -2)
    };
    private static readonly int NSTEPS = Spiral.Length;

    private record struct Spherical(double r, double alpha, double theta);
    private enum GDBPointType { Empty, Specified, Modeled }
    private const GDBPointType GP_EMPTY = GDBPointType.Empty;
    private const GDBPointType GP_SPECIFIED = GDBPointType.Specified;
    private const GDBPointType GP_MODELED = GDBPointType.Modeled;

    private record struct GDBPoint(GDBPointType Type, Spherical p);
    private struct GDB
    {
        public Context ContextID;
        public GDBPoint* Gamut;
        public GDBPoint* GamutPtr(int theta, int alpha) =>
            &Gamut[theta * SECTORS + alpha];
    }
    private record struct Line(VEC3 a, VEC3 u);
    private record struct Plane(VEC3 b, VEC3 v, VEC3 w);

    internal static double _cmsAtan2(double y, double x)
    {
        // Deal with undefined case
        if (x is 0 && y is 0) return 0;

        var a = Atan2(y, x) * 180.0 / M_PI;

        while (a < 0)
            a += 360;

        return a;
    }

    private static void ToSpherical(Spherical* sp, in VEC3* v)
    {
        var L = v->X;
        var a = v->Y;
        var b = v->Z;

        sp->r = Sqrt(L * L + a * a + b * b);

        if (sp->r is 0)
        {
            sp->alpha = sp->theta = 0;
            return;
        }

        sp->alpha = _cmsAtan2(a, b);
        sp->theta = _cmsAtan2(Sqrt(a*a + b*b), L);
    }

    private static void ToCartesian(VEC3* v, in Spherical* sp)
    {
        var sin_alpha = Sin(M_PI * sp->alpha / 180.0);
        var cos_alpha = Cos(M_PI * sp->alpha / 180.0);
        var sin_theta = Sin(M_PI * sp->theta / 180.0);
        var cos_theta = Cos(M_PI * sp->theta / 180.0);

        var a = sp->r * sin_theta * sin_alpha;
        var b = sp->r * sin_theta * cos_alpha;
        var L = sp->r * cos_theta;

        v->X = L;
        v->Y = a;
        v->Z = b;
    }

    private static void QuantizeToSector(in Spherical* sp, int* alpha, int* theta)
    {
        *alpha = (int)Floor(sp->alpha * SECTORS / 360.0);
        *theta = (int)Floor(sp->theta * SECTORS / 180.0);

        if (*alpha >= SECTORS)
            *alpha = SECTORS - 1;
        if (*theta >= SECTORS)
            *theta = SECTORS - 1;
    }

    private static void LineOf2Points(Line* line, VEC3* a, VEC3* b)
    {
        VEC3 vA, vU;
        _cmsVEC3init(&vA, a->X, a->Y, a->Z);
        _cmsVEC3init(&vU, b->X - a->X,
                          b->Y - a->Y,
                          b->Z - a->Z);
        (line->a, line->u) = (vA, vU);
    }

    private static void GetPointOfLine(VEC3* p, in Line* line, double t)
    {
        p->X = line->a.X + t * line->u.X;
        p->Y = line->a.Y + t * line->u.Y;
        p->X = line->a.X + t * line->u.X;
    }

    private static bool ClosestLineToLine(VEC3* r, in Line* line1, in Line* line2)
    {
        VEC3 a1 = line1->a, a2 = line2->a;
        VEC3 u1 = line1->u, u2 = line2->u;
        VEC3 w0;

        double sN, sD, tN, tD;

        _cmsVEC3minus(&w0, &a1, &a2);

        var a = _cmsVEC3dot(&u1, &u1);
        var b = _cmsVEC3dot(&u1, &u2);
        var c = _cmsVEC3dot(&u2, &u2);
        var d = _cmsVEC3dot(&u1, &w0);
        var e = _cmsVEC3dot(&u2, &w0);

        var D = a * c - b * b;      // Denominator
        sD = tD = D;                // default sD = D >= 0

        if (D < MATRIX_DET_TOLERANCE)   // the lines are almost parallel
        {
            sN = 0.0;       // force using point P0 on segment S1
            sD = 1.0;       // to prevent possible division by 0.0 later
            tN = e;
            tD = c;
        }
        else                // get the closest points on the infinite lines
        {
            sN = b * e - c * d;
            tN = a * e - b * d;

            if (sN < 0.0)       // sc < 0 => the s=0 edge is visible
            {
                sN = 0.0;
                tN = e;
                tD = c;
            }
            else if(sN > sD)    // sc > 1 => the s=1 edge is visible
            {
                sN = sD;
                tN = e + b;
                tD = c;
            }
        }

        if (tN < 0.0)           // tc < 0 => the t=0 edge is visible
        {

            tN = 0.0;

            // recompute sc for this edge
            if (-d < 0.0)
                sN = 0.0;
            else if (-d > a)
                sN = sD;
            else
            {
                sN = -d;
                sD = a;
            }
        }
        else if (tN > tD)      // tc > 1 => the t=1 edge is visible
        {

            tN = tD;

            // recompute sc for this edge
            if ((-d + b) < 0.0)
                sN = 0;
            else if ((-d + b) > a)
                sN = sD;
            else
            {
                sN = (-d + b);
                sD = a;
            }
        }

        // finally do the division to get sc and tc
        var sc = Abs(sN) < MATRIX_DET_TOLERANCE ? 0.0 : sN / sD;
        //var tc = Abs(tN) < MATRIX_DET_TOLERANCE ? 0.0 : tN / tD; // left for future use.

        GetPointOfLine(r, line1, sc);

        return true;
    }

    public static HANDLE cmsGBDAlloc(Context ContextID)
    {
        var gbd = _cmsMallocZero<GDB>(ContextID);
        if (gbd is null) return null;

        gbd->ContextID = ContextID;
        gbd->Gamut = _cmsCalloc<GDBPoint>(gbd->ContextID, SECTORS * SECTORS);
        if (gbd->Gamut is null)
        {
            _cmsFree(ContextID, gbd);
            return null;
        }

        return gbd;
    }

    public static void cmsGBDFree(HANDLE hGBD)
    {
        var gbd = (GDB*)hGBD;
        if (hGBD is not null)
        {
            if (gbd->Gamut is null)
                _cmsFree(gbd->ContextID, gbd->Gamut);
            _cmsFree(gbd->ContextID, hGBD);
        }
    }

    private static GDBPoint* GetPoint(GDB* gbd, in CIELab* Lab, Spherical* sp)
    {
        VEC3 v;
        int alpha, theta;

        // Housekeeping
        _cmsAssert(gbd);
        _cmsAssert(Lab);
        _cmsAssert(sp);

        // Center L* by subtracting half of its domain, that's 50
        _cmsVEC3init(&v, Lab->L - 50.0, Lab->a, Lab->b);

        // Convert to spherical coordinates
        ToSpherical(sp, &v);

        if (sp->r < 0 || sp->alpha < 0 || sp->theta < 0)
        {
            cmsSignalError(gbd->ContextID, cmsERROR_RANGE, "spherical value out of range");
            return null;
        }

        // On which sector it falls?
        QuantizeToSector(sp, &alpha, &theta);

        if (alpha is < 0 or >= SECTORS || theta is <0 or >= SECTORS)
        {
            cmsSignalError(gbd->ContextID, cmsERROR_RANGE, "quadrant out of range");
            return null;
        }

        // Get pointer to the sector
        return gbd->GamutPtr(theta, alpha);
    }

    public static bool cmsGDBAddPoint(HANDLE hGBD, in CIELab* Lab)
    {
        var gbd = (GDB*)hGBD;
        Spherical sp;

        // Get pointer to the sector
        var ptr = GetPoint(gbd, Lab, &sp);
        if (ptr is null) return false;

        // If no samples at this sector, add it
        if (ptr->Type is GP_EMPTY)
        {
            ptr->Type = GP_SPECIFIED;
            ptr->p = sp;
        }
        else
        {
            // Substitute only if radius is greater
            if (sp.r > ptr->p.r)
            {
                ptr->Type = GP_SPECIFIED;
                ptr->p = sp;
            }
        }

        return true;
    }

    public static bool cmsGDBCheckPoint(HANDLE hGBD, in CIELab* Lab)
    {
        var gbd = (GDB*)hGBD;
        Spherical sp;

        // Get pointer to the sector
        var ptr = GetPoint(gbd, Lab, &sp);
        if (ptr is null) return false;

        // If no samples at this sector, return no data
        if (ptr->Type is GP_EMPTY) return false;

        // In gamut only if radius is greater
        return (sp.r <= ptr->p.r);
    }

    private static int FindNearSectors(GDB* gbd, int alpha, int theta, GDBPoint** Close)
    {
        int nSectors = 0;

        for (var i = 0u; i < NSTEPS; i++)
        {
            var a = alpha + Spiral[i].AdvX;
            var t = theta + Spiral[i].AdvY;

            // Cycle at the end
            a %= SECTORS;
            t %= SECTORS;

            // Cycle at the begin
            if (a < 0) a = SECTORS + a;
            if (t < 0) t = SECTORS + t;

            var pt = gbd->GamutPtr(t, a);

            if (pt->Type is not GP_EMPTY)
                Close[nSectors++] = pt;
        }

        return nSectors;
    }

    private static bool InterpolateMissingSector(GDB* gbd, int alpha, int theta)
    {
        Spherical sp = new();
        VEC3 Lab;
        VEC3 Center;
        Line ray;
        var Close = stackalloc GDBPoint*[NSTEPS + 1];
        Spherical closel = new();

        // Is that point already specified?
        if (gbd->GamutPtr(theta, alpha)->Type is not GP_EMPTY) return true;

        // Fill close points
        var nCloseSectors = FindNearSectors(gbd, alpha, theta, Close);

        // Find a central point on the sector
        sp.alpha = ((alpha + 0.5) * 360.0) / SECTORS;
        sp.theta = ((theta + 0.5) * 180.0) / SECTORS;
        sp.r = 50.0;

        // Convert to Cartesian
        ToCartesian(&Lab, &sp);

        // Create a ray line from center to this point
        _cmsVEC3init(&Center, 50.0, 0, 0);
        LineOf2Points(&ray, &Lab, &Center);

        // For all close sectors
        closel.r = 0.0;
        closel.alpha = 0;
        closel.theta = 0;

        for (var k = 0; k < nCloseSectors; k++)
        {
            for (var m = 0; m < nCloseSectors; m++)
            {
                Spherical templ;
                VEC3 temp, a1, a2;
                Line edge;

                var Closekp = Close[k]->p;
                var Closemp = Close[m]->p;

                // A line from sector to sector
                ToCartesian(&a1, &Closekp);
                ToCartesian(&a2, &Closemp);

                LineOf2Points(&edge, &a1, &a2);

                // Find a line
                ClosestLineToLine(&temp, &ray, &edge);

                // Convert to spherical
                ToSpherical(&templ, &temp);

                if (templ.r > closel.r &&
                    templ.theta >= (theta*180.0/SECTORS) &&
                    templ.theta <= ((theta+1)*180.0/SECTORS) &&
                    templ.alpha >= (alpha*360.0/SECTORS) &&
                    templ.alpha <= ((alpha+1)*360.0/SECTORS))
                {
                    closel = templ;
                }
            }
        }

        var result = new GDBPoint
        {
            p = closel,
            Type = GP_MODELED
        };

        *gbd->GamutPtr(theta, alpha) = result;

        return true;
    }

    public static bool cmsGDBCompute(HANDLE hGBD, uint _)
    {
        var gbd = (GDB*)hGBD;

        _cmsAssert(hGBD);

        // Interpolate black
        for (var alpha = 0; alpha < SECTORS; alpha++)
            if (!InterpolateMissingSector(gbd, alpha, 0)) return false;

        // Interpolate white
        for (var alpha = 0; alpha < SECTORS; alpha++)
            if (!InterpolateMissingSector(gbd, alpha, SECTORS - 1)) return false;

        // Interpolate Mid
        for (var theta = 1; theta < SECTORS; theta++)
        {
            for (var alpha = 0; alpha < SECTORS; alpha++)
                if (!InterpolateMissingSector(gbd, alpha, theta)) return false;
        }

        // Done
        return true;
    }

#if DEBUG
    public static bool cmsGBDdumpVRML(HANDLE hGBD, string fname)
    {
        var gbd = (GDB*)hGBD;

        using var fp = new StreamWriter(File.OpenWrite(fname));

        fp.WriteLine("#VRML V2.0 utf8");

        // set the viewing orientation and distance
        fp.WriteLine("DEF CamTest Group {");
        fp.WriteLine("\tchildren [");
        fp.WriteLine("\t\tDEF Cameras Group {");
        fp.WriteLine("\t\t\tchildren [");
        fp.WriteLine("\t\t\t\tDEF DefaultView Viewpoint {");
        fp.WriteLine("\t\t\t\t\tposition 0 0 340");
        fp.WriteLine("\t\t\t\t\torientation 0 0 1 0");
        fp.WriteLine("\t\t\t\t\tdescription \"default view\"");
        fp.WriteLine("\t\t\t\t}");
        fp.WriteLine("\t\t\t]");
        fp.WriteLine("\t\t},");
        fp.WriteLine("\t]");
        fp.WriteLine("}");

        // Output the background stuff
        fp.WriteLine("Background {");
        fp.WriteLine("\tskyColor [");
        fp.WriteLine("\t\t.5 .5 .5");
        fp.WriteLine("\t]");
        fp.WriteLine("}");

        // Output the shape stuff
        fp.WriteLine("Transform {");
        fp.WriteLine("\tscale .3 .3 .3");
        fp.WriteLine("\tchildren [");

        // Draw the axes as a shape:
        fp.WriteLine("\t\tShape {");
        fp.WriteLine("\t\t\tappearance Appearance {");
        fp.WriteLine("\t\t\t\tmaterial Material {");
        fp.WriteLine("\t\t\t\t\tdiffuseColor 0 0.8 0");
        fp.WriteLine("\t\t\t\t\temissiveColor 1.0 1.0 1.0");
        fp.WriteLine("\t\t\t\t\tshininess 0.8");
        fp.WriteLine("\t\t\t\t}");
        fp.WriteLine("\t\t\t}");
        fp.WriteLine("\t\t\tgeometry IndexedLineSet {");
        fp.WriteLine("\t\t\t\tcoord Coordinate {");
        fp.WriteLine("\t\t\t\t\tpoint [");
        fp.WriteLine("\t\t\t\t\t0.0 0.0 0.0,");
        fp.WriteLine("\t\t\t\t\t{0:f} 0.0 0.0,", 255.0);
        fp.WriteLine("\t\t\t\t\t0.0 {0:f} 0.0,", 255.0);
        fp.WriteLine("\t\t\t\t\t0.0 0.0 {0:f}]", 255.0);
        fp.WriteLine("\t\t\t\t}");
        fp.WriteLine("\t\t\t\tcoordIndex [");
        fp.WriteLine("\t\t\t\t\t0, 1, -1");
        fp.WriteLine("\t\t\t\t\t0, 2, -1");
        fp.WriteLine("\t\t\t\t\t0, 3, -1]");
        fp.WriteLine("\t\t\t}");
        fp.WriteLine("\t\t}");

        fp.WriteLine("\t\tShape {");
        fp.WriteLine("\t\t\tappearance Appearance {");
        fp.WriteLine("\t\t\t\tmaterial Material {");
        fp.WriteLine("\t\t\t\t\tdiffuseColor 0 0.8 0");
        fp.WriteLine("\t\t\t\t\temissiveColor 1 1 1");
        fp.WriteLine("\t\t\t\t\tshininess 0.8");
        fp.WriteLine("\t\t\t\t}");
        fp.WriteLine("\t\t\t}");
        fp.WriteLine("\t\t\tgeometry PointSet {");

        // fill in the points here
        fp.WriteLine("\t\t\t\tcoord Coordinate {");
        fp.WriteLine("\t\t\t\t\tpoint [");

        // We need to transverse all gamut hull.
        for (var i = 0; i < SECTORS; i++)
        {
            for (var j = 0; j < SECTORS; j++)
            {
                VEC3 v;

                var pt = gbd->GamutPtr(i, j);
                var ptp = pt->p;
                ToCartesian(&v, &ptp);

                fp.Write("\t\t\t\t\t{0:g} {1:g} {2:g}", v.X + 50, v.Y, v.Z);

                if ((j == SECTORS - 1) && (i == SECTORS - 1))
                    fp.WriteLine("]");
                else
                    fp.WriteLine(",");
            }
        }

        fp.WriteLine("\t\t\t\t}");

        // fill in the face colors
        fp.WriteLine("\t\t\t\tcolor Color {");
        fp.WriteLine("\t\t\t\t\tcolor [");

        for (var i = 0; i < SECTORS; i++)
        {
            for (var j = 0; j < SECTORS; j++)
            {
                VEC3 v;

                var pt = gbd->GamutPtr(i, j);
                var ptp = pt->p;
                ToCartesian(&v, &ptp);

                switch (pt->Type)
                {
                    case GP_EMPTY: fp.Write("\t\t\t\t\t{0:g} {1:g} {2:g}", 0.0, 0.0, 0.0); break;
                    case GP_MODELED: fp.Write("\t\t\t\t\t{0:g} {1:g} {2:g}", 1.0, .5, .5); break;
                    default: fp.Write("\t\t\t\t\t{0:g} {1:g} {2:g}", 1.0, 1.0, 1.0); break;
                }

                if ((j == SECTORS - 1) && (i == SECTORS - 1))
                    fp.WriteLine("]");
                else
                    fp.WriteLine(",");
            }
        }

        fp.WriteLine("\t\t\t}");


        fp.WriteLine("\t\t\t}");
        fp.WriteLine("\t\t}");
        fp.WriteLine("\t]");
        fp.WriteLine("}");

        fp.Close();
        return true;
    }
#endif
}
