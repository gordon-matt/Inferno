using Inferno.Web.ContentManagement.Areas.Admin.Blog.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.Pages
{
    public partial class Posts
    {
        [Inject]
        private IBlogCategoryService BlogCategoryService { get; set; }

        [Inject]
        private IBlogTagService BlogTagService { get; set; }

        [Inject]
        private IBlogPostTagService BlogPostTagService { get; set; }

        private IEnumerable<BlogCategory> CategoriesSelectList { get; set; } = Enumerable.Empty<BlogCategory>();

        private IEnumerable<BlogTag> TagsSelectList { get; set; } = Enumerable.Empty<BlogTag>();

        private IEnumerable<int> SelectedTagIds { get; set; } = Enumerable.Empty<int>();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            LoadCategories();
            LoadTags();
        }

        protected override void Create()
        {
            Model = new BlogPost
            {
                CategoryId = CategoriesSelectList.FirstOrDefault()?.Id ?? 0,
                DateCreatedUtc = DateTime.UtcNow,
                Headline = string.Empty,
                Slug = string.Empty,
                ShortDescription = string.Empty
            };
            SelectedTagIds = Enumerable.Empty<int>();
            Model.Tags = new HashSet<BlogPostTag>();
            ShowEditMode = true;
        }

        protected override async Task EditAsync(Guid id)
        {
            await base.EditAsync(id);
            if (ShowEditMode)
            {
                using var connection = BlogPostTagService.OpenConnection();
                SelectedTagIds = connection
                    .Query(x => x.PostId == id)
                    .Select(x => x.TagId)
                    .ToList();
            }
        }

        protected override async Task OnValidSumbitAsync()
        {
            Model.Tags = SelectedTagIds
                .Select(tagId => new BlogPostTag { PostId = Model.Id, TagId = tagId })
                .ToList();

            await base.OnValidSumbitAsync();
        }

        protected override void Cancel()
        {
            SelectedTagIds = Enumerable.Empty<int>();
            base.Cancel();
        }

        private void LoadCategories()
        {
            using var connection = BlogCategoryService.OpenConnection();
            CategoriesSelectList = connection.Query()
                .OrderBy(x => x.Name)
                .ToList();
        }

        private void LoadTags()
        {
            using var connection = BlogTagService.OpenConnection();
            TagsSelectList = connection.Query()
                .OrderBy(x => x.Name)
                .ToList();
        }

        private string GetCategoryName(int categoryId) =>
            CategoriesSelectList.FirstOrDefault(x => x.Id == categoryId)?.Name ?? string.Empty;
    }
}
