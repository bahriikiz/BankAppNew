using MediatR;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Application.DTOs;

namespace OnlineBankAppServer.Application.Features.Exchange.Queries.GetLiveRates;

internal sealed class GetLiveRatesQueryHandler(
    IExchangeService exchangeService) : IRequestHandler<GetLiveRatesQuery, LiveExchangeRatesDto>
{
    public async Task<LiveExchangeRatesDto> Handle(GetLiveRatesQuery request, CancellationToken cancellationToken)
    {
        // 1. Görevleri (Task) Başlat ama BEKLEME (Paralel İşlem)
        var usdTask = exchangeService.GetRateAsync("USD", "TRY", cancellationToken);
        var eurTask = exchangeService.GetRateAsync("EUR", "TRY", cancellationToken);
        var gbpTask = exchangeService.GetRateAsync("GBP", "TRY", cancellationToken); // Sterlin
        var chfTask = exchangeService.GetRateAsync("CHF", "TRY", cancellationToken); // İsviçre Frangı
        var jpyTask = exchangeService.GetRateAsync("JPY", "TRY", cancellationToken); // Japon Yeni
        var xauTask = exchangeService.GetRateAsync("XAU", "TRY", cancellationToken);

        // 2. Tüm görevlerin AYNI ANDA bitmesini bekle (Performans Zirvesi)
        await Task.WhenAll(usdTask, eurTask, gbpTask, chfTask, jpyTask, xauTask);

        // 3. Sonuçları listeye doldur
        var rates = new List<ExchangeRateDetailDto>
        {
            new("USD", "Amerikan Doları", usdTask.Result),
            new("EUR", "Euro", eurTask.Result),
            new("GBP", "İngiliz Sterlini", gbpTask.Result),
            new("CHF", "İsviçre Frangı", chfTask.Result),
            new("JPY", "Japon Yeni", jpyTask.Result),
            new("XAU", "Altın (Gram)", xauTask.Result)
        };

        // 4. Angular'a Gönder
        return new LiveExchangeRatesDto(
            rates,
            DateTime.Now.ToString("HH:mm:ss")
        );
    }
}