using System.Text.Json.Serialization;

namespace OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

public sealed record VakifbankBranchDto(
    [property: JsonPropertyName("CityCode")] string? CityCode,
    [property: JsonPropertyName("DistrictCode")] string? DistrictCode, 
    [property: JsonPropertyName("BranchCode")] string? BranchCode,
    [property: JsonPropertyName("BranchName")] string? BranchName,
    [property: JsonPropertyName("BranchAddress")] string? BranchAddress,
    [property: JsonPropertyName("Latitude")] string? Latitude,   
    [property: JsonPropertyName("Longitude")] string? Longitude,
    [property: JsonPropertyName("Type")] string? Type
);

public sealed record VakifbankBranchDataDto(
    [property: JsonPropertyName("Branch")] List<VakifbankBranchDto> Branch
);

public sealed record VakifbankBranchResponseDto(
    [property: JsonPropertyName("Header")] VakifbankHeaderDto Header,
    [property: JsonPropertyName("Data")] VakifbankBranchDataDto Data
);