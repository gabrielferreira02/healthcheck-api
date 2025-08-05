using System;
using HealthCheckApi.Entity;
using Microsoft.EntityFrameworkCore;

namespace HealthCheckApi.Data;

public class AppDbContext : DbContext
{
    private readonly IConfiguration? _configuration;
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<UrlEntity> Urls { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public AppDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(_configuration != null && !optionsBuilder.IsConfigured) 
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("SqlServer"));

        base.OnConfiguring(optionsBuilder);
    }
}
