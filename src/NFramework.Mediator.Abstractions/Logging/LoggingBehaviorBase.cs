using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace NFramework.Mediator.Abstractions.Logging;

/// <summary>
/// Handles structured logging with support for parameter exclusion and masking.
/// Masking logic is optimized for common patterns like Emails and Credit Cards/Numbers.
/// </summary>
public abstract class LoggingBehaviorBase<TRequest, TResponse>(ILogger logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    protected ILogger Logger { get; } = logger;

    private static readonly Action<ILogger, string, string, Exception?> LogRequestFailureAction = LoggerMessage.Define<
        string,
        string
    >(LogLevel.Error, new EventId(1, nameof(HandleAsync)), "Request failed for {RequestName}: {ExceptionMessage}");

    private static readonly Action<ILogger, string, string, Exception?> LogRequestStartAction = LoggerMessage.Define<
        string,
        string
    >(LogLevel.Information, new EventId(2, nameof(LogRequestStart)), "Handling {RequestName} {RequestDetails}");

    private static readonly Action<ILogger, string, string, Exception?> LogRequestEndAction = LoggerMessage.Define<
        string,
        string
    >(LogLevel.Information, new EventId(3, nameof(LogRequestEnd)), "Handled {RequestName} {ResponseDetails}");

    protected async ValueTask<TResponse> HandleAsync(
        TRequest request,
        Func<CancellationToken, ValueTask<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(next);

        if (request is not ILoggableRequest loggable)
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        string requestName = typeof(TRequest).Name;
        var requestParameters = SafeExtractRequestParameters(request, loggable.LogOptions);

        LogRequestStart(requestName, requestParameters, loggable.LogOptions.User);

        try
        {
            var response = await next(cancellationToken).ConfigureAwait(false);

            if (loggable.LogOptions.LogResponse)
            {
                LogRequestEnd(requestName, response);
            }

            return response;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogRequestFailureAction(Logger, requestName, ex.Message, ex);
            throw;
        }
    }

    /// <summary>
    /// Identifies and filters request properties based on <see cref="LogOptions"/>.
    /// Handles reflection or serialization errors gracefully with fallback.
    /// </summary>
    protected static Dictionary<string, object> SafeExtractRequestParameters(TRequest request, in LogOptions logOptions)
    {
        try
        {
            return ExtractRequestParameters(request, logOptions);
        }
        catch (Exception ex)
            when (ex
                    is JsonException
                        or ReflectionTypeLoadException
                        or AmbiguousMatchException
                        or TargetInvocationException
            )
        {
            return new Dictionary<string, object> { { "_fallbackError", "Failed to extract parameters" } };
        }
    }

    private static Dictionary<string, object> ExtractRequestParameters(TRequest request, in LogOptions logOptions)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();

        foreach (var prop in typeof(TRequest).GetProperties())
        {
            object? value = prop.GetValue(request);
            if (value is null)
                continue;

            LogExcludeParameter excludeParam = default;
            if (logOptions.ExcludeParameters != null && logOptions.ExcludeParameters.Count > 0)
            {
                excludeParam = logOptions.ExcludeParameters.FirstOrDefault(p => p.Name == prop.Name);
            }

            if (excludeParam.Name != prop.Name)
            {
                parameters[prop.Name] = value;
                continue;
            }

            if (!excludeParam.Mask)
                continue;

            if (value is string strValue)
            {
                parameters[prop.Name] = MaskValue(strValue, excludeParam);
            }
        }

        return parameters;
    }

    /// <summary>
    /// Dispatches the value to specialized masking logic (Email, Numeric, or Default).
    /// </summary>
    protected static string MaskValue(string value, in LogExcludeParameter param)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains('@', StringComparison.Ordinal))
            return MaskEmail(value, param);
        else if (value.All(char.IsDigit))
            return MaskNumeric(value, param);

        return param.KeepEndChars == 0 ? MaskWithFixedLength(value, param) : MaskDefault(value, param);
    }

    private static string MaskEmail(string value, in LogExcludeParameter param)
    {
        if (value.Length < 9)
            return value;

        int startChars = param.KeepStartChars;
        int endChars = param.KeepEndChars;
        int maskLength = value.Length - startChars - endChars;

        if (maskLength <= 0)
            return value;

        char[] result = value.ToCharArray();
        for (int i = startChars; i < startChars + maskLength; i++)
        {
            result[i] = param.MaskChar;
        }

        return new string(result);
    }

    private static string MaskNumeric(string value, in LogExcludeParameter param)
    {
        if (value.Length <= 4)
            return value;

        int keepStart = param.KeepStartChars == 0 ? 2 : param.KeepStartChars;
        int keepEnd = param.KeepEndChars == 0 ? 2 : param.KeepEndChars;

        char[] result = value.ToCharArray();
        for (int i = keepStart; i < value.Length - keepEnd; i++)
        {
            result[i] = param.MaskChar;
        }

        return new string(result);
    }

    private static string MaskWithFixedLength(string value, in LogExcludeParameter param)
    {
        if (value.Length <= param.KeepStartChars)
            return value;

        const int fixedMaskLength = 3;
        char[] result = new char[param.KeepStartChars + fixedMaskLength];

        for (int i = 0; i < param.KeepStartChars; i++)
        {
            result[i] = value[i];
        }

        for (int i = param.KeepStartChars; i < result.Length; i++)
        {
            result[i] = param.MaskChar;
        }

        return new string(result);
    }

    private static string MaskDefault(string value, in LogExcludeParameter param)
    {
        if (value.Length == param.KeepStartChars + param.KeepEndChars + 1)
        {
            char[] simpleMaskedResult = value.ToCharArray();
            simpleMaskedResult[param.KeepStartChars] = param.MaskChar;
            return new string(simpleMaskedResult);
        }

        if (value.Length <= param.KeepStartChars + param.KeepEndChars)
            return value;

        if (value.Length < param.KeepStartChars + param.KeepEndChars + 5)
        {
            char[] simpleMaskedResult = value.ToCharArray();
            for (int i = param.KeepStartChars; i < value.Length - param.KeepEndChars; i++)
            {
                simpleMaskedResult[i] = param.MaskChar;
            }

            return new string(simpleMaskedResult);
        }

        int maskLength = 5;
        char[] longMaskedResult = new char[param.KeepStartChars + maskLength + param.KeepEndChars];

        for (int i = 0; i < param.KeepStartChars; i++)
        {
            longMaskedResult[i] = value[i];
        }

        for (int i = param.KeepStartChars; i < param.KeepStartChars + maskLength; i++)
        {
            longMaskedResult[i] = param.MaskChar;
        }

        for (int i = 0; i < param.KeepEndChars; i++)
        {
            longMaskedResult[param.KeepStartChars + maskLength + i] = value[value.Length - param.KeepEndChars + i];
        }

        return new string(longMaskedResult);
    }

    /// <summary>
    /// Logs metadata and parameters at the start of the request.
    /// </summary>
    protected virtual void LogRequestStart(string requestName, Dictionary<string, object> parameters, string user)
    {
        if (Logger.IsEnabled(LogLevel.Information))
        {
            var logDetail = new
            {
                MethodName = "RequestHandler",
                User = user,
                Parameters = new[] { new { Type = requestName, Value = parameters } },
            };

            LogRequestStartAction(Logger, requestName, JsonSerializer.Serialize(logDetail, JsonOptions), null);
        }
    }

    /// <summary>
    /// Logs the response object if enabled by <see cref="LogOptions"/>.
    /// </summary>
    protected virtual void LogRequestEnd(string requestName, TResponse response)
    {
        if (Logger.IsEnabled(LogLevel.Information))
        {
            var logDetail = new
            {
                MethodName = "RequestHandler",
                Response = new { Type = typeof(TResponse).Name, Value = response },
            };

            LogRequestEndAction(Logger, requestName, JsonSerializer.Serialize(logDetail, JsonOptions), null);
        }
    }
}
