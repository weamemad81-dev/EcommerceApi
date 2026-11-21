namespace Ecommerce.Core.DTOs;

// --- Category ---
public record CategoryDto(int Id, string Name);
public record CreateCategoryDto(string Name);

// --- Product ---
public record ProductDto(int Id, string Name, decimal Price, int CategoryId, string CategoryName);
public record CreateProductDto(string Name, decimal Price, int CategoryId);
public record UpdateProductDto(string Name, decimal Price, int CategoryId);

// --- Customer ---
public record CustomerDto(int Id, string Name, string Email);
public record CreateCustomerDto(string Name, string Email);

// --- Order & OrderDetail ---
public record OrderItemRequestDto(int ProductId, int Quantity);
public record CreateOrderDto(int CustomerId, List<OrderItemRequestDto> Items);

public record OrderItemResponseDto(int ProductId, string ProductName, int Quantity, decimal UnitPrice);
public record OrderDto(
    int Id,
    DateTime OrderDate,
    int CustomerId,
    string CustomerName,
    decimal TotalAmount,
    List<OrderItemResponseDto> Items
);