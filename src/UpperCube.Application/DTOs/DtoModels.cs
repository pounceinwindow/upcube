namespace UpperCube.Application.DTOs;

public sealed record PropertyListItemDto(
    int Id,
    string Title,
    decimal Price,
    string Currency,
    decimal Area,
    int Rooms,
    string City,
    string District,
    string? PrimaryImagePath);

public sealed record PropertyDetailsDto(int Id, string Title, string Description);

public sealed record CreatePropertyDto(
    string Title,
    string Description,
    decimal Price,
    string Currency,
    decimal Area,
    int Rooms,
    int Floor,
    int TotalFloors,
    int TransactionType,
    int CityId,
    int DistrictId,
    int PropertyTypeId,
    int CategoryId,
    string Address);

public sealed record UpdatePropertyDto(
    int Id,
    string Title,
    string Description,
    decimal Price,
    string Currency,
    decimal Area,
    int Rooms,
    int Floor,
    int TotalFloors,
    int TransactionType,
    int CityId,
    int DistrictId,
    int PropertyTypeId,
    int CategoryId,
    string Address);

public sealed record UserDto(string Id, string Email, string FirstName, string LastName);

public sealed record InquiryDto(int Id, int PropertyId, string FromUserId, string InitialMessage);

public sealed record MessageDto(int Id, int InquiryId, string SenderId, string Text, DateTime SentAt, bool IsRead);

public sealed record ValuationRequestDto(int CityId, int DistrictId, int PropertyTypeId, decimal Area, int Rooms, int Floor, int TotalFloors);

public sealed record ComparisonDto(int Id, string Name, IReadOnlyList<PropertyListItemDto> Items);

public sealed record FeatureCatalogDto(int Id, string Code, string DisplayName, bool IsEnabled);

public sealed record DictionaryItemDto(int Id, string Name, string Slug);
