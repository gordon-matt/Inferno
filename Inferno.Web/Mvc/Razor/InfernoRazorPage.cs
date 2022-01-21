//using System.Collections.Generic;
//using System.Globalization;
//using Inferno.Infrastructure;
//using Inferno.Security.Membership.Permissions;
//using Inferno.Web.Configuration;
//using Inferno.Web.Navigation;
//using Microsoft.AspNetCore.Mvc.Razor;
//using Microsoft.AspNetCore.Mvc.Razor.Internal;
//using Microsoft.Extensions.Localization;

//namespace Inferno.Web.Mvc.Razor
//{
//    public abstract class InfernoRazorPage<TModel> : RazorPage<TModel>
//    {
//        public IEnumerable<MenuItem> GetMenu(string menuName)
//        {
//            return EngineContext.Current.Resolve<INavigationManager>().BuildMenu(menuName);
//        }

//        [RazorInject]
//        public SiteSettings SiteSettings { get; set; }

//        [RazorInject]
//        public IStringLocalizer T { get; set; }

//        [RazorInject]
//        public IWorkContext WorkContext { get; set; }

//        public bool IsRightToLeft
//        {
//            get { return CultureInfo.CurrentCulture.TextInfo.IsRightToLeft; }
//        }

//        public bool CheckPermission(Permission permission)
//        {
//            var authorizationService = EngineContext.Current.Resolve<IAuthorizationService>();
//            if (authorizationService.TryCheckAccess(permission, WorkContext.CurrentUser))
//            {
//                return true;
//            }

//            return false;
//        }
//    }

//    public abstract class InfernoRazorPage : InfernoRazorPage<dynamic>
//    {
//    }
//}