using Asp.Versioning;
using JWTAuth.DataAccess;
using JWTAuth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JWTAuth.Controllers;
[ApiController]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/[controller]")]
public class UserController(UserDbContext context) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<Profile>>> GetUsers()
    {
        return Ok(await context.Users.ToListAsync());
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<Profile>> GetUserById(int id)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null)
            return NotFound();
        return Ok(user);
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Profile>> UpdateUser(int id, Register user)
    {
        var profile = await context.Users.FindAsync(id);
        if (profile == null)
            return NotFound();
        profile.FirstName = user.FirstName;
        profile.LastName = user.LastName;
        profile.Email = user.Email;
        profile.Role = user.Role;
        context.Users.Update(profile);
        await context.SaveChangesAsync();
        return profile;
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<string>> DeleteUser(int id)
    {
        var profile = await context.Users.FindAsync(id);
        if (profile == null)
            return NotFound();
        context.Users.Remove(profile);
        await context.SaveChangesAsync();
        return Ok($"User Id: {id} is deleted Successfully");
    }
}