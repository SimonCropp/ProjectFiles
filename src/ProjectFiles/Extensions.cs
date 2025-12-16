using Microsoft.CodeAnalysis.Diagnostics;

static class Extensions
{
    public static IncrementalValuesProvider<TResult> Select<TSource, TResult>(this IncrementalValuesProvider<TSource> source, Func<TSource, TResult> selector) =>
        source.Select((item, _)
            => selector(item));

    public static string? GetValue(this AnalyzerConfigOptions options, string property)
    {
        if (!options.TryGetValue(property, out var value))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(value) || value == "*Undefined*")
        {
            return null;
        }

        return value;
    }
}