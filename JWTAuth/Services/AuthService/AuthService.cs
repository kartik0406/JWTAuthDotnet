using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JWTAuth.DataAccess;
using JWTAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JWTAuth.Services.AuthService;

public class AuthService(UserDbContext context, IConfiguration configuration) : IAuthService
{
    public async Task<Profile> RegisterAsync(Register user)
    { 
        if (await context.Users.AnyAsync(x => x.Email == user.Email))
        {
            return null;
        }

        var userProfile = new Profile();
        var passwordHash = new PasswordHasher<Profile>().HashPassword(userProfile,user.Password);

        userProfile.UserName = user.UserName;
        userProfile.FirstName = user.FirstName;
        userProfile.LastName = user.LastName;
        userProfile.Email = user.Email;
        userProfile.PasswordHash = passwordHash;
        context.Users.AddRange(userProfile);
        await context.SaveChangesAsync();
        return userProfile;
    }

    public async Task<string> LoginAsync(Login login)
    {
        if ((await context.Users.AnyAsync(x => x.Email == login.Email) ||
             await context.Users.AnyAsync(x => x.UserName == login.UserName)))
        {
            var profile = await context.Users.FirstOrDefaultAsync(x => (x.Email == login.Email || x.UserName==login.UserName));
            if (new PasswordHasher<Profile>().VerifyHashedPassword(profile, profile.PasswordHash, login.Password) ==
                PasswordVerificationResult.Success)
            {
                return GenerateToken(profile);   
            }
            
        }
        return null;
    }
    
    private string GenerateToken(Profile user)
    {
        var claims = new List<Claim>()
        {
            new (ClaimTypes.Name, user.UserName),
            new (ClaimTypes.Email, user.Email),
            new (ClaimTypes.NameIdentifier, user.Id.ToString()),
            new ("FirstName", user.FirstName),
            new ("LastName", user.LastName),
            new (ClaimTypes.Role,user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("JwtKey:JwtSecret")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var tokeDescripter = new JwtSecurityToken(
            issuer: configuration.GetValue<string>("JwtKey:JwtIssuer"),
            audience: configuration.GetValue<string>("JwtKey:JwtAudience"),
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokeDescripter);

    }
}