//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022      Stefan Kewatt
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

using System.Resources;

using static lcms2.res.Strings;

namespace lcms2;

internal static class Errors
{
    #region Fields

    internal static readonly ResourceManager resources;

    #endregion Fields

    #region Public Constructors

    static Errors()
    {
        resources = new("res/Strings", typeof(Errors).Assembly);
    }

    #endregion Public Constructors

    #region Internal Methods

    internal static bool BadDictionaryNameValue(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.CorruptionDetected,
                resources.GetString(
                    nameof(bad_dictionary_name_value))
                    ?? bad_dictionary_name_value));

    internal static bool CannotSaveFloatingPointClut(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.NotSuitable,
                resources.GetString(
                    nameof(bad_dictionary_name_value))
                    ?? bad_dictionary_name_value));

    internal static bool ContextChunkOutOfRange(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.Internal,
                resources.GetString(
                    nameof(context_chunk_out_of_range))
                    ?? context_chunk_out_of_range));

    internal static bool InvalidAlarmCode(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.Range,
                resources.GetString(
                    nameof(invalid_alarm_code))
                    ?? invalid_alarm_code));

    internal static bool InvalidLcmsVersion(object? state, uint expectedVersion, uint actualVersion) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(invalid_lcms_version))
                    ?? invalid_lcms_version,
                expectedVersion,
                actualVersion));

    internal static bool InvalidParametricCurveType(object? state, int type) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(invalid_parametric_curve_type))
                    ?? invalid_parametric_curve_type,
                type));

    internal static bool Lut8InvalidEntryCount(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.Range,
                resources.GetString(
                    nameof(lut8_invalid_entry_count))
                    ?? lut8_invalid_entry_count));

    internal static bool NotSuitableLut16Save(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(not_suitable_lut16_save))
                    ?? not_suitable_lut16_save));

    internal static bool NotSuitableLut8Save(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(not_suitable_lut8_save))
                    ?? not_suitable_lut8_save));

    internal static bool NotSuitableLutAToBSave(object? state) =>
                ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.NotSuitable,
                resources.GetString(
                    nameof(not_suitable_lutatob_save))
                    ?? not_suitable_lutatob_save));

    internal static bool NotSuitableLutBToASave(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.NotSuitable,
                resources.GetString(
                    nameof(not_suitable_lutbtoa_save))
                    ?? not_suitable_lutbtoa_save));

    internal static bool NotSupportedMluLength(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(not_supported_mlu_length))
                    ?? not_supported_mlu_length));

    internal static bool ParametricCurveCannotWrite(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(parametric_curve_cannot_write))
                    ?? parametric_curve_cannot_write));

    internal static bool ToneCurveSmoothFailed(object? state) =>
            ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.Range,
                resources.GetString(
                    nameof(tonecurve_smooth_failed))
                    ?? tonecurve_smooth_failed));

    internal static bool ToneCurveSmoothMostlyPoles(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.Range,
                resources.GetString(
                    nameof(tonecurve_smooth_mostly_poles))
                    ?? tonecurve_smooth_mostly_poles));

    internal static bool ToneCurveSmoothMostlyZeros(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.Range,
                resources.GetString(
                    nameof(tonecurve_smooth_mostly_zeros))
                    ?? tonecurve_smooth_mostly_zeros));

    internal static bool ToneCurveSmoothNonMonotonic(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.Range,
                resources.GetString(
                    nameof(tonecurve_smooth_non_monotonic))
                    ?? tonecurve_smooth_non_monotonic));

    internal static bool ToneCurveSmoothTooManyPoints(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.Range,
                resources.GetString(
                    nameof(tonecurve_smooth_too_many_points))
                    ?? tonecurve_smooth_too_many_points));

    internal static bool ToneCurveTooFewEntries(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.Range,
                resources.GetString(
                    nameof(tonecurve_too_few_entries))
                    ?? tonecurve_too_few_entries));

    internal static bool ToneCurveTooManyEntries(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.Range,
                resources.GetString(
                    nameof(tonecurve_too_many_entries))
                    ?? tonecurve_too_many_entries));

    internal static bool TooManyDeviceCoordinates(object? state, uint numCoords) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.Range,
                resources.GetString(
                    nameof(too_many_device_coordinates))
                    ?? too_many_device_coordinates,
                numCoords));

    internal static bool TooManyColorants(object? state, uint numCoords) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.Range,
                resources.GetString(
                    nameof(too_many_colorants))
                    ?? too_many_colorants,
                numCoords));

    internal static bool TooManyInputChannels(object? state, uint inputChannels, int max) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.Range,
                resources.GetString(
                    nameof(too_many_input_channels))
                    ?? too_many_input_channels,
                inputChannels,
                max));

    internal static bool TooManyNamedColors(object? state, uint count) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.Range,
                resources.GetString(
                    nameof(too_many_named_colors))
                    ?? too_many_named_colors,
                count));

    internal static bool UnknownCurveType(object? state, Signature type) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(unknown_curve_type))
                    ?? unknown_curve_type,
                type));

    internal static bool UnknownDictionaryRecordLength(object? state, uint length) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(unknown_dictionary_record_length))
                    ?? unknown_dictionary_record_length,
                length));

    internal static bool UnknownMpeType(object? state, Signature type) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(unknown_mpe_type))
                    ?? unknown_mpe_type,
                type));

    internal static bool UnknownParametricCurveType(object? state, int type) =>
                ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(unknown_parametric_curve_type))
                    ?? unknown_parametric_curve_type,
                type));

    internal static bool UnknownPrecision(object? state, byte precision) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(unknown_precision))
                    ?? unknown_precision,
                precision));

    internal static bool UnrecognizedPlugin(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(unrecognized_plugin))
                    ?? unrecognized_plugin));

    internal static bool UnrecognizedPluginType(object? state, Signature type) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(unrecognized_plugin_type))
                    ?? unrecognized_plugin_type,
                type));

    internal static bool UnsupportedInterpolation(object? state, uint inputChan, uint outputChan) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(unsupported_interpolation))
                    ?? unsupported_interpolation,
                inputChan,
                outputChan));

    internal static bool UnsupportedParametricCurve(object? state) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(unsupported_parametric_curve))
                    ?? unsupported_parametric_curve));

    internal static bool UnsupportedVcgtBitDepth(object? state, int bitDepth) =>
                        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(unsupported_vcgt_bit_depth))
                    ?? unsupported_vcgt_bit_depth,
                bitDepth));

    internal static bool UnsupportedVcgtChannelCount(object? state, int numChannels) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(unsupported_vcgt_channel_count))
                    ?? unsupported_vcgt_channel_count,
                numChannels));

    internal static bool UnsupportedVcgtType(object? state, int type) =>
        ReturnFalse(() =>
            State.SignalError(
                state,
                ErrorCode.UnknownExtension,
                resources.GetString(
                    nameof(unsupported_vcgt_type))
                    ?? unsupported_vcgt_type,
                type));

    #endregion Internal Methods

    #region Private Methods

    private static bool ReturnFalse(Action fn)
    {
        fn();
        return false;
    }

    #endregion Private Methods
}
