using System.Text.Json.Serialization;

namespace OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

public sealed record VakifbankNeighborhoodDto(
    [property: JsonPropertyName("NeighborhoodName")] string NeighborhoodName,
    [property: JsonPropertyName("NeighborhoodCode")] string NeighborhoodCode
);

public sealed record VakifbankNeighborhoodDataDto(
    [property: JsonPropertyName("Neighborhood")] List<VakifbankNeighborhoodDto> Neighborhood
);

public sealed record VakifbankNeighborhoodResponseDto(
    [property: JsonPropertyName("Header")] VakifbankHeaderDto Header,
    [property: JsonPropertyName("Data")] VakifbankNeighborhoodDataDto Data
);