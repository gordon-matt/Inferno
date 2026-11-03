using Ardalis.Result;
using Heroicsoft.NetForge.Components.Tables;
using Heroicsoft.NetForge.Results;
using Inferno.Web.Components;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Menus.Pages;

public partial class Items
{
    private sealed record ParentOption(Guid Id, string Text, int Depth);

    private Menu menu;
    private IReadOnlyList<MenuItem> items;
    private List<MenuItem> roots = [];
    private ILookup<Guid, MenuItem> childrenByParent;
    private TreeTable<MenuItem> tree;
    private bool canWrite;

    private MenuItem editing;
    private IReadOnlyDictionary<string, string> errors;
    private bool saving;

    [Parameter]
    public Guid MenuId { get; set; }

    [Inject]
    private IMenuAdminService MenuAdminService { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        canWrite = await HasPolicyAsync(CmsConstants.Policies.MenusWrite);

        var menuResult = await MenuAdminService.GetMenuAsync(TenantId, MenuId);
        if (!menuResult.IsSuccess)
        {
            Toasts.ShowFailure(menuResult);
            menu = null;
            SetItems([]);
            return;
        }

        menu = menuResult.Value;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var result = await MenuAdminService.ListItemsAsync(TenantId, MenuId);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            SetItems([]);
            return;
        }

        SetItems(result.Value);
    }

    private void SetItems(IReadOnlyList<MenuItem> value)
    {
        items = value;
        var ids = value.Select(item => item.Id).ToHashSet();

        // Items whose parent no longer exists are shown at the top level rather than hidden.
        roots = value.Where(item => item.ParentId is null || !ids.Contains(item.ParentId.Value)).ToList();
        childrenByParent = value.Where(item => item.ParentId.HasValue).ToLookup(item => item.ParentId.Value);
    }

    private IEnumerable<MenuItem> ChildrenOf(MenuItem item) => childrenByParent[item.Id];

    private IEnumerable<ParentOption> ParentOptions()
    {
        HashSet<Guid> excluded = editing.Id == Guid.Empty ? [] : Descendants(editing.Id).Append(editing.Id).ToHashSet();
        return Flatten(roots, 0).Where(option => !excluded.Contains(option.Id));
    }

    private IEnumerable<ParentOption> Flatten(IEnumerable<MenuItem> level, int depth, HashSet<Guid> seen = null)
    {
        seen ??= [];
        foreach (var item in level)
        {
            if (!seen.Add(item.Id))
            {
                continue;
            }

            yield return new ParentOption(item.Id, item.Text, depth);
            foreach (var child in Flatten(childrenByParent[item.Id], depth + 1, seen))
            {
                yield return child;
            }
        }
    }

    private IEnumerable<Guid> Descendants(Guid id)
    {
        var found = new HashSet<Guid>();
        var pending = new Stack<Guid>([id]);
        while (pending.Count > 0)
        {
            foreach (var child in childrenByParent[pending.Pop()])
            {
                if (found.Add(child.Id))
                {
                    pending.Push(child.Id);
                }
            }
        }

        return found;
    }

    private void ParentChanged(ChangeEventArgs args) =>
        editing.ParentId = Guid.TryParse(args.Value?.ToString(), out var parentId) ? parentId : null;

    private void Create(Guid? parentId)
    {
        int nextPosition = items.Where(item => item.ParentId == parentId).Select(item => item.Position + 1).DefaultIfEmpty(0).Max();
        Open(new MenuItem { MenuId = MenuId, ParentId = parentId, Enabled = true, Position = nextPosition });
    }

    // Edit a copy, so cancelling leaves the cached item as it was.
    private void Edit(MenuItem item) => Open(new MenuItem
    {
        Id = item.Id,
        MenuId = item.MenuId,
        Text = item.Text,
        Url = item.Url,
        Description = item.Description,
        CssClass = item.CssClass,
        Position = item.Position,
        ParentId = item.ParentId,
        Enabled = item.Enabled,
        IsExternalUrl = item.IsExternalUrl,
        RefId = item.RefId
    });

    private void Open(MenuItem item)
    {
        errors = null;
        editing = item;
    }

    private void Close() => editing = null;

    private async Task SaveAsync()
    {
        if (saving || !await EnsurePolicyAsync(CmsConstants.Policies.MenusWrite))
        {
            return;
        }

        saving = true;
        try
        {
            var result = await MenuAdminService.SaveItemAsync(TenantId, editing);
            if (result.Status == ResultStatus.Invalid)
            {
                errors = ErrorText.From(result);
                return;
            }

            if (!result.IsSuccess)
            {
                Toasts.ShowFailure(result);
                return;
            }

            Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Saved]);
            editing = null;
            await LoadAsync();
        }
        finally
        {
            saving = false;
        }
    }

    private async Task DeleteAsync(MenuItem item)
    {
        bool hasChildren = childrenByParent[item.Id].Any();
        bool confirmed = await Dialogs.ConfirmAsync(
            T[InfernoWebLocalizableStrings.Admin.DeleteTitleFormat, item.Text],
            hasChildren ? T[InfernoCmsLocalizableStrings.Menus.DeleteItemWithChildrenWarning] : T[InfernoWebLocalizableStrings.Admin.DeleteWarning],
            T[InfernoWebLocalizableStrings.General.Delete],
            isDestructive: true);

        if (!confirmed || !await EnsurePolicyAsync(CmsConstants.Policies.MenusWrite))
        {
            return;
        }

        var result = await MenuAdminService.DeleteItemAsync(TenantId, item.Id);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Deleted]);
        await LoadAsync();
    }
}
