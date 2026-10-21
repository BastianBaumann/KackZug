using KackZug.Core.Planning;

namespace KackZug.Core.Tests.Planning;

/// <summary>Prueft das Lesen getippter Daten fuer Einzelfahrten</summary>
public class DateTextTests
{
    [Theory]
    [InlineData("23.10.2026", 2026, 10, 23)]
    [InlineData("23.10.26", 2026, 10, 23)]
    [InlineData("1.2.2027", 2027, 2, 1)]
    [InlineData("23102026", 2026, 10, 23)]
    [InlineData("231026", 2026, 10, 23)]
    [InlineData("23/10/2026", 2026, 10, 23)]
    [InlineData(" 29.02.2028 ", 2028, 2, 29)]
    public void ReadsCommonForms(string text, int year, int month, int day)
    {
        Assert.Equal(new DateOnly(year, month, day), DateText.Parse(text));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("23.10.")]
    [InlineData("32.10.2026")]
    [InlineData("29.02.2027")]
    [InlineData("23.13.2026")]
    [InlineData("23.10.202")]
    [InlineData("ab.10.2026")]
    public void RefusesWrongDates(string? text)
    {
        Assert.Null(DateText.Parse(text));
    }

    [Theory]
    [InlineData("2", "2")]
    [InlineData("23", "23")]
    [InlineData("231", "23.1")]
    [InlineData("2310", "23.10")]
    [InlineData("23102", "23.10.2")]
    [InlineData("23102026", "23.10.2026")]
    [InlineData("231020261", "23.10.2026")]
    [InlineData("23.", "23.")]
    [InlineData("1.2.2027", "1.2.2027")]
    [InlineData("23.10.2026", "23.10.2026")]
    public void SetsDotsWhileTyping(string typed, string expected)
    {
        Assert.Equal(expected, DateText.FormatTyping(typed));
    }

    [Fact]
    public void FormatsWithLeadingZeros()
    {
        Assert.Equal("01.02.2027", DateText.Format(new DateOnly(2027, 2, 1)));
    }
}
