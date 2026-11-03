using BlogKrafter.Models;
using BlogKrafter.Taxonomy;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.Components;

public partial class FilteredPostsBlockEditor
{
    private IReadOnlyList<CategoryDto> categories = [];
    private IReadOnlyList<TagDto> tags = [];

    [Inject]
    private IBlogCategoryService CategoryService { get; set; }

    [Inject]
    private IBlogTagService TagService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var categoryResult = await CategoryService.ListAsync();
        categories = categoryResult.IsSuccess ? categoryResult.Value : [];

        var tagResult = await TagService.ListAsync();
        tags = tagResult.IsSuccess ? tagResult.Value : [];
    }
}
