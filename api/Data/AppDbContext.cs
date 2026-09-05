using Microsoft.EntityFrameworkCore;
using MiniTwise.Api.Models;

namespace MiniTwise.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Item> Items => Set<Item>();
}
