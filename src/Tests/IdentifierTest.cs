[TestFixture]
public class IdentifierTest
{
    [Test]
    public Task NormalName() =>
        Verify(Identifier.Build("MyFile"));

    [Test]
    public Task NameWithSpaces() =>
        Verify(Identifier.Build("My File"));

    [Test]
    public Task NameWithHyphens() =>
        Verify(Identifier.Build("my-file"));

    [Test]
    public Task NameWithDots() =>
        Verify(Identifier.Build("my.config.json"));

    [Test]
    public Task NameStartingWithDigit() =>
        Verify(Identifier.Build("123file"));

    [Test]
    public Task NameWithSpecialCharacters() =>
        Verify(Identifier.Build("file@#$%name"));

    [Test]
    public Task NameWithUnderscore() =>
        Verify(Identifier.Build("_myfile"));

    [Test]
    public Task NameWithMultipleUnderscores() =>
        Verify(Identifier.Build("__my__file__"));

    [Test]
    public Task KeywordName() =>
        Verify(Identifier.Build("class"));

    [Test]
    public Task AnotherKeywordName() =>
        Verify(Identifier.Build("namespace"));

    [Test]
    public Task MixedCaseKeyword() =>
        Verify(Identifier.Build("Class"));

    [Test]
    public Task EmptyString()
    {
        Assert.Throws<IndexOutOfRangeException>(() => Identifier.Build(""));
        return Task.CompletedTask;
    }

    [Test]
    public Task SingleCharacter() =>
        Verify(Identifier.Build("a"));

    [Test]
    public Task SingleDigit() =>
        Verify(Identifier.Build("1"));

    [Test]
    public Task SingleSpecialCharacter() =>
        Verify(Identifier.Build("@"));

    [Test]
    public Task UnicodeCharacters() =>
        Verify(Identifier.Build("café"));

    [Test]
    public Task ChineseCharacters() =>
        Verify(Identifier.Build("文件"));

    [Test]
    public Task NumbersInMiddle() =>
        Verify(Identifier.Build("file123name"));
}
