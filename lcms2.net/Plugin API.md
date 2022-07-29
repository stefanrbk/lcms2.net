# Plugin API

## Structs/Typedefs

### cmsInterpFnFactory ⇆ InterpFnFactory
```C
typedef cmsInterpFunction *cmsInterpFnFactory(cmsUInt32Number nInputChannels, cmsUInt32Number nOutputChannels, cmsUInt32Number dwFlags);
```
```csharp
namespace lcms2.plugins;
public delegate InterpFunction InterpFnFactory(int numInputChannels, int numOutputChannels, LerpFlag flags);
```
### cmsInterpFunction ⇆ InterpFunction

```c
typedef union {
    _cmsInterpFn16       Lerp16;
    _cmsInterpFnFloat    LerpFloat;
} cmsInterpFunction;
```
```csharp
[StructLayout(LayoutKind.Explicit)]
public struct InterpFunction
{
    [FieldOffset(0)]
    public InterpFn16 Lerp16;
    [FieldOffset(0)]
    public InterpFnFloat LerpFloat;
}

```

|                  Original API | New C# API                         |
| ----------------------------: | :--------------------------------- |
|           `_cms_interp_struc` | `plugins.InterpParams`             |
|             `_cms_io_handler` | Refactored into `Stream`           |
|              `_cmsInterpFn16` | `plugins.InterpFn16`               |
|           `_cmsInterpFnFloat` | `plugins.InterpFnFloat`            |
|        `_cmsPluginBaseStruct` | `plugins.PluginBase`               |
|        `_cmstransform_struct` | `plugins.Transform`                |
|                               |                                    |
|             `cmsInterpParams` | `plugins.InterpParams`             |
| `cmsParametricCurveEvaluator` | `plugins.ParametricCurveEvaluator` |
|               `cmsPluginBase` | `plugins.PluginBase`               |
|      `cmsPluginInterpolation` | `plugins.PluginInterpolation`      |
|   `cmsPluginParametricCurves` | `plugins.PluginParametricCurves`   |
|                               |                                    |

## Functions

|               Original API | New C# API                                       |
| -------------------------: | :----------------------------------------------- |
|   `_cmsAdjustEndianness16` | `io.IOHandler.AdjustEndianness` static method    |
|   `_cmsAdjustEndianness32` | `io.IOHandler.AdjustEndianness` static method    |
|   `_cmsAdjustEndianness64` | `io.IOHandler.AdjustEndianness` static method    |
|      `_cmsReadUInt8Number` | `io.IOHandler.ReadUInt8Number` static method     |
|     `_cmsReadUInt16Number` | `io.IOHandler.ReadUInt16Number` static method    |
|     `_cmsReadUInt32Number` | `io.IOHandler.ReadUInt32Number` static method    |
|    `_cmsReadFloat32Number` | `io.IOHandler.ReadFloat32Number` static method   |
|     `_cmsReadUInt64Number` | `io.IOHandler.ReadUInt64Number` static method    |
|  `_cmsRead15Fixed16Number` | `io.IOHandler.Read15Fixed16Number` static method |
|        `_cmsReadXYZNumber` | `io.IOHandler.ReadXYZNumber` static method       |
|     `_cmsWriteUInt16Array` | `io.IOHandler.Write` static method               |
|     `_cmsWriteUInt8Number` | `io.IOHandler.Write` static method               |
|    `_cmsWriteUInt16Number` | `io.IOHandler.Write` static method               |
|    `_cmsWriteUInt32Number` | `io.IOHandler.Write` static method               |
|   `_cmsWriteFloat32Number` | `io.IOHandler.Write` static method               |
|    `_cmsWriteUInt64Number` | `io.IOHandler.Write` static method               |
| `_cmsWrite15Fixed16Number` | `io.IOHandler.Write` static method               |
|       `_cmsWriteXYZNumber` | `io.IOHandler.Write` static method               |
|     `_cmsWriteUInt16Array` | `io.IOHandler.Write` static method               |
|    `_cms15Fixed16toDouble` | `io.IOHandler.S15Fixed16toDouble` static method  |
|    `_cmsDoubleTo15Fixed16` | `io.IOHandler.DoubleToS15Fixed16` static method  |
|      `_cms8Fixed8toDouble` | `io.IOHandler.U8Fixed8toDouble` static method    |
|      `_cmsDoubleTo8Fixed8` | `io.IOHandler.DoubleToU8Fixed8` static method    |
| `_cmsEncodeDateTimeNumber` | `types.DateTimeNumber` explicit cast operator    |
| `_cmsDecodeDateTimeNumber` | `types.DateTimeNumber` implicit cast operator    |
|         `_cmsReadTypeBase` | `io.IOHandler.ReadTypeBase` static method        |
|        `_cmsWriteTypeBase` | `io.IOHandler.Write` static method               |
|        `_cmsReadAlignment` | `io.IOHandler.ReadAlignment` static method       |
|       `_cmsWriteAlignment` | `io.IOHandler.WriteAlignment` static method      |
|                            |                                                  |
