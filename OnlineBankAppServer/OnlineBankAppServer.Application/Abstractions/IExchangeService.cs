namespace OnlineBankAppServer.Application.Abstractions;

public interface IExchangeService
{
    // Kaynak ve hedef para birimini ver,güncel kuru dönsün
    Task<decimal> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken);
}