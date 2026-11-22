namespace Inferno.Web.Navigation;

/// <summary>
/// Treats two items as the same menu entry only when their text matches and neither has a URL.
/// Items that both have a URL are not equal, even when that URL is the same.
/// <see cref="GetHashCode(MenuItem)"/> uses the text alone.
/// </summary>
public class MenuItemComparer : IEqualityComparer<MenuItem>
{
    #region IEqualityComparer<MenuItem> Members

    /// <inheritdoc />
    public bool Equals(MenuItem x, MenuItem y)
    {
        string xTextHint = x.Text ?? null;
        string yTextHint = y.Text ?? null;
        if (!string.Equals(xTextHint, yTextHint))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(x.Url) && !string.IsNullOrWhiteSpace(y.Url))
        {
            if (!string.Equals(x.Url, y.Url))
            {
                return false;
            }
        }

        return string.IsNullOrWhiteSpace(x.Url) && string.IsNullOrWhiteSpace(y.Url);
    }

    /// <inheritdoc />
    public int GetHashCode(MenuItem obj)
    {
        int hash = 0;

        if (!string.IsNullOrEmpty(obj.Text))
        {
            hash ^= obj.Text.GetHashCode();
        }

        return hash;
    }

    #endregion IEqualityComparer<MenuItem> Members
}