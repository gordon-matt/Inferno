﻿using Dependo;
using Inferno.Security.Membership.Permissions;
using Inferno.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Mvc
{
    public class InfernoController : Controller
    {
        public ILogger Logger { get; private set; }

        public IStringLocalizer T { get; private set; }

        public IWorkContext WorkContext { get; private set; }

        public Lazy<SiteSettings> SiteSettings { get; private set; }

        protected InfernoController()
        {
            WorkContext = EngineContext.Current.Resolve<IWorkContext>();
            T = EngineContext.Current.Resolve<IStringLocalizer>();
            var loggerFactory = EngineContext.Current.Resolve<ILoggerFactory>();
            Logger = loggerFactory.CreateLogger(GetType());
            SiteSettings = new Lazy<SiteSettings>(() => EngineContext.Current.Resolve<SiteSettings>());
        }

        protected virtual bool CheckPermission(Permission permission)
        {
            if (permission == null)
            {
                return true;
            }

            var authorizationService = EngineContext.Current.Resolve<IAuthorizationService>();
            return authorizationService.TryCheckAccess(permission, WorkContext.CurrentUser);
        }

        protected virtual IActionResult RedirectToHomePage()
        {
            return RedirectToAction("Index", "Home", new { area = string.Empty });
        }
    }
}