namespace NFramework.Mediator.Abstractions.Logging;

public readonly struct LogExcludeParameter : IEquatable<LogExcludeParameter>
{
    public string Name { get; init; }

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
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Parameter name cannot be empty.", nameof(name));

        if (keepStartChars < 0)
            throw new ArgumentOutOfRangeException(nameof(keepStartChars), "KeepStartChars must be non-negative.");

        if (keepEndChars < 0)
            throw new ArgumentOutOfRangeException(nameof(keepEndChars), "KeepEndChars must be non-negative.");

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

    public static LogExcludeParameter FromString(string name)
    {
        return new(name);
    }

    public readonly bool Equals(LogExcludeParameter other)
    {
        return Name == other.Name
            && Mask == other.Mask
            && MaskChar == other.MaskChar
            && KeepStartChars == other.KeepStartChars
            && KeepEndChars == other.KeepEndChars;
    }

    public override readonly bool Equals(object? obj) => obj is LogExcludeParameter other && Equals(other);

    public override readonly int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(Name);
        hash.Add(Mask);
        hash.Add(MaskChar);
        hash.Add(KeepStartChars);
        hash.Add(KeepEndChars);
        return hash.ToHashCode();
    }

    public static bool operator ==(LogExcludeParameter left, LogExcludeParameter right) => left.Equals(right);

    public static bool operator !=(LogExcludeParameter left, LogExcludeParameter right) => !left.Equals(right);
}
