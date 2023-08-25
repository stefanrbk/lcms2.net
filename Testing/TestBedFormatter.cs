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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace lcms2.testbed;

public sealed class TestBedFormatter : ConsoleFormatter, IDisposable
{
    private readonly struct ConsoleColors(ConsoleColor? foreground, ConsoleColor? background)
    {
        public ConsoleColor? Foreground { get; } = foreground;
        public ConsoleColor? Background { get; } = background;

    }

    private const string LoglevelPadding = "~ ";
    private static readonly string _messagePadding = new(' ', GetLogLevelString(LogLevel.Information).Length + LoglevelPadding.Length);
    private static readonly string _newLineWithMessagePadding = Environment.NewLine + _messagePadding;
    private IDisposable? _optionsReloadToken;
    internal SimpleConsoleFormatterOptions FormatterOptions { get; set; }

    public TestBedFormatter(IOptionsMonitor<SimpleConsoleFormatterOptions> options)
        :base("TestBed")
    {
        ReloadLoggerOptions(options.CurrentValue);
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
    }

    [MemberNotNull(nameof(FormatterOptions))]
    private void ReloadLoggerOptions(SimpleConsoleFormatterOptions options) =>
        FormatterOptions = options;

    public void Dispose()
    {
        _optionsReloadToken?.Dispose();
        GC.SuppressFinalize(this);
    }

    [DebuggerStepThrough]
    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var formattedText = logEntry.Formatter(logEntry.State, logEntry.Exception);

        if (logEntry.Exception is not null || formattedText is not null)
        {
            var logLevel = logEntry.LogLevel;
            var logLevelConsoleColors = GetLogLevelConsoleColors(logLevel);
            var logLevelString = GetLogLevelString(logLevel);
            string? timestampString = null;
            var timestampFormat = FormatterOptions.TimestampFormat;

            if (timestampFormat is not null)
                timestampString = GetCurrentDateTime().ToString(timestampFormat);

            if (timestampString is not null)
                textWriter.Write(timestampString);

            if (logLevelString is not null)
                textWriter.WriteColoredMessage(logLevelString, logLevelConsoleColors.Background, logLevelConsoleColors.Foreground);

            CreateDefaultLogMessage(textWriter, in logEntry, formattedText, scopeProvider);
        }
    }

    private void CreateDefaultLogMessage<TState>(TextWriter textWriter, in LogEntry<TState> logEntry, string message, IExternalScopeProvider scopeProvider)
    {
        var singleLine = FormatterOptions.SingleLine;
        var id = logEntry.EventId.Id;
        var exception = logEntry.Exception;

        textWriter.Write(": ");
        textWriter.Write(logEntry.Category);
        textWriter.Write('[');

        Span<char> destination = stackalloc char[10];
        textWriter.Write(id.TryFormat(destination, out var charsWritten)
            ? destination[..charsWritten]
            : id.ToString());

        textWriter.Write("]");
        if (!singleLine)
            textWriter.WriteLine();

        WriteScopeInformation(textWriter, scopeProvider, singleLine);
        WriteMessage(textWriter, message, singleLine);

        if (exception is not null)
            WriteMessage(textWriter, exception.ToString(), singleLine);

        if (singleLine)
            textWriter.WriteLine();
    }

    private static void WriteMessage(TextWriter textWriter, string message, bool singleLine)
    {
        if (!string.IsNullOrEmpty(message))
        {
            if (singleLine)
            {
                textWriter.Write(' ');
                WriteReplacing(textWriter, Environment.NewLine, " ", message);
            }
            else
            {
                textWriter.Write(_messagePadding);
                WriteReplacing(textWriter, Environment.NewLine, _newLineWithMessagePadding, message);
                textWriter.WriteLine();
            }
        }
        static void WriteReplacing(TextWriter writer, string oldValue, string newValue, string message)
        {
            var value = message.Replace(oldValue, newValue);
            writer.Write(value);
        }
    }

    private DateTimeOffset GetCurrentDateTime() =>
        !FormatterOptions.UseUtcTimestamp
            ? DateTimeOffset.Now
            : DateTimeOffset.UtcNow;

    private static string GetLogLevelString(LogLevel logLevel) =>
        logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
        };

    [DebuggerStepThrough]
    private ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel)
    {
        if (FormatterOptions.ColorBehavior is LoggerColorBehavior.Disabled ||
            (FormatterOptions.ColorBehavior is LoggerColorBehavior.Default && !Testbed.EmitAnsiColorCodes))
        {
            return new(null, null);
        }
        return logLevel switch
        {
            LogLevel.Trace => new(ConsoleColor.Gray, ConsoleColor.Black),
            LogLevel.Debug => new(ConsoleColor.Gray, ConsoleColor.Black),
            LogLevel.Information => new(ConsoleColor.DarkGreen, ConsoleColor.Black),
            LogLevel.Warning => new(ConsoleColor.Yellow, ConsoleColor.Black),
            LogLevel.Error => new(ConsoleColor.DarkRed, ConsoleColor.Black),
            LogLevel.Critical => new(ConsoleColor.Black, ConsoleColor.DarkRed),
        };
    }

    private void WriteScopeInformation(TextWriter textWriter, IExternalScopeProvider? scopeProvider, bool singleLine)
    {
        if (!FormatterOptions.IncludeScopes || scopeProvider == null)
            return;
        var paddingNeeded = !singleLine;
        scopeProvider.ForEachScope((scope, state) =>
        {
            if (paddingNeeded)
            {
                paddingNeeded = false;
                state.Write(_messagePadding);
                state.Write("=> ");
            }
            else
            {
                state.Write(" => ");
            }
            state.Write(scope);
        }, textWriter);
        textWriter.Write(" =>");
        if (!paddingNeeded && !singleLine)
            textWriter.WriteLine();
    }
}