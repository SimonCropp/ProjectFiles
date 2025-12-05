public static class Diagnostics
{
    public static readonly DiagnosticDescriptor MinLangVersion = new(
        id: "PROJFILES003",
        title: "C# 14 or later is required",
        messageFormat: "This generator requires C# 14 or later to run",
        category: "ProjectFiles",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);


    public  static readonly DiagnosticDescriptor ReservedFileNameConflict = new(
        id: "PROJFILES001",
        title: "File name conflicts with reserved property",
        messageFormat: "File '{0}' would generate property name '{1}' that conflicts with reserved MSBuild property. Rename the file or exclude it from CopyToOutputDirectory.",
        category: "ProjectFiles",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public  static readonly DiagnosticDescriptor ReservedDirectoryNameConflict = new(
        id: "PROJFILES002",
        title: "Directory name conflicts with reserved property",
        messageFormat: "Directory '{0}' would generate property name '{1}' that conflicts with reserved MSBuild property. Rename the directory or exclude its files from CopyToOutputDirectory.",
        category: "ProjectFiles",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}