using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Application.Features.Accounts.Commands.SyncVakifbankAccounts;
using OnlineBankAppServer.Persistance;

namespace OnlineBankAppServer.WebApi.BackgroundServices;

public class OpenBankingSyncWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OpenBankingSyncWorker> _logger;

    public OpenBankingSyncWorker(IServiceProvider serviceProvider, ILogger<OpenBankingSyncWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Açık Bankacılık Arka Plan Senkronizasyon Servisi Başladı.");

        // TEST İÇİN 1 DAKİKA YAPTIK. Canlıya alırken TimeSpan.FromMinutes(45) yapacağız.
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(45));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Vakıfbank hesapları otomatik güncelleniyor...");

            try
            {
                // BackgroundService Singleton'dır. Scoped olan veritabanı ve MediatR'ı kullanmak için Scope aç
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                // Veritabanındaki Vakıfbank hesaplarını ve benzersiz Rıza No'ları bul
                var distinctConsents = await dbContext.Accounts
                    .Where(a => a.ProviderBank == "Vakifbank" && !string.IsNullOrEmpty(a.RizaNo))
                    .Select(a => new { a.UserId, a.RizaNo })
                    .Distinct()
                    .ToListAsync(stoppingToken);

                foreach (var consent in distinctConsents)
                {
                    if (consent.RizaNo != null)
                    {
                        // Zaten yazdığımız MediatR komutunu tekrar kullanarak kod tekrarından kurtuluyoruz!
                        var command = new SyncVakifbankAccountsCommand(consent.RizaNo) { UserId = consent.UserId };
                        await mediator.Send(command, stoppingToken);

                        _logger.LogInformation($"User {consent.UserId} için Vakıfbank bakiyeleri güncellendi.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Arka plan senkronizasyonu sırasında hata: {ex.Message}");
            }
        }
    }
}