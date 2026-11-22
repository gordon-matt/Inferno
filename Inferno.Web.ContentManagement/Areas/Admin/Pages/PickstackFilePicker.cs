using Microsoft.JSInterop;
using PageKrafter.Files;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages;

/// <summary>
/// Lets PageKrafter choose files with whichever Pickstack file manager the site registered, opened on the
/// current tenant's folder like the rich text editors are. Pages that use it need <c>&lt;PickstackScripts /&gt;</c>.
/// </summary>
/// <param name="js">Imports the picker script and opens the file manager.</param>
/// <param name="fileRoots">Supplies the current tenant's folder, which the picker opens in.</param>
public sealed class PickstackFilePicker(IJSRuntime js, IFileRootProvider fileRoots) : IFilePicker, IAsyncDisposable
{
    private const string Module = "./_content/Inferno.Web.ContentManagement/js/pickstack-file-picker.js";

    private IJSObjectReference? module;

    /// <summary>Opens the Pickstack file manager and waits for a file.</summary>
    /// <param name="request">The kind of file and the dialog title. <see cref="FilePickRequest.Current"/> is not sent.</param>
    /// <param name="cancellationToken">Stops waiting for the script import or the file choice.</param>
    /// <returns>The chosen file, or <see langword="null"/> when the editor closes the dialog or the chosen URL is blank.</returns>
    /// <remarks>A video request is sent to the script as <c>media</c>. An image request is sent as <c>image</c>. Anything else is sent as <c>any</c>.</remarks>
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
            root = fileRoots.GetFileRoot(),
            title = request.Title,
        });

        return string.IsNullOrWhiteSpace(picked?.Url) ? null : picked;
    }

    /// <summary>Releases the imported picker script. A disconnected circuit is ignored.</summary>
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
