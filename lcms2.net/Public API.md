# Public API

#### Constants/Defines
|            Original API | New C# API              |
| ----------------------: | :---------------------- |
|          `LCMS_VERSION` | `Lcms2.Version`         |
|           `cmsMAX_PATH` | `Lcms2.MaxPath`         |
|               `cmsD50X` | `Lcms2.D50`             |
|               `cmsD50Y` | `Lcms2.D50`             |
|               `cmsD50Z` | `Lcms2.D50`             |
| `cmsPERCEPTUAL_BLACK_X` | `Lcms2.PerceptualBlack` |
| `cmsPERCEPTUAL_BLACK_Y` | `Lcms2.PerceptualBlack` |
| `cmsPERCEPTUAL_BLACK_Z` | `Lcms2.PerceptualBlack` |
|        `cmsMAXCHANNELS` | `Lcms2.MaxChannels`     |
|                         |                         |

## Structs/Typedefs

|                  Original API | New C# API                 |
| ----------------------------: | :------------------------- |
|                `cmsSignature` | `types.Signature`          |
|           `cmsU8Fixed8Number` | `types.FixedPoint` methods |
|         `cmsS15Fixed16Number` | `types.FixedPoint` methods |
|         `cmsU16Fixed16Number` | `types.FixedPoint` methods |
|       `cmsICCData` and fields | `types.ICCData`            |
|        `cmsCIEXYZ` and fields | `types.XYZ`                |
|     `cmsProfileID` and fields | `types.ProfileID`          |
|   `cmsContext` without fields | `state.Context`            |
| `cmsIOHANDLER` without fields | `io.IOHandler`             |
|                               |                            |

## Functions

|            Original API | New C# API                                |
| ----------------------: | :---------------------------------------- |
|      `cmsCreateContext` | `state.Context.Create` static method      |
|      `cmsDeleteContext` | `state.Context.Delete` local method       |
|         `cmsDupContext` | `state.Context.Duplicate` local method    |
| `cmsGetContextUserData` | `state.Context.GetUserData` static method |
|                         |                                           |
