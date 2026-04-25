namespace UpperCube.Application.DTOs;

public sealed record PropertySearchFilter(
    int? CityId = null,
    int? DistrictId = null,
    int? PropertyTypeId = null,
    int? CategoryId = null,
    int? TransactionType = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    decimal? MinArea = null,
    decimal? MaxArea = null,
    int? Rooms = null,
    string? Query = null);
