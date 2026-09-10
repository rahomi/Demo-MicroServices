using Refit;

namespace BFF.Clients;

// --- Products service client ---

public interface IProductsClient
{
    [Get("/api/products")]
    Task<List<ProductDto>> GetProductsAsync();

    [Get("/api/products/{id}")]
    Task<ProductDto> GetProductByIdAsync(Guid id);

    [Post("/api/products")]
    Task<ProductDto> CreateProductAsync([Body] CreateProductRequest request);

    [Put("/api/products/{id}")]
    Task<ProductDto> UpdateProductAsync(Guid id, [Body] UpdateProductRequest request);

    [Delete("/api/products/{id}")]
    Task DeleteProductAsync(Guid id);
}

// --- Baskets service client ---

public interface IBasketsClient
{
    [Get("/api/baskets/{customerId}")]
    Task<BasketDto> GetBasketAsync(string customerId);

    [Post("/api/baskets/{customerId}/items")]
    Task<BasketDto> AddBasketItemAsync(string customerId, [Body] AddBasketItemRequest request);

    [Delete("/api/baskets/{customerId}/items/{productId}")]
    Task<BasketDto> RemoveBasketItemAsync(string customerId, Guid productId);

    [Post("/api/baskets/{customerId}/checkout")]
    Task<CheckoutResult> CheckoutAsync(string customerId);
}

// --- Orders service client ---

public interface IOrdersClient
{
    [Post("/api/orders")]
    Task<OrderDto> SubmitOrderAsync([Body] SubmitOrderRequest request);

    [Get("/api/orders/{id}")]
    Task<OrderDto> GetOrderByIdAsync(Guid id);

    [Get("/api/orders")]
    Task<List<OrderDto>> GetOrdersByCustomerAsync([AliasAs("customerId")] string customerId);
}

// --- Identity service client ---

public interface IIdentityClient
{
    [Get("/api/customer")]
    Task<CustomerDto> GetCustomerAsync();
}

// --- DTOs ---

public record ProductDto(Guid Id, string Name, decimal Price, string Category);
public record CreateProductRequest(string Name, decimal Price, string Category);
public record UpdateProductRequest(Guid Id, string Name, decimal Price, string Category);

public record BasketDto(Guid Id, string CustomerId, List<BasketItemDto> Items);
public record BasketItemDto(Guid Id, Guid BasketId, Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
public record AddBasketItemRequest(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);

public record CheckoutResult(string CustomerId, List<CheckoutItemDto> Items, DateTime CheckedOutAt);
public record CheckoutItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);

public record OrderDto(Guid Id, string CustomerId, List<OrderItemDto> Items, decimal Total, string Status, DateTime CreatedAt);
public record OrderItemDto(Guid Id, Guid OrderId, Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
public record SubmitOrderRequest(string CustomerId, List<SubmitOrderItem> Items);
public record SubmitOrderItem(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);

public record CustomerDto(string Id, string Name, string Email);
