using AdminPanel.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Api.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "Full access" },
            new Role { Id = 2, Name = "Editor", Description = "Edit content" },
            new Role { Id = 3, Name = "Viewer", Description = "Read-only" }
        );
    }
}
