using Base.Helpers;

namespace Base.Tests;

public class TestFooTest
{
    private TestFoo TestFoo { get; } = new();

    [Fact]
    public void InitialValueShouldBeZero()
    {
        //Arrange
        //Act
        //
        Assert.Equal(0, TestFoo.State);
    }

    [Fact]
    public void AddTwoNumbers()
    {
        //Arrange
        TestFoo.State = -1;
        //Act
        TestFoo.Add(2);
        //Assert
        Assert.Equal(1, TestFoo.State);
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(-1, 1, 0)]
    [InlineData(1, 1, 2)]
    public void AddTwoNumbersViaParams(int initialValue, int valueToAdd, int expectedValue)
    {
        // Arrange
        TestFoo.State = initialValue;
        // Act
        TestFoo.Add(valueToAdd);
        // Assert
        Assert.Equal(expectedValue, TestFoo.State);
    }

    [Fact]
    public void DivideByZeroShouldGiveException()
    {
        TestFoo.State = 1;
        Assert.Throws<DivideByZeroException>(() => TestFoo.Div(0));
    }
}