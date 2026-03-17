using System.Text.Json.Serialization;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Abstractions;

public interface IVakifbankService
{
    // Vakıfbank API'si üzerinden hesap bilgilerini, hareketlerini ve makbuzları almak için tanımlanan servis arayüzü
    Task<VakifbankAccountListResponseDto?> GetAccountsAsync(string rizaNo, CancellationToken cancellationToken = default);

    // Belirli bir hesap için hareketleri almak için tanımlanan metod
    Task<VakifbankAccountTransactionsResponseDto?> GetAccountTransactionsAsync(string rizaNo, string accountNumber, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    // Belirli bir hesap için detay bilgilerini almak için tanımlanan metod
    Task<VakifbankAccountDetailResponseDto?> GetAccountDetailAsync(string rizaNo, string accountNumber, CancellationToken cancellationToken = default);

    // Belirli bir işlem için makbuz bilgilerini almak için tanımlanan metod
    Task<VakifbankReceiptResponseDto?> GetReceiptAsync(string rizaNo, string accountNumber, string transactionId, string receiptFormat, CancellationToken cancellationToken = default);

    // Vakıfbank API'si üzerinden şehir bilgilerini almak için tanımlanan metod
    Task<VakifbankCityResponseDto?> GetCitiesAsync(CancellationToken cancellationToken = default);
}

// DTO'lar:
public class VakifbankAccountTransactionsResponseDto
{
    [JsonPropertyName("Header")]
    public VakifbankHeaderDto? Header { get; set; } // API yanıtının başlık bilgisi

    [JsonPropertyName("Data")]
    public VakifbankTransactionsDataDto? Data { get; set; } // Hesap hareketleri verisi
}

public class VakifbankHeaderDto
{
    [JsonPropertyName("StatusCode")]
    public string? StatusCode { get; set; } // API yanıtının durum kodu

    [JsonPropertyName("StatusDescription")]
    public string? StatusDescription { get; set; } // API yanıtının durum açıklaması
}

public class VakifbankTransactionsDataDto
{
    [JsonPropertyName("AccountTransactions")]
    public List<VakifbankTransactionItemDto>? AccountTransactions { get; set; } // Hesap hareketleri listesi
}

public class VakifbankAccountDetailResponseDto
{
    [JsonPropertyName("Header")]
    public VakifbankHeaderDto? Header { get; set; }     // API yanıtının başlık bilgisi

    [JsonPropertyName("Data")]
    public VakifbankAccountDetailDataDto? Data { get; set; }  // Hesap detay verisi
}

public class VakifbankAccountDetailDataDto
{
    [JsonPropertyName("AccountInfo")]
    public VakifbankAccountInfoDto? AccountInfo { get; set; } // Hesap bilgisi
}

public class VakifbankAccountInfoDto
{
    [JsonPropertyName("CurrencyCode")]
    public string? CurrencyCode { get; set; } // Hesap para birimi kodu

    [JsonPropertyName("Balance")]
    public string? Balance { get; set; }  // Hesap bakiyesi

    [JsonPropertyName("AccountNumber")]
    public string? AccountNumber { get; set; }  // Hesap numarası

    [JsonPropertyName("AccountStatus")]
    public string? AccountStatus { get; set; } // Hesap durumu

    [JsonPropertyName("IBAN")]
    public string? IBAN { get; set; }  // Hesap IBAN numarası

    [JsonPropertyName("AccountType")]
    public string? AccountType { get; set; }  // Hesap türü
}

public class VakifbankTransactionItemDto
{
    [JsonPropertyName("CurrencyCode")]
    public string? CurrencyCode { get; set; }  // İşlem para birimi kodu

    [JsonPropertyName("TransactionType")]
    public string? TransactionType { get; set; }  // İşlem türü (örneğin, "Kredi", "Debet")

    [JsonPropertyName("Description")]
    public string? Description { get; set; } // İşlem açıklaması

    [JsonPropertyName("Amount")]
    public string? Amount { get; set; }  // İşlem tutarı

    [JsonPropertyName("TransactionDate")]
    public string? TransactionDate { get; set; }  // İşlem tarihi (örneğin, "2024-01-01T12:00:00")

    [JsonPropertyName("TransactionId")]
    public string? TransactionId { get; set; }  // İşlem ID'si
}

public class VakifbankReceiptResponseDto
{
    [JsonPropertyName("Header")]
    public VakifbankHeaderDto? Header { get; set; }  // API yanıtının başlık bilgisi

    [JsonPropertyName("Data")]
    public VakifbankReceiptDataDto? Data { get; set; }  // Makbuz verisi

    [JsonPropertyName("Documents")]
    public VakifbankReceiptDocumentsDto? Documents { get; set; }   // Makbuz belgeleri (örneğin, metin ve PDF formatında makbuz)
}

public class VakifbankReceiptDataDto
{
    [JsonPropertyName("Info")]
    public VakifbankReceiptInfoDto? Info { get; set; }  // Makbuz bilgisi
}

public class VakifbankReceiptInfoDto
{
    [JsonPropertyName("TransactionId")]
    public string? TransactionId { get; set; }  // İşlem ID'si
}

public class VakifbankReceiptDocumentsDto
{
    [JsonPropertyName("TextReceipt")]
    public string? TextReceipt { get; set; }  // Metin formatında makbuz

    [JsonPropertyName("PdfReceipt")]
    public string? PdfReceipt { get; set; }  // PDF formatında makbuz (base64 kodlu)

    [JsonPropertyName("TransactionId")]
    public string? TransactionId { get; set; }  // İşlem ID'si (makbuzun hangi işleme ait olduğunu belirtir)
}