[TestFixture]
public class KeywordDetectTests
{
    [Test]
    public Task KeywordClass()
    {
        var builder = new StringBuilder("class");
        return Verify(KeywordDetect.Sanitize(builder));
    }

    [Test]
    public Task KeywordNamespace()
    {
        var builder = new StringBuilder("namespace");
        return Verify(KeywordDetect.Sanitize(builder));
    }

    [Test]
    public Task KeywordString()
    {
        var builder = new StringBuilder("string");
        return Verify(KeywordDetect.Sanitize(builder));
    }

    [Test]
    public Task KeywordInt()
    {
        var builder = new StringBuilder("int");
        return Verify(KeywordDetect.Sanitize(builder));
    }

    [Test]
    public Task KeywordFor()
    {
        var builder = new StringBuilder("for");
        return Verify(KeywordDetect.Sanitize(builder));
    }

    [Test]
    public Task KeywordWhile()
    {
        var builder = new StringBuilder("while");
        return Verify(KeywordDetect.Sanitize(builder));
    }

    [Test]
    public Task KeywordPublic()
    {
        var builder = new StringBuilder("public");
        return Verify(KeywordDetect.Sanitize(builder));
    }

    [Test]
    public Task KeywordPrivate()
    {
        var builder = new StringBuilder("private");
        return Verify(KeywordDetect.Sanitize(builder));
    }

    [Test]
    public Task NonKeywordNormal()
    {
        var builder = new StringBuilder("MyClass");
        return Verify(KeywordDetect.Sanitize(builder));
    }

    [Test]
    public Task NonKeywordWithUnderscore()
    {
        var builder = new StringBuilder("my_class");
        return Verify(KeywordDetect.Sanitize(builder));
    }

    [Test]
    public Task NonKeywordWithNumbers()
    {
        var builder = new StringBuilder("class123");
        return Verify(KeywordDetect.Sanitize(builder));
    }

    [Test]
    public Task EmptyString()
    {
        var builder = new StringBuilder("");
        return Verify(KeywordDetect.Sanitize(builder));
    }

    [Test]
    public Task KeywordWithDifferentCase()
    {
        var builder = new StringBuilder("Class");
        return Verify(KeywordDetect.Sanitize(builder));
    }

    [Test]
    public Task AllKeywordsAreSanitized()
    {
        var keywords = new[]
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch",
            "char", "checked", "class", "const", "continue", "decimal", "default",
            "delegate", "do", "double", "else", "enum", "event", "explicit",
            "extern", "false", "finally", "fixed", "float", "for", "foreach",
            "goto", "if", "implicit", "in", "int", "interface", "internal",
            "is", "lock", "long", "namespace", "new", "null", "object",
            "operator", "out", "override", "params", "private", "protected",
            "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
            "sizeof", "stackalloc", "static", "string", "struct", "switch",
            "this", "throw", "true", "try", "typeof", "uint", "ulong",
            "unchecked", "unsafe", "ushort", "using", "virtual", "void",
            "volatile", "while"
        };

        var results = keywords
            .Select(_ => new
            {
                Keyword = _,
                Sanitized = KeywordDetect.Sanitize(new(_))
            })
            .ToList();

        return Verify(results);
    }
}
