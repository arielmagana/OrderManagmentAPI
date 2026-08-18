namespace OrderManagement.Application.Products.Commands;

using DTOs;

/// <summary>
/// Command to create a new product.
/// CQRS-style command object passed to handler.
/// </summary>
public class CreateProductCommand
{
    public CreateProductRequest Request { get; }

    public CreateProductCommand(CreateProductRequest request)
    {
        Request = request;
    }
}
