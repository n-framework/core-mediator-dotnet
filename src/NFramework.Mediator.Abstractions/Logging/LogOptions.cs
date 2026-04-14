namespace NFramework.Mediator.Abstractions.Logging;

public readonly struct LogOptions
{
    public LogExcludeParameter[] ExcludeParameters { get; init; }

    public bool LogResponse { get; init; }

    /// <summary>
    /// User information to be included in logs (defaults to "?" if empty).
    /// </summary>
    public string User { get; init; }

    public LogOptions()
    {
        ExcludeParameters = [];
        LogResponse = false;
        User = "?";
    }

    public LogOptions(params LogExcludeParameter[] excludeParameters)
    {
        ExcludeParameters = excludeParameters;
        LogResponse = false;
        User = "?";
    }

    public LogOptions(bool logResponse, params LogExcludeParameter[] excludeParameters)
    {
        ExcludeParameters = excludeParameters;
        LogResponse = logResponse;
        User = "?";
    }

    public LogOptions(string? user, bool logResponse = false, params LogExcludeParameter[] excludeParameters)
    {
        ExcludeParameters = excludeParameters;
        LogResponse = logResponse;
        User = string.IsNullOrEmpty(user) ? "?" : user!;
    }

    public static readonly LogOptions Default = new();
}
