namespace Inferno.Web.Navigation
{
    public class MenuItemComparer : IEqualityComparer<MenuItem>
    {
        #region IEqualityComparer<MenuItem> Members

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

            if (!string.IsNullOrWhiteSpace(x.Url))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(y.Url))
            {
                return false;
            }

            return true;
        }

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
}