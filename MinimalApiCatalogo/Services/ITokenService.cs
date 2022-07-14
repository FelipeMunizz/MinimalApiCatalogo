using MinimalApiCatalogo.Models;

namespace MinimalApiCatalogo.Services
{
    public interface ITokenService
    {
        string GerarToken(string key, string issuer, string audience, UserModel user);
    }
}
