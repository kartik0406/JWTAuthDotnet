using JWTAuth.DataAccess;
using JWTAuth.Services.AuthService;
using Microsoft.EntityFrameworkCore;

namespace JWTAuth.Core.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    { 
        services.AddDbContext<UserDbContext>(options=>options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
    
}