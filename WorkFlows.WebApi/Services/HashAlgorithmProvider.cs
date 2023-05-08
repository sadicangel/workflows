using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace WorkFlows.WebApi.Services;

public delegate int HashFunc(ReadOnlySpan<byte> source, Span<byte> destination);

public sealed record class HashAlgorithmRef(int HashSizeInBytes, HashFunc HashFunc);

public interface IHashAlgorithmProvider
{
    HashAlgorithmRef GetHashAlgorithm(string name);
    bool TryGetHashAlgorithm(string name, [MaybeNullWhen(false)] out HashAlgorithmRef? hashAlgorithm);
}

internal sealed class HashAlgorithmProvider : IHashAlgorithmProvider
{
    private readonly IReadOnlyDictionary<string, HashAlgorithmRef> _algorithmsByName = new Dictionary<string, HashAlgorithmRef>
    {
        ["MD5"] = new(MD5.HashSizeInBytes, MD5.HashData),
        ["SHA1"] = new(SHA1.HashSizeInBytes, SHA1.HashData),
        ["SHA256"] = new(SHA256.HashSizeInBytes, SHA256.HashData),
        ["SHA512"] = new(SHA512.HashSizeInBytes, SHA512.HashData)
    };

    public HashAlgorithmRef GetHashAlgorithm(string name) => _algorithmsByName[name];

    public bool TryGetHashAlgorithm(string name, [NotNullWhen(true)] out HashAlgorithmRef? hashAlgorithm) => _algorithmsByName.TryGetValue(name, out hashAlgorithm);
}
