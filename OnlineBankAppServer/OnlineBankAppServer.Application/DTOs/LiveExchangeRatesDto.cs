namespace OnlineBankAppServer.Application.DTOs;

public sealed record LiveExchangeRatesDto(
    List<ExchangeRateDetailDto> Rates,
    string LastUpdated
);

public sealed record ExchangeRateDetailDto(
    string CurrencyCode, // USD, EUR, GBP, XAU vs.
    string CurrencyName, // Amerikan Doları, Euro, Altın (Gram) vs.
    decimal Rate
);