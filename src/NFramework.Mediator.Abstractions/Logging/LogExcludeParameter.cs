namespace NFramework.Mediator.Abstractions.Logging;

/// <summary>
/// Defines how a sensitive request parameter should be handled in logs.
/// </summary>
public readonly struct LogExcludeParameter
{
    public string Name { get; init; }

    /// <summary>
    /// If true, the value is masked rather than completely removed.
    /// </summary>
    public bool Mask { get; init; }

    public char MaskChar { get; init; }

    public int KeepStartChars { get; init; }

    public int KeepEndChars { get; init; }

    public LogExcludeParameter(
        string name,
        bool mask = false,
        char maskChar = '*',
        int keepStartChars = 0,
        int keepEndChars = 0
    )
    {
        Name = name;
        Mask = mask;
        MaskChar = maskChar;
        KeepStartChars = keepStartChars;
        KeepEndChars = keepEndChars;
    }

    public static implicit operator LogExcludeParameter(string name)
    {
        return new(name);
    }
}
