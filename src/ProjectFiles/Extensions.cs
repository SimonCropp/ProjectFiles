static class Extensions
{
    public static IncrementalValuesProvider<TResult> Select<TSource, TResult>(this IncrementalValuesProvider<TSource> source, Func<TSource, TResult> selector) =>
        source.Select((item, _)
            => selector(item));
}