using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace blog_app_ai_dotnet.models;

public partial class DefaultContext : DbContext
{
    IConfiguration _configuration;
    public DefaultContext(IConfiguration config)
    {
        _configuration = config;
    }

    public DefaultContext(DbContextOptions<DefaultContext> options, IConfiguration config)
        : base(options)
    {
        _configuration = config;
    }

    public virtual DbSet<AuthenticationUser> AuthenticationUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(
        _configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            _configuration.GetConnectionString("DefaultConnection")
        ));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AuthenticationUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("authentication_user");

            entity.HasIndex(e => e.Username, "username").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BackgroundImage)
                .HasMaxLength(100)
                .HasColumnName("background_image");
            entity.Property(e => e.Bio)
                .HasMaxLength(100)
                .HasColumnName("bio");
            entity.Property(e => e.DateJoined)
                .HasMaxLength(6)
                .HasColumnName("date_joined");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(150)
                .HasColumnName("first_name");
            entity.Property(e => e.FriendsNumber).HasColumnName("friends_number");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.IsStaff).HasColumnName("is_staff");
            entity.Property(e => e.IsSuperuser).HasColumnName("is_superuser");
            entity.Property(e => e.LastLogin)
                .HasMaxLength(6)
                .HasColumnName("last_login");
            entity.Property(e => e.LastName)
                .HasMaxLength(150)
                .HasColumnName("last_name");
            entity.Property(e => e.Password)
                .HasMaxLength(128)
                .HasColumnName("password");
            entity.Property(e => e.ProfileImage)
                .HasMaxLength(100)
                .HasColumnName("profile_image");
            entity.Property(e => e.Username)
                .HasMaxLength(150)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
