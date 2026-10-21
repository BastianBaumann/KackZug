using KackZug.Core.Planning;

namespace KackZug.Core.Tests.Planning;

/// <summary>Prueft das Lesen getippter Uhrzeiten</summary>
public class ClockTextTests
{
    [Theory]
    [InlineData("19:30", 19, 30)]
    [InlineData("19.30", 19, 30)]
    [InlineData("19,30", 19, 30)]
    [InlineData("1930", 19, 30)]
    [InlineData("930", 9, 30)]
    [InlineData("7:05", 7, 5)]
    [InlineData("19", 19, 0)]
    [InlineData("19:", 19, 0)]
    [InlineData(" 20 Uhr ", 20, 0)]
    [InlineData("0:00", 0, 0)]
    public void ReadsCommonForms(string text, int hour, int minute)
    {
        Assert.Equal(new TimeOnly(hour, minute), ClockText.Parse(text));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("24:00")]
    [InlineData("19:60")]
    [InlineData("abc")]
    [InlineData("19:3a")]
    [InlineData("12345")]
    [InlineData(":30")]
    public void RejectsInvalidInput(string? text)
    {
        Assert.Null(ClockText.Parse(text));
    }

    [Fact]
    public void FormatsWithLeadingZero()
    {
        Assert.Equal("07:05", ClockText.Format(new TimeOnly(7, 5)));
    }
}
