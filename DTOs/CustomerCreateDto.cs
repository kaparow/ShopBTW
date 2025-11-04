namespace ShopBTW.DTOs;

public record CustomerCreateDto(
    string FirstName,
    string LastName,
    string Email
);
