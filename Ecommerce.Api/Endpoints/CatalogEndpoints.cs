using Ecommerce.Core.Data;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Endpoints;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        // --- Categories ---
        var categories = app.MapGroup("/api/categories").WithTags("Categories");

        categories.MapGet("/", async (EcommerceDbContext db) =>
        {
            var items = await db.Categories
                .AsNoTracking()
                .Select(c => new CategoryDto(c.Id, c.Name))
                .ToListAsync();

            return Results.Ok(items);
        });

        categories.MapGet("/{id:int}", async (int id, EcommerceDbContext db) =>
        {
            var category = await db.Categories
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CategoryDto(c.Id, c.Name))
                .FirstOrDefaultAsync();

            return category is null ? Results.NotFound() : Results.Ok(category);
        });

        categories.MapPost("/", async (CreateCategoryDto dto, EcommerceDbContext db) =>
        {
            var category = new Category { Name = dto.Name.Trim() };
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            var result = new CategoryDto(category.Id, category.Name);
            return Results.Created($"/api/categories/{category.Id}", result);
        });

        categories.MapDelete("/{id:int}", async (int id, EcommerceDbContext db) =>
        {
            var category = await db.Categories.FindAsync(id);
            if (category is null) return Results.NotFound();

            var hasProducts = await db.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
                return Results.BadRequest("Cannot delete a category that has products.");

            db.Categories.Remove(category);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // --- Products ---
        var products = app.MapGroup("/api/products").WithTags("Products");

        products.MapGet("/", async (EcommerceDbContext db) =>
        {
            var items = await db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Select(p => new ProductDto(
                    p.Id,
                    p.Name,
                    p.Price,
                    p.CategoryId,
                    p.Category.Name))
                .ToListAsync();

            return Results.Ok(items);
        });

        products.MapGet("/{id:int}", async (int id, EcommerceDbContext db) =>
        {
            var product = await db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.Id == id)
                .Select(p => new ProductDto(
                    p.Id,
                    p.Name,
                    p.Price,
                    p.CategoryId,
                    p.Category.Name))
                .FirstOrDefaultAsync();

            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        products.MapPost("/", async (CreateProductDto dto, EcommerceDbContext db) =>
        {
            var categoryExists = await db.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return Results.BadRequest($"Category with Id {dto.CategoryId} does not exist.");

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Price = dto.Price,
                CategoryId = dto.CategoryId
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            var categoryName = await db.Categories
                .Where(c => c.Id == product.CategoryId)
                .Select(c => c.Name)
                .FirstAsync();

            var result = new ProductDto(product.Id, product.Name, product.Price, product.CategoryId, categoryName);
            return Results.Created($"/api/products/{product.Id}", result);
        });

        products.MapPut("/{id:int}", async (int id, UpdateProductDto dto, EcommerceDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return Results.NotFound();

            var categoryExists = await db.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return Results.BadRequest($"Category with Id {dto.CategoryId} does not exist.");

            product.Name = dto.Name.Trim();
            product.Price = dto.Price;
            product.CategoryId = dto.CategoryId;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        products.MapDelete("/{id:int}", async (int id, EcommerceDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return Results.NotFound();

            var usedInOrders = await db.OrderDetails.AnyAsync(od => od.ProductId == id);
            if (usedInOrders)
                return Results.BadRequest("Cannot delete a product that is referenced in existing orders.");

            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}