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

    [Theory]
    [InlineData("", "")]
    [InlineData("2", "2")]
    [InlineData("22", "22")]
    [InlineData("223", "22:3")]
    [InlineData("2230", "22:30")]
    [InlineData("22:30", "22:30")]
    [InlineData("22:", "22")]
    [InlineData("9", "9")]
    [InlineData("93", "9:3")]
    [InlineData("930", "9:30")]
    [InlineData("0930", "09:30")]
    [InlineData("245", "24:5")]
    [InlineData("25:00", "25:00")]
    [InlineData("22305", "22:30")]
    [InlineData("19.30", "19:30")]
    public void ColonComesWhileTyping(string typed, string expected)
    {
        Assert.Equal(expected, ClockText.FormatTyping(typed));
    }

    [Theory]
    [InlineData("2230")]
    [InlineData("930")]
    [InlineData("0005")]
    public void TypedTextStaysReadable(string typed)
    {
        Assert.Equal(ClockText.Parse(typed), ClockText.Parse(ClockText.FormatTyping(typed)));
    }
}
