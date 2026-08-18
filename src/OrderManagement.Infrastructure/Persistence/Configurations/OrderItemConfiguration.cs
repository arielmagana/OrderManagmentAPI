namespace OrderManagement.Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedOnAdd();
        builder.Property(item => item.Quantity).IsRequired();
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.LineTotal).HasPrecision(18, 2).IsRequired();

        builder.HasOne(item => item.Order)
            .WithMany(order => order.OrderItems)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(item => item.Product)
            .WithMany(product => product.OrderItems)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
