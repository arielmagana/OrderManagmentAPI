namespace Microsoft.Extensions.DependencyInjection;

using FluentValidation;
using OrderManagement.Application.Customers;
using OrderManagement.Application.Customers.Commands;
using OrderManagement.Application.Customers.DTOs;
using OrderManagement.Application.Customers.Queries;
using OrderManagement.Application.Customers.Validators;
using OrderManagement.Application.Orders;
using OrderManagement.Application.Orders.Commands;
using OrderManagement.Application.Orders.DTOs;
using OrderManagement.Application.Orders.Queries;
using OrderManagement.Application.Orders.Validators;
using OrderManagement.Application.Products;
using OrderManagement.Application.Products.Commands;
using OrderManagement.Application.Products.DTOs;
using OrderManagement.Application.Products.Queries;
using OrderManagement.Application.Products.Validators;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateCustomerCommandHandler>();
        services.AddScoped<UpdateCustomerCommandHandler>();
        services.AddScoped<GetCustomerQueryHandler>();
        services.AddScoped<GetCustomersPagedQueryHandler>();

        services.AddScoped<CreateProductCommandHandler>();
        services.AddScoped<UpdateProductCommandHandler>();
        services.AddScoped<GetProductQueryHandler>();
        services.AddScoped<GetProductsPagedQueryHandler>();

        services.AddScoped<CreateOrderCommandHandler>();
        services.AddScoped<ChangeOrderStatusCommandHandler>();
        services.AddScoped<GetOrderQueryHandler>();
        services.AddScoped<GetOrdersPagedQueryHandler>();

        services.AddScoped<IValidator<CreateCustomerRequest>, CreateCustomerValidator>();
        services.AddScoped<IValidator<UpdateCustomerRequest>, UpdateCustomerValidator>();
        services.AddScoped<IValidator<CreateProductRequest>, CreateProductValidator>();
        services.AddScoped<IValidator<UpdateProductRequest>, UpdateProductValidator>();
        services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderValidator>();
        services.AddScoped<IValidator<ChangeOrderStatusRequest>, ChangeOrderStatusValidator>();

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
