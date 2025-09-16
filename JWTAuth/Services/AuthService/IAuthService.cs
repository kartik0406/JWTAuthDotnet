[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(UserDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null) return Unauthorized();

        // Validate password...
        
        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id.ToString(),
            Expires = DateTime.UtcNow.AddDays(7)
        });
        await _context.SaveChangesAsync();

        // Set refresh token as HttpOnly cookie
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // only HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return Ok(new TokenResponse { AccessToken = accessToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        // Read refresh token from HttpOnly cookie
        var token = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(token)) return Unauthorized();

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsRevoked);

        if (refreshToken == null || refreshToken.Expires < DateTime.UtcNow)
            return Unauthorized();

        // Revoke old token
        refreshToken.IsRevoked = true;

        var user = await _context.Users.FindAsync(int.Parse(refreshToken.UserId));
        var newAccessToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id.ToString(),
            Expires = DateTime.UtcNow.AddDays(7)
        });

        await _context.SaveChangesAsync();

        // Set new refresh token cookie
        Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return Ok(new TokenResponse { AccessToken = newAccessToken });
    }

    private string GenerateJwtToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_config["JwtKey:JwtSecret"]);
        var token = new JwtSecurityToken(
            issuer: _config["JwtKey:JwtIssuer"],
            audience: _config["JwtKey:JwtAudience"],
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            },
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
