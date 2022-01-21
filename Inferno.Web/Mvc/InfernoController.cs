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
        private Lazy<IAuthorizationService> authorizationService;

        public IStringLocalizer T { get; private set; }

        public Lazy<IWorkContext> WorkContext { get; private set; }

        public Lazy<SiteSettings> SiteSettings { get; private set; }

        public Lazy<ILogger> Logger { get; private set; }

        protected InfernoController()
        {
            T = EngineContext.Current.Resolve<IStringLocalizer>();
            WorkContext = new Lazy<IWorkContext>(() => EngineContext.Current.Resolve<IWorkContext>());
            SiteSettings = new Lazy<SiteSettings>(() => EngineContext.Current.Resolve<SiteSettings>());

            Logger = new Lazy<ILogger>(() =>
            {
                var loggerFactory = EngineContext.Current.Resolve<ILoggerFactory>();
                return loggerFactory.CreateLogger(GetType());
            });

            authorizationService = new Lazy<IAuthorizationService>(() => EngineContext.Current.Resolve<IAuthorizationService>());
        }

        protected virtual bool CheckPermission(Permission permission)
        {
            if (permission == null)
            {
                return true;
            }

            return authorizationService.Value.TryCheckAccess(permission, WorkContext.Value.CurrentUser);
        }

        protected virtual IActionResult RedirectToHomePage()
        {
            return RedirectToAction("Index", "Home", new { area = string.Empty });
        }
    }
}