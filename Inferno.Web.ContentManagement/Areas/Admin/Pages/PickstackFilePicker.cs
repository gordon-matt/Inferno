using Microsoft.JSInterop;
using PageKrafter.Files;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages;

/// <summary>
/// Lets PageKrafter choose files with whichever Pickstack file manager the site registered, opened on the
/// current tenant's folder like the rich text editors are. Pages that use it need <c>&lt;PickstackScripts /&gt;</c>.
/// </summary>
public sealed class PickstackFilePicker(IJSRuntime js, IWorkContext workContext) : IFilePicker, IAsyncDisposable
{
    private const string Module = "./_content/Inferno.Web.ContentManagement/js/pickstack-file-picker.js";

    private IJSObjectReference? module;

    public async ValueTask<PickedFile?> PickAsync(FilePickRequest request, CancellationToken cancellationToken = default)
    {
        module ??= await js.InvokeAsync<IJSObjectReference>("import", cancellationToken, Module);

        var picked = await module.InvokeAsync<PickedFile?>("pick", cancellationToken, new
        {
            type = request.Kind switch
            {
                FilePickerKind.Image => "image",
                FilePickerKind.Video => "media",
                _ => "any",
            },
            root = workContext.TenantRoot,
            title = request.Title,
        });

        return string.IsNullOrWhiteSpace(picked?.Url) ? null : picked;
    }

    public async ValueTask DisposeAsync()
    {
        if (module is null)
        {
            return;
        }

        try
        {
            await module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // The circuit is already gone, so there is nothing to release.
        }
    }
}
