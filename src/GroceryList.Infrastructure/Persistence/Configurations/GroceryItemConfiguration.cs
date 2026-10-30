using GroceryList.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroceryList.Infrastructure.Persistence.Configurations;

internal sealed class GroceryItemConfiguration : IEntityTypeConfiguration<GroceryItem>
{
    public void Configure(EntityTypeBuilder<GroceryItem> builder)
    {
        builder.ToTable("GroceryItems");

        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();

        builder.Property(item => item.Name)
            .HasMaxLength(GroceryItem.NameMaxLength)
            .IsRequired();

        builder.Property(item => item.Quantity).IsRequired();
        builder.Property(item => item.IsInBasket).IsRequired();
        builder.Property(item => item.CreatedAtUtc).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Enforces "no duplicate names per user" at database level as well. SQL Server's default
        // collation (SQL_Latin1_General_CP1_CI_AS) is case-insensitive, so "Milk" and "milk" collide,
        // matching the Application's case-insensitive duplicate check.
        // UserId is the leading column, so this index also serves the per-user queries and the FK;
        // a separate UserId index would be redundant (EF Core does not add one when it is covered).
        builder.HasIndex(item => new { item.UserId, item.Name }).IsUnique();
    }
}
