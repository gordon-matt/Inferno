using System.Globalization;
using Extenso;
using Extenso.Data.Entity;
using Inferno.Localization.Entities;
using Inferno.Localization.Services;
using Inferno.Web.Areas.Admin.Localization.Models;
using Inferno.Web.Areas.Admin.Localization.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace Inferno.Web.Areas.Admin.Localization.Pages;

public partial class Index
{
    protected bool ShowUploadSection { get; set; }

    [Inject]
    public ILanguageService LanguageService { get; set; }

    [Inject]
    public ILocalizableStringService LocalizableStringService { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject]
    public IWorkContext WorkContext { get; set; }

    public async Task ClearAsync()
    {
        bool success = await (ODataService as LanguageODataService).ResetLocalizableStringsAsync();
        if (success)
        {
            NotificationService.Notify(NotificationSeverity.Info, "Info", T[InfernoWebLocalizableStrings.Localization.ResetLocalizableStringsSuccess]);
        }
        else
        {
            NotificationService.Notify(NotificationSeverity.Error, "Error", T[InfernoWebLocalizableStrings.Localization.ResetLocalizableStringsError]);
        }
    }

    public void ImportLanguagePack() => ShowUploadSection = true;

    protected override void Cancel()
    {
        base.Cancel();
        ShowUploadSection = false;
    }

    private async Task OnLanguagePackSelected(InputFileChangeEventArgs e)
    {
        try
        {
            var file = e.File;
            await using var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
            using var reader = new StreamReader(stream);
            string json = await reader.ReadToEndAsync();
            var languagePackFile = json.JsonDeserialize<LanguagePackFile>();

                #region Update Database

                if (string.IsNullOrEmpty(languagePackFile.CultureCode))
                {
                    //return Json(new { Success = false, error = "Cannot import language pack for the invariant culture. Please provide a culture code." });
                }

                bool cultureExistsInDb = false;
                using (var connection = LanguageService.OpenConnection())
                {
                    cultureExistsInDb = await connection.Query(x => x.CultureCode == languagePackFile.CultureCode).AnyAsync();
                }

                int tenantId = WorkContext.CurrentTenant.Id;
                if (!cultureExistsInDb)
                {
                    try
                    {
                        var culture = new CultureInfo(languagePackFile.CultureCode);
                        await LanguageService.InsertAsync(new Language
                        {
                            Id = Guid.NewGuid(),
                            TenantId = tenantId,
                            CultureCode = languagePackFile.CultureCode,
                            Name = culture.DisplayName
                        });
                    }
                    catch (CultureNotFoundException)
                    {
                        //return Json(new { Success = false, error = "The specified culture code is not recognized." });
                    }
                }

                var localizedStrings = languagePackFile.LocalizedStrings.Select(x => new LocalizableString
                {
                    TenantId = tenantId,
                    CultureCode = languagePackFile.CultureCode,
                    TextKey = x.Key,
                    TextValue = x.Value
                });

                // Ignore strings that don't have an invariant version
                var allInvariantStrings = (await LocalizableStringService
                    .FindAsync(new SearchOptions<LocalizableString>
                    {
                        Query = x => x.TenantId == tenantId && x.CultureCode == null
                    }))
                    .Select(x => x.TextKey);

                var toInsert = localizedStrings.Where(x => allInvariantStrings.Contains(x.TextKey));

                await LocalizableStringService.InsertAsync(toInsert);

                #endregion Update Database
        }
        catch (Exception x)
        {
            Console.WriteLine(x.Message);
        }
        finally
        {
            StateHasChanged();
        }
    }

    public void Localize(string cultureCode) => NavigationManager.NavigateTo($"/admin/localization/localizable-strings/{cultureCode}", forceLoad: true);
}