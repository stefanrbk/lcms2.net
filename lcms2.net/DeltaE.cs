using lcms2.types;

using static System.Math;

namespace lcms2;
public static class DeltaE
{
    public static double De76(CIELab Lab1, CIELab Lab2)
    {
        var dL = Abs(Lab1.L - Lab2.L);
        var da = Abs(Lab1.a - Lab2.a);
        var db = Abs(Lab1.b - Lab2.b);

        return Pow(Sqr(dL) + Sqr(da) + Sqr(db), 0.5);
    }

    public static double CIE94(CIELab Lab1, CIELab Lab2)
    {
        var dL = Abs(Lab1.L - Lab2.L);

        var LCh1 = Lab1.AsLCh;
        var LCh2 = Lab2.AsLCh;

        var dC = Abs(LCh1.C - LCh2.C);
        var dE = De76(Lab1, Lab2);

        var dhsq = Sqr(dE) - Sqr(dL) - Sqr(dC);
        var dh = dhsq < 0 ? 0 : Pow(dhsq, 0.5);

        var c12 = Sqrt(LCh1.C * LCh2.C);

        var sc = 1.0 + (0.048 * c12);
        var sh = 1.0 + (0.014 * c12);

        return Sqrt(Sqr(dL) + (Sqr(dC) / Sqr(sc)) + (Sqr(dh) / Sqr(sh)));
    }

    private static double ComputeLBFD(CIELab Lab) =>
        (54.6 * (M_LOG10E * Log((Lab.L > 7.996969 ? Sqr((Lab.L + 16) / 116) * ((Lab.L + 16) / 116) * 100 : 100 * (Lab.L / 903.3)) + 1.5))) - 9.6;

    public static double BFD(CIELab Lab1, CIELab Lab2)
    {
        var lbfd1 = ComputeLBFD(Lab1);
        var lbfd2 = ComputeLBFD(Lab2);
        var deltaL = lbfd2 - lbfd1;

        var LCh1 = Lab1.AsLCh;
        var LCh2 = Lab2.AsLCh;

        var deltaC = LCh2.C - LCh1.C;
        var AveC = (LCh1.C + LCh2.C) / 2;
        var Aveh = (LCh1.h + LCh2.h) / 2;

        var dE = De76(Lab1, Lab2);

        var deltah =
            Sqr(dE) > (Sqr(Lab2.L - Lab1.L) + Sqr(deltaC))
            ? Sqrt(Sqr(dE) - Sqr(Lab2.L - Lab1.L) - Sqr(deltaC))
            : 0;

        var dc = (0.035 * AveC / (1 + (0.00365 * AveC))) + 0.521;
        var g = Sqrt(Sqr(Sqr(AveC)) / (Sqr(Sqr(AveC)) + 14000));
        var t = 0.627 + ((0.055 * Cos((Aveh - 254) / (180 / M_PI))) -
               (0.040 * Cos(((2 * Aveh) - 136) / (180 / M_PI))) +
               (0.070 * Cos(((3 * Aveh) - 31) / (180 / M_PI))) +
               (0.049 * Cos(((4 * Aveh) + 114) / (180 / M_PI))) -
               (0.015 * Cos(((5 * Aveh) - 103) / (180 / M_PI))));

        var dh = dc * ((g * t) + 1 - g);
        var rh = (-0.260 * Cos((Aveh - 308) / (180 / M_PI))) -
               (0.379 * Cos(((2 * Aveh) - 160) / (180 / M_PI))) -
               (0.636 * Cos(((3 * Aveh) + 254) / (180 / M_PI))) +
               (0.226 * Cos(((4 * Aveh) + 140) / (180 / M_PI))) -
               (0.194 * Cos(((5 * Aveh) + 280) / (180 / M_PI)));

        var rc = Sqrt(AveC * AveC * AveC * AveC * AveC * AveC / ((AveC * AveC * AveC * AveC * AveC * AveC) + 70000000));
        var rt = rh * rc;

        return Sqrt(Sqr(deltaL) + Sqr(deltaC / dc) + Sqr(deltah / dh) + (rt * (deltaC / dc) * (deltah / dh)));
    }

