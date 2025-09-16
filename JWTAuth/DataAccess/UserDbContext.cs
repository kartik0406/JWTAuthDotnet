using JWTAuth.Models;
using Microsoft.EntityFrameworkCore;

namespace JWTAuth.DataAccess;

public class UserDbContext(DbContextOptions<UserDbContext> options):DbContext(options)
{

    public DbSet<Profile> Users => Set<Profile>();
}