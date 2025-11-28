class MockOptionsProvider(
    Dictionary<string, Dictionary<string, string>> fileOptions,
    Dictionary<string, string>? globalOptions = null) :
    AnalyzerConfigOptionsProvider
{
    Dictionary<string, string> globalOptions = globalOptions ?? new();

    public override AnalyzerConfigOptions GlobalOptions =>
        new MockGlobalOptions(globalOptions);

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) =>
        new MockOptions(new());

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
    {
        var path = textFile.Path;
        if (fileOptions.TryGetValue(path, out var options))
        {
            return new MockOptions(options);
        }

        return new MockOptions(new());
    }

    class MockGlobalOptions(Dictionary<string, string> options) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value) =>
            options.TryGetValue(key, out value!);
    }

    class MockOptions(Dictionary<string, string> options) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value) =>
            options.TryGetValue(key, out value!);
    }
}