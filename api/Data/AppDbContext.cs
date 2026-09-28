using Microsoft.EntityFrameworkCore;
using MiniDev.Api.Models;

namespace MiniDev.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Item> Items => Set<Item>();
}
