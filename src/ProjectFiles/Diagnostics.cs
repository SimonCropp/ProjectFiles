public static class Diagnostics
{
    public static readonly DiagnosticDescriptor PF0001_CSharp12Required = new DiagnosticDescriptor(
        id: "PF0001",
        title: "C# 12 or later is required",
        messageFormat: "This generator requires C# 12 or later to run",
        category: "ProjectFiles",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

}