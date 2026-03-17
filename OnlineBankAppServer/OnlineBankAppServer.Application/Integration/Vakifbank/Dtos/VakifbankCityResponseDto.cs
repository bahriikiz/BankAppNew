using System.Text.Json.Serialization;

namespace OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;


public sealed record VakifbankCityDto(
    [property: JsonPropertyName("CityCode")] string CityCode,
    [property: JsonPropertyName("CityName")] string CityName
);

public sealed record VakifbankCityDataDto(
    [property: JsonPropertyName("City")] List<VakifbankCityDto> City
);

public sealed record VakifbankCityResponseDto(
    [property: JsonPropertyName("Header")] VakifbankHeaderDto Header,
    [property: JsonPropertyName("Data")] VakifbankCityDataDto Data
);