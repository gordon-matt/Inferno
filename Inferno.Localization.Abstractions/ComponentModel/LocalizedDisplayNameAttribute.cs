﻿using System.ComponentModel;
using Dependo;
using Microsoft.Extensions.Localization;

namespace Inferno.Localization.ComponentModel
{
    //TODO: Implement this with a custom DataAnnotationsModelMetadataProvider?
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        private static IStringLocalizer localizer;

        private static IStringLocalizer T
        {
            get
            {
                if (localizer == null)
                {
                    localizer = EngineContext.Current.Resolve<IStringLocalizer>();
                }
                return localizer;
            }
        }

        public LocalizedDisplayNameAttribute(string resourceKey)
            : base(resourceKey)
        {
            ResourceKey = resourceKey;
        }

        public string ResourceKey { get; set; }

        public override string DisplayName
        {
            get { return T[ResourceKey]; }
        }
    }
}