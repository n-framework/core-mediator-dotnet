namespace NFramework.Mediator.Abstractions.Logging;

public readonly struct LogOptions(
    string? user = null,
    bool logResponse = false,
    params LogExcludeParameter[] excludeParameters
) : IEquatable<LogOptions>
{
    public IReadOnlyList<LogExcludeParameter> ExcludeParameters { get; init; } = excludeParameters ?? [];

    public bool LogResponse { get; init; } = logResponse;

    public string User { get; init; } = string.IsNullOrWhiteSpace(user) ? "?" : user;

    public static readonly LogOptions Default = new(null);

    public readonly bool Equals(LogOptions other)
    {
        return LogResponse == other.LogResponse
            && User == other.User
            && ExcludeParameters.SequenceEqual(other.ExcludeParameters);
    }

    public override readonly bool Equals(object? obj) => obj is LogOptions other && Equals(other);

    public override readonly int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(LogResponse);
        hash.Add(User);
        foreach (var param in ExcludeParameters)
        {
            hash.Add(param);
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(LogOptions left, LogOptions right) => left.Equals(right);

    public static bool operator !=(LogOptions left, LogOptions right) => !left.Equals(right);
}
