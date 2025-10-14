namespace ConsoleApp.Dtos;

public record ProductDto(
    string Id,
    string Name,
    string Description,
    decimal Price
);