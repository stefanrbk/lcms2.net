﻿using System;

using Microsoft.CodeAnalysis;

namespace CodeGen
{
    [Generator]
    public class InterpGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            const string code = @"// <auto-generated>
using static lcms2.Helpers;

namespace lcms2.types;

public partial class InterpParams
{{
    private static void Eval{0}Inputs(in ushort[] input, ref ushort[] output, InterpParams p)
    {{
        var lutTable = p.Table16;

        var tmp1 = new ushort[maxStageChannels];
        var tmp2 = new ushort[maxStageChannels];

        var fk = ToFixedDomain(input[0] * p.Domain[0]);
        var k0 = FixedToInt(fk);
        var rk = FixedRestToInt(fk);

        k0 *= p.Opta[{1}];
        var k1 = p.Opta[{1}] * (k0 + (input[0] != 0xFFFF ? 1 : 0));

        var t = lutTable[k0..];
        var p1 = new InterpParams(p.Context, p.Flags, p.NumInputs, p.NumOutputs, t);
        p.Domain[1..{1}].CopyTo(p1.Domain.AsSpan());

        var inp = input[1..];
        Eval{1}Inputs(inp, ref tmp1, p);

        t = lutTable[k1..];
        p1.Table = t;
        Eval{1}Inputs(inp, ref tmp2, p);

        for (var i = 0; i < p.NumOutputs; i++)
            output[i] = LinearInterp(rk, tmp1[i], tmp2[i]);
    }}

    private static void Eval{0}Inputs(in float[] input, ref float[] output, InterpParams p)
    {{
        var lutTable = p.TableFloat;
        var tmp1 = new float[maxStageChannels];
        var tmp2 = new float[maxStageChannels];

        var pk = fclamp(input[0]) * p.Domain[0];
        var k0 = QuickFloor(pk);
        var rest = pk - k0;

        k0 *= p.Opta[{1}];
        var k1 = k0 + (fclamp(input[0]) >= 1.0 ? 0 : p.Opta[{1}]);

        var t = lutTable[k0..];
        var p1 = new InterpParams(p.Context, p.Flags, p.NumInputs, p.NumOutputs, t);

        p.Domain[1..{1}].CopyTo(p1.Domain.AsSpan());

        var inp = input[1..];
        Eval{1}Inputs(inp, ref tmp1, p);

        t = lutTable[k1..];
        p1.Table = t;

        Eval{1}Inputs(inp, ref tmp2, p);

        for (var i = 0; i < p.NumOutputs; i++) {{

            var y0 = tmp1[i];
            var y1 = tmp2[i];

            output[i] = lerp(rest, y0, y1);
        }}
    }}
}}";
            for (var i = 5; i <= 15; i++)
                context.AddSource($"interp{i}.g.cs", String.Format(code, i, i - 1));
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}
