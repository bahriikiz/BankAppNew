using System.Text.Json.Serialization;

namespace OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

public sealed record VakifbankNearestDto(
    [property: JsonPropertyName("Type")] string? Type,
    [property: JsonPropertyName("Address")] string? Address,
    [property: JsonPropertyName("Latitude")] string? Latitude,
    [property: JsonPropertyName("Longitude")] string? Longitude,
    [property: JsonPropertyName("Distance")] double? Distance,
    [property: JsonPropertyName("Name")] string? Name
);

public sealed record VakifbankNearestDataDto(
    [property: JsonPropertyName("BranchandATM")] List<VakifbankNearestDto> BranchandATM 
);

public sealed record VakifbankNearestResponseDto(
    [property: JsonPropertyName("Header")] VakifbankHeaderDto Header,
    [property: JsonPropertyName("Data")] VakifbankNearestDataDto Data
);