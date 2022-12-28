﻿using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages
{
    public abstract class InfernoPageType
    {
        public InfernoPageType()
        {
            LayoutPath = "~/Views/Shared/_Layout.cshtml";
        }

        public abstract string Name { get; }

        public abstract bool IsEnabled { get; }

        public abstract string DisplayTemplatePath { get; }

        public abstract string EditorTemplatePath { get; }

        #region Instance Properties

        public string InstanceName { get; set; }

        public Guid? InstanceParentId { get; set; }

        public string LayoutPath { get; set; }

        #endregion Instance Properties

        public abstract void InitializeInstance(PageVersion pageVersion);

        //public abstract void PopulateDocumentIndex(IDocumentIndex document, out string description);

        public abstract void ReplaceContentTokens(Func<string, string> func);
    }
}