using System.Text.Json;
using Inferno.Localization.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Pages;

public partial class Versions
{
    private const string StandardPageName = "Standard Page";

    private static readonly JsonSerializerOptions FieldsJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Parameter]
    public Guid PageId { get; set; }

    [Inject]
    private IPageService PageService { get; set; }

    [Inject]
    private IPageTypeService PageTypeService { get; set; }

    [Inject]
    private ILanguageService LanguageService { get; set; }

    [Inject]
    private IPageVersionODataService PageVersionODataService { get; set; }

    [Inject]
    private IWorkContext WorkContext { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    private string PageName { get; set; }

    private bool IsStandardPage { get; set; }

    private StandardPageFields Fields { get; set; } = new();

    private IEnumerable<CultureOption> CultureSelectList { get; set; } = Enumerable.Empty<CultureOption>();

    private IEnumerable<VersionStatus> StatusSelectList { get; } = Enum.GetValues<VersionStatus>();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        LoadPageContext();
        LoadCultures();
    }

    protected override string GetODataFilter(LoadDataArgs args)
    {
        string pageFilter = $"PageId eq {PageId}";
        return string.IsNullOrWhiteSpace(args.Filter)
            ? pageFilter
            : $"({args.Filter}) and {pageFilter}";
    }

    protected override void Create()
    {
        Fields = new StandardPageFields();
        Model = new PageVersion
        {
            PageId = PageId,
            Status = VersionStatus.Draft,
            DateCreatedUtc = DateTime.UtcNow,
            DateModifiedUtc = DateTime.UtcNow,
            Title = string.Empty,
            Slug = string.Empty
        };
        ShowEditMode = true;
    }

    protected override async Task EditAsync(Guid id)
    {
        await base.EditAsync(id);

        if (ShowEditMode && IsStandardPage)
        {
            Fields = ParseStandardPageFields(Model.Fields);
        }
    }

    protected override async Task OnValidSumbitAsync()
    {
        if (IsStandardPage)
        {
            Model.Fields = JsonSerializer.Serialize(Fields, FieldsJsonOptions);
        }

        Model.DateModifiedUtc = DateTime.UtcNow;
        await base.OnValidSumbitAsync();
    }

    protected override void Cancel()
    {
        Fields = new StandardPageFields();
        base.Cancel();
    }

    private async Task RestoreAsync(PageVersion record)
    {
        try
        {
            if (await DialogService.Confirm(T[InfernoCmsLocalizableStrings.Pages.PageHistoryRestoreConfirm].Value) != true)
            {
                return;
            }

            var response = await PageVersionODataService.RestoreVersionAsync(record.Id);
            if (response.Succeeded)
            {
                await DataGrid.Reload();
                NotificationService.Notify(NotificationSeverity.Info, "Info", T[InfernoCmsLocalizableStrings.Pages.PageHistoryRestoreSuccess].Value);
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", T[InfernoCmsLocalizableStrings.Pages.PageHistoryRestoreError].Value);
            }
        }
        catch
        {
            NotificationService.Notify(NotificationSeverity.Error, "Error", T[InfernoCmsLocalizableStrings.Pages.PageHistoryRestoreError].Value);
        }
    }

    private void GoBack() => NavigationManager.NavigateTo("/admin/pages/index");

    private void LoadPageContext()
    {
        using var pageConnection = PageService.OpenConnection();
        var page = pageConnection.Query(x => x.Id == PageId).FirstOrDefault();

        if (page == null)
        {
            PageName = string.Empty;
            IsStandardPage = false;
            return;
        }

        PageName = page.Name;

        using var pageTypeConnection = PageTypeService.OpenConnection();
        var pageType = pageTypeConnection.Query(x => x.Id == page.PageTypeId).FirstOrDefault();
        IsStandardPage = string.Equals(pageType?.Name, StandardPageName, StringComparison.Ordinal);
    }

    private void LoadCultures()
    {
        int tenantId = WorkContext.CurrentTenant?.Id ?? 0;
        var languages = LanguageService
            .GetActiveLanguages(tenantId)
            .OrderBy(x => x.Name)
            .Select(x => new CultureOption { Value = x.CultureCode, Name = $"{x.Name} ({x.CultureCode})" })
            .ToList();

        languages.Insert(0, new CultureOption { Value = null, Name = "(invariant)" });

        CultureSelectList = languages;
    }

    private static StandardPageFields ParseStandardPageFields(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new StandardPageFields();
        }

        try
        {
            return JsonSerializer.Deserialize<StandardPageFields>(json, FieldsJsonOptions)
                ?? new StandardPageFields();
        }
        catch (JsonException)
        {
            return new StandardPageFields();
        }
    }

    private sealed class CultureOption
    {
        public string Value { get; set; }
        public string Name { get; set; }
    }

    private sealed class StandardPageFields
    {
        public string MetaTitle { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string BodyContent { get; set; }
    }
}
