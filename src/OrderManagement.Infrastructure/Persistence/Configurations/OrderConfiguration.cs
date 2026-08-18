namespace OrderManagement.Infrastructure.Persistence.Configurations;

using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id).ValueGeneratedOnAdd();
        builder.Property(order => order.OrderNumber).HasMaxLength(64).IsRequired();
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(order => order.TotalAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(order => order.Notes).HasMaxLength(2000).IsRequired();
        builder.Property(order => order.CreatedAt).IsRequired();
        builder.Property(order => order.UpdatedAt).IsRequired();
        builder.HasIndex(order => order.OrderNumber).IsUnique();

        builder.HasOne(order => order.Customer)
            .WithMany(customer => customer.Orders)
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
