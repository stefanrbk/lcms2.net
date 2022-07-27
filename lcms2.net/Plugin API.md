# Plugin API

## Functions

|      Original API | New C# API               |
| ----------------: | :----------------------- |
| `_cms_io_handler` | Refactored into `Stream` |
|                   |                          |

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
|                            |                                                  |
