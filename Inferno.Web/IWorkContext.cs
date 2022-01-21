using Inferno.Security.Membership;
using Inferno.Tenants.Entities;
using Inferno.Web.Navigation;

namespace Inferno.Web
{
    public interface IWorkContext
    {
        T GetState<T>(string name);

        void SetState<T>(string name, T value);

        string CurrentCultureCode { get; }

        Tenant CurrentTenant { get; }

        InfernoUser CurrentUser { get; }

        BreadcrumbCollection Breadcrumbs { get; set; }

        string CurrentTheme { get; }
    }
}