using Inferno.Plugins.GoogleAnalytics;
using Inferno.Web.Components;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

using Xunit;

namespace Inferno.Tests;

public class ContentBlockTypesTests
{
    [Fact]
    public void Resolve_Blank_ReturnsNull()
    {
        Assert.Null(ContentBlockTypes.Resolve(null));
        Assert.Null(ContentBlockTypes.Resolve("  "));
    }

    [Fact]
    public void Resolve_FullName_FindsTypeInAnotherAssembly()
    {
        var resolved = ContentBlockTypes.Resolve(typeof(AdminPage).FullName);

        Assert.Equal(typeof(AdminPage), resolved);
    }

    [Fact]
    public void Resolve_AssemblyQualifiedName_FindsType()
    {
        var resolved = ContentBlockTypes.Resolve(typeof(AdminPage).AssemblyQualifiedName);

        Assert.Equal(typeof(AdminPage), resolved);
    }

    [Fact]
    public void Resolve_Unknown_ReturnsNull()
    {
        Assert.Null(ContentBlockTypes.Resolve("Inferno.Missing.Type, Inferno.Web"));
    }
}

public class GoogleAnalyticsIdsTests
{
    [Fact]
    public void IsMeasurementId_AcceptsAGoogleTag()
    {
        Assert.True(GoogleAnalyticsIds.IsMeasurementId(" g-ab12cd34 "));
        Assert.False(GoogleAnalyticsIds.IsMeasurementId("UA-123"));
        Assert.False(GoogleAnalyticsIds.IsMeasurementId(null));
    }

    [Fact]
    public void PropertyId_AcceptsTheNumberAndTheResourceName()
    {
        Assert.Equal("123456789", GoogleAnalyticsIds.PropertyId("properties/123456789"));
        Assert.Equal("123456789", GoogleAnalyticsIds.PropertyId(" 123456789 "));
        Assert.Equal(string.Empty, GoogleAnalyticsIds.PropertyId("properties/abc"));
    }

    [Fact]
    public void EffectiveMeasurementId_PrefersAValidBlockValue()
    {
        Assert.Equal("G-BLOCK", GoogleAnalyticsIds.EffectiveMeasurementId("g-block", "G-SETTINGS"));
        Assert.Equal("G-SETTINGS", GoogleAnalyticsIds.EffectiveMeasurementId("not-an-id", "G-SETTINGS"));
        Assert.Equal(string.Empty, GoogleAnalyticsIds.EffectiveMeasurementId("", "UA-1"));
    }
}

public class AnalyticsReportParserTests
{
    [Fact]
    public void Parse_Empty_Fails()
    {
        var table = AnalyticsReportParser.Parse("  ");

        Assert.False(table.Succeeded);
    }

    [Fact]
    public void Parse_ErrorMessage_FailsWithThatMessage()
    {
        var table = AnalyticsReportParser.Parse("""{"error":{"message":"Property not found."}}""");

        Assert.Equal("Property not found.", table.Error);
    }

    [Fact]
    public void Parse_Rows_ReadsDimensionsAndNamedMetrics()
    {
        const string json = """
            {
              "dimensionHeaders": [{ "name": "date" }],
              "metricHeaders": [{ "name": "activeUsers" }, { "name": "screenPageViews" }],
              "rows": [
                {
                  "dimensionValues": [{ "value": "20261111" }],
                  "metricValues": [{ "value": "4" }, { "value": "9" }]
                }
              ]
            }
            """;

        var table = AnalyticsReportParser.Parse(json);
        var row = Assert.Single(table.Rows);

        Assert.Equal("20261111", row.Dimensions[0]);
        Assert.Equal(4, table.Metric(row, "activeUsers"));
        Assert.Equal(9, table.Metric(row, "screenPageViews"));
        Assert.Equal(0, table.Metric(row, "missing"));
    }
}

public class ServiceAccountKeyTests
{
    [Fact]
    public void TryParse_ReadsTheEmailKeyAndTokenUri()
    {
        const string json = """
            {
              "client_email": " reports@example.iam.gserviceaccount.com ",
              "private_key": "-----BEGIN PRIVATE KEY-----\nABC\n-----END PRIVATE KEY-----\n",
              "token_uri": " https://oauth2.googleapis.com/token "
            }
            """;

        var parsed = ServiceAccountKey.TryParse(json, out var key, out var error);

        Assert.True(parsed);
        Assert.Null(error);
        Assert.Equal("reports@example.iam.gserviceaccount.com", key.ClientEmail);
        Assert.Contains("BEGIN PRIVATE KEY", key.PrivateKey, StringComparison.Ordinal);
        Assert.Equal("https://oauth2.googleapis.com/token", key.TokenUri);
    }

    [Fact]
    public void TryParse_InvalidJson_ExplainsWhy()
    {
        var parsed = ServiceAccountKey.TryParse("{", out var key, out var error);

        Assert.False(parsed);
        Assert.Null(key);
        Assert.Equal("The service account key is not valid JSON.", error);
    }
}
