namespace OrderManagement.Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Id).ValueGeneratedOnAdd();
        builder.Property(customer => customer.Name).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.Email).HasMaxLength(254).IsRequired();
        builder.Property(customer => customer.Phone).HasMaxLength(50).IsRequired();
        builder.Property(customer => customer.Address).HasMaxLength(250).IsRequired();
        builder.Property(customer => customer.City).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.PostalCode).HasMaxLength(20).IsRequired();
        builder.Property(customer => customer.Country).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.IsActive).IsRequired();
        builder.Property(customer => customer.CreatedAt).IsRequired();
        builder.Property(customer => customer.UpdatedAt).IsRequired();
        builder.HasIndex(customer => customer.Email).IsUnique();
    }
}
