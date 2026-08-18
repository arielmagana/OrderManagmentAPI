namespace OrderManagement.Application.Products.Commands;

using DTOs;

/// <summary>
/// Command to update an existing product.
/// CQRS-style command object passed to handler.
/// </summary>
public class UpdateProductCommand
{
    public int ProductId { get; }
    public UpdateProductRequest Request { get; }

    public UpdateProductCommand(int productId, UpdateProductRequest request)
    {
        ProductId = productId;
        Request = request;
    }
}
