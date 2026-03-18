using System.Text.Json.Serialization;

namespace OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

public sealed record VakifbankDepositProductDto(
    [property: JsonPropertyName("ProductCode")] string? ProductCode,
    [property: JsonPropertyName("CampaignId")] string? CampaignId,
    [property: JsonPropertyName("ProductName")] string? ProductName,
    [property: JsonPropertyName("DetailInfoLink")] string? DetailInfoLink,
    [property: JsonPropertyName("InformationMessage")] string? InformationMessage,
    [property: JsonPropertyName("CurrencyCode")] List<string>? CurrencyCode, 
    [property: JsonPropertyName("MinTerm")] int? MinTerm,
    [property: JsonPropertyName("MaxTerm")] int? MaxTerm,
    [property: JsonPropertyName("MinAmount")] double? MinAmount,
    [property: JsonPropertyName("MaxAmount")] double? MaxAmount
);

public sealed record VakifbankDepositProductDataDto(
    [property: JsonPropertyName("DepositProduct")] List<VakifbankDepositProductDto>? DepositProduct
);

public sealed record VakifbankDepositProductResponseDto(
    [property: JsonPropertyName("Header")] VakifbankHeaderDto Header,
    [property: JsonPropertyName("Data")] VakifbankDepositProductDataDto Data
);