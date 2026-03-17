using System.Text.Json.Serialization;

namespace OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;


public sealed record VakifbankDistrictDto(
    [property: JsonPropertyName("DistrictName")] string DistrictName,
    [property: JsonPropertyName("NVIDistrictCode")] string NVIDistrictCode,
    [property: JsonPropertyName("BankDistrictCode")] string BankDistrictCode,
    [property: JsonPropertyName("DistrictCode")] string DistrictCode
);

public sealed record VakifbankDistrictDataDto(
    [property: JsonPropertyName("District")] List<VakifbankDistrictDto> District
);

public sealed record VakifbankDistrictResponseDto(
    [property: JsonPropertyName("Header")] VakifbankHeaderDto Header,
    [property: JsonPropertyName("Data")] VakifbankDistrictDataDto Data
);