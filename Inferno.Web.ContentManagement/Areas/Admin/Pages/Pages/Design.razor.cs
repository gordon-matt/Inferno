using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Microsoft.AspNetCore.Components;
using PageKrafter.Documents;
using PageKrafter.Rendering;
using Radzen;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Pages;

public partial class Design
{
    private string documentJson;
    private string savedAt;
    private string message;
    private bool found;
    private bool published;
    private bool publishing;
    private Guid pageId;
    private string cultureCode;

    private string Heading { get; set; } = string.Empty;

    [Parameter]
    public Guid VersionId { get; set; }

    [Inject]
    private IPageVersionService PageVersionService { get; set; }

    [Inject]
    private IPageService PageService { get; set; }

    [Inject]
    private IPageAuthoringService Authoring { get; set; }

    [Inject]
    private PageRenderer Renderer { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject]
    private NotificationService Notifications { get; set; }

    protected override void OnInitialized()
    {
        PageVersion version;
        using (var connection = PageVersionService.OpenConnection())
        {
            version = connection.Query(candidate => candidate.Id == VersionId).FirstOrDefault();
        }

        if (version is null || version.Status == VersionStatus.Archived)
        {
            message = T[InfernoCmsLocalizableStrings.Pages.ErrorNotFound].Value;
            return;
        }

        Page page;
        using (var connection = PageService.OpenConnection())
        {
            page = connection.Query(candidate => candidate.Id == version.PageId).FirstOrDefault();
        }

        pageId = page?.Id ?? Guid.Empty;
        cultureCode = version.CultureCode;
        published = version.Status == VersionStatus.Published;
        Heading = string.IsNullOrWhiteSpace(version.Title) ? page?.Name : version.Title;
        documentJson = StandardPageContent.ForEditor(StandardPageContent.Parse(version.Fields));
        found = true;
    }

    private async Task SaveAsync(PageDocument document)
    {
        string html = await Renderer.RenderAsync(document);
        var result = await Authoring.SaveContentAsync(VersionId, PageKrafterJson.Serialize(document), html);

        if (result.Succeeded)
        {
            savedAt = $"{T[InfernoCmsLocalizableStrings.Pages.Saved].Value} {DateTime.Now:HH:mm:ss}";
        }
        else
        {
            Notifications.Notify(NotificationSeverity.Error, "Error", T[result.ErrorKey].Value);
        }
    }

    private async Task SetPublishedAsync(bool publish)
    {
        publishing = true;

        try
        {
            var result = await Authoring.SetPublishedAsync(VersionId, publish);
            if (result.Succeeded)
            {
                published = publish;
            }
            else
            {
                Notifications.Notify(NotificationSeverity.Error, "Error", T[result.ErrorKey].Value);
            }
        }
        finally
        {
            publishing = false;
        }
    }

    private void GoBack() =>
        NavigationManager.NavigateTo(pageId == Guid.Empty ? "/admin/pages/index" : $"/admin/pages/edit/{pageId}");
}
