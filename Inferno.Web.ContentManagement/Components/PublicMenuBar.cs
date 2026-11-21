namespace Inferno.Web.ContentManagement.Components;

/// <summary>
/// Open dropdown for the public menu. A click arms the bar. Hover can then switch menus, including
/// after the pointer passes a top-level item that has no dropdown. Clicking outside, pressing Escape,
/// or clicking the open item disarms it.
/// </summary>
public sealed class PublicMenuBar
{
    /// <summary>Raised when the open item or the armed state changes.</summary>
    public event Action Changed;

    /// <summary>The open top-level item, or null when no dropdown panel is showing.</summary>
    public object OpenItem { get; private set; }

    /// <summary>True from the click that opens a menu until the bar is dismissed.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Whether <paramref name="item"/> is the open dropdown.</summary>
    public bool IsOpen(object item) => ReferenceEquals(OpenItem, item);

    /// <summary>Opens <paramref name="item"/>, or closes it and disarms the bar when it is already open.</summary>
    public void Toggle(object item)
    {
        if (ReferenceEquals(OpenItem, item))
        {
            OpenItem = null;
            IsActive = false;
        }
        else
        {
            OpenItem = item;
            IsActive = true;
        }

        Changed?.Invoke();
    }

    /// <summary>
    /// Switches the open dropdown to <paramref name="item"/>. A null item hides the panel but leaves the bar armed.
    /// Does nothing while the bar is idle.
    /// </summary>
    public void Hover(object item)
    {
        if (!IsActive || ReferenceEquals(OpenItem, item))
        {
            return;
        }

        OpenItem = item;
        Changed?.Invoke();
    }

    /// <summary>Closes the open dropdown and disarms the bar.</summary>
    public void Close()
    {
        if (!IsActive && OpenItem == null)
        {
            return;
        }

        OpenItem = null;
        IsActive = false;
        Changed?.Invoke();
    }
}
