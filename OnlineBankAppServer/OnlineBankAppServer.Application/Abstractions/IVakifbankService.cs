using System.Text.Json.Serialization;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Integration.Vakifbank;

public interface IVakifbankService
{
    // Vakıfbank API'si, hesap listesini çekmek için rizaNo (consentId) gerektirir. Bu nedenle, GetAccountsAsync metoduna rizaNo parametresi eklenmiştir.
    Task<VakifbankAccountListResponseDto?> GetAccountsAsync(string rizaNo, CancellationToken cancellationToken = default);

    // Vakıfbank API'si, hesap hareketlerini tarih aralığına göre çekmeye izin verir. Bu nedenle, startDate ve endDate parametreleri eklenmiştir.
    Task<VakifbankAccountTransactionsResponseDto?> GetAccountTransactionsAsync(string rizaNo, string accountNumber, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

// DTO'lar:
public class VakifbankAccountTransactionsResponseDto
{
    [JsonPropertyName("Header")]
    public VakifbankHeaderDto? Header { get; set; }

    [JsonPropertyName("Data")]
    public VakifbankTransactionsDataDto? Data { get; set; }
}

public class VakifbankHeaderDto
{
    [JsonPropertyName("StatusCode")]
    public string? StatusCode { get; set; }

    [JsonPropertyName("StatusDescription")]
    public string? StatusDescription { get; set; }
}

public class VakifbankTransactionsDataDto
{
    [JsonPropertyName("AccountTransactions")]
    public List<VakifbankTransactionItemDto>? AccountTransactions { get; set; }
}

public class VakifbankTransactionItemDto
{
    [JsonPropertyName("CurrencyCode")]
    public string? CurrencyCode { get; set; }

    [JsonPropertyName("TransactionType")]
    public string? TransactionType { get; set; } 

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("Amount")]
    public string? Amount { get; set; }

    [JsonPropertyName("TransactionDate")]
    public string? TransactionDate { get; set; }
}