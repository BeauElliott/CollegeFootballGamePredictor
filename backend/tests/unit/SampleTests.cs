namespace Unit.Tests;

/// <summary>
/// Sample unit test to verify xUnit setup is working correctly.
/// This test should pass and can be removed once real tests are added.
/// </summary>
public class SampleTests
{
    [Fact]
    public void SampleTest_ShouldPass()
    {
        // Arrange
        var expected = 2;
        
        // Act
        var actual = 1 + 1;
        
        // Assert
        Assert.Equal(expected, actual);
    }
}
