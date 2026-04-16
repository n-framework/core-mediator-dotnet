namespace NFramework.Mediator.Abstractions.Caching;

/// <summary>
/// Handles object-to-binary conversion for cache storage.
/// Native AOT compatible implementations should use source-generated serializers.
/// </summary>
public interface ISerializer
{
    byte[] SerializeToUtf8Bytes<T>(T value);

    T? Deserialize<T>(byte[] data);
}
