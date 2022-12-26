namespace Inferno.Web.ContentManagement
{
    public static class InfernoCmsLocalizableStrings
    {
        public static class Blog
        {
            public const string Categories = "Inferno.Web.ContentManagement/Blog.Categories";
            public const string ManageBlog = "Inferno.Web.ContentManagement/Blog.ManageBlog";
            public const string PostedByXOnX = "Inferno.Web.ContentManagement/Blog.PostedByXOnX";
            public const string Posts = "Inferno.Web.ContentManagement/Blog.Posts";
            public const string Tags = "Inferno.Web.ContentManagement/Blog.Tags";
            public const string Title = "Inferno.Web.ContentManagement/Blog.Title";

            public static class CategoryModel
            {
                public const string Name = "Inferno.Web.ContentManagement/Blog.CategoryModel.Name";
                public const string UrlSlug = "Inferno.Web.ContentManagement/Blog.CategoryModel.UrlSlug";
            }

            public static class PostModel
            {
                public const string CategoryId = "Inferno.Web.ContentManagement/Blog.PostModel.CategoryId";
                public const string DateCreatedUtc = "Inferno.Web.ContentManagement/Blog.PostModel.DateCreatedUtc";
                public const string ExternalLink = "Inferno.Web.ContentManagement/Blog.PostModel.ExternalLink";
                public const string FullDescription = "Inferno.Web.ContentManagement/Blog.PostModel.FullDescription";
                public const string Headline = "Inferno.Web.ContentManagement/Blog.PostModel.Headline";
                public const string MetaDescription = "Inferno.Web.ContentManagement/Blog.PostModel.MetaDescription";
                public const string MetaKeywords = "Inferno.Web.ContentManagement/Blog.PostModel.MetaKeywords";
                public const string ShortDescription = "Inferno.Web.ContentManagement/Blog.PostModel.ShortDescription";
                public const string Slug = "Inferno.Web.ContentManagement/Blog.PostModel.Slug";
                public const string TeaserImageUrl = "Inferno.Web.ContentManagement/Blog.PostModel.TeaserImageUrl";
                public const string UseExternalLink = "Inferno.Web.ContentManagement/Blog.PostModel.UseExternalLink";
            }

            public static class TagModel
            {
                public const string Name = "Inferno.Web.ContentManagement/Blog.TagModel.Name";
                public const string UrlSlug = "Inferno.Web.ContentManagement/Blog.TagModel.UrlSlug";
            }
        }

        public static class ContentBlocks
        {
            public const string ManageContentBlocks = "Inferno.Web.ContentManagement/ContentBlocks.ManageContentBlocks";
            public const string ManageZones = "Inferno.Web.ContentManagement/ContentBlocks.ManageZones";
            public const string Title = "Inferno.Web.ContentManagement/ContentBlocks.Title";
            public const string Zones = "Inferno.Web.ContentManagement/ContentBlocks.Zones";

            #region Blog

            public static class AllPostsBlock
            {
                public const string CategoryId = "Inferno.Web.ContentManagement/ContentBlocks.AllPostsBlock.CategoryId";
                public const string FilterType = "Inferno.Web.ContentManagement/ContentBlocks.AllPostsBlock.FilterType";
                public const string TagId = "Inferno.Web.ContentManagement/ContentBlocks.AllPostsBlock.TagId";
            }

            public static class CategoriesBlock
            {
                public const string NumberOfCategories = "Inferno.Web.ContentManagement/ContentBlocks.CategoriesBlock.NumberOfCategories";
            }

            public static class LastNPostsBlock
            {
                public const string NumberOfEntries = "Inferno.Web.ContentManagement/ContentBlocks.LastNPostsBlock.NumberOfEntries";
            }

            public static class TagCloudBlock
            {
                //public const string Width = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.Width";
                //public const string WidthUnit = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.WidthUnit";
                //public const string Height = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.Height";
                //public const string HeightUnit = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.HeightUnit";
                //public const string CenterX = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.CenterX";
                //public const string CenterY = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.CenterY";
                public const string AutoResize = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.AutoResize";

                public const string Steps = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.Steps";
                public const string ClassPattern = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.ClassPattern";
                public const string AfterCloudRender = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.AfterCloudRender";
                public const string Delay = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.Delay";
                public const string Shape = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.Shape";
                public const string RemoveOverflowing = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.RemoveOverflowing";
                public const string EncodeURI = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.EncodeURI";
                public const string Colors = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.Colors";
                public const string FontSizeFrom = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.FontSizeFrom";
                public const string FontSizeTo = "Inferno.Web.ContentManagement/ContentBlocks.TagCloudBlock.FontSizeTo";
            }

            #endregion Blog

            public static class FormBlock
            {
                public static class HelpText
                {
                    public const string EmailAddress = "Inferno.Web.ContentManagement/ContentBlocks.FormBlock.HelpText.EmailAddress";
                    public const string FormUrl = "Inferno.Web.ContentManagement/ContentBlocks.FormBlock.HelpText.FormUrl";
                }

                public const string EmailAddress = "Inferno.Web.ContentManagement/ContentBlocks.FormBlock.EmailAddress";
                public const string FormUrl = "Inferno.Web.ContentManagement/ContentBlocks.FormBlock.FormUrl";
                public const string HtmlTemplate = "Inferno.Web.ContentManagement/ContentBlocks.FormBlock.HtmlTemplate";
                public const string PleaseEnterCaptcha = "Inferno.Web.ContentManagement/ContentBlocks.FormBlock.PleaseEnterCaptcha";
                public const string PleaseEnterCorrectCaptcha = "Inferno.Web.ContentManagement/ContentBlocks.FormBlock.PleaseEnterCorrectCaptcha";
                public const string RedirectUrl = "Inferno.Web.ContentManagement/ContentBlocks.FormBlock.RedirectUrl";
                public const string SaveResultIfNotRedirectPleaseClick = "Inferno.Web.ContentManagement/ContentBlocks.FormBlock.SaveResultIfNotRedirectPleaseClick";
                public const string SaveResultRedirect = "Inferno.Web.ContentManagement/ContentBlocks.FormBlock.SaveResultRedirect";
                public const string ThankYouMessage = "Inferno.Web.ContentManagement/ContentBlocks.FormBlock.ThankYouMessage";
                public const string UseAjax = "Inferno.Web.ContentManagement/ContentBlocks.FormBlock.UseAjax";
            }

            public static class HtmlBlock
            {
                public static class HelpText
                {
                    public const string BodyContent = "Inferno.Web.ContentManagement/ContentBlocks.HtmlBlock.HelpText.BodyContent";
                    public const string Script = "Inferno.Web.ContentManagement/ContentBlocks.HtmlBlock.HelpText.Script";
                }

                public const string BodyContent = "Inferno.Web.ContentManagement/ContentBlocks.HtmlBlock.BodyContent";
                public const string Script = "Inferno.Web.ContentManagement/ContentBlocks.HtmlBlock.Script";
            }

            public static class LanguageSwitchBlock
            {
                public const string CustomTemplatePath = "Inferno.Web.ContentManagement/ContentBlocks.LanguageSwitchBlock.CustomTemplatePath";
                public const string IncludeInvariant = "Inferno.Web.ContentManagement/ContentBlocks.LanguageSwitchBlock.IncludeInvariant";
                public const string InvariantText = "Inferno.Web.ContentManagement/ContentBlocks.LanguageSwitchBlock.InvariantText";
                public const string Style = "Inferno.Web.ContentManagement/ContentBlocks.LanguageSwitchBlock.Style";
                public const string UseUrlPrefix = "Inferno.Web.ContentManagement/ContentBlocks.LanguageSwitchBlock.UseUrlPrefix";
            }

            public static class Model
            {
                public const string BlockType = "Inferno.Web.ContentManagement/ContentBlocks.Model.BlockType";
                public const string CustomTemplatePath = "Inferno.Web.ContentManagement/ContentBlocks.Model.CustomTemplatePath";
                public const string IsEnabled = "Inferno.Web.ContentManagement/ContentBlocks.Model.IsEnabled";
                public const string Order = "Inferno.Web.ContentManagement/ContentBlocks.Model.Order";
                public const string Title = "Inferno.Web.ContentManagement/ContentBlocks.Model.Title";
                public const string ZoneId = "Inferno.Web.ContentManagement/ContentBlocks.Model.ZoneId";
            }

            public static class NewsletterSubscriptionBlock
            {
                public const string Email = "Inferno.Web.ContentManagement/ContentBlocks.NewsletterSubscriptionBlock.Email";
                public const string EmailPlaceholder = "Inferno.Web.ContentManagement/ContentBlocks.NewsletterSubscriptionBlock.EmailPlaceholder";
                public const string Name = "Inferno.Web.ContentManagement/ContentBlocks.NewsletterSubscriptionBlock.Name";
                public const string NamePlaceholder = "Inferno.Web.ContentManagement/ContentBlocks.NewsletterSubscriptionBlock.NamePlaceholder";
                public const string SignUpLabel = "Inferno.Web.ContentManagement/ContentBlocks.NewsletterSubscriptionBlock.SignUpLabel";
            }

            public static class VideoBlock
            {
                public const string ControlId = "Inferno.Web.ContentManagement/ContentBlocks.VideoBlock.ControlId";
                public const string Type = "Inferno.Web.ContentManagement/ContentBlocks.VideoBlock.Type";
                public const string Source = "Inferno.Web.ContentManagement/ContentBlocks.VideoBlock.Source";
                public const string ShowControls = "Inferno.Web.ContentManagement/ContentBlocks.VideoBlock.ShowControls";
                public const string AutoPlay = "Inferno.Web.ContentManagement/ContentBlocks.VideoBlock.AutoPlay";
                public const string Loop = "Inferno.Web.ContentManagement/ContentBlocks.VideoBlock.Loop";
                public const string VideoTagNotSupported = "Inferno.Web.ContentManagement/ContentBlocks.VideoBlock.VideoTagNotSupported";
            }

            public static class ZoneModel
            {
                public const string Name = "Inferno.Web.ContentManagement/ContentBlocks.ZoneModel.Name";
            }
        }

        public static class Localization
        {
            public const string IsRTL = "Inferno.Web.ContentManagement/Localization.IsRTL";
            public const string Languages = "Inferno.Web.ContentManagement/Localization.Languages";
            public const string LocalizableStrings = "Inferno.Web.ContentManagement/Localization.LocalizableStrings";
            public const string Localize = "Inferno.Web.ContentManagement/Localization.Localize";
            public const string ManageLanguages = "Inferno.Web.ContentManagement/Localization.ManageLanguages";
            public const string ManageLocalizableStrings = "Inferno.Web.ContentManagement/Localization.ManageLocalizableStrings";
            public const string SelectLanguage = "Inferno.Web.ContentManagement/Localization.SelectLanguage";
            public const string Title = "Inferno.Web.ContentManagement/Localization.Title";
            public const string Translate = "Inferno.Web.ContentManagement/Localization.Translate";
            public const string Translations = "Inferno.Web.ContentManagement/Localization.Translations";

            public static class LanguageModel
            {
                public const string CultureCode = "Inferno.Web.ContentManagement/Localization.LanguageModel.CultureCode";
                public const string IsEnabled = "Inferno.Web.ContentManagement/Localization.LanguageModel.IsEnabled";
                public const string IsRTL = "Inferno.Web.ContentManagement/Localization.LanguageModel.IsRTL";
                public const string Name = "Inferno.Web.ContentManagement/Localization.LanguageModel.Name";
                public const string SortOrder = "Inferno.Web.ContentManagement/Localization.LanguageModel.SortOrder";
            }

            public static class LocalizableStringModel
            {
                public const string InvariantValue = "Inferno.Web.ContentManagement/Localization.LocalizableStringModel.InvariantValue";
                public const string Key = "Inferno.Web.ContentManagement/Localization.LocalizableStringModel.Key";
                public const string LocalizedValue = "Inferno.Web.ContentManagement/Localization.LocalizableStringModel.LocalizedValue";
            }
        }

        public static class Media
        {
            public const string FileBrowserTitle = "Inferno.Web.ContentManagement/Media.FileBrowserTitle";
            public const string ManageMedia = "Inferno.Web.ContentManagement/Media.ManageMedia";
            public const string Title = "Inferno.Web.ContentManagement/Media.Title";
        }

        public static class Menus
        {
            public const string IsExternalUrl = "Inferno.Web.ContentManagement/Menus.IsExternalUrl";
            public const string Items = "Inferno.Web.ContentManagement/Menus.Items";
            public const string ManageMenuItems = "Inferno.Web.ContentManagement/Menus.ManageMenuItems";
            public const string ManageMenus = "Inferno.Web.ContentManagement/Menus.ManageMenus";
            public const string MenuItems = "Inferno.Web.ContentManagement/Menus.MenuItems";
            public const string NewItem = "Inferno.Web.ContentManagement/Menus.NewItem";
            public const string Title = "Inferno.Web.ContentManagement/Menus.Title";

            public static class MenuItemModel
            {
                public const string CssClass = "Inferno.Web.ContentManagement/Menus.MenuItemModel.CssClass";
                public const string Description = "Inferno.Web.ContentManagement/Menus.MenuItemModel.Description";
                public const string Enabled = "Inferno.Web.ContentManagement/Menus.MenuItemModel.Enabled";
                public const string Position = "Inferno.Web.ContentManagement/Menus.MenuItemModel.Position";
                public const string Text = "Inferno.Web.ContentManagement/Menus.MenuItemModel.Text";
                public const string Url = "Inferno.Web.ContentManagement/Menus.MenuItemModel.Url";
            }

            public static class MenuModel
            {
                public const string Name = "Inferno.Web.ContentManagement/Menus.MenuModel.Name";
                public const string UrlFilter = "Inferno.Web.ContentManagement/Menus.MenuModel.UrlFilter";
            }
        }

        public static class Messages
        {
            public const string CircularRelationshipError = "Inferno.Web.ContentManagement/Messages.CircularRelationshipError";
            public const string ConfirmClearLocalizableStrings = "Inferno.Web.ContentManagement/Messages.ConfirmClearLocalizableStrings";
            public const string GetTranslationError = "Inferno.Web.ContentManagement/Messages.GetTranslationError";
            public const string UpdateTranslationError = "Inferno.Web.ContentManagement/Messages.UpdateTranslationError";
            public const string UpdateTranslationSuccess = "Inferno.Web.ContentManagement/Messages.UpdateTranslationSuccess";
        }

        public static class Navigation
        {
            public const string CMS = "Inferno.Web.ContentManagement/Navigation.CMS";
        }

        public static class Newsletters
        {
            public const string ExportToCSV = "Inferno.Web.ContentManagement/Newsletters.ExportToCSV";
            public const string Subscribers = "Inferno.Web.ContentManagement/Newsletters.Subscribers";
            public const string SuccessfullySignedUp = "Inferno.Web.ContentManagement/Newsletters.SuccessfullySignedUp";
            public const string Title = "Inferno.Web.ContentManagement/Newsletters.Title";
        }

        public static class Pages
        {
            public const string ConfirmRestoreVersion = "Inferno.Web.ContentManagement/Pages.ConfirmRestoreVersion";
            public const string History = "Inferno.Web.ContentManagement/Pages.History";
            public const string ManagePages = "Inferno.Web.ContentManagement/Pages.ManagePages";
            public const string Page = "Inferno.Web.ContentManagement/Pages.Page";
            public const string PageHistory = "Inferno.Web.ContentManagement/Pages.PageHistory";
            public const string PageHistoryRestoreConfirm = "Inferno.Web.ContentManagement/Pages.PageHistoryRestoreConfirm";
            public const string PageHistoryRestoreError = "Inferno.Web.ContentManagement/Pages.PageHistoryRestoreError";
            public const string PageHistoryRestoreSuccess = "Inferno.Web.ContentManagement/Pages.PageHistoryRestoreSuccess";
            public const string Restore = "Inferno.Web.ContentManagement/Pages.Restore";
            public const string RestoreVersion = "Inferno.Web.ContentManagement/Pages.RestoreVersion";
            public const string SelectPageToBeginEdit = "Inferno.Web.ContentManagement/Pages.SelectPageToBeginEdit";
            public const string Tags = "Inferno.Web.ContentManagement/Pages.Tags";
            public const string Title = "Inferno.Web.ContentManagement/Pages.Title";
            public const string Translations = "Inferno.Web.ContentManagement/Pages.Translations";
            public const string Versions = "Inferno.Web.ContentManagement/Pages.Versions";

            public const string CannotDeleteOnlyVersion = "Inferno.Web.ContentManagement/Pages.CannotDeleteOnlyVersion";

            public static class PageModel
            {
                public const string IsEnabled = "Inferno.Web.ContentManagement/Pages.PageModel.IsEnabled";
                public const string Name = "Inferno.Web.ContentManagement/Pages.PageModel.Name";
                public const string Order = "Inferno.Web.ContentManagement/Pages.PageModel.Order";
                public const string PageTypeId = "Inferno.Web.ContentManagement/Pages.PageModel.PageTypeId";
                public const string Roles = "Inferno.Web.ContentManagement/Pages.PageModel.Roles";
                public const string ShowOnMenus = "Inferno.Web.ContentManagement/Pages.PageModel.ShowOnMenus";
            }

            public static class PageVersionModel
            {
                public const string CultureCode = "Inferno.Web.ContentManagement/Pages.PageVersionModel.CultureCode";
                public const string DateCreated = "Inferno.Web.ContentManagement/Pages.PageVersionModel.DateCreated";
                public const string DateModified = "Inferno.Web.ContentManagement/Pages.PageVersionModel.DateModified";
                public const string IsDraft = "Inferno.Web.ContentManagement/Pages.PageVersionModel.IsDraft";
                public const string Slug = "Inferno.Web.ContentManagement/Pages.PageVersionModel.Slug";
                public const string Status = "Inferno.Web.ContentManagement/Pages.PageVersionModel.Status";
                public const string Title = "Inferno.Web.ContentManagement/Pages.PageVersionModel.Title";
            }

            public static class PageTypeModel
            {
                public const string DisplayTemplatePath = "Inferno.Web.ContentManagement/Pages.PageTypeModel.DisplayTemplatePath";
                public const string EditorTemplatePath = "Inferno.Web.ContentManagement/Pages.PageTypeModel.EditorTemplatePath";
                public const string LayoutPath = "Inferno.Web.ContentManagement/Pages.PageTypeModel.LayoutPath";
                public const string Name = "Inferno.Web.ContentManagement/Pages.PageTypeModel.Name";
            }

            public static class PageTypes
            {
                public const string Title = "Inferno.Web.ContentManagement/Pages.PageTypes.Title";

                public static class StandardPage
                {
                    public const string BodyContent = "Inferno.Web.ContentManagement/Pages.PageTypes.StandardPage.BodyContent";
                    public const string MetaDescription = "Inferno.Web.ContentManagement/Pages.PageTypes.StandardPage.MetaDescription";
                    public const string MetaKeywords = "Inferno.Web.ContentManagement/Pages.PageTypes.StandardPage.MetaKeywords";
                    public const string MetaTitle = "Inferno.Web.ContentManagement/Pages.PageTypes.StandardPage.MetaTitle";
                }
            }
        }

        public static class Settings
        {
            public static class Blog
            {
                public const string AccessRestrictions = "Inferno.Web.ContentManagement/Settings.Blog.AccessRestrictions";
                public const string DateFormat = "Inferno.Web.ContentManagement/Settings.Blog.DateFormat";
                public const string ItemsPerPage = "Inferno.Web.ContentManagement/Settings.Blog.ItemsPerPage";
                public const string MenuPosition = "Inferno.Web.ContentManagement/Settings.Blog.MenuPosition";
                public const string PageTitle = "Inferno.Web.ContentManagement/Settings.Blog.PageTitle";
                public const string ShowOnMenus = "Inferno.Web.ContentManagement/Settings.Blog.ShowOnMenus";
                public const string LayoutPathOverride = "Inferno.Web.ContentManagement/Settings.Blog.LayoutPathOverride";
            }

            public static class Pages
            {
                public const string NumberOfPageVersionsToKeep = "Inferno.Web.ContentManagement/Settings.Pages.NumberOfPageVersionsToKeep";
            }
        }

        public static class Sitemap
        {
            public static class Model
            {
                public static class ChangeFrequencies
                {
                    public const string Always = "Inferno.Web.ContentManagement/Sitemap.Model.ChangeFrequencies.Always";
                    public const string Hourly = "Inferno.Web.ContentManagement/Sitemap.Model.ChangeFrequencies.Hourly";
                    public const string Daily = "Inferno.Web.ContentManagement/Sitemap.Model.ChangeFrequencies.Daily";
                    public const string Weekly = "Inferno.Web.ContentManagement/Sitemap.Model.ChangeFrequencies.Weekly";
                    public const string Monthly = "Inferno.Web.ContentManagement/Sitemap.Model.ChangeFrequencies.Monthly";
                    public const string Yearly = "Inferno.Web.ContentManagement/Sitemap.Model.ChangeFrequencies.Yearly";
                    public const string Never = "Inferno.Web.ContentManagement/Sitemap.Model.ChangeFrequencies.Never";
                }

                public const string ChangeFrequency = "Inferno.Web.ContentManagement/SitemapModel.ChangeFrequency";
                public const string Id = "Inferno.Web.ContentManagement/SitemapModel.Id";
                public const string Location = "Inferno.Web.ContentManagement/SitemapModel.Location";
                public const string Priority = "Inferno.Web.ContentManagement/SitemapModel.Priority";
            }

            public const string ConfirmGenerateFile = "Inferno.Web.ContentManagement/Sitemap.ConfirmGenerateFile";
            public const string GenerateFile = "Inferno.Web.ContentManagement/Sitemap.GenerateFile";
            public const string GenerateFileError = "Inferno.Web.ContentManagement/Sitemap.GenerateFileError";
            public const string GenerateFileSuccess = "Inferno.Web.ContentManagement/Sitemap.GenerateFileSuccess";
            public const string Title = "Inferno.Web.ContentManagement/Sitemap.Title";
            public const string XMLSitemap = "Inferno.Web.ContentManagement/Sitemap.XMLSitemap";
        }

        public static class UserProfile
        {
            public static class Newsletter
            {
                public const string SubscribeToNewsletters = "Inferno.Web.ContentManagement/UserProfile.Newsletter.SubscribeToNewsletters";
            }
        }
    }
}