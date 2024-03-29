﻿namespace Inferno.Web.Security.Membership
{
    public interface IUserProfileProvider
    {
        string Name { get; }

        string DisplayTemplatePath { get; }

        string EditorTemplatePath { get; }

        int Order { get; }

        IEnumerable<string> GetFieldNames();

        Task PopulateFieldsAsync(string userId);
    }
}