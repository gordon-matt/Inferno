﻿using Extenso;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components
{
    public abstract class ContentBlockEditor<T> : ComponentBase, IContentBlockEditor
        where T : IContentBlock, new()
    {
        [Parameter]
        public ContentBlock Data { get; set; }

        public T Model { get; set; }

        public string Save()
        {
            return Model.JsonSerialize();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (!string.IsNullOrEmpty(Data?.BlockValues))
            {
                Model = Data.BlockValues.JsonDeserialize<T>();
            }
            else
            {
                Model = new T();
            }
        }
    }
}