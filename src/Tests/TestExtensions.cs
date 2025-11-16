public static class TestExtensions
{
    public static GeneratorDriver AddAdditionalText(this GeneratorDriver driver, IEnumerable<AdditionalText> additionalTexts) =>
        driver.AddAdditionalTexts([..additionalTexts]);
}