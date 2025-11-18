using Microsoft.CodeAnalysis.CSharp;

public static class KeywordDetect
{
    public static string Sanitize(StringBuilder builder)
    {
        var result = builder.ToString();
        if (SyntaxFacts.IsKeywordKind(SyntaxFacts.GetKeywordKind(result)))
        {
            return string.Concat("@", result);
        }
        return result;
    }
}