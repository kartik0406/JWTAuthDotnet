using JWTAuth.Models;

namespace JWTAuth.Services.AuthService;

public interface IAuthService
{
    Task<Profile> RegisterAsync(Register user);
    Task<string> LoginAsync(Login login);

}