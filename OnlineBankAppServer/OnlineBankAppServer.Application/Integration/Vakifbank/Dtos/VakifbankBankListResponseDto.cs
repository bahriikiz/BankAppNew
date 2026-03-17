using System.Text.Json.Serialization;

namespace OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

public sealed record VakifbankBankDto(
    [property: JsonPropertyName("BankName")] string? BankName,
    [property: JsonPropertyName("BankAddress")] string? BankAddress,
    [property: JsonPropertyName("BankCode")] string? BankCode
);

public sealed record VakifbankBankDataDto(
    [property: JsonPropertyName("Banks")] List<VakifbankBankDto> Banks 
);

public sealed record VakifbankBankListResponseDto(
    [property: JsonPropertyName("Header")] VakifbankHeaderDto Header,
    [property: JsonPropertyName("Data")] VakifbankBankDataDto Data
);