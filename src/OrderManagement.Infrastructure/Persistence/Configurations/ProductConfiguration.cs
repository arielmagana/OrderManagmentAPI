namespace OrderManagement.Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).ValueGeneratedOnAdd();
        builder.Property(product => product.Sku).HasMaxLength(50).IsRequired();
        builder.Property(product => product.Name).HasMaxLength(200).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(2000).IsRequired();
        builder.Property(product => product.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(product => product.StockQuantity).IsRequired();
        builder.Property(product => product.IsActive).IsRequired();
        builder.Property(product => product.CreatedAt).IsRequired();
        builder.Property(product => product.UpdatedAt).IsRequired();
        builder.HasIndex(product => product.Sku).IsUnique();
    }
}
