namespace Integration.Tests;

/// <summary>
/// Sample integration test to verify xUnit and test setup is working correctly.
/// This test should pass and can be removed once real tests are added.
/// </summary>
public class SampleIntegrationTests
{
    [Fact]
    public void SampleIntegrationTest_ShouldPass()
    {
        // Arrange
        var expected = "Hello, World!";
        
        // Act
        var actual = "Hello, " + "World!";
        
        // Assert
        Assert.Equal(expected, actual);
    }
}
