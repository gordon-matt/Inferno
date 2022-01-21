namespace Inferno.Security.Membership.Permissions
{
    /// <summary>
    /// Entry-point for configured authorization scheme. Role-based system provided by default.
    /// </summary>
    public interface IAuthorizationService //: IDependency
    {
        void CheckAccess(Permission permission, InfernoUser user);

        bool TryCheckAccess(Permission permission, InfernoUser user);
    }
}