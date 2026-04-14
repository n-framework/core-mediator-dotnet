using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace NFramework.Mediator.Abstractions.Logging;

/// <summary>
/// Handles structured logging with support for parameter exclusion and masking.
/// Masking logic is optimized for common patterns like Emails and Credit Cards/Numbers.
/// </summary>
public abstract class LoggingBehaviorBase<TRequest, TResponse>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    protected LoggingBehaviorBase(ILogger logger)
    {
        Logger = logger;
    }

    protected ILogger Logger { get; }

    protected async ValueTask<TResponse> HandleAsync(
        TRequest request,
        Func<CancellationToken, ValueTask<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        if (request is not ILoggableRequest loggable)
        {
            return await next(cancellationToken);
        }

        string requestName = typeof(TRequest).Name;
        var requestParameters = ExtractRequestParameters(request, loggable.LogOptions);

        LogRequestStart(requestName, requestParameters, loggable.LogOptions.User);

        var response = await next(cancellationToken);

        if (loggable.LogOptions.LogResponse)
        {
            LogRequestEnd(requestName, response);
        }

        return response;
    }

    /// <summary>
    /// Identifies and filters request properties based on <see cref="LogOptions"/>.
    /// </summary>
    protected static Dictionary<string, object> ExtractRequestParameters(TRequest request, in LogOptions logOptions)
    {
        var parameters = new Dictionary<string, object>();

        foreach (PropertyInfo prop in typeof(TRequest).GetProperties())
        {
            object? value = prop.GetValue(request);
            if (value is null)
                continue;

            LogExcludeParameter excludeParam = Array.Find(logOptions.ExcludeParameters, p => p.Name == prop.Name);

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

        if (value.Contains('@'))
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
        var logDetail = new
        {
            MethodName = "RequestHandler",
            User = user,
            Parameters = new[] { new { Type = requestName, Value = parameters } },
        };

        Logger.LogInformation(
            "Handling {RequestName} {RequestDetails}",
            requestName,
            JsonSerializer.Serialize(logDetail, JsonOptions)
        );
    }

    /// <summary>
    /// Logs the response object if enabled by <see cref="LogOptions"/>.
    /// </summary>
    protected virtual void LogRequestEnd(string requestName, TResponse response)
    {
        var logDetail = new
        {
            MethodName = "RequestHandler",
            Response = new { Type = typeof(TResponse).Name, Value = response },
        };

        Logger.LogInformation(
            "Handled {RequestName} {ResponseDetails}",
            requestName,
            JsonSerializer.Serialize(logDetail, JsonOptions)
        );
    }
}
