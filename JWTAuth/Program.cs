using System.Text;
using Asp.Versioning.ApiExplorer;
using JWTAuth.Core.Extensions;
using JWTAuth.Core.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddSwaggerService();
builder.Services.AddAppServices(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtKey:JwtSecret"])),
        ValidIssuer = builder.Configuration["JwtKey:JwtIssuer"],
        ValidAudience = builder.Configuration["JwtKey:JwtAudience"],
        ValidateLifetime = true
    };

});
var app = builder.Build();

var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseSwaggerService(provider);
app.UseScalarService(provider);

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();