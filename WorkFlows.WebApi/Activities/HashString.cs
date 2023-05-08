using Elsa;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Expressions;
using Elsa.Services;
using Elsa.Services.Models;
using Humanizer;
using System.Text;
using WorkFlows.WebApi.Services;

namespace WorkFlows.WebApi.Activities;



[Activity(
    Category = "Hash",
    DisplayName = "Hash Data",
    Description = "Hash string data",
    Outcomes = new[] { OutcomeNames.Done, OutcomeNames.Cancel })]
public sealed class HashString : Activity
{
    private readonly IHashAlgorithmProvider _hashAlgorithmProvider;

    public HashString(IHashAlgorithmProvider hashAlgorithmProvider)
    {
        Name = nameof(HashString);
        DisplayName = nameof(HashString).Humanize();
        Description = "Hashes a string";
        LoadWorkflowContext = true;
        SaveWorkflowContext = true;
        _hashAlgorithmProvider = hashAlgorithmProvider;
    }

    [ActivityInput(Hint = "The name of the hash algorithm", DefaultValue = "MD5", SupportedSyntaxes = new[] { SyntaxNames.Variable, SyntaxNames.JavaScript, SyntaxNames.Liquid })]
    public string AlgorithmName { get; set; } = "MD5";

    [ActivityInput(Hint = "The string to hash", SupportedSyntaxes = new[] { SyntaxNames.Variable, SyntaxNames.JavaScript, SyntaxNames.Liquid })]
    public string StringValue { get; set; } = String.Empty;

    [ActivityOutput]
    public string? Output { get; set; }

    protected override IActivityExecutionResult OnExecute(ActivityExecutionContext context)
    {
        if (!_hashAlgorithmProvider.TryGetHashAlgorithm(AlgorithmName, out var hasher))
        {
            Output = $"Invalid Hash Algorithm '{AlgorithmName}'";
            return Outcome(OutcomeNames.Cancel);
        }

        Span<byte> target = stackalloc byte[hasher.HashSizeInBytes];
        ReadOnlySpan<byte> source = Encoding.UTF8.GetBytes(StringValue);
        hasher.HashFunc.Invoke(source, target);

        Output = Convert.ToBase64String(target);
        return Done();
    }
}
