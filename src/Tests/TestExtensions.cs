public static class TestExtensions
{
    public static GeneratorDriver AddAdditionalTexts(this GeneratorDriver driver, IEnumerable<AdditionalText> additionalTexts) =>
        driver.AddAdditionalTexts([..additionalTexts]);
}