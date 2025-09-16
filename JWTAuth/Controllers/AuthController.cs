using Asp.Versioning;
using JWTAuth.Models;
using JWTAuth.Services.AuthService;
using Microsoft.AspNetCore.Mvc;

namespace JWTAuth.Controllers;
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{ 
    [HttpPost]
    [Route("Register")]
    public async Task<ActionResult<string>> Register(Register user)
    {
        var response = await authService.RegisterAsync(user);
        if (response is null)
            return BadRequest("User Already Exists");
        return Ok("User Successfully Registered");
    }

    [HttpPost]
    [Route("Login")]
    public async Task<ActionResult<string>> Login(Login login)
    {
        var token = await authService.LoginAsync(login);
        if (token is null)
            return BadRequest("Invalid Login Request");
        return Ok(token);
    }
}


//Register, Login - Return JWT, UpdateUser, DeleteUser, GetAllUsers, GetUsersById, Role based data Authorized  