    public static double CMC(CIELab Lab1, CIELab Lab2, double l, double c)
    {
        if (Lab1.L == 0 && Lab2.L == 0) return 0;

        var LCh1 = Lab1.AsLCh;
        var LCh2 = Lab2.AsLCh;

        var dL = Lab2.L - Lab1.L;
        var dC = LCh2.C - LCh1.C;

        var dE = De76(Lab1, Lab2);

        var dh =
            Sqr(dE) > (Sqr(dL) + Sqr(dC))
            ? Sqrt(Sqr(dE) - Sqr(dL) - Sqr(dC))
            : 0;

        var t = LCh1.h is > 164 and < 345
            ? 0.56 + Abs(0.2 * Cos((LCh1.h + 168) / (180 / M_PI)))
            : 0.36 + Abs(0.4 * Cos((LCh1.h + 35) / (180 / M_PI)));

        var sc = (0.0638 * LCh1.C / (1 + (0.0131 * LCh1.C))) + 0.638;
        var sl = 0.040975 * Lab1.L / (1 + (0.01765 * Lab1.L));

        if (Lab1.L < 16)
            sl = 0.511;

        var f = Sqrt(LCh1.C * LCh1.C * LCh1.C * LCh1.C / ((LCh1.C * LCh1.C * LCh1.C * LCh1.C) + 1900));
        var sh = sc * ((t * f) + 1 - f);
        return Sqrt(Sqr(dL / (l * sl)) + Sqr(dC / (c * sc)) + Sqr(dh / sh));
    }

    public static double CIE2000(CIELab Lab1, CIELab Lab2, double Kl, double Kc, double Kh)
    {
        var L1 = Lab1.L;
        var a1 = Lab1.a;
        var b1 = Lab1.b;
        var C = Sqrt(Sqr(a1) + Sqr(b1));

        var Ls = Lab2.L;
        var @as = Lab2.a;
        var bs = Lab2.b;
        var Cs = Sqrt(Sqr(@as) + Sqr(bs));

        var G = 0.5 * (1 - Sqrt(Pow((C + Cs) / 2, 7.0) / (Pow((C + Cs) / 2, 7.0) + Pow(25.0, 7.0))));

        var a_p = (1 + G) * a1;
        var b_p = b1;
        var C_p = Sqrt(Sqr(a_p) + Sqr(b_p));
        var h_p = atan2deg(b_p, a_p);

        var a_ps = (1 + G) * @as;
        var b_ps = bs;
        var C_ps = Sqrt(Sqr(a_ps) + Sqr(b_ps));
        var h_ps = atan2deg(b_ps, a_ps);

        var meanC_p = (C_p + C_ps) / 2;

        var hps_plus_hp = h_ps + h_p;
        var hps_minus_hp = h_ps - h_p;

        var meanh_p = Abs(hps_minus_hp) <= 180.000001 ? hps_plus_hp / 2 :
                                hps_plus_hp < 360 ? (hps_plus_hp + 360) / 2 :
                                                     (hps_plus_hp - 360) / 2;

        var delta_h = hps_minus_hp <= -180.000001 ? (hps_minus_hp + 360) :
                                hps_minus_hp > 180 ? (hps_minus_hp - 360) :
                                                        hps_minus_hp;
        var delta_L = Ls - L1;
        var delta_C = C_ps - C_p;

        var delta_H = 2 * Sqrt(C_ps * C_p) * Sin(RADIANS(delta_h) / 2);

        var T = 1 - (0.17 * Cos(RADIANS(meanh_p - 30)))
                     + (0.24 * Cos(RADIANS(2 * meanh_p)))
                     + (0.32 * Cos(RADIANS((3 * meanh_p) + 6)))
                     - (0.2 * Cos(RADIANS((4 * meanh_p) - 63)));

        var Sl = 1 + (0.015 * Sqr(((Ls + L1) / 2) - 50) / Sqrt(20 + Sqr(((Ls + L1) / 2) - 50)));

        var Sc = 1 + (0.045 * (C_p + C_ps) / 2);
        var Sh = 1 + (0.015 * ((C_ps + C_p) / 2) * T);

        var delta_ro = 30 * Exp(-Sqr((meanh_p - 275) / 25));

        var Rc = 2 * Sqrt(Pow(meanC_p, 7.0) / (Pow(meanC_p, 7.0) + Pow(25.0, 7.0)));

        var Rt = -Sin(2 * RADIANS(delta_ro)) * Rc;

        return Sqrt(Sqr(delta_L / (Sl * Kl)) +
                            Sqr(delta_C / (Sc * Kc)) +
                            Sqr(delta_H / (Sh * Kh)) +
                            (Rt * (delta_C / (Sc * Kc)) * (delta_H / (Sh * Kh))));
    }
}
