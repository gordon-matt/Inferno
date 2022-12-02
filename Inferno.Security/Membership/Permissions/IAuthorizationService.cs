namespace Inferno.Security.Membership.Permissions
{
    /// <summary>
    /// Entry-point for configured authorization scheme. Role-based system provided by default.
    /// </summary>
    public interface IAuthorizationService //: IDependency
    {
        Task CheckAccessAsync(Permission permission, InfernoUser user);

        Task<bool> TryCheckAccessAsync(Permission permission, InfernoUser user);
    }
}