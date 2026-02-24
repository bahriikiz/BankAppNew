using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Integration.Vakifbank;

public interface IVakifbankService
{
    // Vakıfbank'tan hesap listesini asenkron olarak çekecek metod
    Task<VakifbankAccountListResponseDto?> GetAccountsAsync(CancellationToken cancellationToken = default);
}