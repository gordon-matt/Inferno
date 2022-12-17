using Microsoft.AspNetCore.Components;
using Radzen;

namespace Inferno.Web.Areas.Admin.Configuration.Pages
{
    public partial class Index
    {
        [Inject]
        private NavigationManager NavigationManager { get; set; }
    }
}