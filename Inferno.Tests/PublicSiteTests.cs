using Inferno.Tenants;
using Inferno.Tenants.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.Messaging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Inferno.Tests;

public class PageVersionSelectorTests
{
    [Fact]
    public void Select_PublishedCulture_BeatsArchivedAndInvariant()
    {
        var published = Version("en-US", VersionStatus.Published, new DateTime(2024, 1, 2));
        var archived = Version("en-US", VersionStatus.Archived, new DateTime(2024, 2, 2));
        var invariant = Version(null, VersionStatus.Published, new DateTime(2024, 3, 2));

        var selected = PageVersionSelector.Select([archived, invariant, published], "en-US");

        Assert.Same(published, selected);
    }

    [Fact]
    public void Select_FallsBackToInvariantPublished_WhenCultureHasNoVersion()
    {
        var invariant = Version(null, VersionStatus.Published, new DateTime(2024, 1, 1));
        var otherCulture = Version("fr-FR", VersionStatus.Published, new DateTime(2024, 6, 1));

        var selected = PageVersionSelector.Select([otherCulture, invariant], "en-US");

        Assert.Same(invariant, selected);
    }

    [Fact]
    public void Select_UsesNewestDate_WhenSeveralMatch()
    {
        var older = Version("en-US", VersionStatus.Published, new DateTime(2024, 1, 1));
        var newer = Version("en-US", VersionStatus.Published, new DateTime(2024, 5, 1));

        var selected = PageVersionSelector.Select([older, newer], "en-US");

        Assert.Same(newer, selected);
    }

    private static PageVersion Version(string cultureCode, VersionStatus status, DateTime modified) => new()
    {
        CultureCode = cultureCode,
        Status = status,
        DateModifiedUtc = modified
    };
}

public class TokenizerTests
{
    [Fact]
    public void Replace_HtmlEncodesTokenValues_WhenRequested()
    {
        var tokenizer = Create();
        string result = tokenizer.Replace(
            "Hello [Name]",
            [new Token("[Name]", "<Ann>")],
            htmlEncode: true);

        Assert.Equal("Hello &lt;Ann&gt;", result);
    }

    [Fact]
    public void Replace_LeavesTokenValuesRaw_WhenHtmlEncodeIsOff()
    {
        var tokenizer = Create();
        string result = tokenizer.Replace(
            "Hello [Name]",
            [new Token("[Name]", "<Ann>")],
            htmlEncode: false);

        Assert.Equal("Hello <Ann>", result);
    }

    [Fact]
    public void Replace_RendersLiquidAndEncodesTheValue()
    {
        var tokenizer = Create();
        string result = tokenizer.Replace(
            "<p>Hello {{ Name }}</p>",
            [new Token("Name", "<Ann>")],
            htmlEncode: true);

        Assert.Equal("<p>Hello &lt;Ann&gt;</p>", result);
    }

    [Fact]
    public void Replace_RendersATokenWrittenAsTheLiquidTag()
    {
        var tokenizer = Create();
        string result = tokenizer.Replace(
            "Hello {{ FIRST_NAME }}",
            [new Token("{{ FIRST_NAME }}", "Ann")],
            htmlEncode: false);

        Assert.Equal("Hello Ann", result);
    }

    [Fact]
    public void Replace_FillsTheOlderBracketFormFromTheLiquidName()
    {
        var tokenizer = Create();
        string result = tokenizer.Replace(
            "Hello [Name] and {{ Name | upcase }}",
            [new Token("Name", "Ann")],
            htmlEncode: false);

        Assert.Equal("Hello Ann and ANN", result);
    }

    [Fact]
    public void Replace_LeavesInvalidLiquidInPlaceAfterBracketReplacement()
    {
        var tokenizer = Create();
        string result = tokenizer.Replace(
            "Hello [Name] {{ Name",
            [new Token("Name", "Ann")],
            htmlEncode: false);

        Assert.Equal("Hello Ann {{ Name", result);
    }

    private static Tokenizer Create() => new(NullLogger<Tokenizer>.Instance);
}

public class TenantUrlExtensionsTests
{
    [Fact]
    public void GetPublicBaseUrl_UsesSecureUrl_WhenSslIsEnabled()
    {
        var tenant = new Tenant
        {
            SslEnabled = true,
            SecureUrl = "example.com"
        };

        Assert.Equal("https://example.com", tenant.GetPublicBaseUrl("http://localhost:5000"));
    }

    [Fact]
    public void GetPublicBaseUrl_KeepsTheRequestOrigin_WhenSslIsDisabled()
    {
        var tenant = new Tenant
        {
            SslEnabled = false,
            SecureUrl = "https://example.com"
        };

        Assert.Equal("http://localhost:5000", tenant.GetPublicBaseUrl("http://localhost:5000/"));
    }
}
