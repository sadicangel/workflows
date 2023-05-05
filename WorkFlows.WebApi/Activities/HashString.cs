using Elsa;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Expressions;
using Elsa.Services;
using Elsa.Services.Models;
using Humanizer;
using System.Security.Cryptography;
using System.Text;

namespace WorkFlows.WebApi.Activities;

public sealed record class HashStringOutput(string HashedValue);

[Activity(
    Category = "Hash",
    DisplayName = "Hash Data",
    Description = "Hash string data",
    Outcomes = new[] { OutcomeNames.Done, OutcomeNames.Cancel })]
public sealed class HashString : Activity
{
    private static readonly IReadOnlyDictionary<string, Func<HashAlgorithm>> AlgorithmFactory = new Dictionary<string, Func<HashAlgorithm>>
    {
        ["MD5"] = MD5.Create,
        ["SHA1"] = SHA1.Create,
        ["SHA256"] = SHA256.Create,
        ["SHA512"] = SHA512.Create
    };

    public HashString()
    {
        Name = nameof(HashString);
        DisplayName = nameof(HashString).Humanize();
        Description = "Hashes a string";
        LoadWorkflowContext = true;
        SaveWorkflowContext = true;
    }

    [ActivityInput(Hint = "The name of the hash algorithm", DefaultValue = "MD5", SupportedSyntaxes = new[] { SyntaxNames.Variable, SyntaxNames.JavaScript, SyntaxNames.Liquid })]
    public string AlgorithmName { get; set; } = "MD5";

    [ActivityInput(Hint = "The string to hash", SupportedSyntaxes = new[] { SyntaxNames.Variable, SyntaxNames.JavaScript, SyntaxNames.Liquid })]
    public string StringValue { get; set; } = String.Empty;

    [ActivityOutput]
    public string? Output { get; set; }

    protected override IActivityExecutionResult OnExecute(ActivityExecutionContext context)
    {
        if (!AlgorithmFactory.TryGetValue(AlgorithmName, out var hasherCtor))
        {
            Output = $"Invalid Hash Algorithm '{AlgorithmName}'";
            return Outcome(OutcomeNames.Cancel);
        }

        var hasher = hasherCtor();

        Span<byte> target = stackalloc byte[hasher.HashSize / 8];
        ReadOnlySpan<byte> source = Encoding.UTF8.GetBytes(StringValue);
        if (!hasher.TryComputeHash(source, target, out var bytesWritten))
        {
            Output = $"Failed to hash '{StringValue}' with algorith '{AlgorithmName}'";
            return Outcome(OutcomeNames.Cancel);
        }

        Output = Convert.ToBase64String(target);
        return Done();
    }
}
