namespace Inferno.Web.Identity
{
    public interface ITokenService
    {
        Task<string> GenerateJsonWebTokenAsync(string userId);
    }
}