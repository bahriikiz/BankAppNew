using System.Text.Json.Serialization;

namespace OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

public sealed record VakifbankDepositDto(
    [property: JsonPropertyName("WithholdingRate")] double? WithholdingRate,
    [property: JsonPropertyName("WithholdingAmount")] double? WithholdingAmount,
    [property: JsonPropertyName("TermDays")] int? TermDays,
    [property: JsonPropertyName("NetInterestAmount")] double? NetInterestAmount,
    [property: JsonPropertyName("NetInterestAmountCurrency")] double? NetInterestAmountCurrency,
    [property: JsonPropertyName("NetAmount")] double? NetAmount,
    [property: JsonPropertyName("WithholdingAmountTL")] double? WithholdingAmountTL,
    [property: JsonPropertyName("InterestRate")] double? InterestRate,
    [property: JsonPropertyName("InformationMessage")] string? InformationMessage
);

public sealed record VakifbankDepositDataDto(
    [property: JsonPropertyName("Deposit")] VakifbankDepositDto? Deposit 
);

public sealed record VakifbankDepositResponseDto(
    [property: JsonPropertyName("Header")] VakifbankHeaderDto Header,
    [property: JsonPropertyName("Data")] VakifbankDepositDataDto Data
);