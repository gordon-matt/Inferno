using System.Globalization;
using Blazorise;
using Extenso;
using Inferno.Localization.Entities;
using Inferno.Localization.Services;
using Inferno.Web.Areas.Admin.Localization.Models;
using Inferno.Web.Areas.Admin.Localization.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace Inferno.Web.Areas.Admin.Localization.Pages
{
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

        private async Task FileOnChangedAsync(FileChangedEventArgs e)
        {
            try
            {
                foreach (var file in e.Files) // TODO: There should only be one..
                {
                    using var stream = new MemoryStream();
                    await file.WriteToStreamAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);

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
                        .FindAsync(x => x.TenantId == tenantId && x.CultureCode == null))
                        .Select(x => x.TextKey);

                    var toInsert = localizedStrings.Where(x => allInvariantStrings.Contains(x.TextKey));

                    await LocalizableStringService.InsertAsync(toInsert);

                    #endregion Update Database
                }
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

        private void FileOnProgressed(FileProgressedEventArgs e)
        {
            Console.WriteLine($"File: {e.File.Name} Progress: {e.Percentage}");
        }

        public void Localize(string cultureCode)
        {
            NavigationManager.NavigateTo($"/admin/localization/localizable-strings/{cultureCode}", forceLoad: true);
        }
    }
}