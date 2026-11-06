using Inferno.Web.Messaging;
using Xunit;

namespace Inferno.Tests;

public class MessageTemplateTokenTests
{
    [Theory]
    [InlineData("FirstName", "{{ FirstName }}")]
    [InlineData("{{FIRST_NAME}}", "{{ FIRST_NAME }}")]
    [InlineData("{{ FirstName | upcase }}", "{{ FirstName | upcase }}")]
    public void Normalize_StoresALiquidTag(string input, string expected)
    {
        var result = MessageTemplateTokens.Normalize([input]);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, Assert.Single(result.Value));
    }

    [Fact]
    public void Normalize_RejectsADuplicateAfterCanonicalForm()
    {
        var result = MessageTemplateTokens.Normalize(["{{FIRST_NAME}}", "FIRST_NAME"]);

        Assert.False(result.IsSuccess);
        Assert.Equal("Tokens", result.ValidationErrors.First().Identifier);
    }

    [Fact]
    public void Normalize_SkipsBlankRows()
    {
        var result = MessageTemplateTokens.Normalize(["", "  ", "Email"]);

        Assert.True(result.IsSuccess);
        Assert.Equal("{{ Email }}", Assert.Single(result.Value));
    }

    [Fact]
    public void Read_ReturnsAnEmptyListForMissingJson()
    {
        Assert.Empty(MessageTemplateTokens.Read(null));
        Assert.Empty(MessageTemplateTokens.Read("not json"));
    }
}
