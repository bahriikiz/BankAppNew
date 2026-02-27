using System.Text.Json.Serialization;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Integration.Vakifbank;

public interface IVakifbankService
{
    Task<VakifbankAccountListResponseDto?> GetAccountsAsync(string rizaNo, CancellationToken cancellationToken = default);

    Task<VakifbankAccountTransactionsResponseDto?> GetAccountTransactionsAsync(string rizaNo, string accountNumber, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    Task<VakifbankAccountDetailResponseDto?> GetAccountDetailAsync(string rizaNo, string accountNumber, CancellationToken cancellationToken = default);

    Task<VakifbankReceiptResponseDto?> GetReceiptAsync(string rizaNo, string accountNumber, string transactionId, string receiptFormat, CancellationToken cancellationToken = default);
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

public class VakifbankAccountDetailResponseDto
{
    [JsonPropertyName("Header")]
    public VakifbankHeaderDto? Header { get; set; }

    [JsonPropertyName("Data")]
    public VakifbankAccountDetailDataDto? Data { get; set; }
}

public class VakifbankAccountDetailDataDto
{
    [JsonPropertyName("AccountInfo")]
    public VakifbankAccountInfoDto? AccountInfo { get; set; }
}

public class VakifbankAccountInfoDto
{
    [JsonPropertyName("CurrencyCode")]
    public string? CurrencyCode { get; set; }

    [JsonPropertyName("Balance")]
    public string? Balance { get; set; }

    [JsonPropertyName("AccountNumber")]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("AccountStatus")]
    public string? AccountStatus { get; set; }

    [JsonPropertyName("IBAN")]
    public string? IBAN { get; set; }

    [JsonPropertyName("AccountType")]
    public string? AccountType { get; set; }
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

    [JsonPropertyName("TransactionId")]
    public string? TransactionId { get; set; }
}

public class VakifbankReceiptResponseDto
{
    [JsonPropertyName("Header")]
    public VakifbankHeaderDto? Header { get; set; }

    [JsonPropertyName("Data")]
    public VakifbankReceiptDataDto? Data { get; set; }

    [JsonPropertyName("Documents")]
    public VakifbankReceiptDocumentsDto? Documents { get; set; }
}

public class VakifbankReceiptDataDto
{
    [JsonPropertyName("Info")]
    public VakifbankReceiptInfoDto? Info { get; set; }
}

public class VakifbankReceiptInfoDto
{
    [JsonPropertyName("TransactionId")]
    public string? TransactionId { get; set; }
}

public class VakifbankReceiptDocumentsDto
{
    [JsonPropertyName("TextReceipt")]
    public string? TextReceipt { get; set; }

    [JsonPropertyName("PdfReceipt")]
    public string? PdfReceipt { get; set; }

    [JsonPropertyName("TransactionId")]
    public string? TransactionId { get; set; }
}