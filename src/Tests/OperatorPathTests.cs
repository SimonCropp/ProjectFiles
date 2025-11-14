[TestFixture]
public class OperatorPathTests
{
    [Test]
    public Task GeneratesPathNodeStruct()
    {
        var project = """
                      <Project Sdk="Microsoft.NET.Sdk">
                        <PropertyGroup>
                          <TargetFramework>net8.0</TargetFramework>
                        </PropertyGroup>
                        <ItemGroup>
                          <None Update="SpecificDirectory\Dir2\File4.txt">
                            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
                          </None>
                        </ItemGroup>
                      </Project>
                      """;

        var result = TestHelper.Run(project, ["SpecificDirectory/Dir2/File4.txt"]);

        return Verify(result).UseFileName("OperatorPathTests.GeneratesPathNodeStruct");
    }

    [Test]
    public Task GeneratesDirectoryPathNodes()
    {
        var project = """
                      <Project Sdk="Microsoft.NET.Sdk">
                        <PropertyGroup>
                          <TargetFramework>net8.0</TargetFramework>
                        </PropertyGroup>
                        <ItemGroup>
                          <None Update="Assets\**\*.*">
                            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
                          </None>
                        </ItemGroup>
                      </Project>
                      """;

        var result = TestHelper.Run(
            project,
            [
                "Assets/Images/logo.png",
                "Assets/Data/users.csv"
            ]);

        return Verify(result).UseFileName("OperatorPathTests.GeneratesDirectoryPathNodes");
    }

    [Test]
    public Task GeneratesFileExtensionClasses()
    {
        var project = """
                      <Project Sdk="Microsoft.NET.Sdk">
                        <PropertyGroup>
                          <TargetFramework>net8.0</TargetFramework>
                        </PropertyGroup>
                        <ItemGroup>
                          <None Update="config.json">
                            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
                          </None>
                          <None Update="data.csv">
                            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
                          </None>
                        </ItemGroup>
                      </Project>
                      """;

        var result = TestHelper.Run(
            project,
            [
                "config.json",
                "data.csv"
            ]);

        return Verify(result).UseFileName("OperatorPathTests.GeneratesFileExtensionClasses");
    }
}
