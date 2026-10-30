using GroceryList.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroceryList.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    // ASP.NET Core Identity v3 hashes are ~84 characters; leave room for future format versions.
    private const int PasswordHashMaxLength = 256;

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();

        builder.Property(user => user.Username)
            .HasMaxLength(User.UsernameMaxLength)
            .IsRequired();

        // SQL Server's default collation (SQL_Latin1_General_CP1_CI_AS) is case-insensitive,
        // so "Alice" and "alice" collide here, matching the Application's uniqueness check.
        builder.HasIndex(user => user.Username).IsUnique();

        builder.Property(user => user.FullName)
            .HasMaxLength(User.FullNameMaxLength)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(PasswordHashMaxLength)
            .IsRequired();

        builder.Property(user => user.CreatedAtUtc).IsRequired();
    }
}
