using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;
using Microsoft.AspNetCore.DataProtection;
using Xunit;

namespace Inferno.Tests;

public class FormBlockTokenTests
{
    private static readonly FormBlockSettings Settings = new(
        "Thanks!", "/thank-you", "sales@example.com", null, "Contact us");

    [Fact]
    public void Unprotect_ValidToken_ReturnsOriginalSettings()
    {
        var provider = new EphemeralDataProtectionProvider();

        var token = FormBlockToken.Protect(provider, Settings);

        Assert.Equal(Settings, FormBlockToken.Unprotect(provider, token));
    }

    [Fact]
    public void Unprotect_TamperedToken_ReturnsNull()
    {
        var provider = new EphemeralDataProtectionProvider();
        var token = FormBlockToken.Protect(provider, Settings);

        var tampered = token[..^4] + (token.EndsWith("AAAA") ? "BBBB" : "AAAA");

        Assert.Null(FormBlockToken.Unprotect(provider, tampered));
    }

    [Fact]
    public void Unprotect_TokenFromAnotherKeyRing_ReturnsNull()
    {
        var token = FormBlockToken.Protect(new EphemeralDataProtectionProvider(), Settings);

        Assert.Null(FormBlockToken.Unprotect(new EphemeralDataProtectionProvider(), token));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not a token")]
    public void Unprotect_MissingOrGarbage_ReturnsNull(string token)
    {
        Assert.Null(FormBlockToken.Unprotect(new EphemeralDataProtectionProvider(), token));
    }
}
